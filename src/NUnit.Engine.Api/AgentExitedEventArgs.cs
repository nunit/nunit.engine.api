// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System;

namespace NUnit.Engine
{
    /// <summary>
    /// Represents the method that handles the event raised when an agent exits.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="AgentExitedEventArgs"/> that contains the event data.</param>
    public delegate void AgentExitedEventHandler(object sender, AgentExitedEventArgs e);

    /// <summary>
    /// Provides data for an event raised when a test agent exits.
    /// </summary>
    /// <remarks>Contains the agent that exited and its process exit code.</remarks>
    public class AgentExitedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentExitedEventArgs"/> class.
        /// </summary>
        /// <param name="agent">The test agent that exited.</param>
        /// <param name="message">An optional message provided by the agent on termination.</param>
        /// <param name="exitCode">An optional process exit code returned by the agent.</param>
        public AgentExitedEventArgs(ITestAgent agent, string? message = null, int? exitCode = null)
        {
            Agent = agent;
            Message = message;
            ExitCode = exitCode;
        }

        /// <summary>
        /// Gets the test agent. This must be the same instance that was returned
        /// by the provider when the agent was acquired.
        /// </summary>
        public ITestAgent Agent { get; }

        /// <summary>
        /// Gets an optional message provided by the agent on termination. Usually,
        /// this will give the reason for the agent exiting.
        /// </summary>
        public string? Message { get; }

        /// <summary>
        /// Gets the exit code returned by the operation, if available. This should
        /// be the native exit code returned by the process, if the agent was running
        /// in a separate process. If the agent was running in-process, or if the
        /// process exit code is unavailable this may be null.
        /// </summary>
        public int? ExitCode { get; }
    }
}
