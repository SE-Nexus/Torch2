using IgniteUtils.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch2API.Models;

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


        internal Task<bool> CallState(ServerStatusEnum status)
        {
            switch (status)
            {
                case ServerStatusEnum.Initializing:
                    return Init();

                case ServerStatusEnum.Idle:
                    AfterInit();
                    return Task.FromResult(true);

                case ServerStatusEnum.Running:
                    ServerStarted();
                    return Task.FromResult(true);

                case ServerStatusEnum.Starting:
                    return ServerStarting();

                case ServerStatusEnum.Stopped:
                    return ServerStopped();

                case ServerStatusEnum.Stopping:
                    return ServerStopping();

                default: 
                    return Task.FromResult(false);
            }
        }
    }
}
