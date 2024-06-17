using System.Collections.Generic;

namespace PVZDotNetResGen.Utils.JsonHelper
{
    internal struct JsonShell<T>
        where T : class, IJsonVersionCheckable
    {
        public string? Source;

        public string? Author;

        public uint? Version;

        public T? Content;
    }

    internal struct JsonShellList<T>
        where T : class, IJsonVersionCheckable
    {
        public string? Source;

        public string? Author;

        public uint? Version;

        public List<T?>? Content;
    }
}
