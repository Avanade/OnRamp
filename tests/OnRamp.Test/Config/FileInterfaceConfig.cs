using OnRamp.Config;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#nullable enable

namespace OnRamp.Test.Config
{
    public class FileInterfaceConfig : ConfigRootBase<FileInterfaceConfig>
    {
        [JsonPropertyName("file")]
        public FileSettings? File { get; set; }

        protected override Task PrepareAsync() => Task.CompletedTask;

        public class FileSettings
        {
            [JsonPropertyName("delimiter")]
            public string? Delimiter { get; set; }
        }
    }
}
