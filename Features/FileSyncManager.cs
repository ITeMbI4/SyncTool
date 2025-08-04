using System.Security.Cryptography;

namespace SyncTool.Features
{
    public class FileSyncManager
    {
        public FileRefCollection SourceFiles { get; set; }
        public FileRefCollection ReplicaFiles { get; set; }

        public FileSyncManager(string source, string replica)
        {
            if (source is null)
                throw new NullReferenceException($"{nameof(source)} cannot be null");
            
            if (replica is null)
                throw new NullReferenceException($"{nameof(replica)} cannot be null");

            if (!Directory.Exists(source))
                throw new InvalidOperationException($"Source directory \"{source}\" does not exist");

            if (!Directory.Exists(replica))
                throw new InvalidOperationException($"Target directory \"{replica}\" does not exist");

            SourceFiles = new FileRefCollection(source);
            ReplicaFiles = new FileRefCollection(replica);
        }

        public void Sync()
        {
            IEnumerable<string> inSourceNotReplica = SourceFiles.LocationAndContent.Keys.Except(ReplicaFiles.LocationAndContent.Keys).Select(f => SourceFiles.LocationAndContent[f].RelativePath);
            foreach (string file in inSourceNotReplica.ToList())
            {
                string sourcePath = Path.Combine(SourceFiles.Path, file);
                string replicaPath = Path.Combine(ReplicaFiles.Path, file);

                Extensions.LogAction(File.Exists(replicaPath) ? $"File {replicaPath} was overwritten!" : $"File {sourcePath} was copyed to {replicaPath}!");
                File.Copy(sourcePath, replicaPath, true);
            }

            IEnumerable<string> inTargetNotSource = ReplicaFiles.Location.Keys.Except(SourceFiles.Location.Keys).Select(f => ReplicaFiles.Location[f].RelativePath);
            foreach (string file in inTargetNotSource.ToList())
            {
                string replicaPath = Path.Combine(ReplicaFiles.Path, file);
                
                File.Delete(replicaPath);
                Extensions.LogAction($"File {replicaPath} was deleted!");
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
            Hash = GenerateHash();

            string filePath = fileInfo.FullName.ToBackSlashes();
            string filePathWithoutRelativeDirectory = filePath.RemoveMatchFromStart(relativeTo.ToBackSlashes(), false);

            RelativePath = filePathWithoutRelativeDirectory.Trim('\\');
        }

        private string GenerateHash()
        {
            using MD5 md5 = MD5.Create();
            using FileStream stream = File.OpenRead();

            byte[] hash = md5.ComputeHash(stream);
            return Convert.ToHexStringLower(hash);
        }
    }
}