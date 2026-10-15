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

        public bool Exists(string path) => File.Exists(path = Resolve(path)) || Directory.Exists(path);

        public string DirectoryName(string path) => Path.GetDirectoryName(Resolve(path));

        public string FileName(string path) => Path.GetFileName(Resolve(path));

        public string Extension(string path) => Path.GetExtension(Resolve(path));

        public IEnumerable<IResourceEntry> GetFolderContents(string path, bool recursive = false)
        {
            var results = new List<IResourceEntry>();
            var root = Resolve(path);
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) return results;

            try
            {
                foreach (var file in Directory.EnumerateFiles(root))
                    results.Add(new ResourceEntry(Path.GetFileName(file), file, isFile: true, isFolder: false));

                foreach (var dir in Directory.EnumerateDirectories(root))
                {
                    results.Add(new ResourceEntry(Path.GetFileName(dir), dir, isFile: false, isFolder: true));
                    if (recursive)
                        results.AddRange(GetFolderContents(dir, true));
                }

            }
            catch (UnauthorizedAccessException)
            {
                // Ignore directories we can't access
            }

            return results;
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
