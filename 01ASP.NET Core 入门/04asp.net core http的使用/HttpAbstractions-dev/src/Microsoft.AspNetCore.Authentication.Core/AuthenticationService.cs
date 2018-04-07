// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Implements <see cref="IAuthenticationService"/>.
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="schemes">The <see cref="IAuthenticationSchemeProvider"/>.</param>
        /// <param name="handlers">The <see cref="IAuthenticationRequestHandler"/>.</param>
        /// <param name="transform">The <see cref="IClaimsTransformation"/>.</param>
        public AuthenticationService(IAuthenticationSchemeProvider schemes, IAuthenticationHandlerProvider handlers, IClaimsTransformation transform)
        {
            Schemes = schemes;
            Handlers = handlers;
            Transform = transform;
        }

        /// <summary>
        /// Used to lookup AuthenticationSchemes.
        /// </summary>
        public IAuthenticationSchemeProvider Schemes { get; }

        /// <summary>
        /// Used to resolve IAuthenticationHandler instances.
        /// </summary>
        public IAuthenticationHandlerProvider Handlers { get; }

        /// <summary>
        /// Used for claims transformation.
        /// </summary>
        public IClaimsTransformation Transform { get; }

        /// <summary>
        /// Authenticate for the specified authentication scheme.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="scheme">The name of the authentication scheme.</param>
        /// <returns>The result.</returns>
        public virtual async Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string scheme)
        {
            if (scheme == null)
            {
                var defaultScheme = await Schemes.GetDefaultAuthenticateSchemeAsync();
                scheme = defaultScheme?.Name;
                if (scheme == null)
                {
                    throw new InvalidOperationException($"No authenticationScheme was specified, and there was no DefaultAuthenticateScheme found.");
                }
            }

            var handler = await Handlers.GetHandlerAsync(context, scheme);
            if (handler == null)
            {
                throw await CreateMissingHandlerException(scheme);
            }

            var result = await handler.AuthenticateAsync();
            if (result != null && result.Succeeded)
            {
                var transformed = await Transform.TransformAsync(result.Principal);
                return AuthenticateResult.Success(new AuthenticationTicket(transformed, result.Properties, result.Ticket.AuthenticationScheme));
            }
            return result;
        }

        /// <summary>
        /// Challenge the specified authentication scheme.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="scheme">The name of the authentication scheme.</param>
        /// <param name="properties">The <see cref="AuthenticationProperties"/>.</param>
        /// <returns>A task.</returns>
        public virtual async Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            if (scheme == null)
            {
                var defaultChallengeScheme = await Schemes.GetDefaultChallengeSchemeAsync();
                scheme = defaultChallengeScheme?.Name;
                if (scheme == null)
                {
                    throw new InvalidOperationException($"No authenticationScheme was specified, and there was no DefaultChallengeScheme found.");
                }
            }

            var handler = await Handlers.GetHandlerAsync(context, scheme);
            if (handler == null)
            {
                throw await CreateMissingHandlerException(scheme);
            }

            await handler.ChallengeAsync(properties);
        }

        /// <summary>
        /// Forbid the specified authentication scheme.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="scheme">The name of the authentication scheme.</param>
        /// <param name="properties">The <see cref="AuthenticationProperties"/>.</param>
        /// <returns>A task.</returns>
        public virtual async Task ForbidAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            if (scheme == null)
            {
                var defaultForbidScheme = await Schemes.GetDefaultForbidSchemeAsync();
                scheme = defaultForbidScheme?.Name;
                if (scheme == null)
                {
                    throw new InvalidOperationException($"No authenticationScheme was specified, and there was no DefaultForbidScheme found.");
                }
            }

            var handler = await Handlers.GetHandlerAsync(context, scheme);
            if (handler == null)
            {
                throw await CreateMissingHandlerException(scheme);
            }

            await handler.ForbidAsync(properties);
        }

        /// <summary>
        /// Sign a principal in for the specified authentication scheme.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="scheme">The name of the authentication scheme.</param>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> to sign in.</param>
        /// <param name="properties">The <see cref="AuthenticationProperties"/>.</param>
        /// <returns>A task.</returns>
        public virtual async Task SignInAsync(HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            if (scheme == null)
            {
                var defaultScheme = await Schemes.GetDefaultSignInSchemeAsync();
                scheme = defaultScheme?.Name;
                if (scheme == null)
                {
                    throw new InvalidOperationException($"No authenticationScheme was specified, and there was no DefaultSignInScheme found.");
                }
            }

            var handler = await Handlers.GetHandlerAsync(context, scheme);
            if (handler == null)
            {
                throw await CreateMissingSignInHandlerException(scheme);
            }

            var signInHandler = handler as IAuthenticationSignInHandler;
            if (signInHandler == null)
            {
                throw await CreateMismatchedSignInHandlerException(scheme, handler);
            }

            await signInHandler.SignInAsync(principal, properties);
        }

        /// <summary>
        /// Sign out the specified authentication scheme.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="scheme">The name of the authentication scheme.</param>
        /// <param name="properties">The <see cref="AuthenticationProperties"/>.</param>
        /// <returns>A task.</returns>
        public virtual async Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            if (scheme == null)
            {
                var defaultScheme = await Schemes.GetDefaultSignOutSchemeAsync();
                scheme = defaultScheme?.Name;
                if (scheme == null)
                {
                    throw new InvalidOperationException($"No authenticationScheme was specified, and there was no DefaultSignOutScheme found.");
                }
            }

            var handler = await Handlers.GetHandlerAsync(context, scheme);
            if (handler == null)
            {
                throw await CreateMissingSignOutHandlerException(scheme);
            }

            var signOutHandler = handler as IAuthenticationSignOutHandler;
            if (signOutHandler == null)
            {
                throw await CreateMismatchedSignOutHandlerException(scheme, handler);
            }

            await signOutHandler.SignOutAsync(properties);
        }

        private async Task<Exception> CreateMissingHandlerException(string scheme)
        {
            var schemes = string.Join(", ", (await Schemes.GetAllSchemesAsync()).Select(sch => sch.Name));

            var footer = $" Did you forget to call AddAuthentication().Add[SomeAuthHandler](\"{scheme}\",...)?";

            if (string.IsNullOrEmpty(schemes))
            {
                return new InvalidOperationException(
                    $"No authentication handlers are registered." + footer);
            }

            return new InvalidOperationException(
                $"No authentication handler is registered for the scheme '{scheme}'. The registered schemes are: {schemes}." + footer);
        }

        private async Task<string> GetAllSignInSchemeNames()
        {
            return string.Join(", ", (await Schemes.GetAllSchemesAsync())
                .Where(sch => typeof(IAuthenticationSignInHandler).IsAssignableFrom(sch.HandlerType))
                .Select(sch => sch.Name));
        }

        private async Task<Exception> CreateMissingSignInHandlerException(string scheme)
        {
            var schemes = await GetAllSignInSchemeNames();

            // CookieAuth is the only implementation of sign-in.
            var footer = $" Did you forget to call AddAuthentication().AddCookies(\"{scheme}\",...)?";

            if (string.IsNullOrEmpty(schemes))
            {
                return new InvalidOperationException(
                    $"No sign-in authentication handlers are registered." + footer);
            }

            return new InvalidOperationException(
                $"No sign-in authentication handler is registered for the scheme '{scheme}'. The registered sign-in schemes are: {schemes}." + footer);
        }

        private async Task<Exception> CreateMismatchedSignInHandlerException(string scheme, IAuthenticationHandler handler)
        {
            var schemes = await GetAllSignInSchemeNames();

            var mismatchError = $"The authentication handler registered for scheme '{scheme}' is '{handler.GetType().Name}' which cannot be used for SignInAsync. ";

            if (string.IsNullOrEmpty(schemes))
            {
                // CookieAuth is the only implementation of sign-in.
                return new InvalidOperationException(mismatchError
                    + $"Did you forget to call AddAuthentication().AddCookies(\"Cookies\") and SignInAsync(\"Cookies\",...)?");
            }

            return new InvalidOperationException(mismatchError + $"The registered sign-in schemes are: {schemes}.");
        }

        private async Task<string> GetAllSignOutSchemeNames()
        {
            return string.Join(", ", (await Schemes.GetAllSchemesAsync())
                .Where(sch => typeof(IAuthenticationSignOutHandler).IsAssignableFrom(sch.HandlerType))
                .Select(sch => sch.Name));
        }

        private async Task<Exception> CreateMissingSignOutHandlerException(string scheme)
        {
            var schemes = await GetAllSignOutSchemeNames();

            var footer = $" Did you forget to call AddAuthentication().AddCookies(\"{scheme}\",...)?";

            if (string.IsNullOrEmpty(schemes))
            {
                // CookieAuth is the most common implementation of sign-out, but OpenIdConnect and WsFederation also support it.
                return new InvalidOperationException($"No sign-out authentication handlers are registered." + footer);
            }

            return new InvalidOperationException(
                $"No sign-out authentication handler is registered for the scheme '{scheme}'. The registered sign-out schemes are: {schemes}." + footer);
        }

        private async Task<Exception> CreateMismatchedSignOutHandlerException(string scheme, IAuthenticationHandler handler)
        {
            var schemes = await GetAllSignOutSchemeNames();

            var mismatchError = $"The authentication handler registered for scheme '{scheme}' is '{handler.GetType().Name}' which cannot be used for {nameof(SignOutAsync)}. ";

            if (string.IsNullOrEmpty(schemes))
            {
                // CookieAuth is the most common implementation of sign-out, but OpenIdConnect and WsFederation also support it.
                return new InvalidOperationException(mismatchError
                    + $"Did you forget to call AddAuthentication().AddCookies(\"Cookies\") and {nameof(SignOutAsync)}(\"Cookies\",...)?");
            }

            return new InvalidOperationException(mismatchError + $"The registered sign-out schemes are: {schemes}.");
        }
    }
}
