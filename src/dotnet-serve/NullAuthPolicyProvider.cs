using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace McMaster.DotNet.Server
{
    class NullAuthPolicyProvider : IAuthorizationPolicyProvider
    {
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return Task.FromResult(new AuthorizationPolicy(
                Enumerable.Empty<IAuthorizationRequirement>(),
                Enumerable.Empty<string>()));
        }

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
            => GetDefaultPolicyAsync();
    }
}
