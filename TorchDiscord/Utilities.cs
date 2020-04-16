using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TorchDiscord
{
    public static class Utilities
    {
        private static Regex _emoteRegex = new Regex("<:(.*):\\d+>");
        private static Regex _tokenRegex = new Regex("//TOKEN====(.*)====TOKEN//");
        public static string StripEmotes(string message)
        {
            return _emoteRegex.Replace(message, EmoteEvaluator);
        }

        private static string EmoteEvaluator(Match match)
        {
            if (match.Groups.Count != 2)
                return match.Groups[0].Value;
            return $":{match.Groups[1].Value}:";
        }

        public static string ExtractToken(string data)
        {
            return _tokenRegex.Match(data).Groups[1].Value;
        }

        public static bool CompareBytes(byte[] lh, byte[] rh)
        {
            if (lh.Length != rh.Length)
                return false;

            for (int i = 0; i < lh.Length; i++)
                if (lh[i] != rh[i])
                    return false;

            return true;
        }

        private static Queue<string> _store = new Queue<string>();
        public static bool CheckDuplicate(string message)
        {
            if (_store.Contains(message))
                return true;

            if (_store.Count > 32)
                _store.Dequeue();

            _store.Enqueue(message);
            return false;
        }
    }
}
