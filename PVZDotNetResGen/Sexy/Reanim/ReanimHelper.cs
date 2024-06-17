using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace PVZDotNetResGen.Sexy.Reanim;

public class ReanimHelper
{
    public const float DEFAULT_FIELD_PLACEHOLDER = -99999f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float? GetValue(ref float prop)
    {
        return prop == ReanimHelper.DEFAULT_FIELD_PLACEHOLDER ? null : prop;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetValue(ref float prop, float? value)
    {
        prop = value ?? ReanimHelper.DEFAULT_FIELD_PLACEHOLDER;
    }
}

[JsonConverter(typeof(JsonStringEnumConverter<ReanimScaleType>))]
public enum ReanimScaleType : sbyte
{
    NoScale = 0,
    InvertAndScale = 1,
    ScaleFromPC = -1,
}