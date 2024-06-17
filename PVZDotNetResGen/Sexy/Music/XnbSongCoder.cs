using PVZDotNetResGen.Utils.StreamHelper;
using PVZDotNetResGen.Utils.XnbContent;
using System.IO;

namespace PVZDotNetResGen.Sexy.Music
{
    public class Song
    {
        public string? Name;
        public int Length;
    }

    internal class XnbSongCoder : IXnbContentCoder<Song>
    {
        public string ReaderTypeString => "Microsoft.Xna.Framework.Content.SongReader";

        public static XnbSongCoder Shared { get; } = new XnbSongCoder();

        public object ReadContent(Stream stream, string originalAssetName, byte version)
        {
            Song song = new Song();
            song.Name = stream.ReadString(stream.Read7BitEncodedInt32(), System.Text.Encoding.UTF8);
            stream.Read7BitEncodedInt32();
            song.Length = stream.ReadInt32LE();
            return song;
        }

        public void WriteContent(object content, Stream stream, string originalAssetName, byte version)
        {
            Song song = (Song)content;
            stream.WriteStringWith7BitEncodedInt32Head(song.Name, System.Text.Encoding.UTF8);
            stream.Write7BitEncodedInt32(2);
            stream.WriteInt32LE(song.Length);
        }
    }

    internal class XnbInt32Coder : IXnbContentCoder<int>
    {
        public string ReaderTypeString => "Microsoft.Xna.Framework.Content.Int32Reader";

        public static XnbInt32Coder Shared { get; } = new XnbInt32Coder();

        public object ReadContent(Stream stream, string originalAssetName, byte version)
        {
            return stream.ReadInt32LE();
        }

        public void WriteContent(object content, Stream stream, string originalAssetName, byte version)
        {
            stream.WriteInt32LE((int)content);
        }
    }
}
