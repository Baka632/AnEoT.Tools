using System.ComponentModel;
using System.Runtime.CompilerServices;
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

            string queryTrimmedUri = imageInfo.ImageUri
                    .Split('?', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    [0];

            Uri imageUri = trimUriQueryPart
                ? new(queryTrimmedUri, UriKind.Absolute)
                : new(imageInfo.ImageUri, UriKind.Absolute);
            Uri sourcePageUri = imageInfo.SourcePageUri;
            string fileName = convertWebP
                ? Path.ChangeExtension(Path.GetFileName(queryTrimmedUri), ".webp")
                : Path.GetFileName(queryTrimmedUri);
            string filePath = Path.Combine(savePath, fileName);

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
}