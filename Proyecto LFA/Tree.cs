using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_LFA
{
    public class Tree
    {
        private Stack<Node> S = new Stack<Node>();  // Pila de árboles
        private Stack<string> T = new Stack<string>();  // Pila de tokens

        public Tree(Stack<string> s)
        {
            Stack<string> tokens = new Stack<string>();

            while (s.Count > 0)
            {
                tokens.Push(s.Pop());
            }

            _ = this.BuildTree(tokens);
        }

        public Node BuildTree(Stack<string> tokens)
        {
            while (tokens.Count > 0)
            {
                var token = tokens.Pop();

                if (IsTerminal(token))
                {
                    // Paso 3: Convertir st en árbol y hacer push a S
                    S.Push(new Node(token));
                }
                else if (token == "(")
                {
                    // Paso 4: Hacer push a T con token "("
                    T.Push(token);
                }
                else if (token == ")")
                {
                    // Paso 5: Procesar el cierre de paréntesis
                    while (T.Count > 0 && T.Peek() != "(")
                    {
                        if (S.Count < 2)
                            throw new ArgumentException("Faltan operandos.");

                        string op = T.Pop();
                        Node right = S.Pop();
                        Node left = S.Pop();
                        Node newNode = new Node(op)
                        {
                            Left = left,
                            Right = right
                        };
                        S.Push(newNode);
                    }

                    if (T.Count == 0 || T.Pop() != "(")
                        throw new ArgumentException("Error de sintaxis: falta '('");
                }
                else if (IsOperator(token))
                {
                    // Paso 6: Procesar los operadores
                    while (T.Count > 0 && T.Peek() != "(" && Precedence(token) <= Precedence(T.Peek()))
                    {
                        string op = T.Pop();
                        if (S.Count < 2)
                            throw new ArgumentException("Faltan operandos.");

                        Node right = S.Pop();
                        Node left = S.Pop();
                        Node newNode = new Node(op)
                        {
                            Left = left,
                            Right = right
                        };
                        S.Push(newNode);
                    }
                    T.Push(token);
                }
                else
                {
                    // Paso 7: Token no reconocido
                    throw new ArgumentException($"Token no reconocido: {token}");
                }
            }

            // Paso 9: Procesar los tokens restantes en T
            while (T.Count > 0)
            {
                string op = T.Pop();
                if (op == "(")
                    throw new ArgumentException("Falta un operador.");

                if (S.Count < 2)
                    throw new ArgumentException("Faltan operandos.");

                Node right = S.Pop();
                Node left = S.Pop();
                Node newNode = new Node(op)
                {
                    Left = left,
                    Right = right
                };
                S.Push(newNode);
            }

            // Paso 11: Verificar que solo haya un árbol en S
            if (S.Count != 1)
                throw new ArgumentException("Faltan operandos.");

            return S.Pop();
        }

        private bool IsTerminal(string token)
        {
            // Verifica si el token es un símbolo terminal
            return !IsOperator(token) && token != "(" && token != ")";
        }

        private bool IsOperator(string token)
        {
            // Verifica si el token es un operador
            return new HashSet<string> { "+", "*", "?", "|" }.Contains(token);
        }

        private int Precedence(string token)
        {
            // Define la precedencia de operadores
            switch (token)
            {
                case "*": case "?": return 3;
                case "+": return 2;
                case "|": return 1;
                default: return 0;
            }
        }
    }
}
