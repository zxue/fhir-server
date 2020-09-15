// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Core.Features.Operations.Convert.Models
{
    public class ConvertOption
    {
        public string InputData { get; set; }

        public ConvertOperationType OperationType { get; set; }

        public string TemplateSetLocation { get; set; }

        public string TemplateSetDigest { get; set; }

        public string EntryPointTemplate { get; set; }
    }
}
