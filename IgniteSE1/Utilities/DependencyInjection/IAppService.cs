using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Utilities
{

    //We will pull most of this stuff out into a seperate library later
    internal interface IAppService
    {

        /// <summary>
        /// Initializes the component and prepares it for use.
        /// </summary>
        /// <remarks>Call this method before performing any operations that depend on the component being
        /// ready. Subsequent calls may have no effect if the component is already initialized.</remarks>
        void Init();

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
