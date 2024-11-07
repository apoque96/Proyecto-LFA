using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_LFA
{
    public class Error(string line) : Part(line)
    {
        public int num;

        public override void Validate(string line)
        {
            //Separates the name of the set and the elements of the set
            string[] arr = line.Split('=', 2, StringSplitOptions.TrimEntries);
            if (arr.Length == 0)
                throw new ArgumentException("Expected '='");

            if (arr[0] != "ERROR")
                throw new ArgumentException("Expected 'ERROR'");

            try
            {
                num = Int32.Parse(arr[1]);
            }catch (FormatException) 
            {
                throw new ArgumentException("Expected a number");
            }
        }

        public override string ToString()
        {
            return "ERROR = " + num;
        }
    }
}
