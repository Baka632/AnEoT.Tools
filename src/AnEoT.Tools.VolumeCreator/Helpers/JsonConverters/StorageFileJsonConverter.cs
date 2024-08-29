using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.Storage;

namespace AnEoT.Tools.VolumeCreator.Helpers.JsonConverters;

[Obsolete("不再使用。")]
public class StorageFileJsonConverter : JsonConverter<StorageFile?>
{
    public override StorageFile? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? path = reader.GetString();

        if (path is null)
        {
            return null;
        }
        else
        {
            if (File.Exists(path))
            {
                StorageFile file = StorageFile.GetFileFromPathAsync(path).GetAwaiter().GetResult();
                return file;
            }
            else
            {
                return null;
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, StorageFile? value, JsonSerializerOptions options)
    {
        if (File.Exists(value?.Path))
        {
            writer.WriteStringValue(value.Path);
        }
        else
        {
            writer.WriteStringValue(string.Empty);
        }
    }
}
