// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System.Runtime.Versioning;

namespace NUnit.Engine
{
    /// <summary>
    /// Interface implemented by objects representing a runtime framework.
    /// </summary>
    public interface IRuntimeFramework
    {
        /// <summary>
        /// Gets a Target Framework Moniker (TFM) string representing the framework, such as "net45"
        /// </summary>
        string TFM { get; }

        /// <summary>
        /// Gets the display name of the framework, such as ".NET 4.5"
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets a FrameworkName object representing the framework, such as .NETFramework,Version=v4.5
        /// </summary>
        FrameworkName FrameworkName { get; }
    }
}
