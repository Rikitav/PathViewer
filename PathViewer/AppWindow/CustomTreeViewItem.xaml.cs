using System.Windows.Controls;
using System.Windows.Media;

namespace PathViewer.AppWindow
{
    /// <summary>
    /// Логика взаимодействия для CustomTreeViewItem.xaml
    /// </summary>
    public partial class CustomTreeViewItem : TreeViewItem
    {
        public ImageSource? Image
        {
            get => ItemImage.Source;
            set => ItemImage.Source = value;
        }

        public string Text
        {
            get => _ActualText;

            set
            {
                _ActualText = value;
                UpdateName();
            }
        }
        private string _ActualText;

        public CustomTreeViewItem(string HeaderText)
        {
            InitializeComponent();
            Text = HeaderText;
            Items.Clear();
        }

        public void UpdateName(int Value = -1)
        {
            if (Value >= 0)
                ItemLabel.Text = string.Format("{0} (Items : {1})", _ActualText, Value);
            else
                ItemLabel.Text = Items.Count == 0 ? _ActualText : string.Format("{0} (Items : {1})", _ActualText, Items.Count);
        }
    }
}