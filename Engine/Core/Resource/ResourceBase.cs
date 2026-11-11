using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MiMFa.Engine.Resource
{
    public abstract class ResourceBase : IResourceProvider
    {
        public string DefaultName { get; set; } = "default";
        public string DirectorySeparator => Path.DirectorySeparatorChar+"";
        public string RootDirectory { get; set; }
        public string TempDirectory { get; set; }

        public ResourceBase(string rootDirectory = null)
        {
            RootDirectory = rootDirectory ?? Directory.GetCurrentDirectory();// Path.GetFullPath(".\\");
            RootDirectory = RootDirectory.TrimEnd('/', '\\') + Path.DirectorySeparatorChar;
            TempDirectory = RootDirectory + "Temporary" + Path.DirectorySeparatorChar;
        }

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
            if (string.IsNullOrEmpty(basePath)) basePath = RootDirectory;
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
        public abstract bool CreateDirectory(string path);

        public abstract byte[] GetFileContents(string path);
        public abstract void SetFileContents(string path, byte[] content);

        public abstract string GetFileText(string path, Encoding encoding = null);
        public abstract void SetFileText(string path, string content, Encoding encoding = null);
        public abstract void PrependFileText(string path, string content, Encoding encoding = null);
        public abstract void AppendFileText(string path, string content, Encoding encoding = null);

        public virtual IEnumerable<string> GetFileLines(string path, Encoding encoding = null)
        {
            return GetFileText(path, encoding).Split('\n', '\r');
        }
        public virtual void SetFileLines(string path, IEnumerable<string> lines, Encoding encoding = null)
        {
            SetFileText(path, string.Join(Environment.NewLine, lines) + Environment.NewLine, encoding);
        }
        public virtual void PrependFileLines(string path, IEnumerable<string> lines, Encoding encoding = null)
        {
            PrependFileText(path, string.Join(Environment.NewLine, lines) + Environment.NewLine, encoding);
        }
        public virtual void AppendFileLines(string path, IEnumerable<string> lines, Encoding encoding = null)
        {
            AppendFileText(path, string.Join(Environment.NewLine, lines) + Environment.NewLine, encoding);
        }
        public virtual void PrependFileLine(string path, string line, Encoding encoding = null)
        {
            PrependFileText(path, line+ Environment.NewLine, encoding);
        }
        public virtual void AppendFileLine(string path, string line, Encoding encoding = null)
        {
            AppendFileText(path, line + Environment.NewLine, encoding);
        }

        public abstract void Move(string source, string destination, bool overwrite = false);
        public abstract void Copy(string source, string destination, bool overwrite = false);
        public abstract void Rename(string path, string newName, bool overwrite = false);
        public abstract void Delete(string path);
    }
}