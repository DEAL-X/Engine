using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
                basePath = string.IsNullOrEmpty(basePath) ? CurrentDirectory : basePath;
            else if (path.StartsWith("~" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                path = path.Substring(2);
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            else basePath = Directory.GetCurrentDirectory();
            return Path.GetFullPath(Path.Combine(basePath, path));
        }

        public string Combine(params string[] paths) => Path.Combine(paths ?? Array.Empty<string>());

        /// <summary>Script-friendly overload for JavaScript arrays marshalled by ClearScript as object[].</summary>
        public string Combine(object paths) => Combine(ToStringArray(paths));

        public virtual string ParentDirectory(string path) => Path.GetDirectoryName(Normalize(path));
        public virtual string DirectoryName(string path) => Path.GetDirectoryName(Normalize(path)).Split(Path.DirectorySeparatorChar).Last();
        public virtual string FileName(string path) => Path.GetFileName(Normalize(path));
        public virtual string Extension(string path) => Path.GetExtension(Normalize(path));

        public abstract IResourceEntry Entity(string path);
        public abstract bool Exists(string path);

        public abstract IEnumerable<string> GetContents(string path, bool recursive = false, Encoding encoding = null);
        public abstract IEnumerable<IResourceEntry> GetFolderContents(string path, bool recursive = false);
        public abstract void SetFolderContents(string path, params string[] files);

        /// <summary>Accepts JavaScript arrays without requiring a strongly typed string[].</summary>
        public virtual void SetFolderContents(string path, object files) =>
            SetFolderContents(path, ToStringArray(files));
        public abstract bool CreateDirectory(string path);

        public abstract byte[] GetFileContents(string path);
        public abstract void SetFileContents(string path, byte[] content);

        /// <summary>Accepts JavaScript arrays and converts their elements to bytes inside the host.</summary>
        public virtual void SetFileContents(string path, object content) =>
            SetFileContents(path, ToByteArray(content));

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
        /// <summary>Accepts JavaScript arrays marshalled as object[] by ClearScript.</summary>
        public virtual void SetFileLines(string path, object lines, Encoding encoding = null) =>
            SetFileLines(path, ToStringArray(lines), encoding);
        public virtual void PrependFileLines(string path, IEnumerable<string> lines, Encoding encoding = null)
        {
            PrependFileText(path, string.Join(Environment.NewLine, lines) + Environment.NewLine, encoding);
        }
        /// <summary>Accepts JavaScript arrays marshalled as object[] by ClearScript.</summary>
        public virtual void PrependFileLines(string path, object lines, Encoding encoding = null) =>
            PrependFileLines(path, ToStringArray(lines), encoding);
        public virtual void AppendFileLines(string path, IEnumerable<string> lines, Encoding encoding = null)
        {
            AppendFileText(path, string.Join(Environment.NewLine, lines) + Environment.NewLine, encoding);
        }
        /// <summary>Accepts JavaScript arrays marshalled as object[] by ClearScript.</summary>
        public virtual void AppendFileLines(string path, object lines, Encoding encoding = null) =>
            AppendFileLines(path, ToStringArray(lines), encoding);

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


        // Script-facing encoding overloads. JavaScript may supply an encoding name or
        // undefined instead of a System.Text.Encoding host object.
        public virtual IEnumerable<string> GetContents(string path, bool recursive, object encoding) =>
            GetContents(path, recursive, ToEncoding(encoding));
        public virtual string GetFileText(string path, object encoding) =>
            GetFileText(path, ToEncoding(encoding));
        public virtual void SetFileText(string path, string content, object encoding) =>
            SetFileText(path, content, ToEncoding(encoding));
        public virtual void PrependFileText(string path, string content, object encoding) =>
            PrependFileText(path, content, ToEncoding(encoding));
        public virtual void AppendFileText(string path, string content, object encoding) =>
            AppendFileText(path, content, ToEncoding(encoding));
        public virtual IEnumerable<string> GetFileLines(string path, object encoding) =>
            GetFileLines(path, ToEncoding(encoding));
        public virtual void SetFileLines(string path, object lines, object encoding) =>
            SetFileLines(path, lines, ToEncoding(encoding));
        public virtual void PrependFileLines(string path, object lines, object encoding) =>
            PrependFileLines(path, lines, ToEncoding(encoding));
        public virtual void AppendFileLines(string path, object lines, object encoding) =>
            AppendFileLines(path, lines, ToEncoding(encoding));
        public virtual string GetFileLine(string path, int index, object encoding) =>
            GetFileLine(path, index, ToEncoding(encoding));
        public virtual void SetFileLine(string path, int index, string line, object encoding) =>
            SetFileLine(path, index, line, ToEncoding(encoding));
        public virtual void PrependFileLine(string path, string line, object encoding) =>
            PrependFileLine(path, line, ToEncoding(encoding));
        public virtual void AppendFileLine(string path, string line, object encoding) =>
            AppendFileLine(path, line, ToEncoding(encoding));

        private static Encoding ToEncoding(object encoding)
        {
            if (encoding == null) return null;
            if (encoding is Encoding typedEncoding) return typedEncoding;
            if (encoding is string name)
                return string.IsNullOrWhiteSpace(name) ? null : Encoding.GetEncoding(name);

            string runtimeTypeName = encoding.GetType().Name;
            if (runtimeTypeName == "Undefined" || runtimeTypeName == "VoidResult")
                return null;

            if (encoding is IConvertible)
                return Encoding.GetEncoding(Convert.ToInt32(encoding, CultureInfo.InvariantCulture));

            throw new ArgumentException("Encoding must be a System.Text.Encoding, an encoding name, or a code page.", nameof(encoding));
        }

        private static IEnumerable<object> EnumerateValues(object values)
        {
            if (values == null) yield break;
            if (values is string)
            {
                yield return values;
                yield break;
            }
            if (values is IEnumerable sequence)
            {
                foreach (object value in sequence) yield return value;
                yield break;
            }
            yield return values;
        }

        private static string[] ToStringArray(object values) =>
            ToStringArray(EnumerateValues(values));

        private static string[] ToStringArray(IEnumerable<object> values) =>
            values.Select(value => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty).ToArray();

        private static byte[] ToByteArray(object values)
        {
            if (values == null) return Array.Empty<byte>();
            if (values is byte[] bytes) return bytes;
            if (values is string text) return Encoding.UTF8.GetBytes(text);
            return EnumerateValues(values)
                .Select(value => Convert.ToByte(value, CultureInfo.InvariantCulture))
                .ToArray();
        }

    }
}