// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Text;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Core.Configs;
using Microsoft.Health.Fhir.Core.Features.Operations.DataConvert.Models;

namespace Microsoft.Health.Fhir.Core.Features.Operations.DataConvert.ConvertTemplateStore
{
    public class ContainerRegistryTokenProvider : IContainerRegistryTokenProvider
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<AccessToken> GetContainerRegistryAccessToken(DataConvertConfiguration convertConfiguration)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var tokenBytes = Encoding.UTF8.GetBytes($"{convertConfiguration.ContainerRegistryUserName}:{convertConfiguration.ContainerRegistryPassword}");
            string basicToken = System.Convert.ToBase64String(tokenBytes);
            return new AccessToken
            {
                Type = "Basic",
                Token = basicToken,
            };
        }
    }
}
