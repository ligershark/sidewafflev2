using System.IO;
using System.Linq;
using System.Xml.Linq;
using EnvDTE;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SideWaffle.Creator.Shared {
    public class TemplateGenerator2 {
        private JObject _templateInfo { get; set; }
        public IList<string> AddMissingFiles(Project proj) {
            if (proj == null) {
                throw new ArgumentNullException("proj");
            }

            string projectName = Path.GetFileNameWithoutExtension(proj.FullName);
            string projectDir = Path.GetDirectoryName(proj.FullName);
            string templateJsonDir = Path.Combine(projectDir, ".template.config");
            if (!Directory.Exists(templateJsonDir)) {
                Directory.CreateDirectory(templateJsonDir);
            }
            // see which files are missing and add then create those

            bool hasTemplateJsonFile = File.Exists(Path.Combine(templateJsonDir, "template.json"));
            bool hasVsHostFile = File.Exists(Path.Combine(templateJsonDir, $"{projectName}.vstemplate"));
            bool hasVstemplateFile = File.Exists(Path.Combine(templateJsonDir, "vs-2017.3.host.json"));
            bool hasCliHostFile = File.Exists(Path.Combine(templateJsonDir, "dotnetcli.host.json"));

            if (hasTemplateJsonFile &&
                hasVsHostFile &&
                hasVstemplateFile &&
                hasCliHostFile) {
                // nothing to do
                return null;
            }

            IList<string> filesCreated = new List<string>();
            JObject templateData = GetTemplateJsonDataFromUser(proj);
            filesCreated.Add(
                CreateTemplateJsonIfNotExists(Path.Combine(templateJsonDir, "template.json"), proj.FullName, templateData));
            filesCreated.Add(
                CreateVsTemplateFileIfNotExists(Path.Combine(templateJsonDir, "template.vstemplate"), proj.FullName, templateData));

            return (from file in filesCreated
                    where !string.IsNullOrWhiteSpace(file)
                    select file).ToList();
        }

        private string CreateTemplateJsonIfNotExists(string templateJsonPath, string projectFilepath, JObject templateData) {
            if (templateJsonPath == null) {
                throw new ArgumentNullException("filepath");
            }
            if (templateData == null) {
                throw new ArgumentNullException("templateData");
            }

            if (File.Exists(templateJsonPath)) {
                return null;
            }

            File.WriteAllText(templateJsonPath, templateData.ToString());
            return templateJsonPath;
        }

        private string CreateVsTemplateFileIfNotExists(string vstemplateFilepath, string projectFilepath, JObject templateData) {
            if (string.IsNullOrWhiteSpace(vstemplateFilepath)) {
                throw new ArgumentNullException("vstemplateFilepath");
            }
            if (templateData == null) {
                throw new ArgumentNullException("templateData");
            }

            if (File.Exists(vstemplateFilepath)) {
                return null;
            }
            // name, description, templateid
            string name = templateData["name"].Value<string>();
            string desc = templateData["description"].Value<string>();
            string templateId = templateData["identity"].Value<string>();
            var templateContent = string.Format(_vstemplateFile, name, desc, templateId);
            File.WriteAllText(vstemplateFilepath, templateContent);
            return vstemplateFilepath;
        }

        private JObject GetTemplateJsonDataFromUser(Project proj) {
            //if (_templateInfo == null) {
                string fullPath = proj.FullName;
                string name = Path.GetFileNameWithoutExtension(fullPath);
                var win = new InfoCollectorDialog(name);
                win.CenterInVs();
                if (win.ShowDialog().GetValueOrDefault()) {
                    const string solutionTemplate = @"{
    ""author"": """",
    ""classifications"": [ ],
    ""description"": """",
    ""name"": """",
    ""defaultName"": """",
    ""identity"": """",
    ""groupIdentity"": """",
    ""tags"": { },
    ""shortName"": """",
    ""sourceName"": """",
    ""guids"": [ ]
}";

                    var o = JObject.Parse(solutionTemplate);
                    o["author"] = win.AuthorTextBox.Text;
                    o["name"] = win.FriendlyNameTextBox.Text;
                    o["defaultName"] = win.DefaultNameTextBox.Text;
                    o["sourceName"] = Path.GetFileNameWithoutExtension(proj.FullName);
                    o["shortName"] = win.ShortNameTextBox.Text;

                    var guids = (JArray)o["guids"];
                    string projectGuid = ExtractProjectGuid(fullPath);

                    if (!string.IsNullOrEmpty(projectGuid)) {
                        guids.Add(ExtractProjectGuid(fullPath));
                    }

                    _templateInfo = o;
                }
            //}

            return _templateInfo;
        }

        private string ExtractProjectGuid(string fullPath) {
            var doc = XDocument.Load(fullPath);
            XElement element = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "ProjectGuid");
            return element?.Value;
        }

        private string _vstemplateFile = @"
<VSTemplate Version=""3.0.0"" xmlns=""http://schemas.microsoft.com/developer/vstemplate/2005"" Type=""ProjectGroup"">
  <TemplateData>
    <Name>{0}</Name>
    <Description>{1}</Description>
    <TemplateID>{2}</TemplateID>
    <DefaultName>NewProject</DefaultName>
    
    <Icon>project-icon.png</Icon>
    
    <ProjectType>CSharp</ProjectType>
    <NumberOfParentCategoriesToRollUp>1</NumberOfParentCategoriesToRollUp>
    <SortOrder>5000</SortOrder>
    <CreateNewFolder>true</CreateNewFolder>
    <ProvideDefaultName>true</ProvideDefaultName>
    <LocationField>Enabled</LocationField>
    <EnableLocationBrowseButton>true</EnableLocationBrowseButton>
  </TemplateData>
  <TemplateContent>
    <ProjectCollection/>
    <CustomParameters>
      <CustomParameter Name = ""$language$"" Value=""CSharp"" />
      <CustomParameter Name = ""$uistyle$"" Value=""none""/>
      <CustomParameter Name = ""$groupid$"" Value=""MyProject.01.Sample"" />
      <CustomParameter Name = ""SideWaffleNewProjNode"" Value=""CSharp\Web\SideWaffle""/>
    </CustomParameters>
  </TemplateContent>
  <WizardExtension>
    <Assembly>Microsoft.VisualStudio.TemplateEngine.Wizard, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a</Assembly>
    <FullClassName>Microsoft.VisualStudio.TemplateEngine.Wizard.TemplateEngineWizard</FullClassName>
  </WizardExtension>
</VSTemplate>
";
    }
}
