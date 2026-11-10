using System.Collections.Generic;
using System.Text;

namespace MiMFa.Engine.Resource
{
    public interface IResourceProvider
    {
        string DirectorySeparator { get; }

        string RootDirectory { get; set; }
        string TempDirectory { get; set; }

        bool IsAbsolute(string path);
        string Normalize(string path);
        string Resolve(string path, string basePath = null);
        string Combine(params string[] paths);

        IResourceEntry Entity(string path);
        bool Exists(string path);
        string ParentDirectory(string path);
        string DirectoryName(string path);
        string FileName(string path);
        string Extension(string path);

        IEnumerable<string> GetContents(string path, bool recursive = false, Encoding encoding = null);
        IEnumerable<IResourceEntry> GetFolderContents(string path, bool recursive = false);
        void SetFolderContents(string path, params string[] files);
        bool CreateDirectory(string path);

        byte[] GetFileContents(string path);
        void SetFileContents(string path, byte[] content);

        string GetFileText(string path, Encoding encoding = null);
        void SetFileText(string path, string text, Encoding encoding = null);
        void PrependFileText(string path, string text, Encoding encoding = null);
        void AppendFileText(string path, string text, Encoding encoding = null);

        IEnumerable<string> GetFileLines(string path, Encoding encoding = null);
        void SetFileLines(string path, IEnumerable<string> lines, Encoding encoding = null);
        void PrependFileLines(string path, IEnumerable<string> lines, Encoding encoding = null);
        void AppendFileLines(string path, IEnumerable<string> lines, Encoding encoding = null);

        void Move(string source, string destination, bool overwrite = false);
        void Copy(string source, string destination, bool overwrite = false);
        void Rename(string path, string newName, bool overwrite = false);
        void Delete(string path);
    }
}