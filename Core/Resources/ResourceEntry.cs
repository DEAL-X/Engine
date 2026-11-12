namespace MiMFa.Core.Resources
{
    public class ResourceEntry : IResourceEntry
    {
        public string Path { get; }
        public bool IsFile { get; }
        public bool IsFolder { get; }
        public string FullName => System.IO.Path.GetFileName(Path);
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
        public string Extension => System.IO.Path.GetExtension(Path);
        public string Directory => System.IO.Path.GetDirectoryName(Path) + System.IO.Path.DirectorySeparatorChar;
        public string Root => System.IO.Path.GetPathRoot(Path);

        public ResourceEntry(string path, bool? isFile = null, bool? isFolder = null)
        {
            Path = path;
            IsFile = isFile?? System.IO.File.Exists(path);
            IsFolder = isFolder?? System.IO.Directory.Exists(path);
        }
    }
}
