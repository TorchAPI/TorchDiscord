using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using NLog;

namespace TorchDiscord.Discord
{
    internal class LocalBotProvider : IDiscordProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public DiscordClient BotClient;

        public async Task Initialize()
        {
            try
            {
                Log.Info("Initializing local Discord bot....");

                var token = DiscordPlugin.Instance.Config?.DiscordToken;

                if (string.IsNullOrEmpty(token))
                {
                    Log.Error("Bot token not set! Cannot initialize Discord bot!");
                    return;
                }

                BotClient = new DiscordClient(new DiscordConfiguration
                                              {
                                                  Token = token,
                                                  TokenType = TokenType.Bot
                                              });

                BotClient.Ready += BotClient_Ready;

                await BotClient.ConnectAsync();

                BotClient.MessageCreated += BotClient_MessageCreated;
                BotClient.GuildCreated += BotClient_GuildCreated;

                Log.Info("Local Discord bot ready.");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public async Task SendMessage(Message m)
        {
            if (m.ChannelId == 0)
                return;

            if (Utilities.CheckDuplicate(m.Content))
                return;

            var channel = await BotClient.GetChannelAsync(m.ChannelId);
            
            await channel.SendMessageAsync(string.Format(DiscordPlugin.Instance.Config.DiscordFormat, m.Author, m.Content));
        }

        public event Action<Message> MessageCreated;

        public async Task<bool> SendPrivateMessage(ulong user, string message)
        {
            throw new NotImplementedException();
        }

        private async Task BotClient_GuildCreated(DSharpPlus.EventArgs.GuildCreateEventArgs e)
        {
        }

        private async Task BotClient_MessageCreated(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            var m = new Message(e.Message);

            MessageCreated?.Invoke(m);
        }

        private async Task BotClient_Ready(DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            BotClient.Ready -= BotClient_Ready;
            if (DiscordPlugin.Instance.Config.GlobalChannel != 0)
            {
                try
                {
                    var global = await BotClient.GetChannelAsync(DiscordPlugin.Instance.Config.GlobalChannel);
                    //var botMember = await global.Guild.GetMemberAsync(BotClient.CurrentUser.Id);
                    //if (!global.PermissionsFor(botMember).HasFlag(Permissions.SendMessages))
                    //    throw new Exception();
                }
                catch (Exception ex)
                {
                    Log.Error("Bot cannot access global chat channel! Discord setup cannot continue!");
                    await BotClient.DisconnectAsync();
                }
            }

            if (DiscordPlugin.Instance.Config.GlobalChannel != 0)
            {
                try
                {
                    var admin = await BotClient.GetChannelAsync(DiscordPlugin.Instance.Config.AdminChannel);
                    //var botMember = await admin.Guild.GetMemberAsync(BotClient.CurrentUser.Id);
                    //if (!admin.PermissionsFor(botMember).HasFlag(Permissions.SendMessages))
                    //    throw new Exception();
                }
                catch (Exception ex)
                {
                    Log.Error("Bot cannot access admin chat channel! Discord setup cannot continue!");
                    await BotClient.DisconnectAsync();
                }
            }

            Log.Info("Discord connected.");
        }
    }
}
