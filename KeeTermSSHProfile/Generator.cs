using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Deployment;
using System.Diagnostics;

using KeePassLib;
using Newtonsoft.Json;
using KeePass.UI;

namespace KeeTermSSHProfile
{
    public enum GeneratorType {
        NotSet = 0,
        WindowsTerminal = 1
    }

    public abstract class AbstractGenerator
    {
        private bool? _isAvailable;

        public bool IsAvailable 
        {
            get 
            {
                if (_isAvailable == null)
                {
                    _isAvailable = DetectInstallation();
                }
                return _isAvailable ?? false;
            }
        }

        public abstract string Title { get; }
        protected abstract bool DetectInstallation();
        public abstract void Generate(List<Tuple<PwEntry, EntrySettings>> entries);
        public abstract UI.GenEntryFormGroup CreateOptionGroup(GenEntry entry);

        protected static bool IsInstalled(string program)
        {
            // TODO: other methods...
            return IsInstalledRegistry(program);
        }

        protected static bool IsInstalledRegistry(string program)
        {
            RegistryKey[] keys = new RegistryKey[] {
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"),
                Registry.ClassesRoot.OpenSubKey(@"Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages"),
            };

            return keys.Any(key => key != null && CheckRegistryKey(key, "DisplayName", program));
        }

        private static bool CheckRegistryKey(RegistryKey entry, string field, string value)
        {
            Debug.Print(entry.ToString());
            return entry.GetSubKeyNames()
                .Select(entry.OpenSubKey)
                .Select(subEntry => subEntry.GetValue(field) as string)
                .Any(fieldValue => !string.IsNullOrEmpty(fieldValue) && fieldValue == value);                
        }

        protected string NewTitle(PwEntry pwEntry, GenEntry genEntry)
        {
            return String.Join(" / ", NewPath(pwEntry, genEntry));
        }

        protected List<string> NewPath(PwEntry pwEntry, GenEntry genEntry)
        {
            List<string> parts = new List<string>();
            parts.Add(genEntry.title);

            var group = pwEntry.ParentGroup;
            while (group != null && group.ParentGroup != null)
            {
                parts.Add(group.Name);
                group = group.ParentGroup;
            }

            parts.Reverse();
            return parts;
        }

        protected Dictionary<string, dynamic> LoadJson(string path)
        {
            using (var data = new StreamReader(path))
            using (var reader = new JsonTextReader(data))
            {
                try
                {
                    return new JsonSerializer().Deserialize<Dictionary<string, dynamic>>(reader);
                }
                catch (JsonReaderException)
                {
                    return null;
                }
            }
        }

        protected void SaveJson(string path, Dictionary<string, dynamic> data)
        {
            using (var writer = new StreamWriter(path))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.Indentation = 4;
                jsonWriter.IndentChar = ' ';
                var serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;                
                serializer.Serialize(jsonWriter, data);
            }
        }
    }

    public class Generator
    {
        public static IEnumerable<Type> Impls 
        {
            get 
            {
                return Assembly.GetExecutingAssembly().GetTypes()
                      .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == "KeeTermSSHProfile.Generators");
            }
        }

        public static IEnumerable<AbstractGenerator> Available()
        {
            return Impls
                .Select(impl => Activator.CreateInstance(impl) as AbstractGenerator)
                .Where(inst => inst != null && inst.IsAvailable);            
        }

        public static void UpdateAll(PwDatabase db)
        {
            List<Tuple<PwEntry, EntrySettings>> settings = new List<Tuple<PwEntry, EntrySettings>>();
            CollectSettings(settings, db, db.RootGroup);

            foreach (var generator in Available())
            {
                generator.Generate(settings);
            }
        }

        private static void CollectSettings(List<Tuple<PwEntry, EntrySettings>> settings, PwDatabase db, PwGroup group)
        {
            if (group.Uuid.ToHexString() == db.RecycleBinUuid.ToHexString())
            {
                return;
            }

            foreach (PwGroup subGroup in group.Groups)
            {                
                CollectSettings(settings, db, subGroup);                
            }

            foreach (PwEntry entry in group.Entries)
            {
                EntrySettings data = EntrySettings.Load(entry);
                if (data != null)
                {
                    settings.Add(new Tuple<PwEntry, EntrySettings>(entry, data));
                }
            }
        }
    }
}
