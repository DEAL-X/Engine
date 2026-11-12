using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MiMFa.Core.Resources
{
    public abstract class ResourceBase : IResourceProvider
    {
        public string DefaultName { get; set; } = "default";
        public string DirectorySeparator => Path.DirectorySeparatorChar+"";
        public string CurrentDirectory { get; set; }
        public string TempDirectory { get; set; }

        public ResourceBase(string currentDirectory = null)
        {
            CurrentDirectory = currentDirectory ?? Directory.GetCurrentDirectory();// Path.GetFullPath(".\\");
            CurrentDirectory = CurrentDirectory.TrimEnd('/', '\\') + Path.DirectorySeparatorChar;
            TempDirectory = CurrentDirectory + "Temporary" + Path.DirectorySeparatorChar;
        }

        public virtual bool IsAbsolute(string path) => Path.IsPathRooted(path);

        public virtual string Normalize(string path, string basePath = null)
        {
            if (string.IsNullOrEmpty(path)) return path;
            path = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            if (IsAbsolute(path)) return Path.GetFullPath(path);
            if (path.StartsWith(Path.DirectorySeparatorChar + "", StringComparison.Ordinal)) 
                basePath = Directory.GetDirectoryRoot(CurrentDirectory);
            else if (path.StartsWith(".", StringComparison.Ordinal))
                basePath = string.IsNullOrEmpty(basePath)? CurrentDirectory: basePath;
            else if (path.StartsWith("~"+ Path.DirectorySeparatorChar, StringComparison.Ordinal))
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            else basePath = Directory.GetCurrentDirectory();
            return Path.GetFullPath(Path.Combine(basePath, path));
        }

        public string Combine(params string[] paths) => Path.Combine(paths ?? Array.Empty<string>());

        public virtual string ParentDirectory(string path) => Path.GetDirectoryName(Normalize(path));
        public virtual string DirectoryName(string path) => Path.GetDirectoryName(Normalize(path)).Split(Path.DirectorySeparatorChar).Last();
        public virtual string FileName(string path) => Path.GetFileName(Normalize(path));
        public virtual string Extension(string path) => Path.GetExtension(Normalize(path));

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

        public virtual string GetFileLine(string path, int index, Encoding encoding = null)
        {
            if (index >= 0)
            {
                foreach (var item in GetFileLines(path, encoding))
                    if (index-- <= 0) return item;
            }
            else
            {
                foreach (var item in GetFileLines(path, encoding).Reverse())
                    if (index++ >= 0) return item;
            }
            return null;
        }
        public virtual void SetFileLine(string path, int index, string line, Encoding encoding = null)
        {
            var lines = GetFileLines(path, encoding);
            SetFileLines(path, lines.Take(index).Concat(new string[] { line }).Concat(lines.Skip(index)), encoding);
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