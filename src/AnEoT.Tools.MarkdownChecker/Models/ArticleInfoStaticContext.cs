using YamlDotNet.Serialization;

namespace AnEoT.Tools.MarkdownChecker.Models;

[YamlStaticContext]
[YamlSerializable(typeof(ArticleInfo))]
public sealed partial class ArticleInfoStaticContext : StaticContext;