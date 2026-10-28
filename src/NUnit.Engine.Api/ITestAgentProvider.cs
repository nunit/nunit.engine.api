// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System;

namespace NUnit.Engine.Services
{
    /// <summary>
    /// Provides test agents suitable for a <see cref="TestPackage"/> and accepts
    /// their return when the caller no longer requires them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This contract applies to in-process, local-process and remote agents. It does
    /// not require the caller to own a process or perform a particular registration
    /// handshake. The caller does not need to know how a provider obtains its agents
    /// or what resources it manages. The provider is responsible for its agents
    /// and their resources.
    /// </para>
    /// <para>
    /// The caller is responsible for returning each acquired agent through the same
    /// provider. Capability queries do not transfer that responsibility to the caller;
    /// see <see cref="IsAgentAvailable"/> for their initialization and discovery limits.
    /// </para>
    /// </remarks>
    public interface ITestAgentProvider : ITestAgentInfo
    {
        /// <summary>
        /// Determines whether the provider can currently supply an agent suitable
        /// for running the provided test package.
        /// </summary>
        /// <param name="package">A TestPackage</param>
        /// <returns>True if the provider can currently supply a compatible agent.</returns>
        /// <remarks>
        /// <para>
        /// This query does not return an agent or make the caller responsible for
        /// returning one. It does not guarantee that no extension or agent implementation
        /// is loaded or instantiated internally. Depending on the information supplied
        /// by an extension, the provider may use declared metadata or instantiate the
        /// extension to determine its capabilities. Other implementations may need to
        /// initialize provider resources or contact another host.
        /// </para>
        /// <para>
        /// A successful query does not reserve an agent. Availability may change before
        /// <see cref="GetAgent"/> is called, so the caller must still handle acquisition failure.
        /// The provider remains responsible for resources created while evaluating the query.
        /// </para>
        /// </remarks>
        bool IsAgentAvailable(TestPackage package);

        /// <summary>
        /// Acquires an agent suitable for the criteria specified in a <see cref="TestPackage"/>.
        /// </summary>
        /// <param name="package">The test package to be run</param>
        /// <returns>The acquired agent, which must be returned through this provider.</returns>
        /// <exception cref="ArgumentException">If the package is invalid.</exception>
        /// <exception cref="InvalidOperationException">
        /// No compatible agent is available. Call <see cref="IsAgentAvailable"/> first
        /// to check suitability, while allowing for availability to change before acquisition.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Successful acquisition makes the caller responsible for returning this exact
        /// agent reference using <see cref="ReturnAgent"/>. The provider retains responsibility
        /// for its underlying implementation and resources; acquiring an agent does not
        /// imply that the caller owns its process, transport or deployment.
        /// </para>
        /// <para>
        /// If acquisition throws, the provider remains responsible for resources created
        /// during the failed attempt.
        /// </para>
        /// </remarks>
        ITestAgent GetAgent(TestPackage package);

        /// <summary>
        /// Returns a previously acquired test agent to the provider that supplied it.
        /// </summary>
        /// <param name="agent">An agent previously acquired by a call to <see cref="GetAgent"/>.</param>
        /// <exception cref="InvalidOperationException">
        /// The reference was not acquired from this provider or was previously returned.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Return the exact reference through the same provider that supplied it so that
        /// ownership bookkeeping remains consistent. After successfully returning the
        /// agent, the caller must not continue using the reference. The provider remains
        /// responsible for its agents and their resources.
        /// </para>
        /// <para>
        /// A stop request, a closed connection or disposal of a runner is not confirmation
        /// that the agent was returned. If returning an agent fails, preserve its association
        /// with the originating provider. Further attempts to return it must use that provider.
        /// A failed call does not guarantee that no work was performed, so the caller must
        /// not assume that repeating it is harmless.
        /// </para>
        /// <para>
        /// Successfully returning an agent does not by itself prove that it terminated or that
        /// external resources, including deployed files, are no longer in use. Before
        /// removing resources used by an agent, the responsible provider must establish
        /// that the agent no longer needs them. This applies to all provider types.
        /// </para>
        /// </remarks>
        void ReturnAgent(ITestAgent agent);
    }
}
