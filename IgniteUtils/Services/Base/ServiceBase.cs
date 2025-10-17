using IgniteUtils.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteUtils.Services
{
    public abstract class ServiceBase : IAppService
    {
        public bool IsInitialized { get; internal set; } = false;

        public IServiceProvider ServiceProvider { get; internal set; }


        internal void SetServiceProvider(IServiceProvider provider)
        {
            ServiceProvider = provider;
        }



        public virtual Task<bool> Init()
        {
            return Task.FromResult(true);
        }

        public virtual void AfterInit()
        {
            
        }

        public virtual void ServerStarted()
        {
            
        }

        public virtual void ServerStarting()
        {
            
        }

        public virtual void ServerStopped()
        {
            
        }

        public virtual void ServerStopping()
        {
            
        }

        public virtual void Stop()
        {
            
        }
    }
}
