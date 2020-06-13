using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

using Newtonsoft.Json;
using KeePassLib;
using KeeTermSSHProfile;


namespace KeeTermSSHProfile.Generators
{
    class WindowsTerminal : AbstractGenerator
    {
        public const string PLACEHOLDER_ID = "{320d98d3-a6bd-432e-95cf-62f2a140bbb3}";
        public const string SOURCE = "KeeAutoProfile";

        public override string Title { get { return "Windows Terminal"; } }

        protected override bool DetectInstallation() {
            return IsInstalled("Windows Terminal");
        }

        public override void Generate(List<Tuple<PwEntry, EntrySettings>> entries)
        {
            Debug.Print("GENERATE");
            var settingsPath = FindSettingsJson();
            var json = LoadJson(settingsPath);
            CleanSettings(json);
            var placeholderIdx = AddPlaceholder(json);
            WriteEntries(json, placeholderIdx, entries);
            SaveJson(settingsPath, json);
        }

        public override UI.GenEntryFormGroup CreateOptionGroup(GenEntry entry)
        {
            var group = UI.GenEntryForm.CreateGroup(Title);
            group.Controls.Add(new UI.GenEntryFormRow("Command", new Control[] { UI.GenEntryForm.CreateTextBox(entry, "wt.command") }));

            return group;
        }

        private string FindSettingsJson()
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Packages");

            var find = Path.Combine(path, "Microsoft.WindowsTerminal_");

            var terminal = Directory.EnumerateDirectories(path).First(dir => dir.StartsWith(find));
            return Path.Combine(terminal, "LocalState", "settings.json");            
        }

        private void CleanSettings(Dictionary<string, dynamic> json)
        {
            var profiles = json["profiles"]["list"];
            List<int> remove = new List<int>();

            int i = 0;
            foreach (var profile in profiles)
            {
                if (profile.ContainsKey("source") && profile["source"] == SOURCE)
                {
                    if (!profile.ContainsKey("guid") || profile["guid"] != PLACEHOLDER_ID)
                    {
                        remove.Add(i);                        
                    }
                }
                i += 1;
            }

            remove.Reverse();
            foreach (var idx in remove)
            {
                profiles.RemoveAt(idx);
            }
            
        }

        private int AddPlaceholder(Dictionary<string, dynamic> json)
        {
            int foundAt = 0;
            var profiles = json["profiles"]["list"];                  

            foreach (var profile in profiles)
            {
                if (profile.ContainsKey("guid") && profile["guid"] == PLACEHOLDER_ID)
                {
                    return foundAt;
                }
                foundAt += 1;
            }

            Dictionary<string, dynamic> placeholder = new Dictionary<string, dynamic>
            {
                ["guid"] = PLACEHOLDER_ID,
                ["hidden"] = true,
                ["name"] = "KeeAutoProfile - Placeholder",
                ["source"] = SOURCE
            };

            profiles.Add(Newtonsoft.Json.Linq.JToken.FromObject(placeholder));

            return profiles.Count - 1;
        }

        private void WriteEntries(Dictionary<string, dynamic> json, int index, List<Tuple<PwEntry, EntrySettings>> entries)
        {
            var converted = entries.SelectMany(ConvertEntry).ToList();
            converted.Sort(delegate (Dictionary<string, dynamic> a, Dictionary<string, dynamic> b) {
                return b["name"].CompareTo(a["name"]);
            });


            var profiles = json["profiles"]["list"];
            foreach (var item in converted)
            {
                profiles.Insert(index + 1, Newtonsoft.Json.Linq.JToken.FromObject(item));
            }
        }

        private IEnumerable<Dictionary<string, dynamic>> ConvertEntry(Tuple<PwEntry, EntrySettings> entry)
        {
            return entry.Item2.entries
                .ConvertAll(genEntry =>
                {
                    if (String.IsNullOrWhiteSpace(genEntry.Get("wt.command", null) as string))
                    {
                        return null;
                    }

                    var result = new Dictionary<string, dynamic>();
                    result["guid"] = "{" + genEntry.guid + "}";
                    result["hidden"] = false;
                    result["name"] = NewTitle(entry.Item1, genEntry);
                    result["source"] = SOURCE;
                    result["commandline"] = genEntry.Get("wt.command");

                    return result;
                })
                .FindAll(v => v != null);
        }
    }
}
