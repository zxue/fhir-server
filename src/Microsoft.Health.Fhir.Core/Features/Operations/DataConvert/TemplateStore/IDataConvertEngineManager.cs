// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Converter.Hl7v2;

namespace Microsoft.Health.Fhir.Core.Features.Operations.DataConvert.TemplateStore
{
    public interface IDataConvertEngineManager
    {
        public Hl7v2Processor GetHl7V2Processor();
    }
}
