using System;
using System.Collections.Generic;

namespace CalculatorParser
{
    ///<summary>
    /// This class uses a recursive descent parser to parse and calculate
    /// an arithmetic expression.  Additionally, it uses follow sets for
    /// format checking.
    ///
    /// It currently supports integer arithmetic, and the following operators:
    ///   +, -, *, /
    /// and grouping with ()
    ///</summary>
    public class Calculator
    {
        public static void Main(string[] args)
        {
            line = Console.ReadLine();
            index = 0;
            int result;
            bool correct = Parse(out result);
            Console.WriteLine("Correct Format: {0}", correct);
            Console.WriteLine("Result: {0}", result);
        }

        public static bool Parse(out int result)
        {
            NextSymbol();
            return Expression(out result);
        }

        #region Symbol Management

        public static char? symbol;
        public static string line;
        public static int index;

        public static bool NextSymbol()
        {
            while (index < line.Length && line[index] == ' ')
            {
                index++;
            }

            if (index == line.Length)
            {
                symbol = null;
                return false;
            }

            symbol = line[index];
            index++;
            return true;
        }

        public static bool Accept(char sym)
        {
            if (symbol == sym)
            {
                NextSymbol();
                return true;
            }
            return false;
        }

        public static bool SymbolInFollowSet(char? symbol, IEnumerable<char> followSet)
        {
            // '\n' is end of input
            char searchSymbol = symbol ?? '\n';
            foreach (char follow in followSet)
            {
                if (follow == searchSymbol)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Recursive Descent Parser
        
        // Implements the following grammar:
        // expression:
        //     addTerm

        // addTerm
        //     multTerm + addTerm
        //     multTerm - addTerm
        //     multTerm

        // multTerm:
        //     term * multTerm
        //     term / multTerm
        //     term

        // term
        //     number
        //     ( expression )

        // Follow Sets:
        // expression: $, )
        // addTerm: $, )
        // multTerm: +, -, $, )
        // term: *, /, +, -, $, )
        // number: *, /, +, -, $, )

        public static readonly List<char> ExpressionFollowSet = new List<char> {'\n', ')'};
        public static readonly List<char> AddTermFollowSet = new List<char> {'\n', ')'};
        public static readonly List<char> MultTermFollowSet = new List<char> {'+', '-', '\n', ')'};
        public static readonly List<char> TermFollowSet = new List<char> {'*', '/', '+', '-', '\n', ')'};
        public static readonly List<char> NumberFollowSet = new List<char> {'*', '/', '+', '-', '\n', ')'};


        public static bool Expression(out int result)
        {
            bool correct = AddTerm(0, "add", out result);
            if (!SymbolInFollowSet(symbol, ExpressionFollowSet))
            {
                result = 0;
                return false;
            }
            return correct;
        }

        public static bool AddTerm(int runningTotal, string op, out int result)
        {
            // Read the term
            int termValue;
            bool correct = MultTerm(1, "multiply", out termValue);
            if (!correct)
            {
                result = 0;
                return false;
            }

            // Add/Subtract to/from the running total
            if (op == "subtract")
            {
                runningTotal -= termValue;
            }
            else
            {
                runningTotal += termValue;
            }

            // Deal with the next element
            if (Accept('+'))
            {
                correct = AddTerm(runningTotal, "add", out result);
                if (!SymbolInFollowSet(symbol, AddTermFollowSet))
                {
                    result = 0;
                    return false;
                }
                return correct;    
            }
            else if (Accept('-'))
            {
                correct = AddTerm(runningTotal, "subtract", out result);
                if (!SymbolInFollowSet(symbol, AddTermFollowSet))
                {
                    result = 0;
                    return false;
                }
                return correct; 
            }
            else
            {
                result = runningTotal;
                if (!SymbolInFollowSet(symbol, AddTermFollowSet))
                {
                    result = 0;
                    return false;
                }
                return true;
            }
        }

        public static bool MultTerm(int runningTotal, string op, out int result)
        {
            // Read the term
            int termValue;
            bool correct = Term(out termValue);
            if (!correct)
            {
                result = 0;
                return false;
            }

            // Update the running total
            if (op == "divide")
            {
                if (termValue == 0)
                {
                    result = 0;
                    Console.WriteLine("Illegal Division By 0");
                    return false;
                }
                runningTotal /= termValue;
            }
            else
            {
                runningTotal *= termValue;
            }

            // Deal with the next element
            if (Accept('*'))
            {
                correct = MultTerm(runningTotal, "multiply", out result);
                if (!SymbolInFollowSet(symbol, MultTermFollowSet))
                {
                    result = 0;
                    return false;
                }
                return correct; 
            }
            else if (Accept('/'))
            {
                correct = MultTerm(runningTotal, "divide", out result);
                if (!SymbolInFollowSet(symbol, MultTermFollowSet))
                {
                    result = 0;
                    return false;
                }
                return correct;
            }
            else
            {
                result = runningTotal;
                if (!SymbolInFollowSet(symbol, MultTermFollowSet))
                {
                    result = 0;
                    return false;
                }
                return true;
            }
        }

        public static bool Term(out int result)
        {
            if (Accept('('))
            {
                // We have a nested expression
                bool correct = Expression(out result);
                return correct && Accept(')');
            }
            else
            {
                bool correct = Number(out result);
                if (!SymbolInFollowSet(symbol, TermFollowSet))
                {
                    result = 0;
                    return false;
                }
                return correct;
            }
        }

        public static bool Number(out int result)
        {
            result = 0;
            // Make sure the number starts with a digit.
            if (symbol == null || symbol < '0' || symbol > '9')
            {
                result = 0;
                return false;
            }

            while (symbol >= '0' && symbol <= '9')
            {
                result *= 10;
                result += ((char) symbol) - '0';
                NextSymbol();
            }

            if (!SymbolInFollowSet(symbol, NumberFollowSet))
            {
                result = 0;
                return false;
            }
            return true;
        }

        #endregion
    }
}
