using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Utilities
{
    public abstract class ServiceBase : IAppService
    {
        public bool IsInitialized { get; internal set; } = false;

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
