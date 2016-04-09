using System;

namespace Microshaoft.Versioning
{
    /// <summary>
    /// An item that exposes a NuGetVersion
    /// </summary>
    public interface INuGetVersionable
    {
        /// <summary>
        /// NuGet semantic version
        /// </summary>
        NuGetVersion Version { get; }
    }
}