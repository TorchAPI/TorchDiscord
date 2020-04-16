using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TorchDiscord.Discord
{
    public interface IDiscordProvider
    {
        Task Initialize();
        Task SendMessage(Message m);
        event Action<Message> MessageCreated;
        Task<bool> SendPrivateMessage(ulong user, string message);
    }
}
