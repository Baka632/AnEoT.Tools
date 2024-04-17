namespace AnEoT.Tools.MarkdownChecker;

internal sealed class FolderChecker : CheckerBase
{
    public static async Task<int> CheckDirectory(string path, string? rootPath, bool isRecursive)
    {
        DirectoryInfo directoryInfo = new(path);
        rootPath ??= path;

        int errorCount = await CheckDirectoryCore(directoryInfo, rootPath, isRecursive);

        return errorCount;
    }

    private static async Task<int> CheckDirectoryCore(DirectoryInfo directoryInfo, string rootPath, bool isRecursive)
    {
        int errorCount = 0;
        foreach (FileInfo file in directoryInfo.EnumerateFiles("*.md"))
        {
            bool isSuccess = await FileChecker.CheckSingleFile(file.FullName, rootPath);

            if (!isSuccess)
            {
                errorCount++;
            }
        }

        if (isRecursive)
        {
            foreach (DirectoryInfo directory in directoryInfo.EnumerateDirectories())
            {
                int errorCountInSubDirectory = await CheckDirectoryCore(directory, rootPath, isRecursive);
                errorCount += errorCountInSubDirectory;
            }
        }

        return errorCount;
    }
}
