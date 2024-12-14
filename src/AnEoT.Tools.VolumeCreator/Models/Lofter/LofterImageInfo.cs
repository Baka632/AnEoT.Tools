using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AnEoT.Tools.VolumeCreator.Models.Lofter;

public sealed partial class LofterImageInfo(Uri imageUri, string title, Uri sourcePageUri) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public Uri SourcePageUri { get; } = sourcePageUri;

    public Uri ImageUri { get; } = imageUri;

    public string Title
    {
        get => title;
        set
        {
            title = value;
            OnPropertiesChanged();
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