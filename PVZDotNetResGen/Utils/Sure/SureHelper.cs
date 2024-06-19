using System;
using System.Diagnostics.CodeAnalysis;

namespace PVZDotNetResGen.Utils.Sure
{
    public static class SureHelper
    {
        public static void MakeSure([DoesNotReturnIf(false)] bool value)
        {
            if (!value)
            {
                throw new Exception();
            }
        }
    }
}
