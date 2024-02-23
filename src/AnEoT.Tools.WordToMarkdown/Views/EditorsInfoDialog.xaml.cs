using System.Windows;

namespace AnEoT.Tools.WordToMarkdown.Views
{
    /// <summary>
    /// EditorsInfoDialog.xaml 的交互逻辑
    /// </summary>
    public partial class EditorsInfoDialog : Window
    {
        public static readonly DependencyProperty EditorStringProperty =
            DependencyProperty.Register("EditorString",
                                        typeof(string),
                                        typeof(EditorsInfoDialog),
                                        new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty WebsiteLayoutDesignerProperty =
            DependencyProperty.Register("WebsiteLayoutDesigner",
                                        typeof(string),
                                        typeof(EditorsInfoDialog),
                                        new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IllustratorProperty =
            DependencyProperty.Register("Illustrator",
                                        typeof(string),
                                        typeof(EditorsInfoDialog),
                                        new PropertyMetadata(string.Empty));

        public string EditorString
        {
            get => (string)GetValue(EditorStringProperty);
            set => SetValue(EditorStringProperty, value);
        }

        public string WebsiteLayoutDesigner
        {
            get => (string)GetValue(WebsiteLayoutDesignerProperty);
            set => SetValue(WebsiteLayoutDesignerProperty, value);
        }

        public string Illustrator
        {
            get => (string)GetValue(IllustratorProperty);
            set => SetValue(IllustratorProperty, value);
        }

        public EditorsInfoDialog()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void OnOkButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
