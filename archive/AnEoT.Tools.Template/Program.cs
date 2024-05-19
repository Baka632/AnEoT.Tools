using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Globalization;
using AnEoT.Tools.Template;

RootCommand rootCommand = new("为《回归线》期刊设计的模板文件夹生成器");

Command createCommand = new("create", "新建一个模板文件夹。");

#region Output Path
Option<string> outputPathOption = new("--output-path", "输出文件夹的路径。")
{
    IsRequired = true,
};
outputPathOption.AddValidator(result =>
{
    string? path = result.Tokens.FirstOrDefault()?.Value;
    if (Directory.Exists(path) != true)
    {
        result.ErrorMessage = "输出文件夹不存在。";
    }
});
outputPathOption.AddAlias("-o");
#endregion

#region Volume Name
Option<string> volumeNameOption = new("--volume-name", "形如“2024-05”的期刊名称。")
{
    IsRequired = true,
};
volumeNameOption.AddValidator(result =>
{
    string? volName = result.Tokens.FirstOrDefault()?.Value;

    if (DateOnly.TryParse(volName, CultureInfo.InvariantCulture, out _) != true)
    {
        result.ErrorMessage = "期刊名称不符合规范。";
    }
});
volumeNameOption.AddAlias("-n");
#endregion

#region Articles
Option<string[]> articlesOption = new("--article-paths", "文章 DOCX 文件的路径，可以指定多个文件。")
{
    IsRequired = true,
    AllowMultipleArgumentsPerToken = true,
};
articlesOption.AddValidator(result =>
{
    foreach (Token token in result.Tokens)
    {
        string path = token.Value;

        if (File.Exists(path) != true)
        {
            result.ErrorMessage = $"文章 DOCX 文件 {path} 不存在。";
            break;
        }
    }
});
articlesOption.AddAlias("-a");
#endregion

#region Intro
Option<string> introPathOption = new("--intro-path", "卷首语 DOCX 文件路径。")
{
    IsRequired = true,
};
introPathOption.AddValidator(result =>
{
    string? path = result.Tokens.FirstOrDefault()?.Value;

    if (File.Exists(path) != true)
    {
        result.ErrorMessage = "卷首语 DOCX 文件不存在。";
    }
});
introPathOption.AddAlias("-intro");
#endregion

#region Interview
Option<string?> interviewPathOption = new("--interview-path", "专访 DOCX 文件路径。");
interviewPathOption.AddValidator(result =>
{
    string? path = result.Tokens.FirstOrDefault()?.Value;

    if (File.Exists(path) != true)
    {
        result.ErrorMessage = "专访 DOCX 文件不存在。";
    }
});
interviewPathOption.AddAlias("-interview");
#endregion

#region Resources Folder
Option<bool> createComicFolderOption = new("--create-comic-res-folder", "创建漫画资源文件夹。");
Option<bool> createIllustrationFolderOption = new("--create-illustration-res-folder", "创建插图资源文件夹。");
Option<bool> createOperatorSecretFolderOption = new("--create-operator-secret-res-folder", "创建干员秘闻资源文件夹。");
Option<bool> createInterviewFolderOption = new("--create-interview-res-folder", "创建专访资源文件夹。");

createComicFolderOption.AddAlias("-comic");
createIllustrationFolderOption.AddAlias("-illust");
createOperatorSecretFolderOption.AddAlias("-opsec");
createInterviewFolderOption.AddAlias("-intervi");
#endregion

createCommand.AddOption(outputPathOption);
createCommand.AddOption(volumeNameOption);
createCommand.AddOption(introPathOption);
createCommand.AddOption(articlesOption);
createCommand.AddOption(interviewPathOption);
createCommand.AddOption(createComicFolderOption);
createCommand.AddOption(createIllustrationFolderOption);
createCommand.AddOption(createOperatorSecretFolderOption);
createCommand.AddOption(createInterviewFolderOption);

rootCommand.AddCommand(createCommand);

createCommand.SetHandler((InvocationContext context) =>
{
    string outputPath = context.ParseResult.GetValueForOption(outputPathOption)!;
    string volName = context.ParseResult.GetValueForOption(volumeNameOption)!;
    string introFilePath = context.ParseResult.GetValueForOption(introPathOption)!;
    string[] articlePaths = context.ParseResult.GetValueForOption(articlesOption)!;
    string? interviewPath = context.ParseResult.GetValueForOption(interviewPathOption);
    bool createComic = context.ParseResult.GetValueForOption(createComicFolderOption);
    bool createIllust = context.ParseResult.GetValueForOption(createIllustrationFolderOption);
    bool createOpSec = context.ParseResult.GetValueForOption(createOperatorSecretFolderOption);
    bool createInterview = context.ParseResult.GetValueForOption(createInterviewFolderOption);

    VolumeFolderCreater.CreateVolumeFolder(
        outputPath, volName, introFilePath, articlePaths, interviewPath, createComic, createIllust, createOpSec,
        createInterview);
});

int exitCode = await rootCommand.InvokeAsync(args);
return exitCode;