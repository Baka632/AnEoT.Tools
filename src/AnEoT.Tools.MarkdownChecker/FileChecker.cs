using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace AnEoT.Tools.MarkdownChecker;

internal sealed class FileChecker : CheckerBase
{
    public static async Task<bool> CheckSingleFile(string path, string? rootPath)
    {
        bool result = true;
        string markdown = await File.ReadAllTextAsync(path);
        MarkdownDocument document = Markdown.Parse(markdown, MarkdownPipeline);

        foreach (MarkdownObject obj in document.Descendants())
        {
            if (obj is LinkInline link && string.IsNullOrWhiteSpace(link.Url) != true)
            {
                string urlString = link.Url;
                int line = link.Line + 1;

                Uri uri = new(urlString, UriKind.RelativeOrAbsolute);

                if (uri.IsAbsoluteUri)
                {
                    if (uri.Scheme is "http" or "https")
                    {
                        try
                        {
                            using HttpResponseMessage httpMessage = await HttpClient.GetAsync(uri);
                            if (httpMessage.IsSuccessStatusCode != true)
                            {
                                LogCannotAccessLink(Logger, path, line, urlString);
                                result = false;
                            }
                        }
                        catch (HttpRequestException)
                        {
                            LogCannotAccessLink(Logger, path, line, urlString);
                            result = false;
                        }
                    }
                }
                else
                {
                    string targetPath;
                    if (urlString.StartsWith('/'))
                    {
                        if (rootPath is not null)
                        {
                            if (rootPath.EndsWith('/') || rootPath.EndsWith('\\'))
                            {
                                rootPath = rootPath[..^1];
                            }
                            targetPath = $"{rootPath}{urlString}";
                        }
                        else
                        {
                            string dirName = Path.GetDirectoryName(path) ?? string.Empty;
                            if (dirName.EndsWith('/') || dirName.EndsWith('\\'))
                            {
                                dirName = dirName[..^1];
                            }
                            targetPath = $"{dirName}{urlString}";
                        }
                    }
                    else
                    {
                        string dirName = Path.GetDirectoryName(path) ?? string.Empty;
                        targetPath = Path.Combine(dirName, urlString);
                    }

                    if (Path.Exists(targetPath) != true)
                    {
                        string fileExt = Path.GetExtension(targetPath);
                        if (!fileExt.Equals(".html", StringComparison.OrdinalIgnoreCase) || !Path.Exists(targetPath.Replace(".html", ".md")))
                        {
                            LogCannotFindFile(Logger, path, line, urlString, targetPath);
                            result = false;
                        }
                    }
                }
            }
        }

        return result;
    }
}
