// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Core.Features.Operations.Export.ExportDestinationClient;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Azure.ExportDestinationClient
{
    public class AzureAccessTokenProvider : IAccessTokenProvider
    {
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private readonly ILogger<AzureAccessTokenProvider> _logger;
        private const string ExchangeTokenUrl = "oauth2/exchange";
        private const string GetAcrTokenUrl = "oauth2/token";

        public AzureAccessTokenProvider(ILogger<AzureAccessTokenProvider> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            _azureServiceTokenProvider = new AzureServiceTokenProvider();
            _logger = logger;
        }

        public async Task<string> GetAccessTokenForResourceAsync(Uri resourceUri, CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNull(resourceUri, nameof(resourceUri));

            string accessToken = await _azureServiceTokenProvider.GetAccessTokenAsync(resourceUri.ToString(), cancellationToken: cancellationToken);
            if (accessToken == null)
            {
                _logger.LogWarning("Failed to retrieve access token");

                throw new AccessTokenProviderException(Resources.CannotGetAccessToken);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved access token");
            }

            return accessToken;
        }

        public async Task<string> ExchangeContainerRegistryAccessToken(Uri registryUri, string aadAccessToken)
        {
            EnsureArg.IsNotNull(aadAccessToken, nameof(aadAccessToken));

            return await ExchangeToken(registryUri, aadAccessToken);
        }

        private async Task<string> ExchangeToken(Uri registryUri, string aadToken)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = registryUri;
                var parameters = new Dictionary<string, string>();
                parameters.Add("grant_type", "access_token");
                parameters.Add("service", registryUri.Host);
                parameters.Add("access_token", aadToken);

                var exchangeUri = new Uri(registryUri, ExchangeTokenUrl);
                var refreshTokenResponse = await httpClient.PostAsync(exchangeUri, new FormUrlEncodedContent(parameters));
                try
                {
                    refreshTokenResponse.EnsureSuccessStatusCode();
                }
                catch
                {
                    throw new Exception($"exchange failed. aadToken:'{aadToken}', {exchangeUri}");
                }

                var refreshTokenText = await refreshTokenResponse.Content.ReadAsStringAsync();
                dynamic refreshTokenJson = JsonConvert.DeserializeObject(refreshTokenText);
                var refreshToken = (string)refreshTokenJson.refresh_token;

                var refreshParameters = new Dictionary<string, string>();
                refreshParameters.Add("grant_type", "refresh_token");
                refreshParameters.Add("refresh_token", refreshToken);
                refreshParameters.Add("scope", "repository:*:*");
                refreshParameters.Add("service", registryUri.Host);
                var accessTokenResponse = await httpClient.PostAsync(new Uri(registryUri, GetAcrTokenUrl), new FormUrlEncodedContent(refreshParameters));
                accessTokenResponse.EnsureSuccessStatusCode();
                var accessTokenText = await accessTokenResponse.Content.ReadAsStringAsync();
                dynamic accessTokenJson = JsonConvert.DeserializeObject(accessTokenText);
                return (string)accessTokenJson.access_token;
            }
        }
    }
}
