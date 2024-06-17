using System;
using System.IO;

namespace PVZDotNetResGen.Sexy.Music
{
    public static class WavHelper
    {
        public static TimeSpan GetWavFileDuration(string fileName)
        {
            using (var fileStream = File.OpenRead(fileName))
            {
                using (var binaryReader = new BinaryReader(fileStream))
                {
                    // 跳过RIFF标签
                    binaryReader.ReadBytes(4);

                    // 跳过文件大小
                    binaryReader.ReadBytes(4);

                    // 跳过WAVE标签
                    binaryReader.ReadBytes(4);

                    // 跳过fmt标签
                    binaryReader.ReadBytes(4);

                    // 读取数据大小
                    int dataSize = binaryReader.ReadInt32();

                    // 跳过其他格式信息
                    binaryReader.ReadBytes(16);

                    // 读取采样率
                    int sampleRate = binaryReader.ReadInt32();

                    // 跳过其他信息
                    binaryReader.ReadBytes(6);

                    // 读取采样位数
                    short bitsPerSample = binaryReader.ReadInt16();

                    // 计算音频长度
                    double duration = (double)dataSize / (sampleRate * bitsPerSample / 8);

                    return TimeSpan.FromSeconds(duration);
                }
            }
        }
    }
}
