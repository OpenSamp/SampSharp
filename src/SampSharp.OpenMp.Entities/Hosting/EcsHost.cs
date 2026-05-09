using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SampSharp.OpenMp.Core;

namespace SampSharp.Entities;

[Extension(0x57e43771d28c5e7e)]
internal class EcsHost(IServiceProvider serviceProvider, UnhandledExceptionHandler? exceptionHandler) : Extension
{
    private IServiceProvider? _serviceProvider = serviceProvider;

    public IServiceProvider ServiceProvider => _serviceProvider ?? throw new InvalidOperationException();

    public void Start(IStartupContext context)
    {
        context.UseSynchronizationContext();

        context.UnhandledExceptionHandler = UnhandledExceptionHandler;

        LoadSystems();

        // Fire initial event
        OnGameModeInit();
    }

    protected override void Cleanup()
    {
        OnGameModeExit();

        if (_serviceProvider is not IDisposable disposable)
        {
            return;
        }

        // Cleanup runs after open.mp has already started tearing down other components
        // pointers in IComponentList / ICore are unsafe to deref by now, so we flag the
        // environment as shutting down and let SafeEventHandlerRegistration etc. skip
        // their native unsubscribe calls. Without this, disposing systems crashes with
        // an AV in IComponentList::QueryComponent.
        if (_serviceProvider.GetService<SampSharpEnvironment>() is { } environment)
        {
            environment.IsShuttingDown = true;
        }

        disposable.Dispose();
        _serviceProvider = null;
    }

    private void UnhandledExceptionHandler(string context, Exception exception)
    {
        if (exceptionHandler != null)
        {
            exceptionHandler(ServiceProvider, context, exception);
        }
        else
        {
            DefaultExceptionHandler(context, exception);
        }
    }

    private void DefaultExceptionHandler(string context, Exception exception)
    {
        ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(context)
            .LogError(exception, "Unhandled exception during: {context}", context);
    }

    private void OnGameModeInit()
    {
        ServiceProvider.GetRequiredService<IEventDispatcher>().Invoke("OnGameModeInit");
    }

    private void OnGameModeExit()
    {
        ServiceProvider.GetRequiredService<IEventDispatcher>().Invoke("OnGameModeExit");
    }

    private void LoadSystems()
    {
        ServiceProvider.GetRequiredService<SystemRegistry>().LoadSystems();
    }

}