using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Dynamic;

namespace Torch2API.Models.SE1
{
    public class ConfigDedicatedSE1
    {
        [Description("Gets or sets administrators. It may contain ids in 2 different formats: First - steamId.ToString() Second - starts with `STEAM_0:`")]
        public List<string> Administrators { get; set; }

        [Description("Not used")]
        public int AsteroidAmount { get; set; }

        [Description("Gets or sets Banned players. SteamId and Xbox live ids")]
        public List<ulong> Banned { get; set; }

        [Description("Gets or sets reserved players (player can join server even if it is full). SteamId and Xbox live ids")]
        public List<ulong> Reserved { get; set; }

        [Description("Steam group id, that blocking access to player not from this group. You need save and restart server to apply changes")]
        public ulong GroupID { get; set; }

        [Description("Setting that is used server start. When it is true, it should not load previous server launch world. You need save and restart server to apply changes")]
        public bool IgnoreLastSession { get; set; }

        [Description("Gets or sets server IP. You need save and restart server to apply changes")]
        public string IP { get; set; }

        [Description("Gets current world load path or sets next server start load path")]
        public string LoadWorld { get; set; }

        [Description("Cross-platform flag")]
        public bool CrossPlatform { get; set; }

        [Description("Pause game when zero players on servers")]
        public bool PauseGameWhenEmpty { get; set; }

        [Description("Shows Gui Popup for players")]
        public string MessageOfTheDay { get; set; }

        [Description("Shows Gui Popup for players but in web browser")]
        public string MessageOfTheDayUrl { get; set; }

        [Description("Gets or sets whether auto restart is enabled")]
        public bool AutoRestartEnabled { get; set; }

        [Description("Gets or sets auto restart time in minutes")]
        public int AutoRestatTimeInMin { get; set; }

        [Description("Gets or sets if game auto update enabled")]
        public bool AutoUpdateEnabled { get; set; }

        [Description("Gets or sets how often game checks if new version is available")]
        public int AutoUpdateCheckIntervalInMin { get; set; }

        [Description("Gets or sets time before restart after new update found")]
        public int AutoUpdateRestartDelayInMin { get; set; }

        [Description("Gets or sets if game should save on server stop")]
        public bool RestartSave { get; set; }

        [Description("Gets or sets name of steam version branch")]
        public string? AutoUpdateSteamBranch { get; set; }

        [Description("Gets or sets password of steam version branch")]
        public string? AutoUpdateBranchPassword { get; set; }

        [Description("Gets or sets server description")]
        public string? ServerDescription { get; set; }

        [Description("Gets or sets server password hash. You need save and restart server to apply changes")]
        public string? ServerPasswordHash { get; set; }

        [Description("Gets or sets server password salt. You need save and restart server to apply changes")]
        public string? ServerPasswordSalt { get; set; }

        [Description("Gets or sets server plugins. List contains file paths to dlls. You need save and restart server to apply changes")]
        public List<string>? Plugins { get; set; }

        [Description("Gets or sets if game should automatically add dependency mods in mods list. You need save and restart server to apply changes")]
        public bool AutodetectDependencies { get; set; }

        [Description("Gets or sets whether to save chat to log")]
        public bool SaveChatToLog { get; set; }

        [Description("Gets or sets whether chat anti spam is enabled")]
        public bool ChatAntiSpamEnabled { get; set; }

        [Description("Gets or sets the timeout for the same message, it cannot be sent again sooner than this (seconds)")]
        public int SameMessageTimeout { get; set; }

        [Description("Gets or sets the time threshold for spam. If elapsed time between messages is less they are considered spam (seconds)")]
        public float SpamMessagesTime { get; set; }

        [Description("If player is considered a spammer based on SpamMessagesTime they cannot send any messages for the duration of this timeout (seconds)")]
        public int SpamMessagesTimeout { get; set; }

        [Description("Id of a dedicated server")]
        public long DedicatedId { get; set; }
    }
}
