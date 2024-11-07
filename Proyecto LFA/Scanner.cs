using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_LFA
{
    public class Scanner
    {
        private MooreMachine mooreMachine { get; set; }
        private List<Set> sets { get; set; }
        private List<Token> tokens { get; set; }
        private List<Action> actions { get; set; }
        private List<Error> errors { get; set; }

        public Scanner(MooreMachine mooreMachine, List<Set> sets, List<Token> tokens, List<Action> actions, List<Error> errors)
        {
            this.mooreMachine = mooreMachine;
            this.sets = sets;
            this.tokens = tokens;
            this.actions = actions;
            this.errors = errors;
        }

        public int getToken(string str)
        {
            Dictionary<HashSet<int>, Dictionary<string, HashSet<int>>> transitions = mooreMachine.transitions;
            HashSet<int> currentState = mooreMachine.initialState;

            foreach (var c in str)
            {
                string character = $"'{c}'";
                if (transitions[currentState].ContainsKey(character))
                {
                    currentState = transitions[currentState][character];
                }
                else
                {
                    bool movedState = false;
                    foreach (Set set in sets)
                    {
                        if (transitions[currentState].ContainsKey(set.name) && set.elements.Contains(c))
                        {
                            currentState = transitions[currentState][set.name];
                            movedState = true;
                            break;
                        }
                    }
                    if (!movedState)
                        return -1;
                }
            }

            string s = transitions[currentState].Keys.ToList().Find(c => c.Contains("#T"));

            try
            {
                int tokenNumber = int.Parse(s?[2..]);

                //Revisa si el token representa una action
                List<string>? tokenActions = tokens?.Find(t => t.number == tokenNumber)?.actions;
                Action? action = null;
                foreach(string tokenAction in tokenActions)
                {
                    var temp = actions.Find(a => a.name.ToLower().Equals(tokenAction[..^2].ToLower()));
                    if(temp != null)
                    {
                        action = temp;
                        break;
                    }
                }

                if(action != null)
                {
                    if (action.reservedWords.ContainsKey(str.ToLower()))
                    {
                        tokenNumber = action.reservedWords[str.ToLower()];
                    }
                }

                return tokenNumber;
            }
            catch
            {
                return -1;    
            }
        }
    }
}
