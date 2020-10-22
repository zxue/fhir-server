// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Core.Configs;
using Microsoft.Health.Fhir.Core.Features.Operations.DataConvert.Models;

namespace Microsoft.Health.Fhir.Core.Features.Operations.DataConvert.TemplateStore
{
    public interface IContainerRegistryTokenProvider
    {
        public Task<AccessToken> GetContainerRegistryAccessToken(string registryServer, DataConvertConfiguration convertConfiguration, CancellationToken cancellationToken);
    }
}
