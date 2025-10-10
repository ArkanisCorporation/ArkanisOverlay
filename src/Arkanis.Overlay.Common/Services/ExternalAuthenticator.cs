namespace Arkanis.Overlay.Common.Services;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using FluentResults;
using Models;

public abstract class ExternalAuthenticator
{
    public abstract ExternalAuthenticatorInfo AuthenticatorInfo { get; }

    /// <summary>
    ///     Validates the provided service credentials.
    /// </summary>
    /// <param name="serviceCredentials">
    ///     The credentials to validate. Can be null if no credentials are provided.
    /// </param>
    /// <returns>
    ///     A <see cref="Result" /> indicating the success or failure of the validation process.
    /// </returns>
    public abstract Result ValidateCredentials(AccountCredentials? serviceCredentials);

    /// <summary>
    ///     Authenticates a user asynchronously using the provided credentials.
    /// </summary>
    /// <param name="credentials">The user credentials used for authentication.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     An <see cref="AuthTaskBase" /> representing the authentication process and results.
    /// </returns>
    public abstract AuthTaskBase AuthenticateAsync(AccountCredentials credentials, CancellationToken cancellationToken);

    public abstract class AuthTaskBase(AccountCredentials credentials, CancellationToken cancellationToken) : ExternalAuthResultBase
    {
        public AccountCredentials Credentials { get; init; } = credentials;

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
    public abstract override TTask AuthenticateAsync(AccountCredentials credentials, CancellationToken cancellationToken);
}
