using Sandbox.Engine.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch2API.Models.Configs;
using Torch2API.Models.SE1;
using VRage.Game;
using VRage.Game.ModAPI;

namespace IgniteSE1.Utilities.Extensions
{
    public static class ConfigExtensions
    {

        private const string _DedicatedCfgFilename = "SpaceEngineers-Dedicated.cfg";

        public static IMyConfigDedicated LoadDedicatedServerConfigs(this ProfileCfg? cfg, string WorldsDir)
        {


            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cfg.InstancePath, _DedicatedCfgFilename);
            var gameconfig = new MyConfigDedicated<MyObjectBuilder_SessionSettings>(configPath);
            gameconfig.SessionSettings = null; //Since the game actually doesnt even use these and the ignore last session doesnt work haha

            /// Load or create the config file
            if (File.Exists(configPath))
            {
                gameconfig.Load(configPath);
            }
            else
            {
                gameconfig.Save();
            }


            gameconfig.WorldName = cfg.TargetWorld;
            gameconfig.LoadWorld = Path.Combine(Path.GetFullPath(WorldsDir), cfg.TargetWorld);


            return gameconfig;
        }


        public static ConfigDedicatedSE1 GetDedicatedConfig(this IMyConfigDedicated dedicated)
        {
            if (dedicated is null) throw new ArgumentNullException(nameof(dedicated));

            return new ConfigDedicatedSE1
            {
                Administrators = dedicated.Administrators,
                AsteroidAmount = dedicated.AsteroidAmount,
                Banned = dedicated.Banned,
                Reserved = dedicated.Reserved,
                GroupID = dedicated.GroupID,
                IgnoreLastSession = dedicated.IgnoreLastSession,
                IP = dedicated.IP,
                LoadWorld = dedicated.LoadWorld,
                CrossPlatform = dedicated.CrossPlatform,
                PauseGameWhenEmpty = dedicated.PauseGameWhenEmpty,
                MessageOfTheDay = dedicated.MessageOfTheDay,
                MessageOfTheDayUrl = dedicated.MessageOfTheDayUrl,
                AutoRestartEnabled = dedicated.AutoRestartEnabled,
                AutoRestatTimeInMin = dedicated.AutoRestatTimeInMin,
                AutoUpdateEnabled = dedicated.AutoUpdateEnabled,
                AutoUpdateCheckIntervalInMin = dedicated.AutoUpdateCheckIntervalInMin,
                AutoUpdateRestartDelayInMin = dedicated.AutoUpdateRestartDelayInMin,
                RestartSave = dedicated.RestartSave,
                AutoUpdateSteamBranch = dedicated.AutoUpdateSteamBranch,
                AutoUpdateBranchPassword = dedicated.AutoUpdateBranchPassword,
                ServerDescription = dedicated.ServerDescription,
                ServerPasswordHash = dedicated.ServerPasswordHash,
                ServerPasswordSalt = dedicated.ServerPasswordSalt,
                Plugins = dedicated.Plugins,
                AutodetectDependencies = dedicated.AutodetectDependencies,
                SaveChatToLog = dedicated.SaveChatToLog,
                ChatAntiSpamEnabled = dedicated.ChatAntiSpamEnabled,
                SameMessageTimeout = dedicated.SameMessageTimeout,
                SpamMessagesTime = dedicated.SpamMessagesTime,
                SpamMessagesTimeout = dedicated.SpamMessagesTimeout,
                DedicatedId = dedicated.DedicatedId
            };
        }

        /// <summary>
        /// Copies values from the DTO/model into the runtime IMyConfigDedicated instance.
        /// This does not persist/save the config to disk — caller should handle saving/restarting if required.
        /// </summary>
        public static void SetDedicatedConfig(this IMyConfigDedicated dedicated, ConfigDedicatedSE1 model)
        {
            if (dedicated is null) throw new ArgumentNullException(nameof(dedicated));
            if (model is null) throw new ArgumentNullException(nameof(model));

            // Lists: if null, assign empty list to avoid passing null into runtime config
            dedicated.Administrators = model.Administrators ?? new List<string>();
            dedicated.AsteroidAmount = model.AsteroidAmount;
            dedicated.Banned = model.Banned ?? new List<ulong>();
            dedicated.Reserved = model.Reserved ?? new List<ulong>();
            dedicated.GroupID = model.GroupID;
            dedicated.IgnoreLastSession = model.IgnoreLastSession;
            dedicated.IP = model.IP;
            dedicated.LoadWorld = model.LoadWorld;
            dedicated.CrossPlatform = model.CrossPlatform;
            dedicated.PauseGameWhenEmpty = model.PauseGameWhenEmpty;
            dedicated.MessageOfTheDay = model.MessageOfTheDay;
            dedicated.MessageOfTheDayUrl = model.MessageOfTheDayUrl;
            dedicated.AutoRestartEnabled = model.AutoRestartEnabled;
            dedicated.AutoRestatTimeInMin = model.AutoRestatTimeInMin;
            dedicated.AutoUpdateEnabled = model.AutoUpdateEnabled;
            dedicated.AutoUpdateCheckIntervalInMin = model.AutoUpdateCheckIntervalInMin;
            dedicated.AutoUpdateRestartDelayInMin = model.AutoUpdateRestartDelayInMin;
            dedicated.RestartSave = model.RestartSave;
            dedicated.AutoUpdateSteamBranch = model.AutoUpdateSteamBranch;
            dedicated.AutoUpdateBranchPassword = model.AutoUpdateBranchPassword;
            dedicated.ServerDescription = model.ServerDescription;
            dedicated.ServerPasswordHash = model.ServerPasswordHash;
            dedicated.ServerPasswordSalt = model.ServerPasswordSalt;
            dedicated.Plugins = model.Plugins ?? new List<string>();
            dedicated.AutodetectDependencies = model.AutodetectDependencies;
            dedicated.SaveChatToLog = model.SaveChatToLog;
            dedicated.ChatAntiSpamEnabled = model.ChatAntiSpamEnabled;
            dedicated.SameMessageTimeout = model.SameMessageTimeout;
            dedicated.SpamMessagesTime = model.SpamMessagesTime;
            dedicated.SpamMessagesTimeout = model.SpamMessagesTimeout;
            dedicated.DedicatedId = model.DedicatedId;
        }
    }
}
