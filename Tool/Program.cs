using Newtonsoft.Json;
using SyncTool.Features;

namespace SyncTool.Tool
{
    public class Program
    {
        internal static DateTime ProgramStartDate { get; private set; }

        internal static string LogsFolderPath { get; private set; }

        internal static string SourceFolderPath { get; private set; }

        internal static string ReplicaFolderPath { get; private set; }

        internal static TimeSpan SyncInterval { get; private set; }

        public static void Main(string[] args)
        {
            ProgramStartDate = DateTime.Now;

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (!args[i].StartsWith("--"))
                        continue;

                    string argValue = args[i + 1];
                    switch (args[i].ToLower())
                    {
                        case "--logs":
                            {
                                if (Extensions.CheckDirectory(argValue))
                                    LogsFolderPath = argValue;

                                break;
                            }

                        case "--source":
                            {
                                if (Extensions.CheckDirectory(argValue))
                                    SourceFolderPath = argValue;

                                break;
                            }

                        case "--replica":
                            {
                                if (Extensions.CheckDirectory(argValue))
                                    ReplicaFolderPath = argValue;

                                break;
                            }

                        case "--interval":
                            {
                                if (Extensions.CheckInterval(argValue, out long seconds))
                                    SyncInterval = TimeSpan.FromSeconds(seconds);

                                break;
                            }
                    }
                }

                if (LogsFolderPath is null || SourceFolderPath is null || ReplicaFolderPath is null || SyncInterval.TotalSeconds <= 0)
                {
                    Extensions.LogAction("Settings are not fully defined!");
                    Console.ReadLine();
                    return;
                }

                goto StartSync;
            }
            
            SyncSettings settings = Settings.SyncSettings;
            if (settings is not null)
            {
                Extensions.LogAction(JsonConvert.SerializeObject(settings));
                bool readSettings = (bool)ReadUserInput("I found some saved settings. Do you want me to load them? Type 'true' or 'false'.", InputType.Bool);

                if (readSettings)
                {
                    LogsFolderPath = settings.LogsFolderPath;
                    SourceFolderPath = settings.SourceFolderPath;
                    ReplicaFolderPath = settings.ReplicaFolderPath;
                    SyncInterval = settings.SyncInterval;

                    Extensions.LogAction("Settings have been loaded.");       
                    goto StartSync;
                }
            }

            LogsFolderPath = ReadUserInput("Please provide the log folder path:", InputType.Path).ToString();
            SourceFolderPath = ReadUserInput("Please provide the source folder path:", InputType.Path).ToString();
            ReplicaFolderPath = ReadUserInput("Please provide the replica folder path:", InputType.Path).ToString();
            SyncInterval = TimeSpan.FromSeconds((long)ReadUserInput("Please provide the sync interval (e.g., 10s, 5m, 2h). Supported units: s = seconds, m = minutes, h = hours, d = days, M = months, y = years.", InputType.Time));

            bool saveSettings = (bool)ReadUserInput("Do you want to save these preferences to the settings file? Please enter 'true' or 'false'.", InputType.Bool);
            if (saveSettings)
            {
                SyncSettings newSettings = new()
                {
                    LogsFolderPath = LogsFolderPath,
                    SourceFolderPath = SourceFolderPath,
                    ReplicaFolderPath = ReplicaFolderPath,
                    SyncInterval = SyncInterval
                };

                Settings.SaveSyncSettings(newSettings);
                
                Extensions.LogAction(JsonConvert.SerializeObject(newSettings));
                Extensions.LogAction("Settings have been saved to the file in the root directory.");
            }

        StartSync:
            new SyncTool().Init().GetAwaiter().GetResult();
        }

        private static object ReadUserInput(string message, InputType type)
        {
            while (true)
            {
                Console.WriteLine(message);
                string userInput = Console.ReadLine();
                if (userInput is null)
                {
                    Extensions.LogAction("Please write something!");
                    continue;
                }

                switch (type)
                {
                    case InputType.Path:
                        {
                            if (Extensions.CheckDirectory(userInput))
                                return userInput;

                            break;
                        }

                    case InputType.Time:
                        {
                            if (Extensions.CheckInterval(userInput, out long seconds))
                                return seconds;

                            break;
                        }

                    case InputType.Bool:
                        {
                            if (bool.TryParse(userInput, out bool result))
                                return result;

                            Extensions.LogAction($"Can't recognize '{userInput}' as clear consent!"); break;
                        }
                }
            }
        }

        private enum InputType
        {
            Path,
            Time,
            Bool
        }
    }
}