﻿namespace Proyecto_LFA
{
    public class Token : Part
    {
        public int number;
        private string expression = "";
        public Node? treeNode = null;

        // Propiedad pública Value para obtener el valor de la expresión
        public string Value
        {
            get { return expression; }
        }

        // Constructor de la clase
        public Token(string line) : base(line)
        {
            Validate(line);
        }

        public override void Validate(string line)
        {
            // Separates the line into "TOKEN", number, and expression
            string[] arr = line.Split('=', 2, StringSplitOptions.TrimEntries);
            if (arr.Length != 2 || !arr[0].StartsWith("TOKEN"))
                throw new ArgumentException("Expected format: TOKEN <number> = <expression>\n");

            // Extract the number after the word TOKEN
            string[] tokenParts = arr[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokenParts.Length != 2 || !int.TryParse(tokenParts[1], out number))
                throw new ArgumentException("Expected format: TOKEN <number>\n");

            this.expression = arr[1];

            // Validate the expression (basic validation, it can be extended)
            ValidateExpression(this.expression);
        }

        private void ValidateExpression(string expression)
        {
            Stack<string> tokens = new Stack<string>();
            bool foundApostrophe = false;
            bool foundInsideApostrophe = false;
            string set = "";

            tokens.Push("(");

            foreach (char c in expression)
            {
                if (c == ' ')
                {
                    if (!string.IsNullOrWhiteSpace(set))
                    {
                        tokens.Push(set);
                        set = "";
                        tokens.Push(".");
                    }
                    continue;
                }

                if (c == '\'')
                {
                    if (!foundInsideApostrophe && foundApostrophe)
                    {
                        tokens.Push("\'");
                        tokens.Push(".");
                        foundInsideApostrophe = true;
                        continue;
                    }
                    foundInsideApostrophe = false;
                    foundApostrophe = !foundApostrophe;
                    continue;
                }

                if (foundApostrophe)
                {
                    tokens.Push("'" + c.ToString() + "'");
                    foundInsideApostrophe = true;
                    tokens.Push(".");
                }
                else if (IsOperator(c) && string.IsNullOrWhiteSpace(set))
                {
                    if (c != '(' && tokens.Peek() != ")")
                        tokens.Pop();
                    tokens.Push(c.ToString());
                    if (c != '|' && c != '(' && tokens.Peek() != ")")
                        tokens.Push(".");
                }
                else
                {
                    set += c;
                }
            }

            if (!string.IsNullOrWhiteSpace(set))
            {
                tokens.Push(set);
                set = "";
                tokens.Push(".");
            }

            if (tokens.Peek() == ".")
            {
                tokens.Pop();
            }

            tokens.Push(")");
            tokens.Push(".");
            tokens.Push("#T" + this.number);

            if (foundApostrophe)
            {
                throw new ArgumentException("Expected ' on " + expression);
            }

            Tree tree = new(tokens);
            this.treeNode = tree.BuildTree();
        }

        // Método que verifica si un carácter es un operador
        private bool IsOperator(char element)
        {
            return new HashSet<char> { '+', '*', '?', '|', '(', ')' }.Contains(element);
        }

        public override string ToString()
        {
            return $"TOKEN {number} = {expression}";
        }
    }
}