using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_LFA
{
    public class MooreMachine
    {
        private Node? root {get; set;}
        private static int count = 1;

        public MooreMachine(List<Token> tokens) {
            buildMachine(tokens);
        }

        private void buildMachine(List<Token> tokens)
        {
            Node? intersection = null;
            // Creamos un nodo con OR "|" y se le asigna los tokens como hijo
            // izquierdo y derecho
            for (int i = 0; i < tokens.Count; i++)
            {
                Node? left = intersection ?? tokens[i++].treeNode;
                Node? right = tokens[i].treeNode;
                intersection = new Node("|")
                {
                    Left = left,
                    Right = right,
                };
            }
            root = intersection;
            determineFirstAndLast(root);
        }

        private void determineFirstAndLast(Node? root)
        {
            if (root == null)
                return;

            // Recorre el subárbol izquierdo
            determineFirstAndLast(root.Left);
            // Recorre el subárbol derecho
            determineFirstAndLast(root.Right);

            if (root.Left == null && root.Right == null)
            {
                root.Firsts.Add(count);
                root.Lasts.Add(count++);
                root.nullable = false;
                return;
            }

            if (root.Value == "|" || root.Value == "?")
            {
                HashSet<int>? concatenatedFirsts = new HashSet<int>(root.Left.Firsts);
                concatenatedFirsts.UnionWith(root.Right.Firsts);

                HashSet<int>? concatenatedLasts = new HashSet<int>(root.Left.Lasts);
                concatenatedLasts.UnionWith(root.Right.Lasts);

                root.Firsts = concatenatedFirsts;
                root.Lasts = concatenatedLasts;

                root.nullable = root.Left.nullable || root.Right.nullable;
                return;
            }

            if( root.Value == "." || root.Value == "+")
            {
                if(root.Left.nullable)
                {
                    HashSet<int>? concatenatedFirsts = new HashSet<int>(root.Left.Firsts);
                    concatenatedFirsts.UnionWith(root.Right.Firsts);
                    root.Firsts = concatenatedFirsts;
                }
                else
                {
                    root.Firsts = root.Left.Firsts;
                }

                if (root.Right.nullable)
                {
                    HashSet<int>? concatenatedLasts = new HashSet<int>(root.Left.Lasts);
                    concatenatedLasts.UnionWith(root.Right.Lasts);
                    root.Lasts = concatenatedLasts;
                }
                else
                {
                    root.Lasts = root.Right.Lasts;
                }

                root.nullable = root.Left.nullable && root.Right.nullable;
                return;
            } 
            if (root.Value == "*")
            {
                root.Firsts = root.Left.Firsts;
                root.Lasts = root.Left.Lasts;
                root.nullable = true;
                return;
            }
        }

        public void displayMachine()
        {
            using (StreamWriter writer = new StreamWriter("Moore Machine.txt"))
            {
                PrintTreeToFile(root, writer);
            }

            Console.WriteLine("Saved tree to Moore Machine.txt");
        }

        private static void PrintTreeToFile(Node? node, StreamWriter writer, string indent = "", bool isRight = true)
        {
            if (node == null) return;

            // Imprime el subárbol derecho
            PrintTreeToFile(node.Right, writer, indent + (isRight ? "    " : "│   "), true);

            // Imprime el nodo actual
            writer.WriteLine(indent + (isRight ? "└── " : "┌── ") + string.Join(", ", node.Firsts) + "  " + node.Value + "  " + string.Join(", ", node.Lasts) + "  " + (node.nullable ? "N": ""));

            // Imprime el subárbol izquierdo
            PrintTreeToFile(node.Left, writer, indent + (isRight ? "    " : "│   "), false);
        }
    }
}
