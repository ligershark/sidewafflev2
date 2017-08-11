using System.IO;
using System.Linq;
using System.Xml.Linq;
using EnvDTE;
using Newtonsoft.Json.Linq;
using System;

namespace SideWaffle.Creator
{
    public class TemplateGenerator
    {
        public static string CreateProjectTemplate(Project proj)
        {
            string fullPath = proj.FullName;
            string dir = Path.GetDirectoryName(fullPath);
            string name = Path.GetFileNameWithoutExtension(fullPath);

            var win = new InfoCollectorDialog(name);
            win.CenterInVs();
            if (win.ShowDialog().GetValueOrDefault())
            {
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

                if (!string.IsNullOrEmpty(projectGuid))
                {
                    guids.Add(ExtractProjectGuid(fullPath));
                }

                return o.ToString();
            }

            return null;
        }

        private static void CreateTemplateJsonIfNotExists(string templateJsonPath,string projectFilepath) {
            if(templateJsonPath == null) {
                throw new ArgumentNullException("filepath");
            }
            if (File.Exists(templateJsonPath)) {
                return;
            }

            string fullPath = System.IO.Path.GetFullPath(templateJsonPath);
            string dir = Path.GetDirectoryName(fullPath);
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
                o["sourceName"] = Path.GetFileNameWithoutExtension(projectFilepath);
                o["shortName"] = win.ShortNameTextBox.Text;

                var guids = (JArray)o["guids"];
                string projectGuid = ExtractProjectGuid(fullPath);

                if (!string.IsNullOrEmpty(projectGuid)) {
                    guids.Add(ExtractProjectGuid(fullPath));
                }

                File.WriteAllText(templateJsonPath, o.ToString());
            }
        }

        private static string ExtractProjectGuid(string fullPath)
        {
            var doc = XDocument.Load(fullPath);
            XElement element = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "ProjectGuid");
            return element?.Value;
        }
    }
}
