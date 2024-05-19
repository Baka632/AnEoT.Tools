using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AnEoT.Tools.VolumeCreator.Models;

/// <summary>
/// 用于支持双向数据绑定的字符串包装类
/// </summary>
public sealed class StringView : INotifyPropertyChanged, IEquatable<StringView?>
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private string _StringContent = string.Empty;

    public StringView()
    {
    }

    public StringView(string content)
    {
        _StringContent = content;
    }

    /// <summary>
    /// 此实例中包装的字符串内容
    /// </summary>
    public string StringContent
    {
        get => _StringContent;
        set
        {
            _StringContent = value;
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

    public override bool Equals(object? obj)
    {
        return Equals(obj as StringView);
    }

    public bool Equals(StringView? other)
    {
        return other is not null &&
               StringContent == other.StringContent;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(StringContent);
    }

    public static bool operator ==(StringView? left, StringView? right)
    {
        return EqualityComparer<StringView>.Default.Equals(left, right);
    }

    public static bool operator !=(StringView? left, StringView? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return StringContent;
    }

    public static implicit operator string(StringView stringView)
    {
        return stringView.StringContent;
    } 

    public static implicit operator StringView(string str)
    {
        return new StringView()
        {
            _StringContent = str,
        };
    }
}
