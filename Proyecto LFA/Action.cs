namespace Proyecto_LFA
{
    public class Action(string line) : Part(line)
    {
        private Dictionary<int, string> reservedWords = new();
        private String name = "";

        public override void Validate(string line)
        {
            // Encontrar el contenido después de la declaración de la función
            int startIndex = line.IndexOf("()");

            if (startIndex == 0 && line[0] == '(')
                throw new ArgumentException("Expected function identifier");
            else if (startIndex < 0)
                throw new ArgumentException("Expected brackets for function '()'");

            for (int i = 0; i < startIndex; i++)
            {
                name += line[i];
            }

            startIndex += 2;

            string actionContent = line.Substring(startIndex).Trim();

            // Verificar que el contenido esté rodeado por llaves
            if (actionContent.Length < 2 || !actionContent.StartsWith("{") || !actionContent.EndsWith("}"))
                throw new ArgumentException("Expected '{' and '}' to enclose reserved words");

            // Quitar las llaves y obtener el contenido
            actionContent = actionContent.Substring(1, actionContent.Length - 2).Trim();

            // Separar el contenido en líneas
            string[] lines = actionContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string reservedLine in lines)
            {
                // Asegurarse de eliminar los espacios alrededor del texto
                string trimmedLine = reservedLine.Trim();

                // Validar el formato: número = 'IDENTIFICADOR'
                string[] parts = trimmedLine.Split('=', 2, StringSplitOptions.TrimEntries);
                if (parts.Length != 2 || !int.TryParse(parts[0].Trim(), out int tokenId))
                    throw new ArgumentException($"Invalid reserved word format in line: {trimmedLine}");
                else if (parts[1][0] != '\'' || parts[1][^1] != '\'')
                    throw new ArgumentException(
                        $"Expected token identifier to be enclosed with apostrophe in line: {trimmedLine}");

                // Eliminar los apóstrofes alrededor del identificador
                string identifier = parts[1].Trim().Trim('\'');
                reservedWords[tokenId] = identifier;
            }
        }

        public override string ToString()
        {
            string result =  name + "() {\n";
            foreach (var word in reservedWords)
            {
                result += $"    {word.Key} = '{word.Value}'\n";
            }
            result += "}";
            return result;
        }
    }
}