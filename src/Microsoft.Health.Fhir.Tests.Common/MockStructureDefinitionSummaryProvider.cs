// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Hl7.Fhir.Specification;

namespace Microsoft.Health.Fhir.Tests.Common
{
    public class MockStructureDefinitionSummaryProvider : IStructureDefinitionSummaryProvider
    {
        public IStructureDefinitionSummary Provide(string canonical)
        {
            return new MockStructureDefinitionSummary(canonical);
        }

        private class MockStructureDefinitionSummary : IStructureDefinitionSummary
        {
            public MockStructureDefinitionSummary(string typeName)
            {
                TypeName = typeName;
            }

            public string TypeName { get; }

            public bool IsAbstract { get; }

            public bool IsResource { get; } = true;

            public IReadOnlyCollection<IElementDefinitionSummary> GetElements()
            {
                return new IElementDefinitionSummary[0];
            }
        }

        private class MockElementDefinitionSummary : IElementDefinitionSummary
        {
            public string ElementName { get; }

            public bool IsCollection { get; }

            public bool IsRequired { get; }

            public bool InSummary { get; }

            public bool IsChoiceElement { get; }

            public bool IsResource { get; } = true;

            public string DefaultTypeName { get; }

            public ITypeSerializationInfo[] Type { get; }

            public string NonDefaultNamespace { get; }

            public XmlRepresentation Representation { get; }

            public int Order { get; }
        }
    }
}
