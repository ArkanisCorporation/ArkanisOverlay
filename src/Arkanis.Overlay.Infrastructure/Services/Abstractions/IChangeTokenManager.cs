namespace Arkanis.Overlay.Infrastructure.Services.Abstractions;

using Microsoft.Extensions.Primitives;

public interface IChangeTokenManager
{
    /// <summary>
    ///     Gets the <see cref="IChangeToken" /> associated with the given type.
    /// </summary>
    /// <typeparam name="T">Type to get the change token for.</typeparam>
    /// <returns>The <see cref="IChangeToken" /> associated with the given type.</returns>
    public IChangeToken GetChangeTokenFor<T>();

    /// <summary>
    ///     Gets the <see cref="IChangeToken" /> associated with the specified reference object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the reference object.</typeparam>
    /// <param name="reference">The reference object to get the change token for.</param>
    /// <returns>The <see cref="IChangeToken" /> associated with the given reference object.</returns>
    public IChangeToken GetChangeTokenFor<T>(T reference);

    /// <summary>
    ///     Triggers a change to the <see cref="IChangeToken" /> associated with the given type and returns the new
    ///     <see cref="IChangeToken" />.
    /// </summary>
    /// <typeparam name="T">Type to reset the change token for.</typeparam>
    /// <returns>The new <see cref="IChangeToken" /> associated with the given type.</returns>
    public Task<IChangeToken> ResetChangeTokenForAsync<T>();

    /// <summary>
    ///     Triggers a change to the <see cref="IChangeToken" /> associated with the given type.
    /// </summary>
    /// <typeparam name="T">Type to reset the change token for.</typeparam>
    public Task TriggerChangeForAsync<T>();

    /// <summary>
    ///     Triggers a change to the <see cref="IChangeToken" /> associated with the specified reference object of type <typeparamref name="T"/>
    ///     and notifies any registered change callbacks.
    /// </summary>
    /// <typeparam name="T">The type of the reference object.</typeparam>
    /// <param name="reference">The reference object to trigger the change for.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task TriggerChangeForAsync<T>(T reference);

    /// <summary>
    ///     Triggers a change to all registered <see cref="IChangeToken" /> instances and notifies all registered change callbacks.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task TriggerChangeForAllAsync();
}
