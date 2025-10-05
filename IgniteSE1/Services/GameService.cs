using EmptyKeys.UserInterface.Generated.StoreBlockView_Bindings;
using HarmonyLib;
using IgniteSE1.Configs;
using IgniteSE1.Utilities;
using Sandbox;
using Sandbox.Engine.Networking;
using Sandbox.Engine.Platform;
using Sandbox.Engine.Utils;
using Sandbox.Game;
using SpaceEngineers.Game;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Dedicated;
using VRage.FileSystem;
using VRage.Game;
using VRage.Game.SessionComponents;
using VRage.Platform.Windows;
using VRage.Trace;
using VRage.Utils;
using VRage.Utils.Keen;

namespace IgniteSE1.Services
{
    [HarmonyPatch]
    public class GameService : ServiceBase
    {
        private ConfigService _configs;
        private InstanceManager _instanceManager;

      

        public GameService(ConfigService configs, InstanceManager instance) 
        {
            _configs = configs;
            _instanceManager = instance;
        }

        

        public override Task<bool> Init()
        {
            /*  This method is largely based on the Space Engineers Dedicated Server from Keen Software House.
             * 
             *  SpaceEngineersDedicated.MyProgram.Main(string[] args)
             * 
             */



            // Set the game to dedicated mode
            Sandbox.Engine.Platform.Game.IsDedicated = true;
            SpaceEngineersGame.SetupBasicGameInfo();
            SpaceEngineersGame.SetupPerGameSettings();
            MyFileSystem.Reset();

            // Set up server-specific settings
            MyPerServerSettings.GameName = MyPerGameSettings.GameName;
            MyPerServerSettings.GameNameSafe = MyPerGameSettings.GameNameSafe;
            MyPerServerSettings.GameDSName = MyPerServerSettings.GameNameSafe + "Dedicated";
            MyPerServerSettings.GameDSDescription = "Your place for space engineering, destruction and exploring.";
            MyPerGameSettings.SendLogToKeen = false;
            MySessionComponentExtDebug.ForceDisable = true;
            MyPerServerSettings.AppId = 244850u;


            int? gameVersion = MyPerGameSettings.BasicGameInfo.GameVersion;
            MyFinalBuildConstants.APP_VERSION = (gameVersion.HasValue ? ((MyVersion)gameVersion.GetValueOrDefault()) : null);

            
            MyVRageWindows.Init(MyPerGameSettings.BasicGameInfo.ApplicationName, MySandboxGame.Log, null, detectLeaks: false);

            //VRage.Platform.Windows.Sys.MyWindowsSystem.WriteLineToConsole()


            if (!VRage.Dedicated.DedicatedServer.IsVcRedist2019Installed())
            {
                AnsiConsole.MarkupLine("[red]Error:[/] Please install latest C++ redistributable package for 2015-2019 x64");
                return Task.FromResult(false);
            }


            MySandboxGame.InitMultithreading();

            //Move this to the config file??
            // Ensure necessary directories exist
            if (!Directory.Exists(_configs.Config.Directories.ModStorage))
                Directory.CreateDirectory(_configs.Config.Directories.ModStorage);

            return base.Init();
        }


        [HarmonyPatch("VRage.Platform.Windows.Sys.MyWindowsSystem, Vrage.Platform.Windows", "WriteLineToConsole")]
        private static bool WriteLineToConsole_Prefix(string msg)
        {

            //Dont write any keen log to console
            return false;
        }

