// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Core.Features.Definition;
using Microsoft.Health.Fhir.Core.Models;
using Microsoft.Health.Fhir.Tests.Common;
using Xunit;

namespace Microsoft.Health.Fhir.SqlServer.UnitTests
{
    public class GetSPDMTests
    {
        [Fact]
        public void GetSPDM()
        {
            var modelInfo = MockModelInfoProviderBuilder
                .Create(FhirSpecification.R4)
                .Build();

            var spdm = new SearchParameterDefinitionManager(modelInfo);
            spdm.Start();
        }
    }
}
