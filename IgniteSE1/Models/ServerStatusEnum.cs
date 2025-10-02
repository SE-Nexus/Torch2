using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Models
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
    /// <remarks>This enumeration defines commands for managing the lifecycle of a server, such as starting,
    /// stopping, or restarting it.</remarks>
    public enum ServerStateCommand
    {
        Idle,
        Start,
        Stop,
        Restart,
        Kill
    }
}
