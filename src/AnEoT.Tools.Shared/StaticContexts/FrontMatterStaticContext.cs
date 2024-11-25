using YamlDotNet.Serialization;
using AnEoT.Tools.Shared.Models;

namespace AnEoT.Tools.Shared.StaticContexts;

[YamlStaticContext]
[YamlSerializable(typeof(FrontMatter))]
public partial class FrontMatterStaticContext : StaticContext
{
}