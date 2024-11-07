namespace Proyecto_LFA
{
    public class Token(string line) : Part(line)
    {
        public int number;
        private string expression = "";
        public Node? treeNode = null;
        public List<string> actions = [];
        private List<string> sets = [];

        // Propiedad pública Value para obtener el valor de la expresión
        public string Value
        {
            get { return expression; }
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
            bool foundBracket = false;
            string set = "";
            string action = "";

            tokens.Push("(");

            foreach (char c in expression)
            {
                if (c == ' ')
                {
                    if (!string.IsNullOrWhiteSpace(set))
                    {
                        tokens.Push(set);
                        sets.Add(set);
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

                if (c == '{' && !foundApostrophe)
                {
                    foundBracket = true;
                    continue;
                }
                else if (foundBracket && !foundApostrophe)
                {
                    if (c == '}')
                    {
                        actions.Add(action);
                        action = "";
                        continue;
                    }
                    action += c;
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

        public void checkThatSetsAndActionsExists(List<Set> sets, List<Action> actions)
        {

            foreach (var TokenSet in this.sets)
            {
                bool setExists = false;
                foreach(var set in sets)
                {
                    if (set.name.ToLower().Equals(TokenSet.ToLower()))
                        setExists = true;
                }

                if (!setExists)
                    throw new ArgumentException($"Didn't find set: {TokenSet}");
            }

            foreach (var TokenAction in this.actions)
            {
                bool actionExists = false;
                foreach (var action in actions)
                {
                    if (action.name.ToLower().Equals(TokenAction[..^2].ToLower()))
                        actionExists = true;
                }

                if (!actionExists)
                    throw new ArgumentException($"Didn't find action: {TokenAction}");
            }
        }

        public override string ToString()
        {
            return $"TOKEN {number} = {expression}";
        }
    }
}