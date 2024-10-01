namespace Proyecto_LFA
{
    public class Set(string line) : Part(line)
    {
        private string name = "";
        private List<char> elements = [];

        public override void Validate(string line)
        {
            //Separates the name of the set and the elements of the set
            string[] arr = line.Split('=', 2, StringSplitOptions.TrimEntries);
            if (arr.Length < 2)
                throw new ArgumentException("Expected '='");
            
            this.name = arr[0];

            //LETRA = 'A'..'Z' + 'a'..'z' + '_'
            //DIGITO = '0'..'9'
            //CHARSET = CHR(32)..CHR(254)

            string[] elements = arr[1].Split("+");

            foreach (string element in elements)
            {
                if (string.IsNullOrWhiteSpace(element))
                    throw new ArgumentException("Expected a character");

                string modifiedElement = ToChar(element);

                //Separetes via ".."
                string[] separation = modifiedElement.Split("..");
                if(separation.Length > 2)
                {
                    throw new ArgumentException("Expected '+'");
                }

                if (separation.Length == 2)
                {
                    if (string.IsNullOrWhiteSpace(separation[0]))
                        throw new ArgumentException("Expected a character before '..'");
                    else if (string.IsNullOrWhiteSpace(separation[1]))
                        throw new ArgumentException("Expected a character after '..'");

                    char starting;
                    char final;

                    //' in ascii is 39
                    if (separation[0][0] != 39 || separation[0][2] != 39)
                    {
                        throw new ArgumentException("Expected \"'\"");
                    }

                    starting = separation[0][1];

                    if (separation[1][0] != 39 || separation[1][2] != 39)
                    {
                        throw new ArgumentException("Expected \"'\"");
                    }

                    final = separation[1][1];

                    for (int i = starting; i <= final; i++)
                    {
                        
                        this.elements.Add((char)i);
                    }
                }
                else
                {
                    //' in ascii is 39
                    if (modifiedElement[0] != 39 || modifiedElement[2] != 39)
                        throw new ArgumentException("Expected \"'\"");
                    this.elements.Add(modifiedElement[1]);
                }
            }
            //throw new NotImplementedException();
        }

        //Turns all the CHR codes to their respective character
        private static string ToChar(string element)
        {
            while(element.Contains("CHR"))
            {
                int index = element.IndexOf("CHR");

                if (element[index+3] != '(')
                {
                    throw new ArgumentException("Expected '(' after CHR\n");
                }

                bool foundEndParenthesis = false;
                string code = "";
                for (int i = index+4; i < element.Length; i++)
                {
                    if (element[i] == ')')
                    {
                        foundEndParenthesis = true;
                        break;
                    }
                    try
                    {
                        code += Int32.Parse(element[i].ToString()).ToString();
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException("Expected a number in CHR");
                    }
                }

                if (!foundEndParenthesis)
                    throw new ArgumentException("Expected ')' after CHR");

                //Copied it from ChatGpt Xd
                int value = int.Parse(code);

                char character = (char)value;

                element = element.Replace("CHR(" + code + ")", 
                    "'" + character + "'");

            }

            return element;
        }

        public override string ToString()
        {
            string ans = this.name + " = {";

            for (int i = 0; i < this.elements.Count - 1; i++)
            {
                ans += "\"" + this.elements[i].ToString() + "\"";
                ans += ",";
            }

            ans += "\"" + this.elements[^1].ToString() + "\"";
            ans += "}";
            return ans;
        }
    }
}
