using MiMFa.Engine.Model;
using MiMFa.Engine.Stage;
using MiMFa.Engine.Walker;
using System.Collections.Generic;
using System.Linq;

namespace MiMFa.Engine.Assembler
{
    public abstract class Assembler : StageBase
    {
        public override object Transform(object input, MiMFa.Engine.Engine compiler)
        {
            var walker = new NodeWalker((Node[])input, compiler?.Input?.Source);
            return new MiMFa.Engine.Model.Program(compiler?.Input?.Source, Assemble(walker, compiler).ToArray());
        }

        public virtual IEnumerable<Node> Assemble(NodeWalker walker, MiMFa.Engine.Engine compiler = null)
        {
            if (!Initialize(compiler)) yield break;
            while (!walker.IsEnded)
                yield return AssembleNode(walker);
        }

        protected virtual Node AssembleNode(NodeWalker walker)
        {
            return AssembleNode(walker.Walk(), walker);
        }
        protected virtual Node AssembleNode(Node node, NodeWalker walker)
        {
            Node lastNode = null;
            if (node.Is(TokenType.Statement))
                lastNode = AssembleStatementNode(node, walker);
            if (lastNode != null) return lastNode;

            if (node.Is(TokenType.Scope))
                lastNode = AssembleScopeNode(node, walker);
            if (lastNode != null) return lastNode;

            if (node.Is(TokenType.Data))
                 lastNode = AssembleDataNode(node, walker);
            if (lastNode != null) return lastNode;

            if (node.Is(TokenType.Symbol))
                 lastNode = AssembleSymbolNode(node, walker);
            if (lastNode != null) return lastNode;

            if (node.Is(TokenType.Keyword))
                 lastNode = AssembleKeywordNode(node, walker);
            if (lastNode != null) return lastNode;

            if (node.Is(TokenType.Comment))
                 lastNode = AssembleCommentNode(node, walker);
            if (lastNode != null) return lastNode;

            if (node.Is(TokenType.Start))
                 lastNode = AssembleStartNode(node, walker);
            if (lastNode != null) return lastNode;

            if (node.Is(TokenType.Prefix))
                 lastNode = AssemblePrefixNode(node, walker);
            if (lastNode != null) return lastNode;

            if (node.Is(TokenType.Middle))
                 lastNode = AssembleMiddleNode(node, walker);
            if (lastNode != null) return lastNode;

            if (node.Is(TokenType.Suffix))
                 lastNode = AssembleSuffixNode(node, walker);
            if (lastNode != null) return lastNode;

            if (node.Is(TokenType.End))
                lastNode = AssembleEndNode(node, walker);
            if (lastNode != null) return lastNode;

            if (node.Is(TokenType.None))
                 lastNode = node.Update(NodeType.None);
            if (lastNode != null) return lastNode;

            return AssembleUnknownNode(node, walker);
        }

        protected abstract Node AssembleStatementNode(Node node, NodeWalker walker);
        protected abstract Node AssembleScopeNode(Node node, NodeWalker walker);
        protected abstract Node AssembleDataNode(Node node, NodeWalker walker);
        protected abstract Node AssembleSymbolNode(Node node, NodeWalker walker);
        protected abstract Node AssembleKeywordNode(Node node, NodeWalker walker);
        protected abstract Node AssembleCommentNode(Node node, NodeWalker walker);
        protected abstract Node AssembleStartNode(Node node, NodeWalker walker);
        protected abstract Node AssemblePrefixNode(Node node, NodeWalker walker);
        protected abstract Node AssembleMiddleNode(Node node, NodeWalker walker);
        protected abstract Node AssembleSuffixNode(Node node, NodeWalker walker);
        protected abstract Node AssembleEndNode(Node node, NodeWalker walker);
        protected abstract Node AssembleUnknownNode(Node node, NodeWalker walker);
    }
}
