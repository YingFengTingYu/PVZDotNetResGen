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
}
