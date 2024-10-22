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
        }

        private void determineFirstAndLast(Node? root)
        {
            if (root == null)
                return;

            // Recorre el subárbol izquierdo
            determineFirstAndLast(root.Left);
            // Recorre el subárbol derecho
            determineFirstAndLast(root.Right);
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
            writer.WriteLine(indent + (isRight ? "└── " : "┌── ") + node.Value);

            // Imprime el subárbol izquierdo
            PrintTreeToFile(node.Left, writer, indent + (isRight ? "    " : "│   "), false);
        }
    }
}
