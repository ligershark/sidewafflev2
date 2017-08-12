using System;
using System.ComponentModel.Design;
using System.IO;
using Microsoft.VisualStudio.Shell;

namespace SideWaffle.Creator
{
    internal sealed class AddVsHostFile
    {
        private readonly Package _package;
        private string _folder;

        private AddVsHostFile(Package package, OleMenuCommandService commandService)
        {
            _package = package;

            var cmdId = new CommandID(PackageGuids.guidPackageCmdSet, PackageIds.AddVsHost);
            var cmd = new OleMenuCommand(Execute, cmdId);
            cmd.BeforeQueryStatus += BeforeQueryStatus;
            commandService.AddCommand(cmd);
        }

        public static AddVsHostFile Instance
        {
            get; private set;
        }

        //private IServiceProvider ServiceProvider
        //{
        //    get { return _package; }
        //}

        public static void Initialize(Package package, OleMenuCommandService commandService)
        {
            Instance = new AddVsHostFile(package, commandService);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;
            button.Enabled = button.Visible = false;

            if (!VsHelpers.IsTemplateFolder(out var item))
                return;

            _folder = item.FileNames[1];
            string path = Path.Combine(_folder, Constants.VsHostFileName);

            button.Visible = true;
            button.Enabled = !File.Exists(path);
        }

        private void Execute(object sender, EventArgs e)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                AddCliHostFile.CopyFile(_folder, "template-icon.png", false);
                AddCliHostFile.CopyFile(_folder, Constants.VsHostFileName, true);
            });
        }
    }
}
