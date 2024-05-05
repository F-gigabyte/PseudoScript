using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PseudoScript
{
    class Scanner
    {
        private string source;
        private LinkedList<Token> tokens;
        private int start;
        private int startLine;
        private int startCol;
        private int current;
        private int column;
        private int line;
        private bool catchNewLine;

        private static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>(
            new KeyValuePair<string, TokenType>[]
            { 
                new KeyValuePair<string, TokenType>("and", TokenType.And),
                new KeyValuePair<string, TokenType>("or", TokenType.Or),
                new KeyValuePair<string, TokenType>("not", TokenType.Not),
                new KeyValuePair<string, TokenType>("equals", TokenType.Equal),
                new KeyValuePair<string, TokenType>("unequals", TokenType.Unequal),
                new KeyValuePair<string, TokenType>("equal", TokenType.Equal),
                new KeyValuePair<string, TokenType>("unequal", TokenType.Unequal),
                new KeyValuePair<string, TokenType>("greater", TokenType.Greater),
                new KeyValuePair<string, TokenType>("less", TokenType.Less),
                new KeyValuePair<string, TokenType>("class", TokenType.Class),
                new KeyValuePair<string, TokenType>("if", TokenType.If),
                new KeyValuePair<string, TokenType>("begin", TokenType.Begin),
                new KeyValuePair<string, TokenType>("end", TokenType.End),
                new KeyValuePair<string, TokenType>("for", TokenType.For),
                new KeyValuePair<string, TokenType>("while", TokenType.While),
                new KeyValuePair<string, TokenType>("do", TokenType.Do),
                new KeyValuePair<string, TokenType>("true", TokenType.True),
                new KeyValuePair<string, TokenType>("false", TokenType.False),
                new KeyValuePair<string, TokenType>("else", TokenType.Else),
                new KeyValuePair<string, TokenType>("elif", TokenType.Elif),
                new KeyValuePair<string, TokenType>("then", TokenType.Then),
                new KeyValuePair<string, TokenType>("ret", TokenType.Ret),
                new KeyValuePair<string, TokenType>("print", TokenType.Print),
                new KeyValuePair<string, TokenType>("input", TokenType.Input),
                new KeyValuePair<string, TokenType>("par", TokenType.Par),
                new KeyValuePair<string, TokenType>("this", TokenType.This),
                new KeyValuePair<string, TokenType>("func", TokenType.Func),
                new KeyValuePair<string, TokenType>("than", TokenType.Than),
                new KeyValuePair<string, TokenType>("set", TokenType.Set),
                new KeyValuePair<string, TokenType>("to", TokenType.To),
                new KeyValuePair<string, TokenType>("?", TokenType.Null)
            });

        public Scanner(string source)
        {
            tokens = new LinkedList<Token>();
            this.source = source;
            start = 0;
            current = 0;
            line = 1;
            column = 1;
            catchNewLine = true;
        }

        public LinkedList<Token> ScanTokens()
        {
            while(!AtEnd())
            {
                start = current;
                startLine = line;
                startCol = column;
                ScanToken();
            }
            tokens.AddLast(new Token(TokenType.NewLine, "\n", null, line, 1));
            tokens.AddLast(new Token(TokenType.EOF, "", null, line + 1, 1));
            return tokens;
        }

        private bool AtEnd()
        {
            return current > source.Length - 1;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch(c)
            {
                case ('['):
                {
                    AddToken(TokenType.LeftSquare);
                    catchNewLine = false;
                    break;
                }
                case (']'):
                {
                    AddToken(TokenType.RightSquare);
                    catchNewLine = true;
                    break;
                }
                case ('('):
                {
                    AddToken(TokenType.LeftPar);
                    catchNewLine = false;
                    break;
                }
                case (')'):
                {
                    AddToken(TokenType.RightPar);
                    catchNewLine = true;
                    break;
                }
                case (','):
                {
                    AddToken(TokenType.Comma);
                    break;
                }
                case ('.'):
                {
                    AddToken(TokenType.Dot);
                    break;
                }
                case ('-'):
                {
                    AddToken(TokenType.Minus);
                    break;
                }
                case ('+'):
                {
                    AddToken(TokenType.Plus);
                    break;
                }
                case ('/'):
                {
                    if(Match('/'))
                    {
                        while(Peek() != '\n')
                        {
                            Advance();
                        }
                    }
                    else if(Match('*'))
                    {
                        bool haveStar = false;
                        while(Peek() != '/' && haveStar)
                        {
                            Advance();
                            if(Peek() == '*')
                            {
                                haveStar = true;
                            }
                            else
                            {
                                haveStar = false;
                            }
                        }
                    }
                    else
                    {
                        AddToken(TokenType.Slash);
                    }
                    break;
                }
                case ('*'):
                {
                    AddToken(TokenType.Star);
                    break;
                }
                case (':'):
                {
                    AddToken(TokenType.Colon);
                    break;
                }
                case ('<'):
                {
                    if(Match('-'))
                    {
                        AddToken(TokenType.LessDash);
                        break;
                    }
                    Program.Error(line, $"Unexpected character at line {line.ToString()}, offset {column.ToString()}");
                    break;
                }
                case ('?'):
                {
                    AddToken(TokenType.Null);
                    break;
                }
                case ('\"'):
                {
                    String();
                    break;
                }
                case (' '):
                case ('\r'):
                {
                    column++;
                    break;
                }
                case ('\t'):
                {
                    column+=4;
                    break;
                }
                case ('\n'):
                {
                    if(catchNewLine)
                    {
                        if(tokens.Count > 0 && tokens.Last.Value.Type != TokenType.NewLine)
                        {
                            AddToken(TokenType.NewLine);
                        }
                    }
                    line++;
                    column = 1;
                    break;
                }
                default:
                {
                    if(IsDigit(c))
                    {
                        Number();
                        break;
                    }
                    else if(IsAlpha(c))
                    {
                        Identifier();
                        break;
                    }
                    Program.Error(line, $"Unexpected character at line {line.ToString()}, offset {column.ToString()}: {c.ToString()}");
                    break;
                }
            }
        }

        private bool IsDigit(char c)
        {
            return '0' - 1 < c && c < '9' + 1;
        }

        private bool IsLetter(char c)
        {
            return ('a' - 1 < c && c < 'z' + 1) || ('A' - 1 < c && c < 'Z' + 1);
        }

        private bool IsAlpha(char c)
        {
            return c == '_' || IsLetter(c);
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private void Number()
        {
            bool floatingPoint = false;
            while(IsDigit(Peek()))
            {
                Advance();
            }
            if(Peek() == '.' && IsDigit(PeekNext()))
            {
                floatingPoint = true;
                Advance();
                while(IsDigit(Peek()))
                {
                    Advance();
                }
            }
            if(floatingPoint)
            {
                try
                {
                    AddToken(TokenType.Float, Double.Parse(StringFunc.RangedSubstr(source, start, current)));
                }
                catch(Exception)
                {
                    Program.Error(line, $"Overflow at line {line.ToString()}, offset {column.ToString()} '{StringFunc.RangedSubstr(source, start, current)}'");
                }
            }
            else
            {
                try
                {
                    AddToken(TokenType.Integer, Int64.Parse(StringFunc.RangedSubstr(source, start, current)));
                }
                catch(Exception)
                {
                    Program.Error(line, $"Overflow at line {line.ToString()}, offset {column.ToString()} '{StringFunc.RangedSubstr(source, start, current)}'");
                }
            }
        }

        private void Identifier()
        {
            while(IsAlphaNumeric(Peek()))
            {
                Advance();
            }
            string text = StringFunc.RangedSubstr(source, start, current);
            if(keywords.ContainsKey(text))
            {
                TokenType type = keywords[text];
                if(tokens.Count > 0 && type == TokenType.Equal)
                {
                    Token last = tokens.Last.Value;
                    switch(tokens.Last.Value.Type)
                    {
                        case (TokenType.Greater):
                        {
                            tokens.RemoveLast();
                            AddToken(last, TokenType.GreaterEqual);
                            break;
                        }
                        case (TokenType.Less):
                        {
                            tokens.RemoveLast();
                            AddToken(last, TokenType.LessEqual);
                            break;
                        }
                        case (TokenType.Not):
                        {
                            tokens.RemoveLast();
                            AddToken(last, TokenType.Unequal);
                            break;
                        }
                        default:
                        {
                            AddToken(type);
                            break;
                        }
                    }
                }
                else if(type == TokenType.Than)
                {
                    if(tokens.Count == 0)
                    {
                        Program.Error(line, $"than is reserved at line {line.ToString()}, offset {(start + 1).ToString()}");
                    }
                    else
                    {
                        Token last = tokens.Last.Value;
                        switch(tokens.Last.Value.Type)
                        {
                            case (TokenType.Greater):
                            {
                                tokens.RemoveLast();
                                AddToken(last, TokenType.Greater);
                                break;
                            }
                            case (TokenType.Less):
                            {
                                tokens.RemoveLast();
                                AddToken(last, TokenType.Less);
                                break;
                            }
                            default:
                            {
                                Program.Error(line, $"than is reserved at line {line.ToString()}, offset {(start + 1).ToString()}");
                                break;
                            }
                        }
                    }
                }
                else
                {
                    AddToken(type);
                }
            }
            else
            {
                AddToken(TokenType.Identifier);
            }
        }

        private char Advance()
        {
            if(current < source.Length)
            {
                return source[current++];
            }
            else
            {
                return '\0';
            }
        }

        private void AddToken(TokenType t)
        {
            AddToken(t, null);
        }

        private void AddToken(Token previous, TokenType t)
        {
            string text = previous.Lexeme + " " + StringFunc.RangedSubstr(source, start, current);
            tokens.AddLast(new Token(t, text, null, previous.Line, previous.Column));
        }

        private bool Match(char expected)
        {
            if(AtEnd())
            {
                return false;
            }
            if(source[current] != expected)
            {
                return false;
            }
            current++;
            return true;
        }

        private void String()
        {
            while(Peek() != '\"' && !AtEnd())
            {
                if(Peek() == '\n')
                {
                    line++;
                    column = 1;
                }
                Advance();
            }
            if(AtEnd())
            {
                Program.Error(line, $"Unterminated string at line {line.ToString()}, offset {column.ToString()}");
                return;
            }
            Advance();
            string value = StringFunc.RangedSubstr(source, start + 1, current - 1);
            AddToken(TokenType.String, value);
        }

        private char Peek()
        {
            if(AtEnd())
            {
                return '\0';
            }
            return source[current];
        }

        private char PeekNext()
        {
            if(current > source.Length - 1)
            {
                return '\0';
            }
            return source[current + 1];

        }

        private void AddToken(TokenType t, object literal)
        {
            string text = StringFunc.RangedSubstr(source, start, current);
            tokens.AddLast(new Token(t, text, literal, startLine, startCol));
            column += current - start + 1;
        }
    }
}
