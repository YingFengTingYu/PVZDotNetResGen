using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
