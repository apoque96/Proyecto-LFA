﻿using System.Text;

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
            bool found_reserved = false;
            bool found_set = false;
            PartType part = PartType.none;

            using StreamReader sr = new(path);

            line = sr.ReadLine();

            while (line != null)
            {
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
                    Error err = new(line);
                    errors.Add(err);
                    line = sr.ReadLine()!;
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
                                Set set = new(line);
                                sets.Add(set);
                                break;
                            case PartType.token:
                                Token token = new(line);
                                tokens.Add(token);
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
                                        throw new ArgumentException($"Error processing ACTIONS section: {ex.Message}");
                                    }
                                }
                                break;
                            default:
                                throw new ArgumentException("Expected 'SETS', 'TOKENS', 'ACTIONS' or 'ERROR'");
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

            return (sets, tokens, actions, errors);
        }
    }
}