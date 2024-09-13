using Proyecto_LFA;
using Action = Proyecto_LFA.Action;

try
{
    (List<Set> sets, List<Token> tokens, List<Action> actions, List<Error> errors)
        = FileManager.ReadFile("../../../../Testing/GRAMATICA.txt");

    Console.WriteLine("Sets:");
    foreach (Set set in sets)
        Console.WriteLine(set.ToString());

    Console.WriteLine("Tokens:");
    foreach (Token token in tokens)
        Console.WriteLine(token.ToString());

    Console.WriteLine("Actions:");
    foreach (Action action in actions)
        Console.WriteLine(action.ToString());

    Console.WriteLine("Errors:");
    foreach (Error err in errors)
        Console.WriteLine(err.ToString());
}
catch (Exception err)
{
    Console.WriteLine("Error: " + err.ToString());
}