using System.Collections.Generic;

namespace PVZDotNetResGen.Utils.JsonHelper
{
    public struct JsonShell<T>
        where T : class, IJsonVersionCheckable
    {
        public string? Source;

        public string? Author;

        public uint? Version;

        public T? Content;
    }

    public struct JsonShellList<T>
        where T : class, IJsonVersionCheckable
    {
        public string? Source;

        public string? Author;

        public uint? Version;

        public List<T?>? Content;
    }
}
