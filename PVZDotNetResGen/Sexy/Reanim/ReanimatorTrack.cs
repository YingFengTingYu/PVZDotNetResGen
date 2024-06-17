using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PVZDotNetResGen.Sexy.Reanim;

public class ReanimatorTrack
{
    [JsonPropertyName("name")]
    [JsonPropertyOrder(1)]
    public string? Name = null;
    
    [JsonPropertyName("transforms")]
    [JsonPropertyOrder(2)]
    public List<ReanimatorTransform> Transforms = [];
}