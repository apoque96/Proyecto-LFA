namespace Proyecto_LFA
{
    public class Action(string line) : Part(line)
    {
        private string name = "";
        private Dictionary<int, string> reservedWords = new();

        public override void Validate(string line)
        {
            if (!line.StartsWith("RESERVADAS()"))
            {
                throw new ArgumentException("Expected 'RESERVADAS()'\n");
            }
            else
            {
                string actionContent = line.Substring("RESERVADAS()".Length).Trim();
                actionContent = actionContent.Replace("\n", "").Replace("\r", "").Trim();

                if (!actionContent.StartsWith("{") && !actionContent.StartsWith("}"))
                {
                    throw new ArgumentException("Expected '{' and '}' to enclose reserved words");
                }

                actionContent = actionContent.Substring(1, actionContent.Length - 2).Trim();
                string[] lines = actionContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                foreach (string reservedLine in lines)
                {
                    string[] parts = reservedLine.Split('=', 2, StringSplitOptions.TrimEntries);
                    if (parts.Length != 2 || !int.TryParse(parts[0], out int tokenId))
                        throw new ArgumentException($"Invalid reserved word format in line: {reservedLine}");

                    string identifier = parts[1].Trim('\'');

                    reservedWords[tokenId] = identifier;
                }
            }
        }

        public override string ToString()
        {
            string result = "RESERVADAS() {\n";

            foreach (var word in reservedWords)
            {
                result += $"    {word.Key} = '{word.Value}'\n";
            }

            result += "}";
            return result;
        }
    }
}

