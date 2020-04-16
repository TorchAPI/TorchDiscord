# TorchDiscord

The Official(tm) Discord plugin for Torch.

Currently barebones as I work to add additional features.

See [here](https://github.com/reactiflux/discord-irc/wiki/Creating-a-discord-bot-&-getting-a-token) for instructions on creating a bot, obtaining a token, and adding the bot to your Discord server.

Presently, this merely relays all global chat ingame to the Discord channel you select.

To get channel IDs, in Discord open user settings, click Appearance in the left-hand panel, then enable developer mode at the bottom of the page. Right-click the channel you want, and select 'copy ID'.

Commands sent in the admin channel forward to Torch's command system. The bot does not currently check for permission when executing commands, so make sure only server admins can see and send messages in your admin channel!

Planned features include a centralized bot hosted by Torch team, which will enable downtime notifications, cross-server chat, and you won't have to futz around with making bots and tokens. The bot will also associate users' Steam and Discord accounts, allowing for advanced faction features. Faction members will get a role managed by the bot, and access to private chat channels for their faction, again all managed automatically.

This bot is friendly to server performance, and is maintained by the same team bringing you Torch, so it will always be kept up to date!
