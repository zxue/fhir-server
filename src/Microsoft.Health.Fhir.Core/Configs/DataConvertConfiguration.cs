// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Fhir.Core.Features.Operations.DataConvert.TemplateStore;

namespace Microsoft.Health.Fhir.Core.Configs
{
    public class DataConvertConfiguration
    {
        /// <summary>
        /// Determines whether convert is enabled or not.
        /// </summary>
        public bool Enabled { get; set; }

        public List<TemplateRegistry> TemplateRegistries { get; } = new List<TemplateRegistry>();

        public TimeSpan RegistryTokenExpiration { get; set; } = TimeSpan.FromMinutes(30);

        public TimeSpan ProcessTimeoutThreshold { get; set; } = TimeSpan.FromSeconds(30);
    }
}
