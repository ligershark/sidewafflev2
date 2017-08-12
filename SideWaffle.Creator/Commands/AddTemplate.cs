using System;
using System.ComponentModel.Design;
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System.Collections;
using System.Collections.Generic;
using TemplateCreator;

namespace SideWaffle.Creator
{
    internal sealed class AddTemplate
    {
        private readonly Package _package;
        private Project _project;

        private AddTemplate(Package package, OleMenuCommandService commandService)
        {
            _package = package;

            var cmdId = new CommandID(PackageGuids.guidPackageCmdSet, PackageIds.AddTemplate);
            var cmd = new OleMenuCommand(Execute, cmdId);
            cmd.BeforeQueryStatus += BeforeQueryStatus;
            commandService.AddCommand(cmd);
        }

        public static AddTemplate Instance
        {
            get; private set;
        }

        //private IServiceProvider ServiceProvider
        //{
        //    get { return _package; }
        //}

        public static void Initialize(Package package, OleMenuCommandService commandService)
        {
            Instance = new AddTemplate(package, commandService);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;
            // button.Enabled = button.Visible = false;
            button.Enabled = button.Visible = true;

            _project = VsHelpers.DTE.SelectedItems.Item(1)?.Project;
            //string root = _project?.GetRootFolder();

            //if (string.IsNullOrEmpty(root))
            //    return;

            //string templateFile = Path.Combine(root, Constants.Folder, Constants.TemplateFileName);

            // button.Enabled = button.Visible = !File.Exists(templateFile);
        }

        private void Execute(object sender, EventArgs e)
        {
            // new TemplateGenerator2().AddMissingFiles(_project);
            IList<string> filesAdded = new TemplateGenerator2().AddMissingFiles(_project);

            if (filesAdded != null) {
                foreach (var file in filesAdded) {
                    if (!string.IsNullOrEmpty(file)) {
                        VsHelpers.OpenFileAndRefresh(file);
                    }
                }
            }
        }
    }
}
