using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slt
{
    public class Subparser
    {
        public enum SubparserState
        {
            Ready,
            WaitingText,
        }

        public SubparserState State;
        public int Tabs => Brackets.Count;
        public StringBuilder CurrentInput = new StringBuilder();
        internal Stack<char> Brackets = new Stack<char>();

        private bool IsOpenBracket(char c)
        {
            return c == '[' || c == '(' || c == '{';
        }
        private bool IsClosingBracket(char c)
        {
            return c == ']' || c == ')' || c == '}';
        }
        private char GetClosingBracket(char c)
        {
            switch (c)
            {
                case '[': return ']';
                case '(': return ')';
                case '{': return '}';
            }
            throw new ArgumentException();
        }

        internal void InternalParse(string s)
        {
            bool next_escape = false;
            bool in_string = false;

            for (var i = 0; i < s.Length; i++)
            {
                if (in_string)
                {
                    if (next_escape)
                    {
                        next_escape = false;
                        continue;
                    }
                    else
                    {
                        if (s[i] == '\\')
                        {
                            next_escape = true;
                            continue;
                        }
                        else if (s[i] == '"')
                        {
                            in_string = false;
                            continue;
                        }
                    }
                }
                else
                {
                    if (IsOpenBracket(s[i]))
                    {
                        Brackets.Push(s[i]);
                        continue;
                    }
                    else if (IsClosingBracket(s[i]))
                    {
                        if (Brackets.Count == 0) throw new FormatException("Closing bracket found");
                        if (GetClosingBracket(Brackets.Peek()) != s[i]) throw new FormatException("Wrong closing bracket");
                        Brackets.Pop();
                        continue;
                    }
                    else if (s[i] == '"')
                    {
                        in_string = true;
                        continue;
                    }
                }
            }

            CurrentInput.Append(s);
        }

        public void Clear()
        {
            Brackets.Clear();
            CurrentInput.Clear();
            State = SubparserState.Ready;
        }

        public SubparserState ParseNew(string s)
        {
            Clear();
            return Parse(s);
        }

        public SubparserState Parse(string s)
        {
            InternalParse(s);
            State = Brackets.Count == 0 ? SubparserState.Ready : SubparserState.WaitingText;
            return State;
        }
    }
}
