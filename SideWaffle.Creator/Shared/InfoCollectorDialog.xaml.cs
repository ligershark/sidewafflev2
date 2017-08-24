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
            IdentityTextBox.Text = "Sample.Template.CSharp";
            GroupIdTextBox.Text = "Sample.Template";
            DefaultNameTextBox.Text = "MyProject";
            ShortNameTextBox.Text = string.Empty;
            DisplayNameTextBox.Text = string.Empty;
            DescriptionTextBox.Text = string.Empty;
        }

        public string Identity => IdentityTextBox.Text;

        public string GroupIdentity => GroupIdTextBox.Text;

        public string Author => AuthorTextBox.Text;

        public string DefaultName => DefaultNameTextBox.Text;

        public string ShortName => ShortNameTextBox.Text;

        public string DisplayName => DisplayNameTextBox.Text;

        public string Description => DescriptionTextBox.Text;

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
