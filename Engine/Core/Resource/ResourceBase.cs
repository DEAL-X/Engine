using System;
using System.Collections.Generic;
using System.Text;

namespace MiMFa.Engine.Resource
{
    public class ResourceBase : IResourceProvider
    {
        public IResourceEntry Entity(string path) => throw new NotSupportedException();
        public virtual bool Exists(string path) => throw new NotSupportedException("The compiler does not support file systems in this provider.");
        public virtual bool IsAbsolute(string path) => throw new NotSupportedException();
        public virtual string Normalize(string path) => throw new NotSupportedException();
        public virtual string Resolve(string path, string basePath = null) => throw new NotSupportedException();
        public virtual string Combine(params string[] paths) => throw new NotSupportedException();
        public virtual string DirectoryName(string path) => throw new NotSupportedException();
        public virtual string FileName(string path) => throw new NotSupportedException();
        public virtual string Extension(string path) => throw new NotSupportedException();
        public IEnumerable<string> GetContents(string path, bool recursive = false, Encoding encoding = null) => throw new NotSupportedException();
        public virtual IEnumerable<IResourceEntry> GetFolderContents(string path, bool recursive = false) => throw new NotSupportedException();
        public virtual void SetFolderContents(string path, params string[] files) => throw new NotSupportedException();
        public virtual string GetFileContents(string path, System.Text.Encoding encoding = null) => throw new NotSupportedException();
        public virtual void SetFileContents(string path, string content) => throw new NotSupportedException();
        public virtual void PrependFileContents(string path, string content) => throw new NotSupportedException();
        public virtual void AppendFileContents(string path, string content) => throw new NotSupportedException();
        public virtual void Delete(string path) => throw new NotSupportedException();

    }
}
