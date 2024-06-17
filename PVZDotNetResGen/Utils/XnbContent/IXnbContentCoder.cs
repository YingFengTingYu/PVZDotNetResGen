using System.IO;

namespace PVZDotNetResGen.Utils.XnbContent;

public interface IXnbContentCoder
{
    string ReaderTypeString { get; }
    
    object ReadContent(Stream stream, string originalAssetName, byte version);

    void WriteContent(object content, Stream stream, string originalAssetName, byte version);
}

public interface IXnbContentCoder<T> : IXnbContentCoder
{
    
}