﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Proyecto_LFA
{
    public class MooreMachine
    {
        private Node? root { get; set; }
        private List<HashSet<int>> Follows { get; set; } = new List<HashSet<int>>();
        private static int count = 1;
        private static int leafCount = 0;
        private List<Token> tokens;
        private Dictionary<(int, char), int> transitionTable = new Dictionary<(int, char), int>();

        private Dictionary<int, char> outputTable = new Dictionary<int, char>();


        public MooreMachine(List<Token> tokens)
        {
            this.tokens = tokens;
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

            //Console.WriteLine("Símbolo\tFollow");
            //for(int i = 0; i < Follows.Count; i++)
            //{
            //    Console.WriteLine(i + 1 + "\t" + string.Join(",", Follows[i]));
            //}
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

            if (root.Value == "." || root.Value == "+")
            {
                if (root.Left.nullable)
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
            FillTransitionTable();
        }

        private void FillTransitionTable()
        {
            FillOutputTable(); // Llama a este método para llenar la tabla de salidas

            foreach (var followSet in Follows.Select((follows, index) => new { follows, index }))
            {
                int currentState = followSet.index + 1;
                foreach (int nextState in followSet.follows)
                {
                    // Asumimos que el valor del token en el nodo es el símbolo de entrada
                    char token = GetTokenForState(currentState)?.Value[0] ?? throw new Exception("Token no encontrado.");

                    // Agregamos la transición a la tabla
                    transitionTable[(currentState, token)] = nextState;
                }
            }
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
                foreach (int lastC1 in root.Left.Lasts)
                {
                    foreach (int firstC2 in root.Right.Firsts)
                    {
                        Follows[lastC1-1].Add(firstC2);
                    }
                }
            }

            if (root.Value == "*")
            {
                foreach (int lastC1 in root.Left.Lasts)
                {
                    foreach (int firstC1 in root.Left.Firsts)
                    {
                        Follows[lastC1-1].Add(firstC1);
                    }
                }
            }
        }

        public int Transition(int currentState, char inputSymbol)
        {
            if (transitionTable.TryGetValue((currentState, inputSymbol), out int nextState))
            {
                return nextState;
            }
            else
            {
                throw new Exception("Transición no válida.");
            }
        }

        public Token? GetTokenForState(int state)
        {
            if (state > 0 && state <= leafCount)
            {
                var token = tokens.FirstOrDefault(t => t.treeNode.Firsts.Contains(state));
                if (token == null)
                {
                    Console.WriteLine($"Token no encontrado para el estado: {state}");
                }
                return token;
            }
            return null;
        }



        public void displayTransitionTable()
        {
            using (StreamWriter writer = new StreamWriter("TransitionTable.csv"))
            {
                writer.WriteLine("Estado Actual;Símbolo de Entrada;Estado Siguiente");

                foreach (var transition in transitionTable)
                {
                    var currentState = transition.Key.Item1;
                    var inputSymbol = transition.Key.Item2;
                    var nextState = transition.Value;
                    writer.WriteLine($"{currentState};{inputSymbol};{nextState}");
                }
            }

            Console.WriteLine("Saved transition table to TransitionTable.csv");
        }

        public void displayMachine()
        {
            using (StreamWriter writer = new StreamWriter("Moore Machine.txt"))
            {
                PrintTreeToFile(root, writer);
            }

            Console.WriteLine("Saved tree to Moore Machine.txt");

            SaveFirstAndLastToCSV("Firsts,Lasts&Follows.csv");
            Console.WriteLine("Saved Firsts and Lasts to Firsts&Lasts.csv");
        }

        private static void PrintTreeToFile(Node? node, StreamWriter writer, string indent = "", bool isRight = true)
        {
            if (node == null) return;

            // Imprime el subárbol derecho
            PrintTreeToFile(node.Right, writer, indent + (isRight ? "    " : "│   "), true);

            // Imprime el nodo actual
            writer.WriteLine(indent + (isRight ? "└── " : "┌── ") + string.Join(", ", node.Firsts) + "  " + node.Value + "  " + string.Join(", ", node.Lasts) + "  " + (node.nullable ? "N" : ""));

            // Imprime el subárbol izquierdo
            PrintTreeToFile(node.Left, writer, indent + (isRight ? "    " : "│   "), false);
        }

        private void SaveFirstAndLastToCSV(string filePath)
        {
            using (StreamWriter csvWriter = new StreamWriter(filePath))
            {

                csvWriter.WriteLine("TABLA DE FIRSTS Y LASTS");
                csvWriter.WriteLine("Node;Firsts;Lasts;Nullable");

                SaveNodeFirstAndLastToCSV(root, csvWriter);

                csvWriter.WriteLine();

                // Ahora escribimos la tabla de Follows
                csvWriter.WriteLine("TABLA DE FOLLOWS");
                csvWriter.WriteLine("Node;Follows");

                SaveNodeFollowsToCSV(csvWriter);
            }
        }

        private void SaveNodeFirstAndLastToCSV(Node? node, StreamWriter csvWriter)
        {
            if (node == null) return;

            csvWriter.WriteLine($"{node.Value};{string.Join(",", node.Firsts)};{string.Join(",", node.Lasts)};{(node.nullable ? "N" : " ")}");

            SaveNodeFirstAndLastToCSV(node.Left, csvWriter);
            SaveNodeFirstAndLastToCSV(node.Right, csvWriter);
        }
        private void SaveNodeFollowsToCSV(StreamWriter csvWriter)
        {
            for (int i = 0; i < Follows.Count; i++)
            {
                string follows = Follows[i].Count > 0 ? string.Join(",", Follows[i]) : " ";
                csvWriter.WriteLine($"{i + 1};{follows}");
            }
        }


        private void FillOutputTable()
        {
            FillOutputTableHelper(root);
        }

        private void FillOutputTableHelper(Node? node)
        {
            if (node == null) return;

            // Suponiendo que cada nodo hoja tiene un valor de salida
            if (node.Left == null && node.Right == null)
            {
                int state = node.Firsts.First(); // Asumiendo que el primer elemento representa el estado
                char output = node.Value[0]; // Suponiendo que el valor del nodo representa la salida
                outputTable[state] = output;
            }

            // Recorrer el subárbol izquierdo y derecho
            FillOutputTableHelper(node.Left);
            FillOutputTableHelper(node.Right);
        }


    }
}