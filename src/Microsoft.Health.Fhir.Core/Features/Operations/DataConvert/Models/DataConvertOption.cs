// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Core.Features.Operations.DataConvert.Models
{
    public class DataConvertOption
    {
        public string InputData { get; set; }

        public DataConvertInputType DataType { get; set; }

        public string TemplateSetLocation { get; set; }

        public string TemplateSetDigest { get; set; }

        public string EntryPointTemplate { get; set; }
    }
}
