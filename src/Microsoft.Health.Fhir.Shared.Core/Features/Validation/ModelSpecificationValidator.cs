// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification.Snapshot;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Validation;
using Microsoft.Health.Fhir.Core.Models;

namespace Microsoft.Health.Fhir.Core.Features.Validation
{
    public class ModelSpecificationValidator : IModelSpecificationValidator
    {
        private static readonly CachedResolver _source = new CachedResolver(new MultiResolver(new ZipSource(Path.Combine(Environment.CurrentDirectory, "definitions.json.zip")), new WebResolver()));

        private static readonly Validator _validator = new Validator(new ValidationSettings
        {
            ResourceResolver = _source,
            Trace = false,
            GenerateSnapshot = true,
            EnableXsdValidation = true,
            ResolveExternalReferences = false,
            SkipConstraintValidation = false,
            GenerateSnapshotSettings = SnapshotGeneratorSettings.CreateDefault(),
        });

        public IEnumerable<ValidationFailure> Validate(ResourceElement value)
        {
            OperationOutcome result = _validator.Validate(value.Instance);

            if (result.Issue.Any())
            {
                return result.Issue
                    .Select(x =>
                        new FhirValidationFailure(
                            x.Location?.FirstOrDefault() ?? x.TypeName,
                            x.Diagnostics ?? x.Details?.Text,
                            new OperationOutcomeIssue(x.Severity.ToString(), x.Code.ToString(), x.Diagnostics ?? x.Details?.Text, x.Location?.ToArray())));
            }

            return Enumerable.Empty<ValidationFailure>();
        }
    }
}
