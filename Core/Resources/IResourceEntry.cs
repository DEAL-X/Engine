namespace MiMFa.Core.Resources
{
    public interface IResourceEntry
    {
         string Path { get; }
         bool IsFile { get; }
         bool IsFolder { get; }
         string FullName { get; }
        string Name { get; }
        string Extension { get; }
        string Directory { get; }
        string Root { get; }
    }
}
