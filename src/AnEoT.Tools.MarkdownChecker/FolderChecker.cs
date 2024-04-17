namespace AnEoT.Tools.MarkdownChecker;

internal sealed class FolderChecker : CheckerBase
{
    public static async Task CheckDirectory(string path, string? rootPath, bool isRecursive)
    {
        DirectoryInfo directoryInfo = new(path);

        rootPath ??= path;

        await CheckDirectoryCore(directoryInfo, rootPath, isRecursive);
    }

    private static async Task CheckDirectoryCore(DirectoryInfo directoryInfo, string rootPath, bool isRecursive)
    {
        foreach (FileInfo file in directoryInfo.EnumerateFiles("*.md"))
        {
            await FileChecker.CheckSingleFile(file.FullName, rootPath);
        }

        if (isRecursive)
        {
            foreach (DirectoryInfo directory in directoryInfo.EnumerateDirectories())
            {
                await CheckDirectoryCore(directory, rootPath, isRecursive);
            }
        }
    }
}
