using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MiMFa.Engine.Resource
{
    public abstract class ResourceBase : IResourceProvider
    {
        public virtual bool IsAbsolute(string path) => Path.IsPathRooted(path);

        public virtual string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            var p = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            try { return Path.GetFullPath(p); } catch { return p; }
        }

        public virtual string Resolve(string path, string basePath = null)
        {
            if (IsAbsolute(path)) return Normalize(path);
            if (string.IsNullOrEmpty(basePath)) basePath = Directory.GetCurrentDirectory();
            var combined = Path.Combine(basePath, path);
            return Normalize(combined);
        }

        public string Combine(params string[] paths) => Path.Combine(paths ?? Array.Empty<string>());

        public virtual string ParentDirectory(string path) => Path.GetDirectoryName(Resolve(path));
        public virtual string DirectoryName(string path) => Path.GetDirectoryName(Resolve(path)).Split(Path.DirectorySeparatorChar).Last();
        public virtual string FileName(string path) => Path.GetFileName(Resolve(path));
        public virtual string Extension(string path) => Path.GetExtension(Resolve(path));

        public abstract IResourceEntry Entity(string path);
        public abstract bool Exists(string path);

        public abstract IEnumerable<string> GetContents(string path, bool recursive = false, Encoding encoding = null);
        public abstract IEnumerable<IResourceEntry> GetFolderContents(string path, bool recursive = false);
        public abstract void SetFolderContents(string path, params string[] files);
        public abstract string GetFileContents(string path, System.Text.Encoding encoding = null);
        public abstract void SetFileContents(string path, string content);
        public abstract void PrependFileContents(string path, string content);
        public abstract void AppendFileContents(string path, string content);
        public abstract void CreateDirectory(string path);
        public abstract void Move(string source, string destination, bool overwrite = false);
        public abstract void Copy(string source, string destination, bool overwrite = false);
        public abstract void Rename(string path, string newName, bool overwrite = false);
        public abstract void Delete(string path);
    }
}