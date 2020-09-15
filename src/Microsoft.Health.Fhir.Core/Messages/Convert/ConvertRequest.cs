// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using MediatR;
using Microsoft.Health.Fhir.Core.Models;

namespace Microsoft.Health.Fhir.Core.Messages.Convert
{
    public class ConvertRequest : IRequest<ConvertResponse>
    {
        public ConvertRequest(ResourceElement resource)
        {
            EnsureArg.IsNotNull(resource);

            Resource = resource;
        }

        public ResourceElement Resource { get; }
    }
}
