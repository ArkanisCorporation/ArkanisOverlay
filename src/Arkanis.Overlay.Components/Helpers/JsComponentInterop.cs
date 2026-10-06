namespace Arkanis.Overlay.Components.Helpers;

using System.Diagnostics.CodeAnalysis;
using Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public static class JsComponentInterop
{
    /// <inheritdoc cref="Create{T}" />
    public static JsComponentInterop<T> CreateLifetimeInterop<T>(this IJSRuntime jsRuntime, T component) where T : ComponentBase
        => Create(jsRuntime, component);

    /// <summary>
    ///     Creates a new instance of <see cref="JsComponentInterop{T}" /> for the specified component.
    /// </summary>
    /// <typeparam name="T">The type of the component, which must inherit from <see cref="ComponentBase" />.</typeparam>
    /// <param name="jsRuntime">The JavaScript runtime to be used for interop calls.</param>
    /// <param name="component">The component for which to create the interop instance.</param>
    /// <returns>A new instance of <see cref="JsComponentInterop{T}" />.</returns>
    private static JsComponentInterop<T> Create<T>(IJSRuntime jsRuntime, T component) where T : ComponentBase
        => new(jsRuntime, component);
}

public sealed class JsComponentInterop<T>(IJSRuntime jsRuntime, T component) : IAsyncDisposable where T : ComponentBase
{
    private const string StaticFactoryMethodName = "createFor";
    private const string ContentScriptPath = "./_content";
    private const string ComponentJavaScriptFileSuffix = "razor.js";

    private IJSObjectReference? _module;

    [MemberNotNullWhen(true, nameof(Controls), nameof(ComponentReference), nameof(_module))]
    public bool IsInitialized
        => this is { _module: not null, Controls: not null, ComponentReference: not null };

    public DotNetObjectReference<T>? ComponentReference { get; private set; }

    public IJSObjectReference? Controls { get; private set; }

    public string ProjectNamespace { get; init; } = SharedComponentsModule.Namespace;

    /// <summary>
    ///     Releases allocated JavaScript-interop objects.
    /// </summary>
    /// <remarks>
    ///     If the component JavaScript logic has been initialized,
    ///     this method disposes the JavaScript module and its controls.
    ///     Otherwise, this is a no-op.
    ///     <para>
    ///         This method is idempotent.
    ///     </para>
    ///     <para>
    ///         It is safe to call this method even in case the JavaScript interop has already been disconnected.
    ///     </para>
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (Controls != null)
            {
                await Controls.InvokeVoidAsync("dispose");
                await Controls.DisposeAsync();
            }

            if (_module != null)
            {
                await _module.DisposeAsync();
            }
        }
        catch (JSException)
        {
            // JS interop error (target object may have already been disposed on page reload)
        }
        catch (JSDisconnectedException)
        {
            // JS no longer available
        }
        catch (ObjectDisposedException)
        {
            // already disposed
        }
        finally
        {
            Controls = null;
            _module = null;
        }

        ComponentReference?.Dispose();
        ComponentReference = null;
    }

    /// <summary>
    ///     Initializes the component JavaScript logic. This method is a no-op if it is called more than once.
    /// </summary>
    /// <remarks>
    ///     Must be called during the <c>OnAfterRenderAsync</c> component lifecycle hook.
    /// </remarks>
    /// <param name="initParameters">The initialization parameters to pass to the JavaScript logic.</param>
    public async Task InitializeAsync(params object?[] initParameters)
    {
        // We need to calculate the path to the JavaScript file for the component.
        // This is done by taking the namespace of the component and replacing the project namespace with the _content prefix.
        // The remaining namespace parts are then joined with the '/' character to form a path.
        var componentName = typeof(T).Name;
        var componentNamespace = typeof(T).Namespace?.Replace(ProjectNamespace, string.Empty).Trim('.') ?? string.Empty;
        var componentNamespacePath = string.Join('/', componentNamespace.Split('.'));

        try
        {
            ComponentReference ??= DotNetObjectReference.Create(component);
            _module ??= await jsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                $"{ContentScriptPath}/{ProjectNamespace}/{componentNamespacePath}/{componentName}.{ComponentJavaScriptFileSuffix}"
            );

            // Call the JavaScript logic's initialization method, passing the component reference and any additional parameters.
            Controls ??= await _module.InvokeAsync<IJSObjectReference>(
                $"{componentName}.{StaticFactoryMethodName}",
                [ComponentReference, ..initParameters]
            );
        }
        catch (Exception e)
        {
            throw new JsInteropException($"Failed properly initializing JavaScript lifetime interop for: {component}", e);
        }
    }
}
