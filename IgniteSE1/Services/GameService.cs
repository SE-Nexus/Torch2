using EmptyKeys.UserInterface.Generated.StoreBlockView_Bindings;
using HarmonyLib;
using IgniteSE1.Configs;
using IgniteSE1.Models;
using IgniteSE1.Utilities;
using NLog;
using Sandbox;
using Sandbox.Engine.Networking;
using Sandbox.Engine.Platform;
using Sandbox.Engine.Utils;
using Sandbox.Game;
using Sandbox.Game.World;
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
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using VRage;
using VRage.Dedicated;
using VRage.FileSystem;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilder;
using VRage.Game.SessionComponents;
using VRage.GameServices;
using VRage.Mod.Io;
using VRage.Platform.Windows;
using VRage.Plugins;
using VRage.Steam;
using VRage.Trace;
using VRage.Utils;
using VRage.Utils.Keen;
using VRageRender;

namespace IgniteSE1.Services
{
    [HarmonyPatch]
    public class GameService : ServiceBase
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private ConfigService _configs;
        private InstanceManager _instanceManager;
        private SteamService _steamService;
        private ServerStateService _serverState;

        private string DedicatedServer64;
        private Thread GameThread;




        public GameService(ConfigService configs, InstanceManager instance, SteamService steam, ServerStateService serverState) 
        {
            _configs = configs;
            _instanceManager = instance;
            _steamService = steam;
            _serverState = serverState;

            GameThread = new Thread(StartServer);
            GameThread.IsBackground = true;


            DedicatedServer64 = Path.Combine(_steamService.GameInstallDir, "DedicatedServer64");
        }

        

        public override Task<bool> Init()
        {
            /*  This method is largely based on the Space Engineers Dedicated Server from Keen Software House.
             * 
             *  SpaceEngineersDedicated.MyProgram.Main(string[] args)
             * 
             */

            // Set the game to dedicated mode
            MySandboxGame.IsConsoleVisible = true;
            SetupMyPerGameSettings();
            MyVRageWindows.Init(MyPerGameSettings.BasicGameInfo.ApplicationName, MySandboxGame.Log, null, detectLeaks: false);

            //VRage.Platform.Windows.Sys.MyWindowsSystem.WriteLineToConsole()


            if (!VRage.Dedicated.DedicatedServer.IsVcRedist2019Installed())
            {
                AnsiConsole.MarkupLine("[red]Error:[/] Please install latest C++ redistributable package for 2015-2019 x64");
                return Task.FromResult(false);
            }


            

            //Move this to the config file??
            // Ensure necessary directories exist
            if (!Directory.Exists(_configs.Config.Directories.ModStorage))
                Directory.CreateDirectory(_configs.Config.Directories.ModStorage);

            MyFileSystem.ExePath = DedicatedServer64;
            InitializeServices(true, false);

            MyNetworkMonitor.Init();
            MySandboxGame.InitMultithreading();

            MyPlugins.RegisterGameAssemblyFile(MyPerGameSettings.GameModAssembly);
            MyPlugins.RegisterGameObjectBuildersAssemblyFile(MyPerGameSettings.GameModObjBuildersAssembly);
            MyPlugins.RegisterSandboxAssemblyFile(MyPerGameSettings.SandboxAssembly);
            MyPlugins.RegisterSandboxGameAssemblyFile(MyPerGameSettings.SandboxGameAssembly);
            MyGlobalTypeMetadata.Static.Init();

            MyRenderProxy.Initialize(new MyNullRender());

            return base.Init();
        }

