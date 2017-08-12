using System;
using System.ComponentModel.Design;
using System.IO;
using Microsoft.VisualStudio.Shell;

namespace SideWaffle.Creator
{
    internal sealed class AddCliHostFile
    {
        private readonly Package _package;
        private string _folder;

        private AddCliHostFile(Package package, OleMenuCommandService commandService)
        {
            _package = package;

            var cmdId = new CommandID(PackageGuids.guidPackageCmdSet, PackageIds.AddCliHost);
            var cmd = new OleMenuCommand(Execute, cmdId);
            cmd.BeforeQueryStatus += BeforeQueryStatus;
            commandService.AddCommand(cmd);
        }

        public static AddCliHostFile Instance
        {
            get; private set;
        }

        //private IServiceProvider ServiceProvider
        //{
        //    get { return _package; }
        //}

        public static void Initialize(Package package, OleMenuCommandService commandService)
        {
            Instance = new AddCliHostFile(package, commandService);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;
            button.Enabled = button.Visible = false;

            if (!VsHelpers.IsTemplateFolder(out var item))
                return;

            _folder = item.FileNames[1];
            string path = Path.Combine(_folder, Constants.CliHostFileName);

            button.Visible = true;
            button.Enabled = !File.Exists(path);
        }

        private void Execute(object sender, EventArgs e)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                CopyFile(_folder, Constants.CliHostFileName, true);
            });
        }

        internal static void CopyFile(string folder, string fileName, bool openInEditor)
        {
            string source = VsHelpers.GetFileInVsix($"Resources\\{fileName}");
            string dest = Path.Combine(folder, fileName);

            if (!File.Exists(dest))
            {
                File.Copy(source, dest);

                if (openInEditor)
                {
                    VsHelpers.OpenFileAndRefresh(dest);
                }
            }
        }
    }
}
