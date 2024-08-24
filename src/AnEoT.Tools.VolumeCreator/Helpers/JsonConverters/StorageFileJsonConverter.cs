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
            StorageFile file = StorageFile.GetFileFromPathAsync(path).GetAwaiter().GetResult();
            return file;
        }
    }

    public override void Write(Utf8JsonWriter writer, StorageFile? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.Path);
    }
}
