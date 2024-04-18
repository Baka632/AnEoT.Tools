using AnEoT.Tools.MarkdownChecker.Models;

namespace AnEoT.Tools.MarkdownChecker.Checkers;

internal sealed partial class FileChecker : CheckerBase
{
    public static async Task<CheckResult> CheckSingleFile(string path, string? rootPath)
    {
        CheckResult result = new();
        string fileExtensions = Path.GetExtension(path);

        if (fileExtensions.Equals(".md", StringComparison.OrdinalIgnoreCase))
        {
            result = await CheckMarkdown(path, rootPath);
        }

        return result;
    }
}
