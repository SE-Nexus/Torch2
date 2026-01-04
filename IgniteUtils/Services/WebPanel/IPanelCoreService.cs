using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InstanceUtils.Services.WebPanel
{
    public interface IPanelCoreService
    {
        public Task SendStatus(CancellationToken ct = default);

        public Task GetPublicIP();

        public Task RunWSCommand(string json);

    }
}
