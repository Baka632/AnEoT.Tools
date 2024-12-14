using SixLabors.ImageSharp;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace AnEoT.Tools.VolumeCreator.Models.ImageConvert;

public partial class ImageConvertItem(string filePath) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private ImageConvertItemState _State = ImageConvertItemState.None;
    private Exception? _Error;

    public string FilePath { get; } = filePath;

    public ImageConvertItemState State
    {
        get => _State;
        set
        {
            _State = value;
            OnPropertiesChanged();
        }
    }

    public Exception? Error
    {
        get => _Error;
        set
        {
            _Error = value;
            OnPropertiesChanged();
        }
    }

    public async Task ConvertItemAsync(ImageFormatType type, ImageConvertSaveMethod saveMethod, string? otherFolderPath)
    {
        if (saveMethod == ImageConvertSaveMethod.SelectOtherFolder && !Directory.Exists(otherFolderPath))
        {
            throw new ArgumentOutOfRangeException(nameof(otherFolderPath), $"在指定 {nameof(ImageConvertSaveMethod.SelectOtherFolder)} 时，提供的文件夹路径必须有效。");
        }

        try
        {
            State = ImageConvertItemState.Converting;
            string rawFileDirectoryPath = Path.GetDirectoryName(FilePath)!;
            string formatString = type.ToString();
            string saveFileRawPath = Path.ChangeExtension(FilePath, formatString.ToLowerInvariant());
            string saveFileName = Path.GetFileName(saveFileRawPath);

            string saveFilePath = saveMethod switch
            {
                ImageConvertSaveMethod.CreateInnerFolder => Path.Combine(
                    Directory.CreateDirectory(Path.Combine(rawFileDirectoryPath, formatString)).FullName,
                    saveFileName),
                ImageConvertSaveMethod.SelectOtherFolder => Path.Combine(
                    otherFolderPath!,
                    saveFileName),
                _ => Path.Combine(rawFileDirectoryPath, saveFileName),
            };

            using Stream fileStream = File.OpenRead(FilePath);
            using ImageSharpImage image = await ImageSharpImage.LoadAsync(fileStream);
            using FileStream saveFileStream = File.Create(saveFilePath);

            await Task.Run(async () =>
            {
                switch (type)
                {
                    case ImageFormatType.Png:
                        await image.SaveAsPngAsync(saveFileStream);
                        break;
                    case ImageFormatType.Jpg:
                        await image.SaveAsJpegAsync(saveFileStream);
                        break;
                    case ImageFormatType.Webp:
                    default:
                        await image.SaveAsWebpAsync(saveFileStream);
                        break;
                }
            });
            State = ImageConvertItemState.Completed;
        }
        catch (Exception ex)
        {
            State = ImageConvertItemState.Error;
            Error = ex;
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