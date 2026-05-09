using System.Reflection;
using SampSharp.OpenMp.Core.Api;

namespace SampSharp.Entities;

/// <summary>
/// Represents the environment of a SampSharp application, providing access to the entry assembly, game server core, and registered components.
/// </summary>
/// <remarks>
/// <see cref="SampSharpEnvironment" /> is initialized during the startup process and provides central access to critical application resources.
/// It is typically used to retrieve services, query components, and invoke core functionality.
/// </remarks>
/// <param name="EntryAssembly">The assembly which was configured to launch in open.mp. Used to discover game mode classes and other application types.</param>
/// <param name="Core">The <see cref="ICore" /> interface for the open.mp server. Provides access to core server functionality and extensions.</param>
/// <param name="Components">The <see cref="IComponentList" /> of open.mp. Manages all game components (players, vehicles, objects, etc.) accessible on the server.</param>
public record SampSharpEnvironment(Assembly EntryAssembly, ICore Core, IComponentList Components)
{
    /// <summary>
    /// Indicates that the host is in the process of shutting down. Set during <c>EcsHost.Cleanup</c>
    /// before the DI scope is disposed. While true, code paths that rely on native open.mp objects
    /// (e.g. component lookups via P/Invoke) must skip those calls — by this point open.mp may have
    /// already torn down components, and re-entering native code can crash with an access violation
    /// (we have observed this in <c>SafeEventHandlerRegistration.Dispose</c>).
    /// </summary>
    public bool IsShuttingDown { get; internal set; }
}