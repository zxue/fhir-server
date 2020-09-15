// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using MediatR;
using Microsoft.Health.Fhir.Converter.Hl7v2;
using Microsoft.Health.Fhir.Core.Exceptions;
using Microsoft.Health.Fhir.Core.Extensions;
using Microsoft.Health.Fhir.Core.Features.Operations.Convert.ConvertTemplateStore;
using Microsoft.Health.Fhir.Core.Features.Operations.Convert.Models;
using Microsoft.Health.Fhir.Core.Features.Security;
using Microsoft.Health.Fhir.Core.Features.Security.Authorization;
using Microsoft.Health.Fhir.Core.Messages.Convert;

namespace Microsoft.Health.Fhir.Api.Features.Resources.Convert
{
    public class ConvertOperationHandler : IRequestHandler<ConvertRequest, ConvertResponse>
    {
        private readonly IFhirAuthorizationService _authorizationService;
        private readonly ITemplateStoreClient _templateStoreClient;
        private readonly FhirJsonParser _parser;

        public ConvertOperationHandler(
            IFhirAuthorizationService authorizationService,
            ITemplateStoreClient templateStoreClient,
            FhirJsonParser parser)
        {
            EnsureArg.IsNotNull(authorizationService, nameof(authorizationService));

            _authorizationService = authorizationService;
            _templateStoreClient = templateStoreClient;
            _parser = parser;
        }

        public async Task<ConvertResponse> Handle(ConvertRequest request, CancellationToken cancellationToken)
        {
            if (await _authorizationService.CheckAccess(DataActions.Convert) != DataActions.Convert)
            {
                throw new UnauthorizedFhirActionException();
            }

            Parameters parameters = request.Resource.ToPoco<Parameters>();
            ConvertOption options = ExtractConvertOptionFromRequestParameters(parameters);
            string templateDirectory = Path.Combine(await _templateStoreClient.GetTemplateSet(options), "templates", "Hl7v2");
            string rawInput = DecodeBase64Input(options.InputData);
            var hl7v2Processor = new Hl7v2Processor(templateDirectory);
            string bundleResult = hl7v2Processor.Convert(rawInput, options.EntryPointTemplate);

            var bundle = _parser.Parse<Hl7.Fhir.Model.Bundle>(bundleResult);
            var outParameters = new Parameters();
            outParameters.Add(ConvertOptionConstants.ResultKey, bundle);

            return new ConvertResponse(outParameters.ToResourceElement());
        }

        private ConvertOption ExtractConvertOptionFromRequestParameters(Parameters parameters)
        {
            string operationStringValue = parameters.GetSingleValue<Element>(ConvertOptionConstants.OperationTypeKey).ToString();
            if (!Enum.TryParse(operationStringValue, out ConvertOperationType operationType))
            {
                throw new InvalidOperationException($"Convert operation type {operationStringValue} is invalid");
            }

            return new ConvertOption
            {
                InputData = parameters.GetSingleValue<Element>(ConvertOptionConstants.InputDataKey).ToString(),
                OperationType = operationType,
                TemplateSetLocation = parameters.GetSingleValue<Element>(ConvertOptionConstants.TemplateSetLocationKey).ToString(),
                TemplateSetDigest = parameters.GetSingleValue<Element>(ConvertOptionConstants.TemplateSetDigestKey).ToString(),
                EntryPointTemplate = parameters.GetSingleValue<Element>(ConvertOptionConstants.EntryPointTemplateKey).ToString(),
            };
        }

        private string DecodeBase64Input(string base64Input)
        {
            var inputBytes = System.Convert.FromBase64String(base64Input);
            return Encoding.UTF8.GetString(inputBytes);
        }
    }
}
