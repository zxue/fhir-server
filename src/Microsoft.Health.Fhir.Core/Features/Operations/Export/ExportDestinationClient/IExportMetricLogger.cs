// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Core.Features.Operations.Export.ExportDestinationClient
{
    public interface IExportMetricLogger
    {
        /// <summary>
        /// Informs the logger that data is being exported.
        /// </summary>
        /// <param name="numberOfBytes">The number of bytes of data being exported</param>
        void SendingBytes(long numberOfBytes);

        /// <summary>
        /// Informs the logger that all data has been commited.
        /// </summary>
        void CommitedBytes();
    }
}
