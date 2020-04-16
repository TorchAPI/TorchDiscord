using System;
using System.Collections.Generic;
using System.Text;
using NLog;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Gui;
using Sandbox.Game.Multiplayer;
using Torch.API.Managers;
using VRage.Game;
using VRageMath;

namespace TorchDiscord
{
    public class SteamChatService : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private IChatManagerServer _backing_chat;
        private IChatManagerServer Chat => _backing_chat ?? (_backing_chat = DiscordPlugin.Instance.Torch.CurrentSession.Managers.GetManager<IChatManagerServer>());

        public event MessageProcessingDel MessageReceived;

        public void Register()
        {
            Chat.MessageProcessing += MessageProcessing;
            MyMultiplayer.Static.ScriptedChatMessageReceived += ScriptedMessageReceived;
        }

        private void ScriptedMessageReceived(string content, string author, string font, Color color)
        {
            //Log.Info($"Scripted Message: {author}: {content}");
            var msg = new TorchChatMessage(author, Sync.MyId, content, ChatChannel.GlobalScripted, 0);
            bool c = false;
            MessageReceived?.Invoke(msg, ref c);
        }

        private void MessageProcessing(TorchChatMessage msg, ref bool consumed)
        {
            Log.Warn($"Message: {msg.AuthorSteamId} - {msg.Author}: {msg.Message} - {consumed} :: {msg.Channel}");
            if (consumed)
                return;
            if (Utilities.CheckDuplicate(msg.Message))
                return;



            if(msg.Channel == ChatChannel.Global || msg.Channel == ChatChannel.GlobalScripted)
                MessageReceived?.Invoke(msg, ref consumed);
        }

        public void Unregister()
        {
            Chat.MessageProcessing -= MessageProcessing;
            MyMultiplayer.Static.ScriptedChatMessageReceived -= ScriptedMessageReceived;
        }

        public void Dispose()
        {
            _backing_chat = null;
            Unregister();
        }

        public void SendMessageAsUser(ulong author, string message)
        {
            Chat.SendMessageAsOther(author, message);
        }

        public void SendMessageAs(string author, string message, Color? authorColor = null)
        {
           // Chat.SendMessageAsOther(author, message, authorColor ?? Color.Blue);
           SendMessageNative(author, message, authorColor);
        }

        /// <summary>
        /// Sends message directly through SE's API instead of Torch
        /// (current Torch public build doesn't work)
        /// </summary>
        /// <param name="author"></param>
        /// <param name="message"></param>
        /// <param name="authorColor"></param>
        public static void SendMessageNative(string author, string message, Color? authorColor = null)
        {
            var scripted = new ScriptedChatMsg()
            {
                Author = author,
                Text = message,
                Font =  MyFontEnum.White,
                Color = authorColor ?? Color.Red,
                Target = 0
            };

            MyMultiplayerBase.SendScriptedChatMessage(ref scripted);
        }
    }
}
