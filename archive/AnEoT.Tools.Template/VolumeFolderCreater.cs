using AnEoT.Tools.Shared;

namespace AnEoT.Tools.Template;

public static class VolumeFolderCreater
{
    public static void CreateVolumeFolder(
        string outputPath, string volName, string introFilePath, string[] articlePaths, string? interviewPath,
        bool createComicFolder, bool createIllustrationFolder, bool createOperatorSecretFolder,
        bool createInterviewFolder)
    {
        string targetFolder = Path.Combine(outputPath, volName);
        Directory.CreateDirectory(targetFolder);

        string introMarkdown = WordToMarkdownService.GetMarkdown(introFilePath);
        string introFileSavePath = Path.Combine(targetFolder, "intro.md");
        File.WriteAllText(introFileSavePath, introMarkdown);
        Console.WriteLine($"已创建卷首语文件：{introFileSavePath}");

        for (int i = 0; i < articlePaths.Length; i++)
        {
            string articlePath = articlePaths[i];
            string articleMarkdown = WordToMarkdownService.GetMarkdown(articlePath);
            string articleFileSavePath = Path.Combine(targetFolder, $"article{i + 1}.md");
            File.WriteAllText(articleFileSavePath, articleMarkdown);
            Console.WriteLine($"已创建文章文件：{articleFileSavePath}");
        }

        if (interviewPath != null)
        {
            string interviewMarkdown = WordToMarkdownService.GetMarkdown(interviewPath);
            string interviewFileSavePath = Path.Combine(targetFolder, "interview.md");
            File.WriteAllText(interviewFileSavePath, interviewMarkdown);
            Console.WriteLine($"已创建专访文件：{interviewFileSavePath}");
        }

        string resourcesFolder = Path.Combine(targetFolder, "res");

        if (createComicFolder)
        {
            string comicFolderPath = Path.Combine(resourcesFolder, "comic");
            Directory.CreateDirectory(comicFolderPath);
            PrintCreateFolderNotification(comicFolderPath);
        }

        if (createIllustrationFolder)
        {
            string illustFolderPath = Path.Combine(resourcesFolder, "illustration");
            Directory.CreateDirectory(illustFolderPath);
            PrintCreateFolderNotification(illustFolderPath);
        }

        if (createOperatorSecretFolder)
        {
            string opeSecFolderPath = Path.Combine(resourcesFolder, "ope_sec");
            Directory.CreateDirectory(opeSecFolderPath);
            PrintCreateFolderNotification(opeSecFolderPath);
        }

        if (createInterviewFolder)
        {
            string interviewFolderPath = Path.Combine(resourcesFolder, "interview");
            Directory.CreateDirectory(interviewFolderPath);
            PrintCreateFolderNotification(interviewFolderPath);
        }

        Console.WriteLine("操作完成。");
    }

    private static void PrintCreateFolderNotification(string path)
    {
        Console.WriteLine($"已创建资源文件夹：{path}");
    }
}
