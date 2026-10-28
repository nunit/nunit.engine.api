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
    /// handshake. The provider is responsible for the implementation-specific setup
    /// and lifetime of the agents it supplies.
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
        /// releasing one. It does not guarantee that no extension or agent implementation
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
        /// agent reference using <see cref="ReleaseAgent"/>. The provider retains responsibility
        /// for its underlying implementation and resources; acquiring an agent does not
        /// imply that the caller owns its process, transport or deployment.
        /// </para>
        /// <para>
        /// If acquisition throws, the caller has no returned agent to release. The provider
        /// remains responsible for resources created during the failed attempt and for
        /// reconciling an outcome that cannot yet be confirmed.
        /// </para>
        /// </remarks>
        ITestAgent GetAgent(TestPackage package);

        /// <summary>
        /// Returns a previously acquired test agent to the provider that supplied it.
        /// </summary>
        /// <param name="agent">An agent previously provided by a call to GetAgent.</param>
        /// <exception cref="InvalidOperationException">
        /// The reference was not acquired from this provider or was previously released.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Return the exact reference through the same provider that supplied it so that
        /// ownership bookkeeping remains consistent. The provider decides whether to stop
        /// the agent, retain it for reuse or return it to another source. After successful
        /// release, the caller must not continue using the reference.
        /// </para>
        /// <para>
        /// A stop request, a closed connection or disposal of a runner is not confirmation
        /// of release. If release throws, preserve the association with the originating
        /// provider until it can reconcile the outcome. This does not guarantee that an
        /// unconfirmed operation can be repeated without reconciliation.
        /// </para>
        /// <para>
        /// Successful release does not by itself prove that the agent terminated or that
        /// external resources, including deployed files, are no longer in use. Before
        /// removing resources used by an agent, the responsible provider must establish
        /// that the agent no longer needs them. This applies to all provider types.
        /// </para>
        /// </remarks>
        void ReleaseAgent(ITestAgent agent);
    }
}
