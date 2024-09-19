namespace Proyecto_LFA
{
    public class Action : Part
    {
        private Dictionary<int, string> reservedWords = new();

        public Action(string line) : base(line)
        {
            Validate(line);
        }

        public override void Validate(string line)
        {
            // Encontrar el contenido después de "RESERVADAS()"
            int startIndex = line.IndexOf("RESERVADAS()") + "RESERVADAS()".Length;
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
                {
                    throw new ArgumentException($"Invalid reserved word format in line: {trimmedLine}");
                }

                // Eliminar los apóstrofes alrededor del identificador
                string identifier = parts[1].Trim().Trim('\'');
                reservedWords[tokenId] = identifier;
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