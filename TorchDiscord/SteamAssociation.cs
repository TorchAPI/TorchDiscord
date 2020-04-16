using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using TorchDiscord.Localization;
using System.Security.Cryptography;

namespace TorchDiscord
{
    public class SteamAssociation
    {
        private Dictionary<ulong, ulong> _associationTable = new Dictionary<ulong, ulong>();
        private Dictionary<string, ulong> _keys = new Dictionary<string, ulong>();
        private HashSet<ulong> _pendingUsers = new HashSet<ulong>();

        private Random _random = new Random();
        private const int KEY_LENGTH = 6;
        private const string CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        

        public async Task<bool> RegisterNewUser(ulong discordUser)
        {
            if (_associationTable.ContainsKey(discordUser) || !_pendingUsers.Add(discordUser))
                return false;

            var key = GetRandomKey();
            _keys.Add(key, discordUser);

            var message = string.Format(Strings.AssociateBegin, key);

            return await DiscordPlugin.Instance.Provider.SendPrivateMessage(discordUser, message);
        }

        public bool VerifyKey(ulong steamUser, string key, out ulong discordUser)
        {
            if (!_keys.TryGetValue(key, out discordUser))
                return false;

            _associationTable.Add(discordUser, steamUser);
            _pendingUsers.Remove(discordUser);
            _keys.Remove(key);

            return true;
        }

        public ulong GetDiscordAssociation(ulong steamUser)
        {
            throw new NotImplementedException();
        }

        private string GetRandomKey()
        {
            int lim = CHARS.Length - 1;
            var key = new char[KEY_LENGTH];

            for (int i = 0; i < KEY_LENGTH; i++)
            {
                key[i] = CHARS[_random.Next(0, lim)];
            }

            return new string(key);
        }

        public string GetAsociationToken(ulong discordUser)
        {
            if (!_associationTable.TryGetValue(discordUser, out ulong steamUser))
                return null;

            var token = new AssociationToken(discordUser, steamUser);

            using (var ms = new MemoryStream(512))
            {
                Serializer.Serialize(ms, token);
                var d = ms.ToArray();

                var ds =  Convert.ToBase64String(d);

                return $"//TOKEN===={ds}====TOKEN//";
            }
        }

        internal bool VerifyAssociationToken(string input, out AssociationToken token)
        {
            string data = Utilities.ExtractToken(input);
            var d = Convert.FromBase64String(data);
            try
            {
                using (var ms = new MemoryStream(d))
                    token = Serializer.Deserialize<AssociationToken>(ms);
            }
            catch (Exception ex)
            {
                token = default(AssociationToken);
                return false;
            }

            return token.Validate();            
        }

        [ProtoContract]
        internal struct AssociationToken
        {
            private static Random _random = new Random();

            [ProtoMember(1)]
            public ulong DiscordUser;
            [ProtoMember(2)]
            public ulong SteamUser;
            [ProtoMember(3)]
            public byte[] RandomData;
            [ProtoMember(4)]
            public byte[] Hash;

            public AssociationToken(ulong discordUser, ulong steamUser)
            {
                DiscordUser = discordUser;
                SteamUser = steamUser;

                RandomData = new byte[256];
                _random.NextBytes(RandomData);
                Hash = null;

                using (var ms = new MemoryStream(512))
                {
                    Serializer.Serialize(ms, this);
                    ms.Position = 0;
                    using (var md5 = MD5.Create())
                    {
                        Hash = md5.ComputeHash(ms);
                    }
                }
            }

            public bool Validate()
            {
                var hash = Hash;
                Hash = null;

                using (var ms = new MemoryStream(512))
                {
                    Serializer.Serialize(ms, this);
                    ms.Position = 0;

                    using (var md5 = MD5.Create())
                    {
                        var hv = md5.ComputeHash(ms);
                        Hash = hash;
                        return hv == hash;
                    }
                }
            }
        }
    }
}
