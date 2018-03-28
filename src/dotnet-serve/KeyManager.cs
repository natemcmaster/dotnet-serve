using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.DataProtection.Internal;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace McMaster.DotNet.Server
{
    // forwards to XmlKeyManager, but with logging disabled
    class KeyManager : IKeyManager
    {
        private readonly XmlKeyManager _wrapped;

        public KeyManager(IOptions<KeyManagementOptions> options, IActivator activator)
        {
            _wrapped = new XmlKeyManager(options, activator, NullLoggerFactory.Instance);
        }

        public IKey CreateNewKey(DateTimeOffset activationDate, DateTimeOffset expirationDate)
            => _wrapped.CreateNewKey(activationDate, expirationDate);

        public IReadOnlyCollection<IKey> GetAllKeys()
            => _wrapped.GetAllKeys();

        public CancellationToken GetCacheExpirationToken()
            => _wrapped.GetCacheExpirationToken();

        public void RevokeAllKeys(DateTimeOffset revocationDate, string reason = null)
            => _wrapped.RevokeAllKeys(revocationDate, reason);

        public void RevokeKey(Guid keyId, string reason = null)
            => _wrapped.RevokeKey(keyId, reason);
    }
}
