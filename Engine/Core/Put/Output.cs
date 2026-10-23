using MiMFa.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiMFa.Engine.Put
{
    public class Output : Put
    {
        public List<string> Errors { get; } = new List<string>();

        public Output(string content = "", string source = null) : base(content, source)
        {
        }

        public Output Error(string message = "")
        {
            Errors.Add(message);
            return this;
        }
        public Output Error(Exception ex)
        {
            return Error(ex.Message);
        }
    }
}
