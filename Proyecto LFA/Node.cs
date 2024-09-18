using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_LFA
{
    public class Node
    {
        public string Value { get; set; }
        public Node Left { get; set; }
        public Node Right { get; set; }

        public Node(string value)
        {
            Value = value;
            Left = null;
            Right = null;
        }

        public override string ToString()
        {
            return $"Node({Value})";
        }
    }
}
