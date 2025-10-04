using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Utilities
{

    //We will pull most of this stuff out into a seperate library later

    /// <summary>
    /// Defines the contract for application service components that manage initialization, lifecycle notifications, and
    /// operational control within the application.
    /// </summary>
    /// <remarks>Implement this interface to provide standardized methods for initializing, starting,
    /// stopping, and handling server lifecycle events. The interface is intended for use by components that require
    /// explicit control over their operational state and need to respond to server startup and shutdown notifications.
    /// Implementations should ensure that lifecycle methods are called in the appropriate order to maintain consistent
    /// application behavior.</remarks>
    internal interface IAppService
    {

        /// <summary>
        /// Initializes the system and prepares it for operation.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the
        /// initialization succeeds; otherwise, <see langword="false"/>.</returns>
        Task<bool> Init();


        /// <summary>
        /// 
        /// </summary>
        void AfterInit();



        /// <summary>
        /// Stops the current operation or service.
        /// </summary>
        /// <remarks>Call this method to halt ongoing activity. The specific behavior depends on the
        /// implementation; some services may require additional cleanup or may not be immediately stopped. This method
        /// is typically used to gracefully terminate processing or release resources.</remarks>
        void Stop();

        /// <summary>
        /// Notifies that the server is beginning its startup process.
        /// </summary>
        /// <remarks>Implement this method to perform any initialization or setup required before the
        /// server becomes fully operational. This method is typically called once during the server's lifecycle, prior
        /// to accepting client connections.</remarks>
        void ServerStarting();


        /// <summary>
        /// Notifies listeners that the server is beginning the shutdown process.
        /// </summary>
        /// <remarks>Implement this method to perform any necessary cleanup or resource release before the
        /// server stops. This notification is typically used to gracefully handle server shutdown events.</remarks>
        void ServerStopping();


        /// <summary>
        /// Notifies that the server has successfully started and is ready to accept requests.
        /// </summary>
        void ServerStarted();

        /// <summary>
        /// Notifies listeners that the server has stopped running.
        /// </summary>
        /// <remarks>Implement this method to handle any cleanup or state updates required when the server
        /// stops. This notification is typically used to trigger actions such as releasing resources or updating user
        /// interfaces.</remarks>
        void ServerStopped();
    }
}
