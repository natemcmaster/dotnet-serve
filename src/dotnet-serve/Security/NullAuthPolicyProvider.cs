// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace McMaster.DotNet.Serve
{
    internal class NullAuthPolicyProvider : IAuthorizationPolicyProvider
    {
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return Task.FromResult(new AuthorizationPolicy(
                Enumerable.Empty<IAuthorizationRequirement>(),
                Enumerable.Empty<string>()));
        }

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
        {
            return Task.FromResult(default(AuthorizationPolicy));
        }

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
            => GetDefaultPolicyAsync();
    }
}
