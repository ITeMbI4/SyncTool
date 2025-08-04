using Newtonsoft.Json;

namespace SyncTool.Tool
{
    internal static class Settings
    {
        private const string SettingsFile = "Settings.json";

        internal static SyncSettings SyncSettings => JsonConvert.DeserializeObject<SyncSettings>(File.Exists(SettingsFile) ? File.ReadAllText(SettingsFile) : string.Empty);

        internal static void SaveSyncSettings(SyncSettings data) => File.WriteAllText(SettingsFile, JsonConvert.SerializeObject(data, Formatting.Indented));

        internal static void DeleteSettings() => File.Delete(SettingsFile);
    }

    public class SyncSettings
    {
        public string LogFolderPath { get; set; }
        public string SourceFolderPath { get; set; }
        public string ReplicaFolderPath { get; set; }
        public TimeSpan SyncInterval { get; set; }
    }
}
