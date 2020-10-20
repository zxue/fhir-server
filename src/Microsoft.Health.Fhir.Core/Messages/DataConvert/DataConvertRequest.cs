// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using MediatR;
using Microsoft.Health.Fhir.Converter.TemplateManagement;
using Microsoft.Health.Fhir.Core.Features.Operations.DataConvert.Models;

namespace Microsoft.Health.Fhir.Core.Messages.DataConvert
{
    public class DataConvertRequest : IRequest<DataConvertResponse>
    {
        public DataConvertRequest(string inputData, DataConvertInputType dataType, ImageInfo templateSetImageInfo, string entryPointTemplate)
        {
            EnsureArg.IsNotNullOrEmpty(inputData);
            EnsureArg.IsNotNull(templateSetImageInfo);
            EnsureArg.IsNotNullOrEmpty(entryPointTemplate);

            InputData = inputData;
            DataType = dataType;
            TemplateSetImageInfo = templateSetImageInfo;
            EntryPointTemplate = entryPointTemplate;
        }

        public string InputData { get; set; }

        public DataConvertInputType DataType { get; set; }

        public ImageInfo TemplateSetImageInfo { get; set; }

        public string EntryPointTemplate { get; set; }
    }
}
