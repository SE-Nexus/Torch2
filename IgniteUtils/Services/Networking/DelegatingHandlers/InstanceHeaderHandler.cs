using InstanceUtils.Models.Server;
using InstanceUtils.Utils.Identification;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Torch2API.Constants;
using static VRage.Dedicated.Configurator.SelectInstanceForm;

namespace InstanceUtils.Services.Networking
{
    internal class InstanceHeaderHandler : DelegatingHandler
    {
        private readonly IConfigService _instance;

        public InstanceHeaderHandler(IConfigService instance)
        {
            _instance = instance;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.TryAddWithoutValidation(TorchConstants.InstanceIdHeader, _instance.Identification.InstanceID.ToString());
            return base.SendAsync(request, cancellationToken);
        }


        
    }
}
