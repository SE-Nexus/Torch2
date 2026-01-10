using IgniteSE1.Services;
using InstanceUtils.Services;
using InstanceUtils.Services.Commands.Contexts;
using InstanceUtils.Services.WebPanel;
using Sandbox.Engine.Networking;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch2API.Attributes;
using Torch2API.Models.Commands;
using Torch2API.Models.Configs;
using VRage;
using VRage.Utils;

namespace IgniteSE1.Commands.Configs
{

    [CommandGroup("config/worlds", "Commands to manage the Instance Profiles", CommandTypeEnum.AdminOnly)]
    public class WorldCommands
    {
        //Prob need to rework the commands here
        
        private ICommandContext ctx;
        private ProfileManager _InstanceManager;
        private PanelHTTPClient _PanelClient;
        private ServerStateService _ServerStateService;


        public WorldCommands(ICommandContext ctxICommandContext, ProfileManager iManager, PanelHTTPClient webPanelClient, ServerStateService serverstate)
        {
            this.ctx = ctxICommandContext;
            _InstanceManager = iManager;
            _PanelClient = webPanelClient;
            _ServerStateService = serverstate;
        }


        [Command("list", "Displays all the worlds")]
        public async Task List()
        {
            var instances = _InstanceManager.GetAllWorlds();
            _InstanceManager.LoadAllWorlds();

            if (ctx is WebPanelContext)
            {
                await _PanelClient.PostAsync("api/instance/allworlds", instances);
                return;
            }

            ctx.RespondLine("All Worlds:");
            foreach (var instance in instances)
            {
                ctx.RespondLine($"{instance.Name} - Created On {instance.CreatedUtc.ToShortDateString()}");
            }
        }

        [Command("custom", "Displays all custom worlds")]
        public async Task GetCustomWorlds()
        {
            if(_ServerStateService.CurrentServerStatus < Torch2API.Models.ServerStatusEnum.Idle)
            {
                ctx.RespondLine("Server isnt loaded");
                return;
            }

            //Retrieves all viable world infos
            List<MyWorldInfo> allWorlds = MyLocalCache.GetAllWorldInfos();

            if (ctx is WebPanelContext)
            {
                //Actual world properties
                List<WorldInfo> worldInfos = new List<WorldInfo>();
                foreach(var world in allWorlds)
                {
                    //Keeen why

                    WorldInfo panelworld = new WorldInfo();
                    panelworld.Name = MyTexts.GetString(world.SessionName);
                    panelworld.Description = MyTexts.GetString(world.DescriptionId);
                    panelworld.ScenarioName = world.ScenarioName;
                    panelworld.Briefing = world.Briefing;
                    panelworld.IsCorrupted = world.IsCorrupted;
                    panelworld.HasPlanets = world.HasPlanets;
                    panelworld.IsCampaign = world.IsCampaign;
                    panelworld.SessionDirectoryPath = world.SessionDirectoryPath;

                    worldInfos.Add(panelworld);
                }


                await _PanelClient.PostAsync("api/instance/customworlds", worldInfos);
                return;
            }

            ctx.RespondLine("All Worlds:");
            foreach (var world in allWorlds)
            {
                ctx.RespondLine($"{world.ScenarioName}");
            }

        }


        [Command("create", "Creates a new world. Optionally uses specific premade name")]
        public async Task NewWorld([Option("--worldname", "User specified worldname")] string worldname, [Option("--template", "Template")] string template = "")
        {
            if(string.IsNullOrEmpty(worldname))
            {
                ctx.RespondLine("A worldname is required");
                return;
            }

            MyWorldInfo TargetWorld;
            List<MyWorldInfo> allWorlds = MyLocalCache.GetAllWorldInfos();
            if (string.IsNullOrEmpty(template))
            {
                //If not id was specified return first item in seq
                TargetWorld = allWorlds.First();
            }
            else
            {
                TargetWorld = allWorlds.Find(x => string.Equals(x.SessionName, template, StringComparison.OrdinalIgnoreCase));
            }

            if(TargetWorld == null)
            {
                ctx.RespondLine($"Unable to find a specified template of {template}");
                return;
            }


            if(!_InstanceManager.TryCreateWorld(worldname, TargetWorld.SessionPath, out string reason))
            {
                ctx.RespondLine($"Failed to create a new world: {reason}");
                return;
            }
            else
            {
                ctx.RespondLine($"$Successfully created new world {worldname}");
            }

        }

        [Command("delete", "Deletes the specified world")]
        public async Task GetCustomScenarios()
        {

        }


    }
}
