// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using FluentValidation.Results;
using Microsoft.Health.Fhir.Core.Models;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace Microsoft.Health.Fhir.Core.Features.Validation
{
    public interface IModelSpecificationValidator
    {
        IEnumerable<ValidationFailure> Validate(ResourceElement value);
    }
}
