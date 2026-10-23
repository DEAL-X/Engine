using System.Collections.Generic;

namespace MiMFa.Engine.Resource
{
    public interface IResourceProvider
    {
        bool IsAbsolute(string path);
        string Normalize(string path);
        string Resolve(string path, string basePath = null);
        string Combine(params string[] paths);

        IResourceEntry Entity(string path);
        bool Exists(string path);
        string DirectoryName(string path);
        string FileName(string path);
        string Extension(string path);

        IEnumerable<string> GetContents(string path, bool recursive = false, System.Text.Encoding encoding = null);
        IEnumerable<IResourceEntry> GetFolderContents(string path, bool recursive = false);
        void SetFolderContents(string path, params string[] files);
        string GetFileContents(string path, System.Text.Encoding encoding);
        void SetFileContents(string path, string content);
        void PrependFileContents(string path, string content);
        void AppendFileContents(string path, string content);
        void Delete(string path);
    }
}