        private static void InitializeServices(bool isDedicated, bool isEOS)
        {
            MyServiceManager instance = MyServiceManager.Instance;

            IMyGameService myGameService = MySteamGameService.Create(isDedicated, MyPerServerSettings.AppId);

            instance.AddService(myGameService);
            instance.AddService((IMyMicrophoneService)new MyNullMicrophone());
            MyPlatformGameSettings.VERBOSE_NETWORK_LOGGING |= MySandboxGame.ConfigDedicated.VerboseNetworkLogging;

            var aggregator = new MyServerDiscoveryAggregator();
            instance.AddService<IMyServerDiscovery>(aggregator);


            IMyGameService service;
            if (false)
            {
                // EOS network setup
                /*
                List<string> networkParameters = m_networkParameters;
                IEnumerable<string> networkParameters2 = MySandboxGame.ConfigDedicated.NetworkParameters;
                IEnumerable<string> parameters = networkParameters.Union(networkParameters2 ?? Enumerable.Empty<string>());
                MyEOSService.InitNetworking(isDedicated, useEOSLobbyDiscovery: false, MyPerGameSettings.GameName, myGameService, "xyza7891A4WeGrpP85BTlBa3BSfUEABN", "ZdHZVevSVfIajebTnTmh5MVi3KPHflszD9hJB7mRkgg", "24b1cd652a18461fa9b3d533ac8d6b5b", "1958fe26c66d4151a327ec162e4d49c8", "07c169b3b641401496d352cad1c905d6", "https://retail.epicgames.com/", MyEOSService.CreatePlatform(), MySandboxGame.ConfigDedicated.VerboseNetworkLogging, parameters, null, MyMultiplayer.Channels);
                MyMockingInventory serviceInstance = new MyMockingInventory(myGameService);
                instance.AddService((IMyInventoryService)serviceInstance);
                */
            }
            else
            {
                MySteamGameService.InitNetworking(isDedicated, myGameService, MyPerGameSettings.GameName, null);
                IMyUGCService ugc = MySteamUgcService.Create(MyPerServerSettings.AppId, myGameService);
                MyGameService.WorkshopService.AddAggregate(ugc);



            }

            IMyUGCService ugc2 = MyModIoService.Create(MyServiceManager.Instance.GetService<IMyGameService>(), "spaceengineers", "264", "1fb4489996a5e8ffc6ec1135f9985b5b", "331", "f2b64abe55452252b030c48adc0c1f0e", MyPlatformGameSettings.UGC_TEST_ENVIRONMENT, true, MyPlatformGameSettings.MODIO_PLATFORM, MyPlatformGameSettings.MODIO_PORTAL);
            MyGameService.WorkshopService.AddAggregate(ugc2);
        }

        private void SetupMyPerGameSettings()
        {
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
        }


        [HarmonyPatch("VRage.Platform.Windows.Sys.MyWindowsSystem, Vrage.Platform.Windows", "WriteLineToConsole")]
        private static bool WriteLineToConsole_Prefix(string msg)
        {

            //Dont write any keen log to console
            return true;
        }

        private bool SetupLogs()
        {
            MyLog log = MySandboxGame.Log;
            string appName = MyPerServerSettings.GameDSName;

            if (log is MyLogKeen myLogKeen)
            {
                myLogKeen.InitWithDateNoCheck(appName, MyFinalBuildConstants.APP_VERSION_STRING, -1);
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



            IMyConfigDedicated dedicated = _instanceManager.GetServerConfigs();
            MySandboxGame.ConfigDedicated = dedicated;
            MySandboxGame.ConfigDedicated.ConsoleCompatibility = true;

            MySandboxGame.Config = new MyConfig(MyPerServerSettings.GameDSName + ".cfg");
            MySandboxGame.Config.Load();


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



        public override void ServerStarting()
        {
            GameThread.Start();
            base.ServerStarting();
        }





        public void StartServer()
        {
            var s = MyFileSystem.ExePath;

            // Initialize the game and load required native libraries
            Console.WriteLine($"Setting working directory to: {DedicatedServer64}");
            Directory.SetCurrentDirectory(DedicatedServer64);

            //Set Events:
            MySession.AfterLoading += MySession_AfterLoading;



            _logger.Info("Starting Game Server...");
            var _game = new MySandboxGame(new string[16]);

            if (MySandboxGame.FatalErrorDuringInit)
            {
                throw new InvalidOperationException("Failed to start sandbox game: see Keen log for details");
            }


            //Blocking call
            _game.Run();
        }


        // This event is called after the game session has finished loading
        private void MySession_AfterLoading()
        {
            _serverState.ChangeServerStatus(ServerStatusEnum.Running);
        }



        public void StopServer()
        {

        }



    }
}

