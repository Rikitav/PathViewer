using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.PortableExecutable;
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

namespace PathViewer.AppWindow
{
    /// <summary>
    /// Логика взаимодействия для CustomButton.xaml
    /// </summary>
    public partial class CustomButton : Button, INotifyPropertyChanged
    {
        public string Header
        {
            get => _Header;
            set
            {
                _Header = value;
                OnPropertyChanged(nameof(Header));
            }
        }
        private string _Header;

        public event PropertyChangedEventHandler? PropertyChanged;

        public CustomButton()
        {
            InitializeComponent();
        }

        private void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
