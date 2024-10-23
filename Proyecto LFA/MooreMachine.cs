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
        private List<HashSet<int>> Follows {get; set;} = new List<HashSet<int>>();
        private static int count = 1;
        private static int leafCount = 0;

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
            //Node concat = new Node(".")
            //{
            //    Left = intersection,
            //    Right = new Node("#")
            //};
            //root = concat;
            root = intersection;
            determineFirstAndLast(root);
            determineFollows();

            Console.WriteLine("Símbolo\tFollow");
            for(int i = 0; i < Follows.Count; i++)
            {
                Console.WriteLine(i + 1 + "\t" + string.Join(",", Follows[i]));
            }
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
                leafCount++;
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

        private void determineFollows()
        {
            for (int i = 0; i < leafCount; i++)
            {
                Follows.Add([]);
            }

            helper(root);
        }

        //determineFollows helper function
        private void helper(Node? root)
        {
            if (root == null)
                return;
            if (root.Left == null && root.Right == null)
                return;

            // Recorre el subárbol izquierdo
            helper(root.Left);
            // Recorre el subárbol derecho
            helper(root.Right);

            if (root.Value == "." || root.Value == "+")
            {
                foreach(int lastC1 in root.Left.Lasts)
                {
                    foreach(int firstC2 in root.Right.Firsts)
                    {
                        Follows[lastC1-1].Add(firstC2-1);
                    }
                }
            }

            if (root.Value == "*")
            {
                foreach (int lastC1 in root.Left.Lasts)
                {
                    foreach (int firstC1 in root.Left.Firsts)
                    {
                        Follows[lastC1].Add(firstC1);
                    }
                }
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
