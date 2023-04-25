using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLDocumentLibrary
{
    public class QueryBuilder
    {
        private StringBuilder _query;
        public QueryBuilder()
        {
            _query = new StringBuilder();
        }

        /// <summary>
        /// Constructor which specified initial node and also defines that is the root node or not
        /// </summary>
        /// <param name="initialNode">Name of the initial node</param>
        /// <param name="root">If true, the initial node is XML root</param>
        public QueryBuilder(string initialNode, bool root = true)
        {
            _query = new StringBuilder((root ? "/" : "//") + initialNode);
        }

        /// <summary>
        /// Goes to the node with specified name
        /// </summary>
        /// <param name="node">Name of the node which we want to go to</param>
        /// <returns>QueryBuilder object</returns>
        public QueryBuilder GoTo(string node)
        {
            _query.Append('/').Append(node);
            return this;
        }

        /// <summary>
        /// Specifies the attribute of the last inserted node
        /// </summary>
        /// <param name="attributeName">Name of the attribute</param>
        /// <param name="attributeValue">Optional attribut value</param>
        /// <returns>QueryBuilder object</returns>
        public QueryBuilder WithAttribute(string attributeName, string? attributeValue = null)
        {
            if (attributeName == null) _query.Append($"[@{attributeName}]");
            else _query.Append($"[@{attributeName}={attributeValue}]");
            return this;
        }

        /// <summary>
        /// Specifies the position of the node in XML document
        /// </summary>
        /// <param name="position">Index of the node (indexed from 1)</param>
        /// <returns>QueryBuilder object</returns>
        public QueryBuilder At(int position)
        {
            _query.Append($"[{position}]");
            return this;
        }

        /// <summary>
        /// Returns string with XQuery expression
        /// </summary>
        /// <returns>String with xQuery</returns>
        public override string ToString()
        {
            return _query.ToString();
        }
    }
}
