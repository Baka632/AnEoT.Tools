using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.Storage;

namespace AnEoT.Tools.VolumeCreator.Helpers.JsonConverters;

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
            if (Path.Exists(path))
            {
                StorageFile file = StorageFile.GetFileFromPathAsync(path).GetAwaiter().GetResult();
                return file;
            }
            else
            {
                // TODO: 如果文件不复存在怎么办？
                return null;
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, StorageFile? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.Path);
    }
}
