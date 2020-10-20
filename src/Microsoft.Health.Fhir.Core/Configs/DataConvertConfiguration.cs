// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Core.Configs
{
    public class DataConvertConfiguration
    {
        /// <summary>
        /// Determines whether convert is enabled or not.
        /// </summary>
        public bool Enabled { get; set; }

#pragma warning disable CA1056 // Uri properties should not be strings
        public string ContainerRegistryUrl { get; set; } = string.Empty;
#pragma warning restore CA1056 // Uri properties should not be strings

        public string ContainerRegistryUserName { get; set; } = string.Empty;

        public string ContainerRegistryPassword { get; set; } = string.Empty;
    }
}
