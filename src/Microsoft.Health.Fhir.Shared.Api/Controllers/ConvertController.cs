// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using EnsureThat;
using Hl7.Fhir.Model;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Health.Api.Features.Audit;
using Microsoft.Health.Fhir.Api.Features.ActionResults;
using Microsoft.Health.Fhir.Api.Features.Filters;
using Microsoft.Health.Fhir.Api.Features.Routing;
using Microsoft.Health.Fhir.Core.Extensions;
using Microsoft.Health.Fhir.Core.Messages.Convert;
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

        public ConvertController(IMediator mediator)
        {
            EnsureArg.IsNotNull(mediator, nameof(mediator));

            _mediator = mediator;
        }

        [HttpPost]
        [Route(KnownRoutes.Convert)]
        [AuditEventType(AuditEventSubType.Convert)]
        public async Task<IActionResult> Convert([FromBody] Resource resource)
        {
            var response = await _mediator.Send(new ConvertRequest(resource.ToResourceElement()));
            var content = new ContentResult();
            return new FhirResult(response.Resource);
        }
    }
}
