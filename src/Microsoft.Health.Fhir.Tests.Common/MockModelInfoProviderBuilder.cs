// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Hl7.Fhir.Specification;
using Microsoft.Health.Fhir.Core.Models;
using NSubstitute;

namespace Microsoft.Health.Fhir.Tests.Common
{
    public class MockModelInfoProviderBuilder
    {
        private readonly IModelInfoProvider _mock;

        private MockModelInfoProviderBuilder(IModelInfoProvider mock)
        {
            _mock = mock;
        }

        public static MockModelInfoProviderBuilder Create(FhirSpecification version)
        {
            IModelInfoProvider provider = Substitute.For<IModelInfoProvider>();
            provider.Version.Returns(version);

            provider.StructureDefinitionSummaryProvider.Returns(new MockStructureDefinitionSummaryProvider());
            provider.GetResourceTypeNames().Returns(new[] { "Patient", "Observation" });
            provider.IsKnownResource(Arg.Any<string>()).Returns(x => provider.GetResourceTypeNames().Contains(x[0]));
            return new MockModelInfoProviderBuilder(provider);
        }

        public MockModelInfoProviderBuilder Mock(Action<IModelInfoProvider> action)
        {
            action(_mock);
            return this;
        }

        public IModelInfoProvider Build()
        {
            return _mock;
        }
    }
}
