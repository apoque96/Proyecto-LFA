using System.Text;

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
            List<Set> sets = new();
            List<Token> tokens = new();
            List<Action> actions = new();
            List<Error> errors = new();

            string line;
            string error_string;
            int line_index = 0;
            bool found_reserved = false;
            bool found_set = false;
            PartType part = PartType.none;

            using StreamReader sr = new(path);

            line = sr.ReadLine();

            while (line != null)
            {
                line_index++;
                error_string = $"{{line {line_index}}}: ";

                if (string.IsNullOrWhiteSpace(line))
                {
                    line = sr.ReadLine();
                    continue;
                }

                line = line.Trim();
                line = line.ReplaceLineEndings();

                // Detecting ERROR lines
                if (line.StartsWith("ERROR"))
                {
                    try
                    {
                        Error err = new(line);
                        errors.Add(err);
                        line = sr.ReadLine()!;
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"{error_string} Error processing ERROR section: {ex.Message}");
                    }
                    continue;
                }

                switch (line)
                {
                    case "SETS":
                        part = PartType.set;
                        found_set = true;
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
                                try
                                {
                                    Set set = new(line);
                                    sets.Add(set);
                                }
                                catch (Exception ex)
                                {
                                    throw new ArgumentException($"{error_string} Error processing SETS section: {ex.Message}");
                                }
                                break;
                            case PartType.token:
                                try
                                {
                                    Token token = new(line);
                                    tokens.Add(token);
                                }
                                catch (Exception ex)
                                {
                                    throw new ArgumentException($"{error_string} Error processing TOKENS section: {ex.Message}");
                                }
                                break;
                            case PartType.action:
                                // Read the full block of ACTIONS
                                while (line.EndsWith("()"))
                                {
                                    if (line.StartsWith("RESERVADAS()"))
                                        found_reserved = true;
                                    // We need to read the entire block for RESERVADAS()
                                    StringBuilder actionContent = new();
                                    actionContent.AppendLine(line);

                                    line = sr.ReadLine();
                                    while (line != null && !line.Trim().EndsWith("}"))
                                    {
                                        actionContent.AppendLine(line);
                                        line = sr.ReadLine();
                                    }

                                    if (line != null)
                                    {
                                        actionContent.AppendLine(line);
                                    }

                                    // Create Action object and add to the list
                                    try
                                    {
                                        Action action = new(actionContent.ToString());
                                        actions.Add(action);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new ArgumentException($"{error_string} Error processing ACTIONS section: {ex.Message}");
                                    }
                                }
                                break;
                            default:
                                throw new ArgumentException($"{error_string} Expected 'SETS', 'TOKENS', 'ACTIONS' or 'ERROR'");
                        }
                        break;
                }

                line = sr.ReadLine();
            }

            if (tokens.Count < 1)
                throw new ArgumentException("Didn't find TOKENS");
            if (actions.Count < 1)
                throw new ArgumentException("Didn't find ACTIONS");
            if (errors.Count < 1)
                throw new ArgumentException("Didn't find ERROR");
            if (found_set && sets.Count < 1)
                throw new ArgumentException("Expected at least one SET");
            if (!found_reserved)
                throw new ArgumentException("Didn't find RESERVADAS in ACTIONS");

            foreach (var token in tokens)
            {
                token.checkThatSetsAndActionsExists(sets, actions);
            }

            return (sets, tokens, actions, errors);
        }
    }
}