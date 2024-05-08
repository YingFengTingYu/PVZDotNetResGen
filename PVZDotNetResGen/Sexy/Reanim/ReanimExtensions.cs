using System.IO;

namespace PVZDotNetResGen.Sexy.Reanim
{
    public static class ReanimExtensions
    {
        public static void Encode<T>(this T coder, ReanimatorDefinition content, string path) where T : IReanimCoder
        {
            using Stream stream = File.Create(path);
            coder.Encode(content, stream);
        }

        public static ReanimatorDefinition Decode<T>(this T coder, string path) where T : IReanimCoder
        {
            using Stream stream = File.OpenRead(path);
            return coder.Decode(stream);
        }
    }
}
