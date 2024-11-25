using System.Text.Json.Serialization;

namespace AnEoT.Tools.VolumeCreator.Models;

[JsonSerializable(typeof(Dictionary<string, FakeAdConfiguration>))]
public partial class StringFakeAdConfigContext : JsonSerializerContext
{
}