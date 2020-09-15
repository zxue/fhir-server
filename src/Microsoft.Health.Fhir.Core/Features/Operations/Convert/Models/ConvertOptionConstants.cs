// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Core.Features.Operations.Convert.Models
{
    public static class ConvertOptionConstants
    {
        // request
        public const string InputDataKey = "inputData";
        public const string OperationTypeKey = "convertType";
        public const string TemplateSetLocationKey = "templateSetLocation";
        public const string TemplateSetDigestKey = "templateSetDigest";
        public const string EntryPointTemplateKey = "entryPointTemplate";

        // response
        public const string ResultKey = "return";
        public const string ConvertVersionKey = "converterVersion";
    }
}
