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

        public virtual Task<bool> ServerStarting()
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> ServerStopped()
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> ServerStopping()
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> Stop()
        {
            return Task.FromResult(true);
        }
    }
}
