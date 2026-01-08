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

namespace IgniteSE1.Commands
{
    [CommandGroup("config/profiles", "Commands to manage the Instance Profiles", CommandTypeEnum.AdminOnly)]
    public class ProfileCommands
    {
        private ICommandContext ctx;
        private ProfileManager _InstanceManager;
        private PanelHTTPClient _PanelClient;

        public ProfileCommands(ICommandContext ctxICommandContext, ProfileManager iManager, PanelHTTPClient webPanelClient)
        {
            this.ctx = ctxICommandContext;
            _InstanceManager = iManager;
            _PanelClient = webPanelClient;
        }


        [Command("list", "Displays all the instance profiles")]
        public async Task List()
        {
            var instances = _InstanceManager.GetAllInstances();

            if(ctx is WebPanelContext)
            {
                await _PanelClient.PostAsync("api/instance/allprofiles", _InstanceManager.GetAllInstances());
                return;
            }

            ctx.RespondLine("All Instances:");
            foreach(var instance in instances)
            {
                ctx.RespondLine($" - {instance.InstanceName}");
            }
        }




    }
}
