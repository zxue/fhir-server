// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using EnsureThat;
using Hl7.Fhir.Model;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Health.Api.Features.Audit;
using Microsoft.Health.Fhir.Api.Features.Filters;
using Microsoft.Health.Fhir.Api.Features.Routing;
using Microsoft.Health.Fhir.Converter.TemplateManagement;
using Microsoft.Health.Fhir.Core.Configs;
using Microsoft.Health.Fhir.Core.Exceptions;
using Microsoft.Health.Fhir.Core.Extensions;
using Microsoft.Health.Fhir.Core.Features.Operations;
using Microsoft.Health.Fhir.Core.Features.Operations.DataConvert.Models;
using Microsoft.Health.Fhir.Core.Messages.DataConvert;
using Microsoft.Health.Fhir.Core.Models;
using Microsoft.Health.Fhir.ValueSets;

namespace Microsoft.Health.Fhir.Api.Controllers
{
    [ServiceFilter(typeof(AuditLoggingFilterAttribute))]
    [ServiceFilter(typeof(OperationOutcomeExceptionFilterAttribute))]
    [ServiceFilter(typeof(ValidateContentTypeFilterAttribute))]
    [ValidateResourceTypeFilter]
    [ValidateModelState]
    public class ConvertController : Controller
    {
        private readonly IMediator _mediator;
        private readonly DataConvertConfiguration _convertConfig;
        private const char ImageDigestDelimiter = '@';
        private const char ImageTagDelimiter = ':';

        public ConvertController(IMediator mediator, IOptions<OperationsConfiguration> operationsConfig)
        {
            EnsureArg.IsNotNull(mediator, nameof(mediator));
            EnsureArg.IsNotNull(operationsConfig, nameof(operationsConfig));

            _mediator = mediator;
            _convertConfig = operationsConfig.Value.Convert;
        }

        [HttpPost]
        [Route(KnownRoutes.DataConvert)]
        [AuditEventType(AuditEventSubType.DataConvert)]
        public async Task<IActionResult> DataConvert([FromBody] Resource resource)
        {
            CheckIfDataConvertIsEnabled();

            DataConvertResponse response = await _mediator.Send(GenerateConvertRequestFromRequestParameters(resource.ToResourceElement()));

            return new ContentResult
            {
                Content = response.Resource,
                ContentType = "application/json",
            };
        }

        private DataConvertRequest GenerateConvertRequestFromRequestParameters(ResourceElement resourceElement)
        {
            var parameters = resourceElement.ToPoco<Parameters>();

            string inputData = parameters.GetSingleValue<Element>(DataConvertOptionConstants.InputDataKey)?.ToString();
            if (string.IsNullOrEmpty(inputData))
            {
                throw new RequestNotValidException("Input data could not be null or empty.");
            }

            string entryPointTemplate = parameters.GetSingleValue<Element>(DataConvertOptionConstants.EntryPointTemplateKey)?.ToString();
            if (string.IsNullOrEmpty(entryPointTemplate))
            {
                throw new RequestNotValidException("Entry point template name could not be null or empty.");
            }

            string dataTypeValue = parameters.GetSingleValue<Element>(DataConvertOptionConstants.OperationTypeKey)?.ToString();
            if (!Enum.TryParse(dataTypeValue, out DataConvertInputType dataType))
            {
                throw new RequestNotValidException($"Convert operation type {dataTypeValue} is invalid");
            }

            string templateSetImage = parameters.GetSingleValue<Element>(DataConvertOptionConstants.TemplateSetImageKey)?.ToString();
            if (string.IsNullOrEmpty(templateSetImage))
            {
                throw new RequestNotValidException("Template image could not be null or empty.");
            }

            var imageInfo = ExtractImageInfo(templateSetImage);
            return new DataConvertRequest(inputData, dataType, imageInfo, entryPointTemplate);
        }

        private ImageInfo ExtractImageInfo(string imageReference)
        {
            if (imageReference.Contains(ImageDigestDelimiter, StringComparison.OrdinalIgnoreCase))
            {
                Tuple<string, string> imageMeta = SplitApart(imageReference, ImageDigestDelimiter);
                if (string.IsNullOrEmpty(imageMeta.Item1) || string.IsNullOrEmpty(imageMeta.Item2))
                {
                    throw new RequestNotValidException("Template image format is invalid.");
                }

                return new ImageInfo(_convertConfig.ContainerRegistryUrl, imageMeta.Item1, tag: null, digest: imageMeta.Item2);
            }
            else if (imageReference.Contains(ImageTagDelimiter, StringComparison.OrdinalIgnoreCase))
            {
                Tuple<string, string> imageMeta = SplitApart(imageReference, ImageTagDelimiter);
                if (string.IsNullOrEmpty(imageMeta.Item1) || string.IsNullOrEmpty(imageMeta.Item2))
                {
                    throw new RequestNotValidException("Template image format is invalid.");
                }

                return new ImageInfo(_convertConfig.ContainerRegistryUrl, imageMeta.Item1, tag: imageMeta.Item2);
            }

            return new ImageInfo(_convertConfig.ContainerRegistryUrl, imageReference);
        }

        private static Tuple<string, string> SplitApart(string input, char dilimeter)
        {
            var index = input.IndexOf(dilimeter, StringComparison.OrdinalIgnoreCase);
            return new Tuple<string, string>(input.Substring(0, index), input.Substring(index + 1));
        }

        private void CheckIfDataConvertIsEnabled()
        {
            if (!_convertConfig.Enabled)
            {
                throw new RequestNotValidException(string.Format(Resources.OperationNotEnabled, OperationsConstants.DataConvert));
            }
        }
    }
}
