using YamlDotNet.Serialization;
using AnEoT.Tools.Shared.Models;

namespace AnEoT.Tools.Shared.StaticContexts;

[Obsolete("此类目前不可用，请查阅 https://github.com/aaubry/YamlDotNet/issues/1009。")]
[YamlStaticContext]
[YamlSerializable(typeof(FrontMatter))]
public partial class FrontMatterStaticContext : StaticContext;

[YamlStaticContext]
[YamlSerializable(typeof(ClassFrontMatter))]
public partial class ClassFrontMatterStaticContext : StaticContext;