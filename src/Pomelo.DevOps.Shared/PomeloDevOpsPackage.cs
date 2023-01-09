// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Pomelo.DevOps.Models.ViewModels;
using YamlDotNet.Serialization;
using System.Linq;

namespace Pomelo.DevOps.Shared
{
    public class PomeloDevOpsPackage : ZipArchive
    {
        private static ISerializer yamlSerializer = new SerializerBuilder().Build();

        private static IDeserializer yamlDeserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

        public PomeloDevOpsPackage(Stream stream) 
            : base(stream, ZipArchiveMode.Read)
        { }

        private async ValueTask<string> GetFileContentFromPackageAsync(string file)
        {
            var entry = GetEntry(file);
            if (entry == null)
            {
                throw new FileNotFoundException("The component has not been found", file);
            }

            using (var stream = entry.Open())
            using (var reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public ValueTask<string> GetComponentInfoAsync()
            => GetFileContentFromPackageAsync("step.yml");

        public async ValueTask<byte[]> GetComponentIconAsync()
        {
            var entry = GetEntry("icon.png");
            if (entry == null)
            {
                throw new FileNotFoundException("The item has not been found", "icon.png");
            }

            using (var stream = entry.Open())
            {
                var buffer = new byte[entry.Length];
                var read = 0;
                while(read < buffer.Length)
                {
                    read += await stream.ReadAsync(buffer, read, buffer.Length - read);
                }
                return buffer;
            }
        }

        public async ValueTask<Step> DeserializeYamlAsync()
        {
            var yml = await GetComponentInfoAsync();
            return yamlDeserializer.Deserialize<Step>(yml);
        }
    }
}
