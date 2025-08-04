namespace SyncTool.Features
{
    public class FileSyncManager(string source, string replica)
    {
        public FileRefCollection SourceFiles { get; set; } = new FileRefCollection(source);
        public FileRefCollection ReplicaFiles { get; set; } = new FileRefCollection(replica);

        public void Sync()
        {
            IEnumerable<string> inSourceNotReplica = SourceFiles.LocationAndContent.Keys.Except(ReplicaFiles.LocationAndContent.Keys).Select(f => SourceFiles.LocationAndContent[f].RelativePath);
            foreach (string file in inSourceNotReplica.ToList())
            {
                string sourcePath = Path.Combine(SourceFiles.Path, file);
                string replicaPath = Path.Combine(ReplicaFiles.Path, file);

                Extensions.LogAction(File.Exists(replicaPath) ? $"The file at '{replicaPath}' was overwritten." : $"The file from '{sourcePath}' was copied to '{replicaPath}'.");
                File.Copy(sourcePath, replicaPath, true);
            }

            IEnumerable<string> inReplicaNotSource = ReplicaFiles.Location.Keys.Except(SourceFiles.Location.Keys).Select(f => ReplicaFiles.Location[f].RelativePath);
            foreach (string file in inReplicaNotSource.ToList())
            {
                string replicaPath = Path.Combine(ReplicaFiles.Path, file);
                
                File.Delete(replicaPath);
                Extensions.LogAction($"File '{replicaPath}' has been deleted!");
            }
        }
    }

    public class FileRefCollection
    {
        public string Path { get; init; }

        public Dictionary<string, FileRef> Location { get; init; } = [];
        public Dictionary<string, FileRef> LocationAndContent { get; init; } = [];

        public FileRefCollection(string path)
        {
            Path = path;
            
            IEnumerable<FileRef> allFiles = new DirectoryInfo(path).GetFiles("*.*", SearchOption.AllDirectories).Select(f => new FileRef(f, path));
            foreach (FileRef file in allFiles)
            {
                Location[file.RelativePath] = file;
                LocationAndContent[file.Hash] = file;
            }
        }
    }

    public class FileRef
    {
        public FileInfo File { get; init; }

        public string RelativePath { get; set; }

        public string Hash { get; set; }

        public FileRef(FileInfo fileInfo, string relativeTo)
        {
            File = fileInfo;
            Hash = fileInfo.GenerateHash();

            string filePath = fileInfo.FullName.ToBackSlashes();
            string filePathWithoutRelativeDirectory = filePath.RemoveMatchFromStart(relativeTo.ToBackSlashes());

            RelativePath = filePathWithoutRelativeDirectory.Trim('\\');
        }
    }
}