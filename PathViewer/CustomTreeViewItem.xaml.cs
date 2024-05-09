using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PathViewer
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