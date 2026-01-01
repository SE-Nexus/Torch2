using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IgniteUtils.Services.WebPanel
{
    public interface IPanelCoreService
    {
        public Task SendStatus();

        public Task GetPublicIP();

    }
}
