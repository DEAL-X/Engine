using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MiMFa.Engine.Resource
{
    public class ResourceProvider : IResourceProvider
    {
        public bool IsAbsolute(string path) => Path.IsPathRooted(path);

        public string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            var p = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            try { return Path.GetFullPath(p); } catch { return p; }
        }

        public string Resolve(string path, string basePath = null)
        {
            if (IsAbsolute(path)) return Normalize(path);
            if (string.IsNullOrEmpty(basePath)) basePath = Directory.GetCurrentDirectory();
            var combined = Path.Combine(basePath, path);
            return Normalize(combined);
        }

        public string Combine(params string[] paths) => Path.Combine(paths ?? Array.Empty<string>());

        public IResourceEntry Entity(string path)
        {
            path = Resolve(path);
            return new ResourceEntry(Path.GetFileName(path), path, isFile: File.Exists(path), isFolder: Directory.Exists(path));
        }
       
        public bool Exists(string path) => path.Split(Path.GetInvalidPathChars()).Length <=2 && (File.Exists(path = Resolve(path)) || Directory.Exists(path));

        public string DirectoryName(string path) => Path.GetDirectoryName(Resolve(path));
        public string FileName(string path) => Path.GetFileName(Resolve(path));
        public string Extension(string path) => Path.GetExtension(Resolve(path));
      
        public IEnumerable<string> GetContents(string path, bool recursive = false, Encoding encoding = null)
        {
            path = Resolve(path);
            if (File.Exists(path)) yield return GetFileContents(path, encoding);
            else if (Directory.Exists(path))
                foreach (var item in GetFolderContents(path, recursive))
                    if (item.IsFile) yield return GetFileContents(item.Path, encoding);
                    else if (recursive)
                        foreach (var c in GetContents(item.Path, recursive))
                            yield return c;
        }
        public IEnumerable<IResourceEntry> GetFolderContents(string path, bool recursive = false)
        {
            var results = new SortedList<string, IResourceEntry>(StringComparer.OrdinalIgnoreCase);
            var root = Resolve(path);
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) return results.Values;

            try
            {
                foreach (var dir in Directory.EnumerateDirectories(root))
                {
                    results.Add(dir, new ResourceEntry(Path.GetFileName(dir), dir, isFile: false, isFolder: true));
                    if (recursive)
                        foreach (var ent in GetFolderContents(dir, true))
                            results.Add(ent.Path, ent);
                }

                foreach (var file in Directory.EnumerateFiles(root))
                    results.Add(file, new ResourceEntry(Path.GetFileName(file), file, isFile: true, isFolder: false));

            }
            catch (UnauthorizedAccessException)
            {
                // Ignore directories we can't access
            }

            return results.Values;
        }
        /// <summary>
        /// To copy or remain files or directories to the directory path and remove all other files and directories
        /// </summary>
        /// <param name="path"></param>
        /// <param name="files"></param>
        public void SetFolderContents(string path, params string[] files)
        {
            path = Resolve(path);
            foreach (var item in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                if(!files.Contains(item)) File.Delete(item);
            foreach (var item in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
                if (!files.Contains(item)) Directory.Delete(item, true);
            foreach (var file in files)
            {
                var dest = Resolve(file, path);
                var dir = Path.GetDirectoryName(dest);
                if(dest.StartsWith(path)) continue; // Prevent copying files into themselves
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.Copy(file, dest, true);
            }
        }

        public string GetFileContents(string path, Encoding encoding = null)
        {
            var p = Resolve(path);
            if (!File.Exists(p)) return null;
            return File.ReadAllText(p, encoding ?? Encoding.UTF8);
        }
        public void SetFileContents(string path, string content)
        {
            path = Resolve(path);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, content ?? string.Empty, Encoding.UTF8);
        }
        public void PrependFileContents(string path, string content)
        {
            path = Resolve(path);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, content ?? string.Empty, Encoding.UTF8);
        }

        public void AppendFileContents(string path, string content)
        {
            path = Resolve(path);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.AppendAllText(path, content ?? string.Empty, Encoding.UTF8);
        }

        public void Delete(string path)
        {
            var p = Resolve(path);
            if (File.Exists(p)) File.Delete(p);
            else if (Directory.Exists(p)) Directory.Delete(p, true);
        }


    }
}
