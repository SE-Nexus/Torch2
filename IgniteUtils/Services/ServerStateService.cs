using IgniteUtils.Commands;
using IgniteUtils.Commands.TestCommand;
using IgniteUtils.Logging;
using IgniteUtils.Models;
using IgniteUtils.Services;
using IgniteUtils.Utils;
using NLog;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch2API.Models;
using Torch2API.Utils;
using VRage;

namespace IgniteUtils.Services
{
    /// <summary>
    /// Provides functionality to manage and monitor the state of a server.
    /// </summary>
    /// <remarks>This service is intended to be used as a base for implementing server state management
    /// features. It extends the <see cref="ServiceBase"/> class, inheriting its core service functionality.</remarks>
    public class ServerStateService : ServiceBase
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ConsoleManager _console;


        //Not sure if this should go here?
        public DateTime? GameRunningTime { get; set; }

        /// <summary>
        /// Occurs when the server state changes.
        /// </summary>
        /// <remarks>This event is triggered whenever a request is made to change the server's state,
        /// represented by the <see cref="ServerStateCommand"/> parameter. Subscribers can use this event to respond to
        /// changes in the server's operational state.</remarks>
        public event EventHandler<ServerStateCommand> ServerStateChanged;

        /// <summary>
        /// Occurs when the server status changes.
        /// </summary>
        /// <remarks>This event is raised whenever the server's status transitions to a new state.
        /// Subscribers can use this event to monitor and respond to changes in the server's operational
        /// status.</remarks>
        public event EventHandler<ServerStatusEnum> ServerStatusChanged;


        /// <summary>
        /// Gets the current operational status of the server.
        /// </summary>
        public ServerStatusEnum CurrentServerStatus { get; private set; } = ServerStatusEnum.Initializing;
        
        /// <summary>
        /// Gets the current state request for the server.
        /// </summary>
        public ServerStateCommand CurrentSateRequest { get; private set; } = ServerStateCommand.Kill;



        /// <summary>
        /// Datetime of the last server status change
        /// </summary>
        public DateTime LastStatusChangeTime { get; private set; } = DateTime.UtcNow;





        public ServerStateService(ConsoleManager console) 
        {
            _console = console;
            ServerStatusChanged += ServerStatusChanged_Event;

            //Debug only
            GameRunningTime = DateTime.UtcNow;
        }

        public override Task<bool> Init()
        {
            // define subcommands
            var startCmd = new Command("start", "Starts the Server")
            {
                new Option<bool>("--force")
            };

            var stopCmd = new Command("stop", "Stops the Server")
            {
               
                new Option<bool>("--force")
                {
                    Description = "Force stop the server",
                }
            };

            var restartCmd = new Command("restart", "Restarts the Server")
            {
                new Option<bool>("--force"),
            };

            var op = new Option<bool>("--test")
            {
                Description = "test group options",
            };





          

            
            




            /*
            stopCmd.SetAction()

            stopCmd.SetAction((bool force) =>
            {
                parseResult.GetResult<bool>("force");

                RequestServerStateChange(ServerStateCommand.Stop);
            });

            */


            return Task.FromResult(true);
        }

        private void SetupConsoleCommands()
        {

        }


        /// <summary>
        /// Requests to change the server state to the specified <paramref name="newState"/>.
        /// </summary>
        /// <remarks>This method updates the <see cref="CurrentServerStatus"/> property to the specified
        /// value and raises the <see cref="ServerStateChanged"/> event to notify subscribers of the state change.
        /// Additionally, a log entry is created to record the state change.</remarks>
        /// <param name="newState">The new state to set for the server.</param>
        public bool RequestServerStateChange(ServerStateCommand newState)
        {
            if (newState == CurrentSateRequest)
            {
                _logger.Warn($"Request to change server state to {newState}, but it is already requested.");
                return false;
            }

            ServerStatusEnum RequestedStatus = ServerStatusEnum.Initializing;

            // Validate transition based on current status
            switch (newState)
            {
                case ServerStateCommand.Idle:

                    if (CurrentServerStatus != ServerStatusEnum.Initializing && CurrentServerStatus != ServerStatusEnum.Stopped)
                        return FailTransition(newState, ServerStatusEnum.Initializing | ServerStatusEnum.Stopped);



                    RequestedStatus = ServerStatusEnum.Idle;
                    break;


                case ServerStateCommand.Start:
                    if (CurrentServerStatus != ServerStatusEnum.Idle)
                        return FailTransition(newState, ServerStatusEnum.Idle);

                    RequestedStatus = ServerStatusEnum.Starting;
                    break;

                case ServerStateCommand.Stop:
                    if (CurrentServerStatus != ServerStatusEnum.Running)
                        return FailTransition(newState, ServerStatusEnum.Running);

                    RequestedStatus = ServerStatusEnum.Stopping;
                    break;

                case ServerStateCommand.Restart:
                    if (CurrentServerStatus != ServerStatusEnum.Running)
                        return FailTransition(newState, ServerStatusEnum.Running);

                    RequestedStatus = ServerStatusEnum.Stopping;
                    break;

                case ServerStateCommand.Kill:
                    // Allow kill from any state, no validation
                    RequestedStatus = ServerStatusEnum.Error;
                    break;

                default:
                    _logger.Error($"Unhandled server state command: {newState}");
                    RequestedStatus = ServerStatusEnum.Error;
                    return false;
            }



            CurrentSateRequest = newState;
            

            // Raise the ServerStateChanged event to notify subscribers of the state change
            _logger.InfoColor($"Server state changed to: {CurrentSateRequest}", Color.Green);

            ServerStateChanged?.GetInvocationList()
                .Cast<EventHandler<ServerStateCommand>>()
                .ToList()
                .ForEach(handler =>
                {
                    Task.Run(() => handler(this, CurrentSateRequest));
                });

            ChangeServerStatus(RequestedStatus);
            return true;
        }

