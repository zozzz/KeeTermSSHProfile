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
using Newtonsoft.Json;

namespace KeeTermSSHProfile
{
    [Serializable]
    public class GenEntry
    {
        public string title { get; set; }
        public string guid { get; set; }
        public Dictionary<string, object> _props = new Dictionary<string, object>();

        public GenEntry Clone()
        {
            var res = new GenEntry();

            res.title = CloneItem(title);
            res.guid = CloneItem(guid);
            res._props = CloneItem(_props);

            return res;
        }

        public void Set(string name, object value)
        {
            _props[name] = value;
        }

        public void SetDefault(string name, object def)
        {
            if (!_props.ContainsKey(name))
            {
                _props[name] = def;
            }
        }

        public object Get(string name)
        {
            return _props[name];
        }

        public object Get(string name, object def)
        {
            if (_props.ContainsKey(name))
            {
                return _props[name];
            }
            else
            {
                return def;
            }
        }

        public void Del(string name)
        {
            _props.Remove(name);
        }

        private static T CloneItem<T>(T item)
        {
            if (item is ICloneable)
            {
                return (T)((ICloneable)item).Clone();
            }
            else if (item is Dictionary<object, object>)
            {
                return (T)(object)((Dictionary<object, object>)(object)item).ToDictionary(
                    entry => entry.Key,
                    entry => CloneItem(entry.Value)
                );
            }

            return item;
        }        
    }

    [Serializable]
    public class EntrySettings
    {
        // Settings...
        public List<GenEntry> entries = new List<GenEntry>();


        // Load & Save utils
        public const string Namespace = "KeeAutoProfile.json";

        private static JsonSerializer _serializer;
        private static JsonSerializer Serializer {
            get {
                if (_serializer == null)
                {
                    _serializer = new JsonSerializer();
                    _serializer.NullValueHandling = NullValueHandling.Ignore;
                }
                return _serializer;
            }
        }

        public static EntrySettings Load(PwEntry entry)
        {
            var data = entry.Binaries.Get(Namespace);
            if (data != null)
            {
                var stringData = Encoding.Unicode.GetString(data.ReadData());
                if (!string.IsNullOrWhiteSpace(stringData))
                {
                    Debug.Print(stringData);
                    using (var reader = new JsonTextReader(new StringReader(stringData)))
                    {
                        try
                        {
                            return Serializer.Deserialize<EntrySettings>(reader);
                        }
                        catch (JsonReaderException)
                        {
                            return null;
                        }                        
                    }
                }                
            }

            return null;
        }        

        public void Save(PwEntry entry)
        {
            Save(entry.Binaries);
        }

        public void Save(ProtectedBinaryDictionary dict)
        {
            using (var sw = new StringWriter())
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                Serializer.Serialize(writer, this);
                var data = new ProtectedBinary(false, Encoding.Unicode.GetBytes(sw.ToString()));
                dict.Set(Namespace, data);
            }
        }

        public EntrySettings Clone()
        {
            var result = new EntrySettings();

            result.entries = entries.Select(v => v.Clone()).ToList();            

            return result;
        }
    }
}
