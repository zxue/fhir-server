// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Fhir.Converter.Hl7v2;

namespace Microsoft.Health.Fhir.Core.Features.Operations.Convert.ConvertTemplateStore
{
    public class ConvertEngineManager : IConvertEngineManager
    {
        private readonly Dictionary<string, Hl7v2Processor> _processorCache = new Dictionary<string, Hl7v2Processor>();

        public Hl7v2Processor GetHl7V2Processor(string templateFolder)
        {
            if (!_processorCache.ContainsKey(templateFolder))
            {
                _processorCache.Add(templateFolder, new Hl7v2Processor(templateFolder));
            }

            return _processorCache[templateFolder];
        }
    }
}
