using System;
using System.Collections.Generic;
using System.IO; // For StreamWriter
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_LFA
{
    public class MooreMachine
    {
        private Node? root { get; set; }
        private List<HashSet<int>> Follows { get; set; } = new List<HashSet<int>>();
        private static int count = 1;
        private static int leafCount = 0;

        public MooreMachine(List<Token> tokens)
        {
            buildMachine(tokens);
        }

        private void buildMachine(List<Token> tokens)
        {
            Node? intersection = null;
            // Build the syntax tree by combining tokens with an OR ("|") operator
            for (int i = 0; i < tokens.Count; i++)
            {
                Node? left = intersection ?? tokens[i++].treeNode;
                if (i >= tokens.Count)
                    break;
                Node? right = tokens[i].treeNode;
                intersection = new Node("|")
                {
                    Left = left,
                    Right = right,
                };
            }

            root = intersection;
            determineFirstAndLast(root);
            determineFollows();
        }

        private void determineFirstAndLast(Node? root)
        {
            if (root == null)
                return;

            // Traverse the left subtree
            determineFirstAndLast(root.Left);
            // Traverse the right subtree
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
                Follows.Add(new HashSet<int>());
            }

            helper(root);
        }

        // Helper function for determineFollows
        private void helper(Node? root)
        {
            if (root == null)
                return;
            if (root.Left == null && root.Right == null)
                return;

            // Traverse the left subtree
            helper(root.Left);
            // Traverse the right subtree
            helper(root.Right);

            if (root.Value == "." || root.Value == "+")
            {
                foreach (int lastC1 in root.Left.Lasts)
                {
                    foreach (int firstC2 in root.Right.Firsts)
                    {
                        Follows[lastC1 - 1].Add(firstC2);
                    }
                }
            }

            if (root.Value == "*")
            {
                foreach (int lastC1 in root.Left.Lasts)
                {
                    foreach (int firstC1 in root.Left.Firsts)
                    {
                        Follows[lastC1 - 1].Add(firstC1);
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

            SaveFirstAndLastToCSV("Firsts,Lasts&Follows.csv");
            Console.WriteLine("Saved Firsts and Lasts to Firsts&Lasts.csv");

            // Collect leaves
            var leaves = CollectLeaves(root);

            // Calculate transitions
            var transitions = CalculateTransitions(Follows, root, leaves);

            // Save transitions to CSV
            SaveTransitionsToCSV(transitions, "Transitions.csv");
            Console.WriteLine("Saved transitions to Transitions.csv");
        }

        private static void PrintTreeToFile(Node? node, StreamWriter writer, string indent = "", bool isRight = true)
        {
            if (node == null) return;

            // Print the right subtree
            PrintTreeToFile(node.Right, writer, indent + (isRight ? "    " : "│   "), true);

            // Print the current node
            writer.WriteLine(indent + (isRight ? "└── " : "┌── ") + string.Join(", ", node.Firsts) + "  " + node.Value + "  " + string.Join(", ", node.Lasts) + "  " + (node.nullable ? "N" : ""));

            // Print the left subtree
            PrintTreeToFile(node.Left, writer, indent + (isRight ? "    " : "│   "), false);
        }

        private void SaveFirstAndLastToCSV(string filePath)
        {
            using (StreamWriter csvWriter = new StreamWriter(filePath))
            {

                csvWriter.WriteLine("TABLA DE FIRSTS Y LASTS");
                csvWriter.WriteLine("Node;Firsts;Lasts;");

                SaveNodeFirstAndLastToCSV(root, csvWriter);

                csvWriter.WriteLine();

                // Now write the Follows table
                csvWriter.WriteLine("TABLA DE FOLLOWS");
                csvWriter.WriteLine("Node;Follows");

                SaveNodeFollowsToCSV(csvWriter);
            }
        }

        private void SaveNodeFirstAndLastToCSV(Node? node, StreamWriter csvWriter)
        {
            if (node == null) return;

            csvWriter.WriteLine($"{node.Value};{string.Join(",", node.Firsts)};{string.Join(",", node.Lasts)}");

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

        // New method to collect leaves
        private Dictionary<string, HashSet<int>> CollectLeaves(Node node)
        {
            var leaves = new Dictionary<string, HashSet<int>>();
            CollectLeavesHelper(node, leaves);
            return leaves;
        }

        private void CollectLeavesHelper(Node node, Dictionary<string, HashSet<int>> leaves)
        {
            if (node == null) return;

            if (node.Left == null && node.Right == null)
            {
                // This is a leaf node
                int position = node.Firsts.First();
                string symbol = node.Value;

                if (!leaves.ContainsKey(symbol))
                {
                    leaves[symbol] = new HashSet<int>();
                }
                leaves[symbol].Add(position);
            }
            else
            {
                if (node.Left != null)
                    CollectLeavesHelper(node.Left, leaves);
                if (node.Right != null)
                    CollectLeavesHelper(node.Right, leaves);
            }
        }

        // New method to calculate transitions
        private Dictionary<HashSet<int>, Dictionary<string, HashSet<int>>> CalculateTransitions(List<HashSet<int>> followers, Node root, Dictionary<string, HashSet<int>> leaves)
        {
            var transitions = new Dictionary<HashSet<int>, Dictionary<string, HashSet<int>>>(new HashSetEqualityComparer<int>());
            var processedStates = new List<HashSet<int>>();
            var pendingStates = new Queue<HashSet<int>>();
            pendingStates.Enqueue(root.Firsts);

            while (pendingStates.Count > 0)
            {
                var currentState = pendingStates.Dequeue();
                processedStates.Add(currentState);
                var stateTransitions = new Dictionary<string, HashSet<int>>();

                foreach (var number in currentState)
                {
                    // Find the symbol for this number
                    foreach (var kvp in leaves)
                    {
                        string symbol = kvp.Key;
                        var numbers = kvp.Value;
                        if (numbers.Contains(number))
                        {
                            if (!stateTransitions.ContainsKey(symbol))
                            {
                                stateTransitions[symbol] = new HashSet<int>();
                            }
                            if (number - 1 < followers.Count)
                            {
                                stateTransitions[symbol].UnionWith(followers[number - 1]);
                            }
                        }
                    }
                }

                transitions[currentState] = stateTransitions;

                foreach (var nextState in stateTransitions.Values)
                {
                    if (!ContainsState(processedStates, nextState) && !ContainsState(pendingStates.ToList(), nextState))
                    {
                        pendingStates.Enqueue(nextState);
                    }
                }
            }

            return transitions;
        }

        private bool ContainsState(List<HashSet<int>> stateList, HashSet<int> state)
        {
            foreach (var s in stateList)
            {
                if (s.SetEquals(state))
                    return true;
            }
            return false;
        }

        // New method to save transitions to CSV
        private void SaveTransitionsToCSV(Dictionary<HashSet<int>, Dictionary<string, HashSet<int>>> transitions, string filePath)
        {
            using (StreamWriter csvWriter = new StreamWriter(filePath))
            {
                // Header row with all symbols
                var allSymbols = transitions.Values.SelectMany(dict => dict.Keys).Distinct().ToList();
                csvWriter.Write("ESTADOS;");
                csvWriter.WriteLine(string.Join(";", allSymbols));

                // Write transitions for each state
                foreach (var state in transitions.Keys)
                {
                    // Write the current state (formatted with curly braces)
                    csvWriter.Write($"{{{string.Join(", ", state)}}};");

                    // Write the transitions for each symbol
                    foreach (var symbol in allSymbols)
                    {
                        if (transitions[state].ContainsKey(symbol))
                        {
                            var nextState = transitions[state][symbol];
                            csvWriter.Write($"{{{string.Join(", ", nextState)}}};");
                        }
                        else
                        {
                            csvWriter.Write(";"); // Empty transition
                        }
                    }

                    csvWriter.WriteLine(); // End of the row
                }
            }
        }
    }

    // Custom equality comparer for HashSet<int>
    public class HashSetEqualityComparer<T> : IEqualityComparer<HashSet<T>>
    {
        public bool Equals(HashSet<T>? x, HashSet<T>? y)
        {
            if (x == null || y == null)
                return false;
            return x.SetEquals(y);
        }

        public int GetHashCode(HashSet<T> obj)
        {
            int hash = 0;
            foreach (T t in obj)
                hash ^= t!.GetHashCode();
            return hash;
        }
    }
}