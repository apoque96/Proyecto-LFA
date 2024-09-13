namespace Proyecto_LFA
{
    public class FileManager
    {
        public FileManager() { }

        enum PartType
        {
            set,
            token,
            action,
            none
        }

        public static (List<Set>, List<Token>, List<Action>, List<Error>) ReadFile(string path)
        {
            List<Set> sets = [];
            List<Token> tokens = [];
            List<Action> actions = [];
            List<Error> errors = [];

            string line;

            PartType part = PartType.none;

            StreamReader sr = new(path);

            line = sr.ReadLine();

            while (line != null)
            {
                if (line.Length == 0)
                {
                    line = sr.ReadLine();
                    continue;
                }

                line = line.Trim();
                line = line.ReplaceLineEndings();

                //Used for finding ERROR
                string[] separateEqualSign = line.Split('=', 2, StringSplitOptions.TrimEntries);
                if (separateEqualSign[0] == "ERROR")
                {
                    Error err = new(line);
                    errors.Add(err);
                    line = sr.ReadLine()!;
                    continue;
                }

                switch (line)
                {
                    case "SETS":
                        part = PartType.set;
                        break;
                    case "TOKENS":
                        part = PartType.token;
                        break;
                    case "ACTIONS":
                        part = PartType.action;
                        break;
                    default:
                        switch (part)
                        {
                            case PartType.set:
                                Set set = new(line);
                                sets.Add(set);
                                break;
                            case PartType.token:
                                Token token = new(line);
                                tokens.Add(token);
                                break;
                            case PartType.action:
                                Action action = new(line);
                                actions.Add(action);
                                break;
                            default:
                                throw new ArgumentException("Expected 'SETS', 'TOKENS', 'ACTIONS' or 'ERROR'");
                        }
                        break;
                }

                line = sr.ReadLine();
            }

            sr.Close();

            if(tokens.Count < 1)
                throw new ArgumentException("Didn't find TOKENS");
            if(actions.Count < 1)
                throw new ArgumentException("Didn't find ACTIONS");
            if (errors.Count < 1)
                throw new ArgumentException("Didn't find ERROR");

            return (sets, tokens, actions, errors);
        }
    }
}
