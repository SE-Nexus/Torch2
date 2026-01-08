using IgniteSE1.Services;
using InstanceUtils.Services.Commands.Contexts;
using InstanceUtils.Services.WebPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch2API.Attributes;
using Torch2API.Models.Commands;

namespace IgniteSE1.Commands.Configs
{

    [CommandGroup("config/worlds", "Commands to manage the Instance Profiles", CommandTypeEnum.AdminOnly)]
    public class WorldCommands
    {
        private ICommandContext ctx;
        private ProfileManager _InstanceManager;
        private PanelHTTPClient _PanelClient;


        public WorldCommands(ICommandContext ctxICommandContext, ProfileManager iManager, PanelHTTPClient webPanelClient)
        {
            this.ctx = ctxICommandContext;
            _InstanceManager = iManager;
            _PanelClient = webPanelClient;
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


    }
}
