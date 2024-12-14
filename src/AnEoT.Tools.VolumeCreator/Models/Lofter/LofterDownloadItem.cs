using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using AnEoT.Tools.VolumeCreator.Helpers;
using SixLabors.ImageSharp;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace AnEoT.Tools.VolumeCreator.Models.Lofter;

public partial class LofterDownloadItem(
    LofterDownloadOptions downloadOptions,
    LofterImageInfo imageInfo) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private LofterDownloadItemState _state = LofterDownloadItemState.Paused;
    private Exception? _errorException;

    public string Title { get; } = imageInfo.Title;

    public LofterDownloadItemState State
    {
        get => _state;
        private set
        {
            _state = value;
            OnPropertiesChanged();
        }
    }

    public Exception? ErrorException
    {
        get => _errorException;
        private set
        {
            _errorException = value;
            OnPropertiesChanged();
        }
    }

    public async Task DownloadAsync()
    {
        try
        {
            State = LofterDownloadItemState.Downloading;
            (string savePath, bool convertWebP, bool trimUriQueryPart) = downloadOptions;

            string queryTrimmedUri = imageInfo.ImageUri.OriginalString
                    .Split('?', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    [0];

            Uri imageUri = trimUriQueryPart
                ? new(queryTrimmedUri, UriKind.Absolute)
                : imageInfo.ImageUri;
            Uri sourcePageUri = imageInfo.SourcePageUri;

            string queryTrimmedUriExtension = Path.GetExtension(queryTrimmedUri);
            string suitableTitle = ReplaceInvaildFileNameChars(imageInfo.Title);

            string fileExtension = convertWebP
                ? ".webp"
                : (string.IsNullOrWhiteSpace(queryTrimmedUriExtension) ? ".jpg" : queryTrimmedUriExtension);
            string fileNameWithoutExtension = string.IsNullOrWhiteSpace(suitableTitle)
                ? Path.GetRandomFileName()
                : suitableTitle;

            string fileName = $"{fileNameWithoutExtension}{fileExtension}";
            string filePath = Path.Combine(savePath, fileName);

            int duplicateCount = 1;
            while (File.Exists(filePath))
            {
                string newFileName = $"{fileNameWithoutExtension}({duplicateCount}){fileExtension}";
                filePath = Path.Combine(savePath, newFileName);
                duplicateCount++;
            }

            using Stream imageStream = await LofterDownloadHelper.GetImage(imageUri, sourcePageUri);
            using FileStream fileStream = File.Create(filePath);

            if (convertWebP)
            {
                await Task.Run(async () =>
                {
                    // 防止卡主线程，ImageSharp 的异步方法有点问题。
                    using ImageSharpImage image = await ImageSharpImage.LoadAsync(imageStream);
                    await image.SaveAsWebpAsync(fileStream);
                });
            }
            else
            {
                await imageStream.CopyToAsync(fileStream);
            }
            State = LofterDownloadItemState.Completed;
        }
        catch (Exception ex)
        {
            ErrorException = ex;
            State = LofterDownloadItemState.Error;
        }
    }

    /// <summary>
    /// 通知运行时属性已经发生更改
    /// </summary>
    /// <param name="propertyName">发生更改的属性名称,其填充是自动完成的</param>
    public void OnPropertiesChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// 将字符串中不能作为文件名的部分字符替换为相近的合法字符
    /// </summary>
    /// <param name="fileName">文件名字符串</param>
    /// <returns>新的字符串</returns>
    private static string ReplaceInvaildFileNameChars(string fileName)
    {
        StringBuilder stringBuilder = new(fileName);
        stringBuilder.Replace('"', '\'');
        stringBuilder.Replace('<', '[');
        stringBuilder.Replace('>', ']');
        stringBuilder.Replace('|', 'I');
        stringBuilder.Replace(':', '：');
        stringBuilder.Replace('*', '★');
        stringBuilder.Replace('?', '？');
        stringBuilder.Replace('/', '↗');
        stringBuilder.Replace('\\', '↘');
        return stringBuilder.ToString();
    }
}