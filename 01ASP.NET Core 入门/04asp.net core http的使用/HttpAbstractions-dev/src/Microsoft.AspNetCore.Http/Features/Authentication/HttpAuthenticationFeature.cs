// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;

namespace Microsoft.AspNetCore.Http.Features.Authentication
{
    public class HttpAuthenticationFeature : IHttpAuthenticationFeature
    {
        public ClaimsPrincipal User
        {
            get;
            set;
        }

        public IAuthenticationHandler Handler
        {
            get;
            set;
        }
    }
}
