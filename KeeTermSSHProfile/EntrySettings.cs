using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Collections;

namespace KeeTermSSHProfile
{
    [Serializable]
    public class EntrySettings
    {
        // Settings...
        public string test { get; set; }


        // Load & Save utils
        public const string Namespace = "KeeTermSSHProfile";

        public static EntrySettings Load(PwEntry entry)
        {
            var data = entry.Binaries.Get(Namespace);
            if (data != null)
            {
                var stringData = Encoding.Unicode.GetString(data.ReadData());
                if (!string.IsNullOrWhiteSpace(stringData))
                {
                    Debug.Print(stringData);
                    using (var reader = XmlReader.Create(new StringReader(stringData)))
                    {                        
                        return Serializer.Deserialize(reader) as EntrySettings;
                    }
                }                
            }

            return new EntrySettings();
        }

        private static XmlSerializer _serializer;
        private static XmlSerializer Serializer 
        {
            get 
            {
                if (_serializer == null)
                {
                    _serializer = new XmlSerializer(typeof(EntrySettings));
                }
                return _serializer;
            }
        }

        public void Save(PwEntry entry)
        {
            Save(entry.Binaries);
        }

        public void Save(ProtectedBinaryDictionary dict)
        {
            using (var writer = new StringWriter())
            {
                Serializer.Serialize(writer, this);
                Debug.Print(writer.ToString());
                var data = new ProtectedBinary(false, Encoding.Unicode.GetBytes(writer.ToString()));
                dict.Set(Namespace, data);
            }
        }

        public EntrySettings Clone()
        {
            var result = new EntrySettings();

            result.test = this.test;

            return result;
        }
    }
}
