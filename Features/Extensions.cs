using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SyncTool.Tool;

namespace SyncTool.Features
{
    internal static class Extensions
    {
        internal static string ToBackSlashes(this string input) => input?.Replace('/', '\\');

        public static string ToForwardSlashes(this string input) => input?.Replace('\\', '/');

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

        internal static string RemoveMatchFromStart(this string source, string prefix, bool caseSensitive = true)
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
                    sourceChar = caseSensitive ? sourceList[i] : char.ToLower(sourceList[i]),
                    prefixChar = caseSensitive ? prefixList[i] : char.ToLower(prefixList[i]);

                if (!sourceChar.Equals(prefixChar))
                    break;
            }

            return new string(sourceList.Skip(i).ToArray());
        }

        

        internal static void LogAction(string action)
        {
            string log = $"[{DateTime.UtcNow:dd.MM.yyyy HH:mm:ss.fff}] {action}";

            Console.WriteLine(log);
            if (Program.LogFolderPath is null)
                return;

            File.AppendAllLines(Program.LogFolderPath + $"\\Log {Program.ProgramStartDate:dd-MM-yyyy HH.mm.ss}.txt", [log]);
        }
    }
}
