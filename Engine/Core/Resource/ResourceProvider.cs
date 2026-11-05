using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MiMFa.Engine.Resource
{
    public class ResourceProvider : ResourceBase
    {
        private static readonly StringComparison PathComparison =
            Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        public override IResourceEntry Entity(string path)
        {
            path = Resolve(path);
            return new ResourceEntry(Path.GetFileName(path), path, isFile: File.Exists(path), isFolder: Directory.Exists(path));
        }

        public override bool Exists(string path) =>
            path != null && (File.Exists(Resolve(path)) || Directory.Exists(Resolve(path)));

        public override IEnumerable<string> GetContents(string path, bool recursive = false, Encoding encoding = null)
        {
            path = Resolve(path);
            if (File.Exists(path)) yield return GetFileContents(path, encoding);
            else if (Directory.Exists(path))
                foreach (var item in GetFolderContents(path, recursive))
                    if (item.IsFile) yield return GetFileContents(item.Path, encoding);
        }

        public override IEnumerable<IResourceEntry> GetFolderContents(string path, bool recursive = false)
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
            catch (UnauthorizedAccessException) { }
            return results.Values;
        }

        public override void SetFolderContents(string path, params string[] files)
        {
            path = Resolve(path);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            foreach (var item in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                if (!files.Contains(item)) File.Delete(item);
            foreach (var item in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
                if (!files.Contains(item)) Directory.Delete(item, true);
            foreach (var file in files)
            {
                var dest = Resolve(file, path);
                var dir = Path.GetDirectoryName(dest);
                if (dest.StartsWith(path, PathComparison)) continue;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.Copy(file, dest, true);
            }
        }
        
        public override bool CreateDirectory(string path)
        {
            path = Resolve(path);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return Directory.Exists(path);
        }
        public override string GetFileContents(string path, Encoding encoding = null)
        {
            var p = Resolve(path);
            if (!File.Exists(p)) return null;
            return File.ReadAllText(p, encoding ?? Encoding.UTF8);
        }

        public override void SetFileContents(string path, string content)
        {
            path = Resolve(path);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, content ?? string.Empty, Encoding.UTF8);
        }

        public override void PrependFileContents(string path, string content)
        {
            path = Resolve(path);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, (content ?? string.Empty) + (File.Exists(path) ? File.ReadAllText(path) : string.Empty), Encoding.UTF8);
        }

        public override void AppendFileContents(string path, string content)
        {
            path = Resolve(path);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.AppendAllText(path, content ?? string.Empty, Encoding.UTF8);
        }

        public override void Move(string source, string destination, bool overwrite = false)
        {
            string from = Resolve(source), to = Resolve(destination);
            EnsureSource(from);
            EnsureDifferentPaths(from, to);
            EnsureNotChildDestination(from, to);
            PrepareDestination(to, overwrite);

            if (File.Exists(from))
            {
                EnsureParentDirectory(to);
                try { File.Move(from, to); }
                catch (IOException)
                {
                    File.Copy(from, to, false);
                    File.Delete(from);
                }
                return;
            }

            try { Directory.Move(from, to); }
            catch (IOException)
            {
                CopyDirectory(from, to);
                Directory.Delete(from, true);
            }
        }

        public override void Copy(string source, string destination, bool overwrite = false)
        {
            string from = Resolve(source), to = Resolve(destination);
            EnsureSource(from);
            EnsureDifferentPaths(from, to);
            EnsureNotChildDestination(from, to);
            PrepareDestination(to, overwrite);

            if (File.Exists(from))
            {
                EnsureParentDirectory(to);
                File.Copy(from, to, false);
            }
            else CopyDirectory(from, to);
        }

        public override void Rename(string path, string newName, bool overwrite = false)
        {
            if (string.IsNullOrWhiteSpace(newName) ||
                newName == "." || newName == ".." ||
                Path.IsPathRooted(newName) ||
                newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                newName.IndexOf('/') >= 0 || newName.IndexOf('\\') >= 0)
                throw new ArgumentException("A valid file or directory name (not a path) is required.", nameof(newName));

            string from = Resolve(path);
            EnsureSource(from);
            string parent = Path.GetDirectoryName(from);
            Move(from, Path.Combine(parent, newName), overwrite);
        }

        private static void EnsureSource(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
                throw new FileNotFoundException("The source resource does not exist.", path);
        }

        private static void EnsureDifferentPaths(string source, string destination)
        {
            if (string.Equals(source.TrimEnd(Path.DirectorySeparatorChar), destination.TrimEnd(Path.DirectorySeparatorChar), PathComparison))
                throw new IOException("The source and destination paths are identical.");
        }

        private static void EnsureNotChildDestination(string source, string destination)
        {
            if (!Directory.Exists(source)) return;
            string prefix = source.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (destination.StartsWith(prefix, PathComparison))
                throw new IOException("A directory cannot be copied or moved into itself.");
        }

        private static void PrepareDestination(string destination, bool overwrite)
        {
            if (!File.Exists(destination) && !Directory.Exists(destination)) return;
            if (!overwrite) throw new IOException("The destination already exists: " + destination);
            if (File.Exists(destination)) File.Delete(destination);
            else Directory.Delete(destination, true);
        }

        private static void EnsureParentDirectory(string path)
        {
            string parent = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);
        }

        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (string file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), false);
            foreach (string directory in Directory.GetDirectories(source))
                CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
        }
        
        public override void Delete(string path)
        {
            var p = Resolve(path);
            if (File.Exists(p)) File.Delete(p);
            else if (Directory.Exists(p)) Directory.Delete(p, true);
        }

    }
}