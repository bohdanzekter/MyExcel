using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MyExcelMauiLab1
{
    public enum TokenType
    {
        INTEGER,
        EOF,            // end of file
        PLUS,
        MINUS,
        MULTIPLY,
        DIVIDE,
        LPAREN,
        RPAREN,
        EXPONENTIATION,
        COMMA,
        INC,
        DEC,
        MAX,
        MIN,

        //ID,
        CELLREFERENCE,
        ASSIGN,
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }

        public Token(TokenType type, string value)
        {
            this.Type = type;
            this.Value = value;
        }
    }

    public class Lexer
    {
        private string text { get; set; }
        public int pos = 0;
        private char current_char;
        private TokenType lastTokenType = TokenType.EOF;

        public Lexer(string input)
        {
            text = input;
            current_char = text[pos];
        }

        public void changeText(string text)
        {
            this.text = text;
        }

        private Exception error()
        {
            throw new Exception("Invalid character");
        }

        private void advance()
        {
            pos++;
            if (pos > text.Length - 1)
            {
                current_char = '\0';
            }
            else
            {
                current_char = text[pos];
            }
        }

        private void skipWhiteSpace()
        {
            while (current_char != '\0' && Char.IsWhiteSpace(current_char))
            {
                advance();
            }
        }

        private int integer()
        {
            string num = "";
            while (current_char != '\0' && Char.IsDigit(current_char))
            {
                num += current_char;
                advance();
            }
            return int.Parse(num);
        }

        private char peek()
        {
            int peek_pos = pos + 1;
            if (peek_pos > text.Length - 1)
            {
                return '\0';
            }
            return text[peek_pos];
        }

        //private Token _id()
        //{
        //    string result = "";
        //    while (current_char != '\0' && Char.IsLetterOrDigit(current_char))
        //    {
        //        result += current_char;
        //        advance();
        //    }
        //    return new Token(TokenType.ID, result);
        //}

        private Token cellRef()
        {

            string cellref = "";

            while (current_char != '\0' && char.IsLetter(current_char))
            {
                cellref += current_char;
                advance();
            }
            while (current_char != '\0' && char.IsDigit(current_char))
            {
                cellref += current_char;
                advance();
            }

            return new Token(TokenType.CELLREFERENCE, cellref);
        }

        public Token getNextToken()
        {
            while (current_char != '\0')
            {
                if (Char.IsWhiteSpace(current_char))
                {
                    skipWhiteSpace();
                    continue;
                }
                //else if (Char.IsLetter(current_char))
                //{
                //    advance();
                //    return _id();
                //}
                else if (Char.IsDigit(current_char))
                {
                    string value = integer().ToString();
                    lastTokenType = TokenType.INTEGER;
                    return new Token(TokenType.INTEGER, value);
                }
                else if (current_char == '+')
                {
                    advance();
                    if (current_char == '+' && CanBePrefixIncrement())
                    {
                        advance();
                        lastTokenType = TokenType.INC;
                        return new Token(TokenType.INC, "INC");
                    }
                    lastTokenType = TokenType.PLUS;
                    return new Token(TokenType.PLUS, "PLUS");
                }
                else if (current_char == '-')
                {
                    advance();
                    if (current_char == '-' && CanBePrefixDecrement())
                    {
                        advance();
                        lastTokenType = TokenType.DEC;
                        return new Token(TokenType.DEC, "DEC");
                    }
                    lastTokenType = TokenType.MINUS;
                    return new Token(TokenType.MINUS, "MINUS");
                }
                else if (current_char == '*')
                {
                    advance();
                    lastTokenType = TokenType.MULTIPLY;
                    return new Token(TokenType.MULTIPLY, "MULTIPLY");
                }
                else if (current_char == '/')
                {
                    advance();
                    lastTokenType = TokenType.DIVIDE;
                    return new Token(TokenType.DIVIDE, "DIVIDE");
                }
                else if (current_char == '(')
                {
                    advance();
                    lastTokenType = TokenType.LPAREN;
                    return new Token(TokenType.LPAREN, "LPAREN");
                }
                else if (current_char == ')')
                {
                    advance();
                    lastTokenType = TokenType.RPAREN;
                    return new Token(TokenType.RPAREN, "RPAREN");
                }
                else if (current_char == '^')
                {
                    advance();
                    lastTokenType = TokenType.EXPONENTIATION;
                    return new Token(TokenType.EXPONENTIATION, "EXPONENTIATION");
                }
                else if (current_char == '=')
                {
                    advance();
                    lastTokenType = TokenType.ASSIGN;
                    return new Token(TokenType.ASSIGN, "ASSIGN");
                }
                else if (current_char == ',')
                {
                    advance();
                    lastTokenType = TokenType.COMMA;
                    return new Token(TokenType.COMMA, "COMMA");
                }
                else if ((current_char == 'M' || current_char == 'm')
                        && (peek() == 'A' || peek() == 'a'))
                {
                    advance();
                    advance();
                    if (current_char == 'X' || current_char == 'x')
                    {
                        advance();
                        lastTokenType = TokenType.MAX;
                        return new Token(TokenType.MAX, "MAX");
                    }
                }
                else if ((current_char == 'M' || current_char == 'm')
                        && (peek() == 'I' || peek() == 'i'))
                {
                    advance();
                    advance();
                    if (current_char == 'N' || current_char == 'n')
                    {
                        advance();
                        lastTokenType = TokenType.MIN;
                        return new Token(TokenType.MIN, "MIN");
                    }
                }
                else if (Char.IsLetter(current_char))
                {
                    Token token = cellRef();
                    lastTokenType = TokenType.CELLREFERENCE;
                    return token;
                }
                throw error();
            }
            return new Token(TokenType.EOF, "EOF");


            throw new Exception("Incorect input");
        }

        private bool CanBePrefixIncrement()
        {
            return lastTokenType == TokenType.PLUS || lastTokenType == TokenType.MINUS ||
                   lastTokenType == TokenType.MULTIPLY || lastTokenType == TokenType.DIVIDE ||
                   lastTokenType == TokenType.EXPONENTIATION || lastTokenType == TokenType.LPAREN ||
                   lastTokenType == TokenType.COMMA || lastTokenType == TokenType.ASSIGN ||
                   lastTokenType == TokenType.INC || lastTokenType == TokenType.DEC ||
                   lastTokenType == TokenType.EOF;
        }

        private bool CanBePrefixDecrement()
        {
            return lastTokenType == TokenType.PLUS || lastTokenType == TokenType.MINUS ||
                   lastTokenType == TokenType.MULTIPLY || lastTokenType == TokenType.DIVIDE ||
                   lastTokenType == TokenType.EXPONENTIATION || lastTokenType == TokenType.LPAREN ||
                   lastTokenType == TokenType.COMMA || lastTokenType == TokenType.ASSIGN ||
                   lastTokenType == TokenType.INC || lastTokenType == TokenType.DEC ||
                   lastTokenType == TokenType.EOF;
        }

    }
}
