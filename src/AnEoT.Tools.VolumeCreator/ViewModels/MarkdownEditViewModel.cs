using AnEoT.Tools.VolumeCreator.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnEoT.Tools.VolumeCreator.ViewModels;

public sealed partial class MarkdownEditViewModel(MarkdownWrapper wrapper) : ObservableObject
{
    public MarkdownWrapper MarkdownWrapper { get; } = wrapper;

    [ObservableProperty]
    private string markdownString = wrapper.Markdown;

    partial void OnMarkdownStringChanged(string value)
    {
        MarkdownWrapper.Markdown = value;
    }
}