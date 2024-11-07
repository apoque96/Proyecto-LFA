using Proyecto_LFA;
using Action = Proyecto_LFA.Action;

bool flag = true;

while (flag)
{
    try
    {
        Console.WriteLine("Ingrese la ruta al archivo de la grámatica(ruta relativa)");
        string path = Console.ReadLine();

        (List<Set> sets, List<Token> tokens, List<Action> actions, List<Error> errors)
            = FileManager.ReadFile(path);

        //Crea la máquina de Moore
        MooreMachine mooreMachine = new(tokens);
        mooreMachine.displayMachine();

        Scanner scanner = new(mooreMachine, sets, tokens, actions, errors);

        while (true)
        {
            try
            {
                Console.WriteLine("Ingrese la linea de codigo");

                string str = Console.ReadLine();

                if (string.IsNullOrEmpty(str))
                {
                    flag = false;
                    break;
                }

                string[] expression = str.Split(' ');

                foreach (string s in expression)
                {
                    int result = scanner.getToken(s);
                    if(result < 0)
                    {
                        Console.WriteLine(s + " = " + errors[0].num);
                        continue;
                    }

                    Console.WriteLine(s + " = " + result);
                }
            }
            catch
            {
                break;
            }
        }
    }
    catch (Exception err)
    {
        Console.WriteLine(err.Message);
    }
}