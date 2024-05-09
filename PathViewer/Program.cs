using System;
using System.Windows;

namespace PathViewer
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            App.Instance.InitializeComponent();
            App.Instance.DispatcherUnhandledException += (o, e) => MessageBox.Show(e.Exception.ToString());
            App.Instance.Run();
        }
    }
}
