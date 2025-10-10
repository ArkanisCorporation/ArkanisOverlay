namespace Arkanis.Overlay.Common.Services;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using FluentResults;
using Models;
using Options;

public abstract class ExternalAuthenticator
{
    public abstract ExternalAuthenticatorInfo AuthenticatorInfo { get; }

    /// <summary>
    ///     Authenticates a user asynchronously using the provided credentials.
    /// </summary>
    /// <param name="credentials">The user credentials used for authentication.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     An <see cref="AuthTaskBase" /> representing the authentication process and results.
    /// </returns>
    public abstract AuthTaskBase AuthenticateAsync(UserPreferences.Credentials credentials, CancellationToken cancellationToken);

    public abstract class AuthTaskBase(UserPreferences.Credentials credentials, CancellationToken cancellationToken) : ExternalAuthResultBase
    {
        public UserPreferences.Credentials Credentials { get; init; } = credentials;

        [field: MaybeNull]
        private Task<Result<ClaimsIdentity>> Task
        {
            get
            {
                lock (this)
                {
                    return field ??= RunAsync(cancellationToken);
                }
            }
        }

        protected abstract Task<Result<ClaimsIdentity>> RunAsync(CancellationToken cancellationToken);

        public TaskAwaiter<Result<ClaimsIdentity>> GetAwaiter()
            => Task.GetAwaiter();
    }
}

public abstract class ExternalAuthenticator<TTask> : ExternalAuthenticator where TTask : ExternalAuthenticator.AuthTaskBase
{
    public abstract override TTask AuthenticateAsync(UserPreferences.Credentials credentials, CancellationToken cancellationToken);
}
