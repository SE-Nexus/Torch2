using HarmonyLib;
using IgniteSE1.Configs;
using IgniteSE1.Utilities;
using Sandbox.Engine.Platform;
using SpaceEngineers.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.FileSystem;

namespace IgniteSE1.Services
{
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
            Game.IsDedicated = true;
            SpaceEngineersGame.SetupBasicGameInfo();
            SpaceEngineersGame.SetupPerGameSettings();
            MyFileSystem.Reset();

            //Move this to the config file??
            // Ensure necessary directories exist
            if (!Directory.Exists(_configs.Config.Directories.ModStorage))
                Directory.CreateDirectory(_configs.Config.Directories.ModStorage);

            return base.Init();
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
