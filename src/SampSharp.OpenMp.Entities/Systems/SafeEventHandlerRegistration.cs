using SampSharp.OpenMp.Core.Api;

namespace SampSharp.Entities;

internal class SafeEventHandlerRegistration<TComponent, TEventHandler>(SampSharpEnvironment environment, TEventHandler handler, Func<TComponent, IEventDispatcher<TEventHandler>> dispatcherProvider) : IDisposable
    where TComponent : unmanaged, IComponent.IManagedInterface
    where TEventHandler : class, IEventHandler<TEventHandler>
{
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // During host shutdown the native IComponentList/ICore pointers may already be
        // invalidated by open.mp. Calling QueryComponent here would crash with an AV in
        // the P/Invoke trampoline before HasValue could even return false. Just free the
        // marshalled handler and skip the unsubscribe — the native side is going away.
        if (environment.IsShuttingDown)
        {
            TEventHandler.Marshaller.Marshal(handler).Free();
            return;
        }

        var component = environment.Components.QueryComponent<TComponent>();

        if (!component.HasValue)
        {
            TEventHandler.Marshaller.Marshal(handler).Free();
            return;
        }

        var dispatcher = dispatcherProvider(component);

        if (!dispatcher.HasValue)
        {
            TEventHandler.Marshaller.Marshal(handler).Free();
            return;
        }

        dispatcher.RemoveEventHandler(handler);
    }
}

internal class SafeEventHandlerRegistration<TEventHandler>(SampSharpEnvironment environment, TEventHandler handler, Func<ICore, IEventDispatcher<TEventHandler>> dispatcherProvider) : IDisposable
    where TEventHandler : class, IEventHandler<TEventHandler>
{
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // See note in the component-typed overload above — same reasoning for ICore.
        if (environment.IsShuttingDown)
        {
            TEventHandler.Marshaller.Marshal(handler).Free();
            return;
        }

        if (!environment.Core.HasValue)
        {
            TEventHandler.Marshaller.Marshal(handler).Free();
            return;
        }

        var dispatcher = dispatcherProvider(environment.Core);

        if (!dispatcher.HasValue)
        {
            TEventHandler.Marshaller.Marshal(handler).Free();
            return;
        }

        dispatcher.RemoveEventHandler(handler);
    }
}