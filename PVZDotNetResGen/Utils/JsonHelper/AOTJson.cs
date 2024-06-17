using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PVZDotNetResGen.Utils.JsonHelper
{
    internal static class AOTJson
    {
        private static readonly AOTJsonSerializerContext s_Context;

        static AOTJson()
        {
            s_Context = new AOTJsonSerializerContext(new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
                IncludeFields = true,
            });
        }

        public static bool TrySerializeToFile<T>(string filePath, T value)
            where T : class, IJsonVersionCheckable
        {
            try
            {
                using (Stream stream = File.Create(filePath))
                {
                    Serialize(stream, value);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static T? TryDeserializeFromFile<T>(string filePath)
            where T : class, IJsonVersionCheckable
        {
            try
            {
                using Stream packInfoStream = File.OpenRead(filePath);
                return Deserialize<T>(packInfoStream);
            }
            catch
            {
                return null;
            }
        }

        public static void Serialize<T>(Stream stream, T value)
            where T : class, IJsonVersionCheckable
        {
            JsonSerializer.Serialize(stream, new JsonShell<T>
            {
                Source = typeof(T).AssemblyQualifiedName,
                Author = "YingFengTingYu",
                Version = T.JsonVersion,
                Content = value
            }, typeof(JsonShell<T>), s_Context);
        }

        public static T? Deserialize<T>(Stream stream)
            where T : class, IJsonVersionCheckable
        {
            return JsonSerializer.Deserialize(stream, typeof(JsonShell<T>), s_Context) is JsonShell<T> shell && shell.Version == T.JsonVersion ? shell.Content : default;
        }

        public static bool TrySerializeListToFile<T>(string filePath, List<T?>? value)
            where T : class, IJsonVersionCheckable
        {
            try
            {
                using (Stream stream = File.Create(filePath))
                {
                    SerializeList(stream, value);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static List<T?>? TryDeserializeListFromFile<T>(string filePath)
            where T : class, IJsonVersionCheckable
        {
            try
            {
                using Stream packInfoStream = File.OpenRead(filePath);
                return DeserializeList<T>(packInfoStream);
            }
            catch
            {
                return null;
            }
        }

        public static void SerializeList<T>(Stream stream, List<T?>? value)
            where T : class, IJsonVersionCheckable
        {
            JsonSerializer.Serialize(stream, new JsonShellList<T>
            {
                Source = typeof(T).AssemblyQualifiedName,
                Author = "YingFengTingYu",
                Version = T.JsonVersion,
                Content = value
            }, typeof(JsonShellList<T>), s_Context);
        }

        public static List<T?>? DeserializeList<T>(Stream stream)
            where T : class, IJsonVersionCheckable
        {
            return JsonSerializer.Deserialize(stream, typeof(JsonShellList<T>), s_Context) is JsonShellList<T> shell && shell.Version == T.JsonVersion ? shell.Content : default;
        }
    }
}
