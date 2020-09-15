﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Core.Configs;

namespace Microsoft.Health.Fhir.Core.Features.Operations.Convert.ConvertTemplateStore
{
    public class ContainerRegistryArtifactProvider : IContainerRegistryArtifactProvider
    {
        private ConvertConfiguration _convertConfiguration;

        public ContainerRegistryArtifactProvider(IOptions<ConvertConfiguration> convertConfiguration)
        {
            _convertConfiguration = convertConfiguration.Value;
        }

        public async Task PullArtifactToDirectory(string artifactName, string digest, string targetDirectory)
        {
            using (var httpClient = new HttpClient())
            {
                var requestUrl = new Url($"https://{_convertConfiguration.ContainerRegistryUrl}/v2/{artifactName}/blobs/{digest}");
                var tokenBytes = Encoding.UTF8.GetBytes($"{_convertConfiguration.ContainerRegistryUserName}:{_convertConfiguration.ContainerRegistryPassword}");
                string basicToken = System.Convert.ToBase64String(tokenBytes);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicToken);

                HttpResponseMessage response = await httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var tempPath = Path.GetTempFileName();
                using (var fs = new FileStream(tempPath, FileMode.OpenOrCreate))
                {
                    await response.Content.CopyToAsync(fs);
                }

                DecompressTgzArtifact(tempPath, targetDirectory);
            }

            return;
        }

        private static void DecompressTgzArtifact(string zipFilename, string targetDirectory)
        {
            var fileToDecompress = new FileInfo(zipFilename);

            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string tarFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                using (FileStream decompressedFileStream = File.Create(tarFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }

                var tarToDecompress = new FileInfo(tarFileName);
                ExtractTar(tarFileName, targetDirectory);
            }
        }

        private static void ExtractTar(string tarFileName, string destFolder)
        {
            Stream inStream = File.OpenRead(tarFileName);

            TarArchive tarArchive = TarArchive.CreateInputTarArchive(inStream);
            tarArchive.ExtractContents(destFolder);
            tarArchive.Close();

            inStream.Close();
        }
    }
}