        private bool SetupLogs()
        {
            MyLog log = MySandboxGame.Log;
            string appName = MyPerServerSettings.GameDSName;

            if (log is MyLogKeen myLogKeen)
            {
                myLogKeen.InitWithDateNoCheck(appName, MyFinalBuildConstants.APP_VERSION_STRING, -1);
            }
            else
            {
                log.InitWithDate(appName, MyFinalBuildConstants.APP_VERSION_STRING, -1);
            }
            MyLog.Default = MySandboxGame.Log;

            MySandboxGame.Log.WriteLineAndConsole(string.Format($"Is official: FALSE, TORCH DS"));
            MySandboxGame.Log.WriteLine("Branch / Sandbox: " + MyGameService.BranchName);
            MySandboxGame.Log.WriteLineAndConsole("Client Build Number: " + MyPerGameSettings.BasicGameInfo.ClientBuildNumber);
            MySandboxGame.Log.WriteLineAndConsole("Server Build Number: " + MyPerGameSettings.BasicGameInfo.ServerBuildNumber);
            MySandboxGame.Log.WriteLineAndConsole("Environment.ProcessorCount: " + Environment.ProcessorCount);
            MySandboxGame.Log.WriteLineAndConsole("Environment.OSVersion: " + MyVRage.Platform.System.GetOsName());
            MySandboxGame.Log.WriteLineAndConsole("Environment.CommandLine: " + Environment.CommandLine);
            MySandboxGame.Log.WriteLineAndConsole("Environment.Is64BitProcess: " + Environment.Is64BitProcess);
            MySandboxGame.Log.WriteLineAndConsole("Environment.Is64BitOperatingSystem: " + Environment.Is64BitOperatingSystem);
            MySandboxGame.Log.WriteLineAndConsole("Environment.Version: " + RuntimeInformation.FrameworkDescription);
            MySandboxGame.Log.WriteLineAndConsole("Environment.CurrentDirectory: " + Environment.CurrentDirectory);
            MySandboxGame.Log.WriteLineAndConsole("CPU Info: " + MyVRage.Platform.System.GetInfoCPU(out var frequency, out var _));
            MySandboxGame.CPUFrequency = frequency;
            MySandboxGame.Log.WriteLine("Default Culture: " + CultureInfo.CurrentCulture.Name);
            MySandboxGame.Log.WriteLine("Default UI Culture: " + CultureInfo.CurrentUICulture.Name);
            MyVRage.Platform.System.LogRuntimeInfo(MySandboxGame.Log.WriteLineAndConsole);


            //Minimum specs for SE Dedicated Server
            ulong totalPhysicalMemory = MyVRage.Platform.System.GetTotalPhysicalMemory();
            if (Environment.ProcessorCount < 3 || MySandboxGame.CPUFrequency < 3200 || totalPhysicalMemory < 6000000000L)
            {
                MySandboxGame.InsufficientHardware = true;
                AnsiConsole.MarkupLine("[yellow]Warning:[/] KSH minimum hardware requirements not met! Torch will continue.");
            }

            return true;
        }

        private void SetupKeenLog()
        {
           
            bool showConsole = false;
            if (showConsole && Environment.UserInteractive)
            {
                MySandboxGame.IsConsoleVisible = true;
            }
        }




        public override void AfterInit()
        {
            //After init, load game specific instance settings
            
            
            InstanceCfg cfg = _instanceManager.GetCurrentInstance();


            //This was hot shit in SE1. Why???
            string _contentPath = Path.Combine(_configs.Config.Directories.Game, "Content");
            string _instancePath = cfg.InstancePath;
            string _modsPath = Path.Combine(_configs.Config.Directories.ModStorage);


            string Game_ContentPath = UnTerminatePath(new DirectoryInfo(_contentPath).FullName);
            string Game_ShadersBasePath = Game_ContentPath;
            string Game_UserDataPath = Path.GetFullPath(_instancePath);
            string Game_ModsPath = Path.GetFullPath(_modsPath);

            FieldInfo m_contentPath = AccessTools.Field(typeof(MyFileSystem), "m_contentPath");
            FieldInfo m_shadersBasePath = AccessTools.Field(typeof(MyFileSystem), "m_shadersBasePath");
            FieldInfo m_userDataPath = AccessTools.Field(typeof(MyFileSystem), "m_userDataPath");
            FieldInfo m_modsPath = AccessTools.Field(typeof(MyFileSystem), "m_modsPath");

            m_contentPath.SetValue(null, Game_ContentPath);
            m_shadersBasePath.SetValue(null, Game_ShadersBasePath);
            m_userDataPath.SetValue(null, Game_UserDataPath);
            m_modsPath.SetValue(null, Game_ModsPath);

            Directory.CreateDirectory(Game_ModsPath);
            MyFileSystem.InitUserSpecific(null);

             _instanceManager.GetServerConfigs();

            SetupLogs();

            base.AfterInit();
        }

        public static string UnTerminatePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (path.Length > 1 &&
                (path[path.Length - 1] == Path.DirectorySeparatorChar ||
                 path[path.Length - 1] == Path.AltDirectorySeparatorChar))
            {
                return path.Substring(0, path.Length - 1);
            }

            return path;
        }








        public void StartServer()
        {

        }


        public void StopServer()
        {

        }



    }
}
