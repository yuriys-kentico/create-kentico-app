using CMS.DocumentEngine;

namespace App.Boilerplate.Core.Routing
{
    public class Page
    {
        private readonly TreeNode node;

        public int DocumentID => node.DocumentID;

        public string DocumentName => node.DocumentName;

        public string NodeClassName => node.NodeClassName;

        public string NodeAliasPath => node.NodeAliasPath;

        public Page(TreeNode node)
        {
            this.node = node;
        }

        public object GetValue(string columnName) => node.GetValue(columnName);
    }
}