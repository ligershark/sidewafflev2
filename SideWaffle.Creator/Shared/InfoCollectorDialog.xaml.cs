using System;
using System.Windows;
using System.Windows.Interop;

namespace SideWaffle.Creator
{
    /// <summary>
    /// Interaction logic for InfoCollectorDialog.xaml
    /// </summary>
    public partial class InfoCollectorDialog
    {
        public InfoCollectorDialog()
        {
            InitializeComponent();
        }

        public InfoCollectorDialog(string name)
            : this()
        {
            AuthorTextBox.Text = "Me";
            FriendlyNameTextBox.Text = name;
            DefaultNameTextBox.Text = name;
            ShortNameTextBox.Text = name;
        }

        public string FriendlyName => FriendlyNameTextBox.Text;

        public string DefaultName => DefaultNameTextBox.Text;

        public string ShortName => ShortNameTextBox.Text;

        public void CenterInVs()
        {
            IntPtr hwnd = new IntPtr(VsHelpers.DTE.MainWindow.HWnd);
            Window vs = HwndSource.FromHwnd(hwnd)?.RootVisual as Window;
            Owner = vs;
        }


        private void OnOk(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
