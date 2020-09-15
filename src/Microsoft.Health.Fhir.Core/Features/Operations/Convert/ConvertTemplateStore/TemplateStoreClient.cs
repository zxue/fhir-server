// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Core.Features.Operations.Convert.Models;

namespace Microsoft.Health.Fhir.Core.Features.Operations.Convert.ConvertTemplateStore
{
    public class TemplateStoreClient : ITemplateStoreClient
    {
        private readonly string _templateHomeFolder = "templates";
        private readonly ConcurrentDictionary<string, string> _templateCache = new ConcurrentDictionary<string, string>();
        private readonly IContainerRegistryArtifactProvider _artifactProvider;

        public TemplateStoreClient(IContainerRegistryArtifactProvider artifactProvider)
        {
            _artifactProvider = artifactProvider;
        }

        public async Task<string> GetTemplateSet(ConvertOption options)
        {
            var templateKey = $"{options.TemplateSetLocation}_{options.TemplateSetDigest}";
            var templateFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _templateHomeFolder, GetBase64(templateKey));

            // Todo: concurrency issue
            if (!_templateCache.ContainsKey(templateKey))
            {
                if (Directory.Exists(templateFolder))
                {
                    Directory.Delete(templateFolder, true);
                }

                Directory.CreateDirectory(templateFolder);
                await _artifactProvider.PullArtifactToDirectory(options.TemplateSetLocation, options.TemplateSetDigest, templateFolder);
                _templateCache.TryAdd(templateKey, templateFolder);
            }

            return _templateCache[templateKey];
        }

        private string GetBase64(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return System.Convert.ToBase64String(bytes);
        }
    }
}
