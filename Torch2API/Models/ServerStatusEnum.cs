using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Torch2API.Models
{

    /// <summary>
    /// Represents the various operational states of a server.
    /// </summary>
    /// <remarks>This enumeration defines the possible statuses a server can have during its lifecycle,  such
    /// as initialization, running, or encountering an error. These values can be used to  monitor and respond to
    /// changes in the server's state.</remarks>
    public enum ServerStatusEnum
    {
        Initializing,
        Idle,
        Starting,
        Running,
        Stopping,
        Stopped,
        Error
    }

    /// <summary>
    /// Represents the set of commands that can be issued to control the state of a server.
    /// </summary>
    /// <remarks>Server boots in kill and will transition to idle when initializing is done.</remarks>
    public enum ServerStateCommand
    {

        /// <summary>
        /// Server is Idle, awaiting start signal. (Enter from Stopped, or kill etc)
        /// </summary>
        Idle,

        /// <summary>
        /// Server is started.
        /// </summary>
        Start,

        /// <summary>
        /// Server stopped
        /// </summary>
        Stop,

        /// <summary>
        /// Server Restart Command
        /// </summary>
        Restart,

        /// <summary>
        /// Server Kill command
        /// </summary>
        Kill
    }
}
