using System;

namespace MiMFa.Engine.Put
{
    public abstract class Put
    {
        public string Content { get; set; }

        public string Source { get; set; }

        public Put(string content = "", string source = null)
        {
            Content = content;
            Source = source;
        }
    }
}
