using AnEoT.Tools.MarkdownChecker.Models;

namespace AnEoT.Tools.MarkdownChecker.Checkers;

public static class FolderChecker
{
    public static async Task<CheckResult> CheckDirectory(string path, string? rootPath, bool isRecursive)
    {
        DirectoryInfo directoryInfo = new(path);
        rootPath ??= path;

        CheckResult result = await CheckDirectoryCore(directoryInfo, rootPath, isRecursive);
        return result;
    }

    private static async Task<CheckResult> CheckDirectoryCore(DirectoryInfo directoryInfo, string rootPath, bool isRecursive)
    {
        CheckResult result = new();
        foreach (FileInfo file in directoryInfo.EnumerateFiles("*.md"))
        {
            CheckResult checkResult = await FileChecker.CheckSingleFile(file.FullName, rootPath);
            result += checkResult;
        }

        if (isRecursive)
        {
            foreach (DirectoryInfo directory in directoryInfo.EnumerateDirectories())
            {
                CheckResult resultInSubDirectory = await CheckDirectoryCore(directory, rootPath, isRecursive);
                result += resultInSubDirectory;
            }
        }

        return result;
    }
}