using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using DSharpPlus;
using NLog;
using Sandbox.Game.Entities;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Commands;
using Torch.Session;
using Torch.Views;
using TorchDiscord.Discord;
using VRage.Game;
using VRageMath;

namespace TorchDiscord
{
    public class DiscordPlugin : TorchPluginBase, IWpfPlugin
    {
        private static readonly Logger Log = LogManager.GetLogger("DiscordPlugin");

        public static DiscordPlugin Instance { get; private set; }


        private Persistent<BotConfig> _config;
        private TorchSessionManager _sessionManager;
        private ConcurrentQueue<Message> _chatQueue;

        private Timer _relayTimer;

        public BotConfig Config => _config?.Data;

        public SteamChatService ChatService { get; } = new SteamChatService();
        
        private IChatManagerServer _chat;
        private CommandManager _commands;
        public CommandManager Commands => _commands ?? (_commands = Torch.CurrentSession?.Managers.GetManager<CommandManager>());

        public IDiscordProvider Provider { get; private set; }

        public override void Init(ITorchBase torch)
        {
            //Log.Warn("DISCORD INIT YOU FUCK");
            base.Init(torch);

            string path = Path.Combine(StoragePath, "DiscordPlugin.cfg");
            _config = Persistent<BotConfig>.Load(path);
            _chatQueue = new ConcurrentQueue<Message>();
            _relayTimer = new Timer(100);
            _relayTimer.Elapsed += RelayTimer_Elapsed;
            _relayTimer.AutoReset = false;
            _relayTimer.Start();

            Instance = this;

            if (Config.Local)
                Provider = new LocalBotProvider();
            else
                throw new NotImplementedException("Centralized bot not implemented yet.");

            Provider.MessageCreated += _provider_MessageCreated;
            _sessionManager = torch.Managers.GetManager<TorchSessionManager>();
            _sessionManager.SessionStateChanged += SessionManagerOnSessionStateChanged;

        }

        private UserControl _control;

        /// <inheritdoc />
        public UserControl GetControl() => _control ?? (_control = new PropertyGrid() { DataContext = Config});

        private void RelayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Task.Run(async () =>
                     {
                         while (_chatQueue.TryDequeue(out Message m))
                             await Provider.SendMessage(m);
                         _relayTimer.Start();
                     });
        }

        private void SessionManagerOnSessionStateChanged(ITorchSession session, TorchSessionState newstate)
        {
            switch (newstate)
            {
                case TorchSessionState.Loading:
                    break;
                case TorchSessionState.Loaded:
                    Task.Run(async () => await Provider.Initialize());
                    _chat = Torch.CurrentSession.Managers.GetManager<IChatManagerServer>();
                    _commands = Torch.CurrentSession.Managers.GetManager<CommandManager>();
                    ChatService.Register();
                    ChatService.MessageReceived += ChatService_MessageReceived;
                    break;
                case TorchSessionState.Unloading:
                    ChatService.Unregister();
                    ChatService.MessageReceived -= ChatService_MessageReceived;
                    break;
                case TorchSessionState.Unloaded:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newstate), newstate, null);
            }
        }

        private void ChatService_MessageReceived(TorchChatMessage msg, ref bool consumed)
        {
            //Log.Warn($"INCOMING CHAT {msg.Message}");
            if (consumed || Commands.IsCommand(msg.Message))
            {
                //Log.Info("Caught command");
                return;
            }

            _chatQueue.Enqueue(new Message(msg, Config.GlobalChannel));
        }

        public override void Dispose()
        {
            _relayTimer.Stop();
            _relayTimer.Elapsed -= RelayTimer_Elapsed;
            _sessionManager.SessionStateChanged -= SessionManagerOnSessionStateChanged;
            Instance = null;
        }

        private void _provider_MessageCreated(Message message)
        {
            try
            {
                if (message.IsBot)
                    return;

                if (message.ChannelId == Config.AdminChannel)
                {
                    Log.Info($"Received command message: {message.Author} : {message.Content}");
                    var res = Commands.HandleCommandFromServer(message.Content);
                    if (res != null)
                    {
                        foreach (var response in res)
                        {
                            var m = new Message("Server", response.Message, Config.AdminChannel);
                            _chatQueue.Enqueue(m);
                        }

                        return;
                    }

                    Log.Error("null response");
                }

                if (message.ChannelId == Config.GlobalChannel)
                    SteamChatService.SendMessageNative(string.Format(Config.GameFormat, message.Author), Utilities.StripEmotes(message.Content), Color.Blue);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}
