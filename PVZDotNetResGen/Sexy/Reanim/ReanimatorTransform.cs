using System.Text.Json.Serialization;

namespace PVZDotNetResGen.Sexy.Reanim;

public class ReanimatorTransform
{
    [JsonPropertyName("x")]
    [JsonPropertyOrder(1)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? JsonSerializableTransX
    {
        get => ReanimHelper.GetValue(ref TransX);
        set => ReanimHelper.SetValue(ref TransX, value);
    }

    [JsonPropertyName("y")]
    [JsonPropertyOrder(2)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? JsonSerializableTransY
    {
        get => ReanimHelper.GetValue(ref TransY);
        set => ReanimHelper.SetValue(ref TransY, value);
    }

    [JsonPropertyName("sx")]
    [JsonPropertyOrder(3)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? JsonSerializableScaleX
    {
        get => ReanimHelper.GetValue(ref ScaleX);
        set => ReanimHelper.SetValue(ref ScaleX, value);
    }

    [JsonPropertyName("sy")]
    [JsonPropertyOrder(4)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? JsonSerializableScaleY
    {
        get => ReanimHelper.GetValue(ref ScaleY);
        set => ReanimHelper.SetValue(ref ScaleY, value);
    }

    [JsonPropertyName("kx")]
    [JsonPropertyOrder(5)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? JsonSerializableSkewX
    {
        get => ReanimHelper.GetValue(ref SkewX);
        set => ReanimHelper.SetValue(ref SkewX, value);
    }

    [JsonPropertyName("ky")]
    [JsonPropertyOrder(6)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? JsonSerializableSkewY
    {
        get => ReanimHelper.GetValue(ref SkewY);
        set => ReanimHelper.SetValue(ref SkewY, value);
    }

    [JsonPropertyName("f")]
    [JsonPropertyOrder(7)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? JsonSerializableFrame
    {
        get => ReanimHelper.GetValue(ref Frame);
        set => ReanimHelper.SetValue(ref Frame, value);
    }

    [JsonPropertyName("a")]
    [JsonPropertyOrder(8)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? JsonSerializableAlpha
    {
        get => ReanimHelper.GetValue(ref Alpha);
        set => ReanimHelper.SetValue(ref Alpha, value);
    }

    public float TransX = ReanimHelper.DEFAULT_FIELD_PLACEHOLDER;
    public float TransY = ReanimHelper.DEFAULT_FIELD_PLACEHOLDER;
    public float ScaleX = ReanimHelper.DEFAULT_FIELD_PLACEHOLDER;
    public float ScaleY = ReanimHelper.DEFAULT_FIELD_PLACEHOLDER;
    public float SkewX = ReanimHelper.DEFAULT_FIELD_PLACEHOLDER;
    public float SkewY = ReanimHelper.DEFAULT_FIELD_PLACEHOLDER;
    public float Frame = ReanimHelper.DEFAULT_FIELD_PLACEHOLDER;
    public float Alpha = ReanimHelper.DEFAULT_FIELD_PLACEHOLDER;

    [JsonPropertyName("i")] [JsonPropertyOrder(9)] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Image = null;

    [JsonPropertyName("font")] [JsonPropertyOrder(10)] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Font = null;

    [JsonPropertyName("text")] [JsonPropertyOrder(11)] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Text = null;

    public bool IsNull()
    {
        return TransX == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
               && TransY == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
               && ScaleX == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
               && ScaleY == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
               && SkewX == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
               && SkewY == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
               && Frame == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
               && Alpha == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER
               && Image == null
               && Font == null
               && Text == null;
    }
}