using Newtonsoft.Json;
using SyncTool.Features;

namespace SyncTool.Tool
{
    public class Program
    {
        internal static DateTime ProgramStartDate { get; private set; }

        internal static string LogFolderPath { get; private set; }

        internal static string SourceFolderPath { get; private set; }

        internal static string ReplicaFolderPath { get; private set; }

        internal static TimeSpan SyncInterval { get; private set; }

        private const string
            Log = "log",
            Source = "source",
            Replica = "replica",
            Interval = "interval";

        public static void Main(string[] args)
        {
            ProgramStartDate = DateTime.Now;

            SyncSettings settings = Settings.SyncSettings;
            if (settings is not null)
            {
                bool readSettings = (bool)ReadUserInput("I found previously recorded settings! Should I read it? Please write 'true' or 'false'.", InputType.Bool);
                if (readSettings)
                {
                    LogFolderPath = settings.LogFolderPath;
                    SourceFolderPath = settings.SourceFolderPath;
                    ReplicaFolderPath = settings.ReplicaFolderPath;
                    SyncInterval = settings.SyncInterval;

                    Extensions.LogAction($"Settings are loaded:\n{JsonConvert.SerializeObject(settings, Formatting.Indented)}");
                    goto StartSync;
                }
                else Settings.DeleteSettings();
            }

            if (args.Length == 0)
            {
                LogFolderPath = ReadUserInput("Please provide Log folder path:", InputType.Path).ToString();
                SourceFolderPath = ReadUserInput("Please provide Source folder path:", InputType.Path).ToString();
                ReplicaFolderPath = ReadUserInput("Please provide Replica folder path:", InputType.Path).ToString();
                SyncInterval = TimeSpan.FromSeconds((long)ReadUserInput("Please provide sync interval in format:", InputType.Time));

                bool saveSettings = (bool)ReadUserInput("Save those preferences in the setting file? Please write 'true' or 'false'.", InputType.Bool);
                if (saveSettings)
                {
                    Settings.SaveSyncSettings(new SyncSettings
                    {
                        LogFolderPath = LogFolderPath,
                        SourceFolderPath = SourceFolderPath,
                        ReplicaFolderPath = ReplicaFolderPath,
                        SyncInterval = SyncInterval
                    });

                    Extensions.LogAction("Settings saved to the file in root directory!");
                }

                goto StartSync;
            }

            if (args.CheckArgument(Log, out string log) && CheckDirectory(log))
                LogFolderPath = log;

            if (args.CheckArgument(Source, out string source) && CheckDirectory(source))
                SourceFolderPath = source;

            if (args.CheckArgument(Replica, out string replica) && CheckDirectory(replica))
                ReplicaFolderPath = replica;

            if (args.CheckArgument(Interval, out string interval) && CheckInterval(interval, out long seconds))
                SyncInterval = TimeSpan.FromSeconds(seconds);

            if (SourceFolderPath is null || ReplicaFolderPath is null || LogFolderPath is null || SyncInterval.TotalSeconds <= 0)
            {
                Extensions.LogAction("Settings are not fully defined!");
                Console.ReadLine();
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
                    Extensions.LogAction("You should write something!");
                    continue;
                }

                switch (type)
                {
                    case InputType.Path:
                        {
                            if (CheckDirectory(userInput))
                                return userInput;

                            break;
                        }

                    case InputType.Time:
                        {
                            if (CheckInterval(userInput, out long seconds))
                                return seconds;

                            break;
                        }

                    case InputType.Bool:
                        {
                            if (bool.TryParse(userInput, out bool result))
                                return result;

                            Extensions.LogAction($"Can't define your '{userInput}' as clear consent!"); break;
                        }
                }
            }
        }

        private static bool CheckDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Extensions.LogAction($"Folder with the path '{path}' defined!");
                return true;
            }

            Extensions.LogAction($"Folder with the path '{path}' does not exist!");
            return false;
        }

        private static bool CheckInterval(string time, out long seconds)
        {
            if (long.TryParse(time, out long result) && result > 0)
            {
                seconds = result;
                Extensions.LogAction($"Sync interval set to '{result}' seconds!");

                return true;
            }

            if (time.Length < 2 || !long.TryParse(time.AsSpan(0, time.Length - 1), out result) || result <= 0)
            {
                Extensions.LogAction($"Input {result} is not a valid time.");

                seconds = 0;
                return false;
            }

            seconds = time[^1] switch
            {
                'S' or 's' => result,
                'm' => result * 60,
                'H' or 'h' => result * 3600,
                'D' or 'd' => result * 86400,
                'M' => result * 2592000,
                'Y' or 'y' => result * 31536000,
                _ => 0
            };

            Extensions.LogAction($"Sync interval set to '{result}' seconds!");
            return seconds > 0;
        }

        private enum InputType
        {
            Path,
            Time,
            Bool
        }
    }
}