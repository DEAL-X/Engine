using System;
using System.Linq;
using MiMFa.Engine.Core;

namespace MiMFa.Engine.Walker
{
    public class CodeWalker : WalkerBase<string>
    {
        private int line = 1;
        private int column = 1;

        public Position Position => new Position(this.Index, this.line, this.column);

        public CodeWalker(string content, string source = null) : base(content?.ToCharArray().Select(c=>c.ToString()).ToArray() ?? new string[0], source)
        {
        }

        public string PeekProcedure()
        {
            return string.Concat(PeekWhile(c => !string.IsNullOrWhiteSpace(c)).ToArray());
        }

        public override string Walk()
        {
            var ch = base.Walk();
            if (ch == "\n")
            {
                line++;
                column = 1;
            }
            else column++;
            return ch;
        }

        public string WalkProcedure()
        {
            return string.Join("", WalkWhile(c => !string.IsNullOrWhiteSpace(c)));
        }

        public string WalkToProcedure()
        {
            return string.Join("", WalkWhile(c => string.IsNullOrWhiteSpace(c)));
        }

        public CodeWalker MoveToProcedure()
        {
            WalkToProcedure();
            return this;
        }

        public bool StartsWith(string text)
        {
            for (int i = 0; i < text.Length; i++) if (Peek(i) != (text[i]+"")) return false;
            return true;
        }

        public string Remaining(int length = int.MaxValue)
        {
            var result = string.Empty;
            for (int i = Index; i < Math.Min(Index + length, Length); i++) result += Peek(i - Index);
            return result;
        }
    }
}
