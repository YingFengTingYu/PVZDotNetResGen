using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PVZDotNetResGen.Sexy.Image;
using PVZDotNetResGen.Sexy.Reanim;
using PVZDotNetResGen.Utils.Graphics.Bitmap;

namespace PVZDotNetResGen.Utils.XnbContent;

public static class XnbCoderManager
{
    private static readonly FrozenDictionary<string, IXnbContentCoder> CodersByName;
    private static readonly FrozenDictionary<Type, IXnbContentCoder> CodersByType;

    static XnbCoderManager()
    {
        Dictionary<string, IXnbContentCoder> codersByName = [];
        Dictionary<Type, IXnbContentCoder> codersByType = [];
        codersByName.Add(XnbReanimCoder.Shared.ReaderTypeString, XnbReanimCoder.Shared);
        codersByType.Add(typeof(ReanimatorDefinition), XnbReanimCoder.Shared);
        codersByName.Add(XnbTexture2DCoder.Shared.ReaderTypeString, XnbTexture2DCoder.Shared);
        codersByType.Add(typeof(IDisposableBitmap), XnbTexture2DCoder.Shared);
        CodersByName = codersByName.ToFrozenDictionary();
        CodersByType = codersByType.ToFrozenDictionary();
    }

    public static bool Get(string typeString, [NotNullWhen(true)] out IXnbContentCoder? coder)
    {
        return CodersByName.TryGetValue(typeString, out coder);
    }
    
    public static bool Get(Type type, [NotNullWhen(true)] out IXnbContentCoder? coder)
    {
        foreach (var pair in CodersByType)
        {
            if (pair.Key.IsAssignableFrom(type))
            {
                coder = pair.Value;
                return true;
            }
        }

        coder = null;
        return false;
    }
}