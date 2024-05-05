using System;
using System.Collections.Generic;
using System.IO;

namespace PseudoScript
{
    class Program
    {

        static bool hadError = false;
        static bool hadRuntimeError = false;
        static Interpreter inter = new Interpreter();

        static void Main(string[] args)
        {
            if(args.Length > 1)
            {
                Console.WriteLine("Usage: pslang [file]");
            }
            else if(args.Length == 1)
            {
                if(!RunFile(args[0]))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: could not read file {args[0]}");
                    Console.ResetColor();
                }
            }
            else
            {
                RunPrompt();
            }
        }

        private static bool RunFile(string file)
        {
            try
            {
                string text = File.ReadAllText(file);
                Run(text);
                if(hadError)
                {
                    Environment.Exit(65);
                }
                else if(hadRuntimeError)
                {
                    Environment.Exit(70);
                }
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        private static void RunPrompt()
        {
            while(true)
            {
                Console.Write(">> ");
                string line = Console.ReadLine();
                if(line.Length == 0)
                {
                    break;
                }
                object value = Run(line);
                if(value != null)
                {
                    Console.WriteLine(inter.Stringify(value));
                }
                hadError = false;
                hadRuntimeError = false;
            }
        }

        private static object Run(string text)
        {
            Scanner scanner = new Scanner(text);
            LinkedList<Token> tokens = scanner.ScanTokens();
            Parser parser = new Parser(tokens);
            LinkedList<Stmt> program = parser.Parse();
            if(hadError)
            {
                return null;
            }
            return inter.Interpret(program);
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void Error(Token token, string message)
        {
            if(token.Type == TokenType.EOF)
            {
                Report(token.Line, "at end", message);
            }
            else
            {
                Report(token.Line, $" at column {token.Column} '{token.Lexeme}'", message);
            }
        }

        private static void Report(int line, string where, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error [line {line.ToString()}]{where}: {message}");
            hadError = true;
            Console.ResetColor();
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{error.Message}\n[line {error.Token.Line.ToString()}, offset {error.Token.Column.ToString()}]");
            hadRuntimeError = true;
            Console.ResetColor();
        }
    }
}
