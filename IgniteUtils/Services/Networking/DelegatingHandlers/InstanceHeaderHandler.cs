using IgniteUtils.Models.Server;
using IgniteUtils.Utils.Identification;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static VRage.Dedicated.Configurator.SelectInstanceForm;

namespace IgniteUtils.Services.Networking
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
            request.Headers.TryAddWithoutValidation("InstanceID", _instance.Identification.InstanceID.ToString());
            return base.SendAsync(request, cancellationToken);
        }


        
    }
}
