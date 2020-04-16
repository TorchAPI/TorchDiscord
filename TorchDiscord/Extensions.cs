using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace TorchDiscord
{
    public static class Extensions
    {
        public static DiscordMember Member(this DiscordUser user)
        {
            return user as DiscordMember;
        }

        public static async Task<DiscordMember> GetMemberAsyncSafe(this DiscordGuild guild, ulong id)
        {
            try
            {
                return await guild.GetMemberAsync(id);
            }
            catch (NotFoundException)
            {
                return null;
            }
        }

        public static string NickOrUserName(this DiscordMember user)
        {
            if (!string.IsNullOrEmpty(user?.Nickname))
                return user.Nickname;

            return user?.Username;
        }

        public static string NickOrUserName(this DiscordUser user)
        {
            DiscordMember gu = user as DiscordMember;

            if (!string.IsNullOrEmpty(gu?.Nickname))
                return gu.Nickname;

            return user.Username;
        }
    }
}
