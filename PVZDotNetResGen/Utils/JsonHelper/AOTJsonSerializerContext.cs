using System.Text.Json.Serialization;
using PVZDotNetResGen.Sexy;
using PVZDotNetResGen.Sexy.Atlas;
using PVZDotNetResGen.Utils.JsonHelper;

namespace PVZDotNetResGen.Utils.JsonHelper
{
    [JsonSourceGenerationOptions(
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        GenerationMode = JsonSourceGenerationMode.Metadata,
        IncludeFields = true,
        WriteIndented = true
        )]
    [JsonSerializable(typeof(ResType))]
    [JsonSerializable(typeof(TextureFormat))]
    [JsonSerializable(typeof(AnimType))]
    [JsonSerializable(typeof(CompiledFileFormat))]
    [JsonSerializable(typeof(SurfaceFormat))]
    [JsonSerializable(typeof(DiskFormat))]
    [JsonSerializable(typeof(JsonShell<WPAtlasInfoAnalyzer.AtlasJson>))]
    [JsonSerializable(typeof(JsonShell<ResBase>))]
    [JsonSerializable(typeof(JsonShell<PackInfo>))]
    internal partial class AOTJsonSerializerContext : JsonSerializerContext
    {
    }
}
