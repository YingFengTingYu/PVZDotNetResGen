using System.IO;

namespace PVZDotNetResGen.Sexy.Reanim
{
    public interface IReanimCoder
    {
        void Encode(ReanimatorDefinition content, Stream stream);

        ReanimatorDefinition Decode(Stream stream);
    }
}
