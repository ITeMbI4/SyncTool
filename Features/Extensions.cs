using SyncTool.Tool;
using System.Security.Cryptography;

namespace SyncTool.Features
{
    internal static class Extensions
    {
        internal static string ToBackSlashes(this string input) => input?.Replace('/', '\\');

        internal static bool CheckArgument(this string[] args, string arg, out string result)
        {
            string userArg = args.FirstOrDefault(a => a.Contains($"{arg}=", StringComparison.OrdinalIgnoreCase));
            if (userArg is null)
            {
                result = null;
                return false;
            }

            result = userArg.Split('=')[1];
            return true;
        }

        internal static string RemoveMatchFromStart(this string source, string prefix)
        {
            if (prefix.Length > source.Length)
                return string.Empty;

            if (prefix.Length == 0)
                return source;

            List<char>
                sourceList = source.ToList(),
                prefixList = prefix.ToList();

            int i = 0;
            for (; i < prefix.Length; i++)
            {
                char
                    sourceChar = char.ToLower(sourceList[i]),
                    prefixChar = char.ToLower(prefixList[i]);

                if (!sourceChar.Equals(prefixChar))
                    break;
            }

            return new string(sourceList.Skip(i).ToArray());
        }

        internal static string GenerateHash(this FileInfo file)
        {
            using MD5 md5 = MD5.Create();
            using FileStream stream = file.OpenRead();

            byte[] hash = md5.ComputeHash(stream);
            return Convert.ToHexStringLower(hash);
        }

        internal static bool CheckDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                LogAction($"Folder at path '{path}' has been defined.");
                return true;
            }

            LogAction($"Folder at path '{path}' does not exist.");
            return false;
        }

        internal static bool CheckInterval(string time, out long seconds)
        {
            if (long.TryParse(time, out long result) && result > 0)
            {
                seconds = result;
                LogAction($"Sync interval set to '{result}' seconds.");

                return true;
            }

            if (time.Length < 2 || !long.TryParse(time.AsSpan(0, time.Length - 1), out result) || result <= 0)
            {
                LogAction($"Input '{result}' is not a valid time.");

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

            LogAction($"Sync interval set to '{result}' seconds.");
            return seconds > 0;
        }

        internal static void LogAction(string action)
        {
            string log = $"[{DateTime.UtcNow:dd.MM.yyyy HH:mm:ss.fff}] {action}";

            Console.WriteLine(log);
            if (Program.LogFolderPath is null)
                return;

            File.AppendAllLines(Path.Combine(Program.LogFolderPath, $"Log {Program.ProgramStartDate:dd-MM-yyyy HH.mm.ss}.txt"), [log]);
        }
    }
}
