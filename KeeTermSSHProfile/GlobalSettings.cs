using System;

using KeePass.Plugins;


namespace KeeTermSSHProfile
{
    [Serializable]
    public class GlobalSettings
    {
        // Settings...
        public ulong TerminalType { get; set; }


        // Load & Save utils
        public const string Namespace = "KeeTermSSHProfile.";

        public static GlobalSettings Load(IPluginHost pluginHost) {
            var cfg = pluginHost.CustomConfig;
            var result = new GlobalSettings();

            result.TerminalType = cfg.GetULong(Namespace + "TerminalType", 0);

            return result;
        }

        

        public void save(IPluginHost pluginHost) {
            var cfg = pluginHost.CustomConfig;

            cfg.SetULong(Namespace + "TerminalType", TerminalType);
        }
    }
}
