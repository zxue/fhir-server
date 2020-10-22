// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Health.Fhir.Core.Configs;
using Microsoft.Health.Fhir.Core.Features.Operations.DataConvert.Models;
using Microsoft.Health.Fhir.Core.Features.Operations.Export.ExportDestinationClient;

namespace Microsoft.Health.Fhir.Core.Features.Operations.DataConvert.TemplateStore
{
    public class ContainerRegistryTokenProvider : IContainerRegistryTokenProvider
    {
        private readonly IMemoryCache _tokenCache;
        private readonly IAccessTokenProvider _accessTokenProvider;

        private readonly Uri _aadResourceUri = new Uri("https://management.azure.com/");
        private const string CachePrefix = "Container_Registry_";

        public ContainerRegistryTokenProvider(IMemoryCache memoryCache, IAccessTokenProvider accessTokenProvider)
        {
            EnsureArg.IsNotNull(memoryCache);
            EnsureArg.IsNotNull(accessTokenProvider);

            _tokenCache = memoryCache;
            _accessTokenProvider = accessTokenProvider;
        }

        public async Task<AccessToken> GetContainerRegistryAccessToken(string registryServer, DataConvertConfiguration dataConvertConfiguration, CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNull(registryServer);
            EnsureArg.IsNotNull(dataConvertConfiguration);

            TemplateRegistry targetRegistry = dataConvertConfiguration.TemplateRegistries.Where(registry => registryServer.Equals(registry.RegistryServer, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (targetRegistry == null)
            {
                throw new ArgumentException($"Registry server {registryServer} not registered.");
            }

            // If container registry credentials not provided, try to get access token from service principle.
            // Otherwise, generate Basic token from username and password.
            string registryToken = null;
            if (string.IsNullOrEmpty(targetRegistry.RegistryUsername) || string.IsNullOrEmpty(targetRegistry.RegistryPassword))
            {
                var cacheKey = GetCacheKeyForRegistryServer(targetRegistry.RegistryServer);
                if (!_tokenCache.TryGetValue(cacheKey, out registryToken))
                {
                    var aadAccesssToken = await _accessTokenProvider.GetAccessTokenForResourceAsync(_aadResourceUri, cancellationToken);
                    var registryUri = new Uri($"https://{targetRegistry.RegistryServer}");
                    registryToken = await _accessTokenProvider.ExchangeContainerRegistryAccessToken(registryUri, aadAccesssToken);
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSize(1)
                        .SetAbsoluteExpiration(dataConvertConfiguration.RegistryTokenExpiration);

                    _tokenCache.Set(cacheKey, registryToken, cacheEntryOptions);
                }

                return new AccessToken
                {
                    Type = "Bearer",
                    Token = registryToken,
                };
            }
            else
            {
                // Generate basic token based on registry credentials
                var tokenBytes = Encoding.UTF8.GetBytes($"{targetRegistry.RegistryUsername}:{targetRegistry.RegistryPassword}");
                registryToken = Convert.ToBase64String(tokenBytes);
                return new AccessToken
                {
                    Type = "Basic",
                    Token = registryToken,
                };
            }
        }

        private static string GetCacheKeyForRegistryServer(string registryServer)
        {
            return $"{CachePrefix}#{registryServer}";
        }
    }
}
