using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PVZDotNetResGen.Sexy.Reanim;

public class ReanimatorDefinition
{
    [JsonPropertyName("doScale")]
    [JsonPropertyOrder(1)]
    public ReanimScaleType DoScale = ReanimScaleType.ScaleFromPC;
    
    [JsonPropertyName("fps")]
    [JsonPropertyOrder(2)]
    public float Fps = 12.0f;
    
    [JsonPropertyName("tracks")]
    [JsonPropertyOrder(3)]
    public List<ReanimatorTrack> Tracks = [];
}