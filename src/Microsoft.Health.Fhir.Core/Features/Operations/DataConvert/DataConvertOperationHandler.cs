// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Converter.TemplateManagement;
using Microsoft.Health.Fhir.Core.Configs;
using Microsoft.Health.Fhir.Core.Exceptions;
using Microsoft.Health.Fhir.Core.Features.Operations.DataConvert.TemplateStore;
using Microsoft.Health.Fhir.Core.Features.Security;
using Microsoft.Health.Fhir.Core.Features.Security.Authorization;
using Microsoft.Health.Fhir.Core.Messages.DataConvert;

namespace Microsoft.Health.Fhir.Core.Features.Operations.DataConvert
{
    public class DataConvertOperationHandler : IRequestHandler<DataConvertRequest, DataConvertResponse>
    {
        private readonly IFhirAuthorizationService _authorizationService;
        private readonly IContainerRegistryTokenProvider _containerRegistryTokenProvider;
        private readonly IDataConvertEngineManager _convertEngineManager;
        private readonly DataConvertConfiguration _dataConvertConfiguration;

        public DataConvertOperationHandler(
            IFhirAuthorizationService authorizationService,
            IContainerRegistryTokenProvider containerRegistryTokenProvider,
            IOptions<DataConvertConfiguration> dataConvertConfiguration,
            IDataConvertEngineManager convertEngineManager)
        {
            EnsureArg.IsNotNull(authorizationService, nameof(authorizationService));
            EnsureArg.IsNotNull(containerRegistryTokenProvider, nameof(containerRegistryTokenProvider));
            EnsureArg.IsNotNull(convertEngineManager, nameof(convertEngineManager));

            _authorizationService = authorizationService;
            _containerRegistryTokenProvider = containerRegistryTokenProvider;
            _dataConvertConfiguration = dataConvertConfiguration.Value;
            _convertEngineManager = convertEngineManager;
        }

        public async Task<DataConvertResponse> Handle(DataConvertRequest request, CancellationToken cancellationToken)
        {
            if (await _authorizationService.CheckAccess(DataActions.Export) != DataActions.Export)
            {
                throw new UnauthorizedFhirActionException();
            }

            var containerRegistryAccessToken = await _containerRegistryTokenProvider.GetContainerRegistryAccessToken(request.TemplateSetImageInfo.Registry, _dataConvertConfiguration, CancellationToken.None);
            var accessToken = $"{containerRegistryAccessToken.Type} {containerRegistryAccessToken.Token}";
            var templates = ServerEngine.Engine.OCIPull(accessToken, request.TemplateSetImageInfo);

            var hl7v2Processor = _convertEngineManager.GetHl7V2Processor();
            string bundleResult = hl7v2Processor.Convert(request.InputData, request.EntryPointTemplate, templates.Layers.Last().templates);
            return new DataConvertResponse(bundleResult);
        }
    }
}
