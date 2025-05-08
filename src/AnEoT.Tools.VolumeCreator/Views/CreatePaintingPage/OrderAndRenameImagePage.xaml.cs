using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AnEoT.Tools.VolumeCreator.Models;
using AnEoT.Tools.VolumeCreator.Models.CreatePaintingPage;

namespace AnEoT.Tools.VolumeCreator.Views.CreatePaintingPage;
public sealed partial class OrderAndRenameImagePage : Page
{
    private CreatePaintingPageWindowAccessor windowAccessor;
    private CreatePaintingPageData data;
    private readonly Dictionary<AssetNode, string> FileNodeAndAuthorNamePair = [];

    // FileNode 中包含 required 成员，而自动生成的代码不能处理这种情况，因此这里的泛型参数仍然是 AssetNode
    public ObservableCollection<AssetNode> FileNodes { get; private set; }

#pragma warning disable CS8618 // OnNavigatedTo 会出手
    public OrderAndRenameImagePage()
    {
        this.InitializeComponent();
    }
#pragma warning restore CS8618

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is CreatePaintingPageWindowAccessor accessor)
        {
            windowAccessor = accessor;
            data = windowAccessor.PaintingPageData;

            FileNodes = [.. data.FileAssets.OrderBy(node => node.DisplayName)];

            foreach (AssetNode node in FileNodes)
            {
                Match match = TryMatchAuthorNameRegex().Match(node.DisplayName);

                FileNodeAndAuthorNamePair[node] = match.Success
                    ? match.Groups[1].Value
                    : node.DisplayName;
            }
        }
    }

    [GeneratedRegex(@"[(（](.*?)[）)]")]
    private static partial Regex TryMatchAuthorNameRegex();

    private void OnAuthorTextBoxLoaded(object sender, RoutedEventArgs e)
    {
        TextBox textBox = (TextBox)sender;
        AssetNode node = (AssetNode)textBox.DataContext;

        textBox.Text = FileNodeAndAuthorNamePair.TryGetValue(node, out string? authorName)
            ? authorName
            : string.Empty;
    }

    private void OnAuthorTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        TextBox textBox = (TextBox)sender;
        AssetNode node = (AssetNode)textBox.DataContext;

        FileNodeAndAuthorNamePair[node] = textBox.Text;

        windowAccessor.EnableForward = !FileNodeAndAuthorNamePair.Any(pair => string.IsNullOrWhiteSpace(pair.Value));
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        if (e.SourcePageType == typeof(CompletePage))
        {
            List<PaintingInfo> paintingInfos = new(FileNodes.Count);

            foreach (AssetNode node in FileNodes)
            {
                if (FileNodeAndAuthorNamePair.TryGetValue(node, out string? authorName))
                {
                    if (node is FileNode file)
                    {
                        paintingInfos.Add(new PaintingInfo(file, authorName));
                    }
                }
            }

            windowAccessor.PaintingPageData = windowAccessor.PaintingPageData with
            {
                PaintingInfos = paintingInfos
            };
        }
    }
}
