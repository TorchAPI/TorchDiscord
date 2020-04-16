using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using Torch;
using Torch.Views;
using TorchDiscord.Localization;

namespace TorchDiscord
{
    public class BotConfig : ViewModel
    {
        private string _discordToken;
        private bool _local = true;
        private Language _language;
        private ulong _globalChannel;
        private ulong _adminChannel;
        private string _discordFormat = "[{0}]: {1}";
        private string _gameFormat = "[{0}]";

        [Display(Name = "Discord Token", Description = "Token for the Discord Bot. See documentation on torch wiki for more information.")]
        public string DiscordToken
        {
            get => _discordToken;
            set => SetValue(ref _discordToken, value);
        }

        [Display(Enabled = false, Name = "Local bot", Description = "Discord bot will run locally on your server, instead of connecting to the central TorchAPI server.")]
        public bool Local
        {
            get => _local;
            set => SetValue(ref _local, value);
        }

        [Display(Name = "Language", Description = "Sets the language for the bot both in Discord and ingame. Translations other than English are provided by the community and may not be correct. Language defaults to system language if available, otherwise English.")]
        public Language Language
        {
            get => _language;
            set
            {
                SetLanguage_Internal(value);
                SetValue(ref _language, value);
            }
        }

        [Display(Name = "Global Channel", Description = "Channel ID where all global chat is relayed to and from the game.")]
        public ulong GlobalChannel
        {
            get => _globalChannel;
            set => SetValue(ref _globalChannel, value);
        }

        [Display(Name ="Admin Channel", Description = "Channel ID where admins may run Torch commands.\r\n!!DANGER!! ANY user in this channel may use ALL torch commands!")]
        public ulong AdminChannel
        {
            get => _adminChannel;
            set => SetValue(ref _adminChannel, value);
        }

        [Display(Name = "Discord Chat Format", Description = "Defines how messages are displayed in Discord. {0} is replaced with username, and {1} is replaced with the message. e.g.: [Torch Survival {0}]: {1} displays as [Torch Survival rexxar]: hello world")]
        public string DiscordFormat
        {
            get => _discordFormat;
            set => SetValue(ref _discordFormat, value);
        }

        [Display(Name = "Game Chat Format", Description ="Defines how messages are displayed ingame. {0} is replaced with username. e.g.: [Discord: {0}] displays as [Discord: rexxar]: hello world")]
        public string GameFormat
        {
            get => _gameFormat;
            set => SetValue(ref _gameFormat, value);
        }

        //To add localization, create a copy of Strings.resx and rename to Strings.xx.resx
        //substitute xx for your language code
        //the language codes supported by Windows can be found here https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c
        private void SetLanguage_Internal(Language newLanguage)
        {
            switch (newLanguage)
            {
                case Language.SystemLocale:
                    Strings.Culture = CultureInfo.CurrentCulture;
                    return;
                case Language.English:
                    Strings.Culture = CultureInfo.GetCultureInfo("en-US");
                    return;
                default:
                    throw new InvalidEnumArgumentException("Selected language is not supported!");
            }
        }
    }

    public enum Language
    {
        SystemLocale,
        English,
    }
}