        private bool FailTransition(ServerStateCommand requested, ServerStatusEnum expected)
        {
            _logger.Warn(
                $"Cannot {requested} server. Current status is {CurrentServerStatus}, expected {expected}."
            );
            return false;
        }




        public bool ChangeServerStatus(ServerStatusEnum newStatus)
        {
            if (newStatus == CurrentServerStatus)
            {
                _logger.Warn($"Server is already in status {newStatus}, ignoring request.");
                return false;
            }

            // Validate allowed transitions
            switch (newStatus)
            {
                //It should never re-enter initializing
                case ServerStatusEnum.Initializing:
                    return FailStatusTransition(newStatus, ServerStatusEnum.Initializing);

                case ServerStatusEnum.Starting:
                    if(CurrentServerStatus != ServerStatusEnum.Idle)
                        return FailStatusTransition(newStatus, ServerStatusEnum.Idle);
                    break;

                case ServerStatusEnum.Running:
                    if (CurrentServerStatus != ServerStatusEnum.Starting)
                        return FailStatusTransition(newStatus, ServerStatusEnum.Starting);

                    GameRunningTime = DateTime.UtcNow;
                    break;

                case ServerStatusEnum.Stopping:
                    if (CurrentServerStatus != ServerStatusEnum.Running)
                        return FailStatusTransition(newStatus, ServerStatusEnum.Running);
                    break;

                case ServerStatusEnum.Stopped:
                    if (CurrentServerStatus != ServerStatusEnum.Stopping && CurrentServerStatus != ServerStatusEnum.Error)
                        return FailStatusTransition(newStatus, ServerStatusEnum.Stopping);
                    break;

                case ServerStatusEnum.Error:
                    // Allow transitions to Error from anywhere
                    break;

                case ServerStatusEnum.Idle:
                    if (CurrentServerStatus != ServerStatusEnum.Initializing && CurrentServerStatus != ServerStatusEnum.Stopped)
                        return FailStatusTransition(newStatus, ServerStatusEnum.Initializing | ServerStatusEnum.Stopped);
                    break;

                default:
                    _logger.Error($"Unhandled server status transition: {newStatus}");
                    return false;
            }

            CurrentServerStatus = newStatus;
            _console.UpdateConsoleTitle(CurrentServerStatus.ToString());
            _logger.InfoColor($"Server status changed to: {CurrentServerStatus}", Color.Yellow);

            ServerStatusChanged?.GetInvocationList()
                .Cast<EventHandler<ServerStatusEnum>>()
                .ToList()
                .ForEach(handler =>
                {
                    Task.Run(() => handler(this, CurrentServerStatus));
                });

            LastStatusChangeTime = DateTime.Now;



            return true;
        }

        private bool FailStatusTransition(ServerStatusEnum requested, ServerStatusEnum expected)
        {
            _logger.Warn(
                $"Cannot change server status to {requested}. Current status is {CurrentServerStatus}, expected {expected}."
            );
            return false;
        }



        /// <summary>
        /// Handles the event triggered when the server status changes.
        /// </summary>
        /// <remarks>If the server status changes to <see cref="ServerStatusEnum.Idle"/> and the
        /// auto-start configuration is enabled,  this method initiates an automatic server start.</remarks>
        /// <param name="sender">The source of the event, typically the object raising the event.</param>
        /// <param name="e">The new server status, represented as a value of the <see cref="ServerStatusEnum"/> enumeration.</param>
        private void ServerStatusChanged_Event(object sender, ServerStatusEnum e)
        {
            

        }

        public TimeSpan GetStateTime()
        {
            return DateTime.UtcNow - LastStatusChangeTime;
        }

        public TimeSpan GetGameRunningTime()
        {
            return DateTime.UtcNow - (GameRunningTime ?? DateTime.UtcNow);
        }


        public override string ToString()
        {
            return $"Server state: [{CurrentSateRequest}] | Status: [{CurrentServerStatus}] for {GetStateTime().ToCompactString()}";
        }

        






    }
}
