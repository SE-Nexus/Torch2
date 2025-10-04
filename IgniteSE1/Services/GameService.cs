using IgniteSE1.Utilities;
using Sandbox.Engine.Platform;
using SpaceEngineers.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Services
{
    public class GameService : ServiceBase
    {
        private ConfigService _configs;

        public GameService(ConfigService configs) 
        {
            _configs = configs;


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

            return base.Init();
        }

    }
}
