using SyncTool.Features;

namespace SyncTool.Tool
{
    public class SyncTool
    {
        internal async Task Init()
        {
            Extensions.LogAction("Start sync!");
            _ = Task.Run(PerformDirectorySync);

            await Task.Delay(-1);
        }

        private static async Task PerformDirectorySync()
        {
            while (true)
            {
                FileSyncManager files = new(Program.SourceFolderPath, Program.ReplicaFolderPath);
                files.Sync();

                await Task.Delay(Program.SyncInterval);
            }
        }
    }
}
