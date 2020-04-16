using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Entities;
using Torch.API.Managers;

namespace TorchDiscord.Discord
{
    public struct Message
    {
        public string Content { get; }
        public string Author { get; }
        public bool IsBot { get; }
        public ulong ChannelId { get; }

        public Message(DiscordMessage m)
        {
            Content = m.Content;
            Author = m.Author.NickOrUserName();
            IsBot = m.Author.IsBot;
            ChannelId = m.ChannelId;
        }

        public Message(string author, string content, ulong channel)
        {
            Author = author;
            Content = content;
            ChannelId = channel;
            IsBot = false;
        }

        public Message(TorchChatMessage m, ulong channel)
        {
            Author = m.Author;
            Content = m.Message;
            IsBot = false;
            ChannelId = channel;
        }
    }
}
