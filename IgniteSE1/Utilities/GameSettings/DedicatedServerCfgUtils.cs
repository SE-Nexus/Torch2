using Sandbox.Engine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Torch2API.Models.Schema;
using VRage.Game.ModAPI;

namespace IgniteSE1.Utilities.GameSettings
{
    public class DedicatedServerCfgUtils
    {
        public static SettingDefinition Create<T>(Expression<Func<IMyConfigDedicated, T>> selector, string description, double? min = null, double? max = null)
        {
            var prop = (selector.Body as MemberExpression)?.Member as PropertyInfo;
            var type = typeof(T);

            return new SettingDefinition
            {
                Name = prop.Name,
                Type = type.FullName,
                Description = description,
                Min = min,
                Max = max,
                IsList = typeof(System.Collections.IEnumerable).IsAssignableFrom(type)
            };
        }

        public static List<SettingDefinition> GetAllDedicatedSchema()
        {
            return new List<SettingDefinition>
            {
               Create(x => x.Administrators, "Gets or sets administrators. It may contain ids in 2 different formats: First - steamId.ToString() Second - starts with `STEAM_0:`"),
                Create(x => x.AsteroidAmount, "Not used"),
                Create(x => x.Banned, "Gets or sets Banned players. SteamId and Xbox live ids"),
                Create(x => x.Reserved, "Gets or sets reserved players (player can join server even if it is full). SteamId and Xbox live ids"),
                Create(x => x.GroupID, "Steam group id, that blocking access to player not from this group. You need save and restart server to apply changes"),
                Create(x => x.IgnoreLastSession, "Setting that is used server start. When it is true, it should not load previous server launch world. You need save and restart server to apply changes"),
                Create(x => x.IP, "Gets or sets server IP. You need save and restart server to apply changes"),
                Create(x => x.LoadWorld, "Gets current world load path or sets next server start load path"),
                Create(x => x.CrossPlatform, "Cross-platform flag"),
                Create(x => x.VerboseNetworkLogging, "Not used"),
                Create(x => x.PauseGameWhenEmpty, "Pause game when zero players on servers"),
                Create(x => x.MessageOfTheDay, "Shows Gui Popup for players"),
                Create(x => x.MessageOfTheDayUrl, "Shows Gui Popup for players but in web browser"),
                Create(x => x.AutoRestartEnabled, "Gets or sets whether auto restart is enabled"),
                Create(x => x.AutoRestatTimeInMin, "Gets or sets auto restart time in minutes"),
                Create(x => x.AutoUpdateEnabled, "Gets or sets if game auto update enabled"),
                Create(x => x.AutoUpdateCheckIntervalInMin, "Gets or sets how often game checks if new version is available"),
                Create(x => x.AutoUpdateRestartDelayInMin, "Gets or sets time before restart after new update found"),
                Create(x => x.RestartSave, "Gets or sets if game should save on server stop"),
                Create(x => x.AutoUpdateSteamBranch, "Gets or sets name of steam version branch"),
                Create(x => x.AutoUpdateBranchPassword, "Gets or sets password of steam version branch"),
                Create(x => x.ServerName, "Gets or sets server name"),
                Create(x => x.ServerPort, "Gets or sets server connection port 27016 - default"),
                Create(x => x.SessionSettings, "Gets or sets (but that doesn't change anything) session settings"),
                Create(x => x.SteamPort, "Gets or sets steam port"),
                Create(x => x.WorldName, "Gets or sets world name. Doesn't change world name in client find server gui when set"),
                Create(x => x.PremadeCheckpointPath, "When IgnoreLastSession is true and LoadWorld is null or empty, or failed - game would start new world from PremadeCheckpointPath"),
                Create(x => x.ServerDescription, "Gets or sets server description"),
                Create(x => x.ServerPasswordHash, "Gets or sets server password hash. You need save and restart server to apply changes"),
                Create(x => x.ServerPasswordSalt, "Gets or sets server password salt. You need save and restart server to apply changes"),
                Create(x => x.RemoteApiEnabled, "Gets or sets if remote api enabled. You need save and restart server to apply changes"),
                Create(x => x.RemoteSecurityKey, "Gets or sets remote api password. You need save and restart server to apply changes"),
                Create(x => x.RemoteApiPort, "Gets or sets remote api port. You need save and restart server to apply changes"),
                Create(x => x.RemoteApiIP, "Gets or sets remote API IP. You need save and restart server to apply changes"),
                Create(x => x.Plugins, "Gets or sets server plugins. List contains file paths to dlls. You need save and restart server to apply changes"),
                Create(x => x.WatcherInterval, "Not used"),
                Create(x => x.WatcherSimulationSpeedMinimum, "Not used"),
                Create(x => x.ManualActionDelay, "Not used"),
                Create(x => x.ManualActionChatMessage, "Not used"),
                Create(x => x.AutodetectDependencies, "Gets or sets if game should automatically add dependency mods in mods list. You need save and restart server to apply changes"),
                Create(x => x.SaveChatToLog, "Gets or sets whether to save chat to log"),
                Create(x => x.NetworkType, "Not used"),
                Create(x => x.NetworkParameters, "Not used"),
                Create(x => x.ConsoleCompatibility, "Not used"),
                Create(x => x.ChatAntiSpamEnabled, "Gets or sets whether chat anti spam is enabled"),
                Create(x => x.SameMessageTimeout, "Gets or sets the timeout for the same message, it cannot be sent again sooner than this (seconds)"),
                Create(x => x.SpamMessagesTime, "Gets or sets the time threshold for spam. If elapsed time between messages is less they are considered spam (seconds)"),
                Create(x => x.SpamMessagesTimeout, "If player is considered a spammer based on SpamMessagesTime they cannot send any messages for the duration of this timeout (seconds)"),
                Create(x => x.DedicatedId, "Id of a dedicated server")
                // Add more settings as needed
            };
        }
    }
}
