using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyExcelMauiLab1
{

    public abstract class AST
    {

    }

    public class BinOp : AST
    {
        public AST Left { get; }
        public Token Op { get; }
        public AST Right { get; }

        public BinOp(AST left, Token op, AST right)
        {
            this.Left = left;
            this.Op = op;
            this.Right = right;
        }
    }

    public class UnOp : AST         
    {
        public Token Op { get; }
        public AST Expr { get; }

        public UnOp(Token op, AST expr)
        {
            this.Op = op;
            this.Expr = expr;
        }
    }

    public class Num : AST
    {
        public Token token { get; }
        public int Value { get; }

        public Num(Token token)
        {
            this.token = token;
            this.Value = int.Parse(token.Value);
        }
    }

    public class Compound : AST
    {
        public List<AST>? children;
    }

    //public class Assign : AST
    //{
    //    public Var Left { get; }
    //    public Token Op { get; }
    //    public AST Right { get; }

    //    public Assign(Var left, Token op, AST right)
    //    {
    //        this.Left = left;
    //        this.Op = op;
    //        this.Right = right;
    //    }
    //}

    
    //public class Var : AST
    //{
    //    public Token token { get; }
    //    public string Name { get; }

    //    public Var(Token token)
    //    {
    //        this.token = token;
    //        this.Name = token.Value;
    //    }
    //}

    public class CellRef : AST
    {
        public Token token { get; }
        public string Name { get; }

        public CellRef(Token token)
        {
            this.token= token;
            this.Name = token.Value;
        }
    }

    public class Clamp : AST
    {
        public Token token { get; }
        public List<AST> Arguments { get; }

        public Clamp(Token token, List<AST> Arguments)
        {
            this.token = token;
            this.Arguments = Arguments;
        }
    }

    public class NoOp : AST
    {

    }

    public class Parser
    {
        public List<string> SeenCellRefs = new();

        private Token current_token = new Token(TokenType.EOF, "EOF");
        private Lexer lexer;

        public HashSet<TokenType> additiveOps = new HashSet<TokenType>
        {
            TokenType.PLUS,
            TokenType.MINUS,
        };
        public HashSet<TokenType> multiplicativeOps = new HashSet<TokenType>
        {
            TokenType.MULTIPLY,
            TokenType.DIVIDE,
            TokenType.EXPONENTIATION
        };


        public Parser(Lexer inputLexer)
        {
            lexer = inputLexer;
            current_token = inputLexer.getNextToken();
        }

        private Exception error()
        {
            throw new Exception("Incorect input");
        }

        public AST Parse()
        {
            AST node = program();
            return node;
        }

        private void eat(TokenType tokenType)
        {
            if (current_token.Type == tokenType)
            {
                current_token = lexer.getNextToken();
            }
            else
            {
                error();
            }
        }

        private AST program()
        {
            //AST node = assingment_statement();
            AST node = expr();
            return node;
        }

        private AST? statement()    
        {
            return null;
        }

        //private AST assingment_statement()
        //{
        //    Var left = variable();
        //    Token token = current_token;
        //    eat(TokenType.ASSIGN);
        //    AST right = expr();
        //    AST node = new Assign(left, token, right);
        //    return node;
        //}

        private CellRef cellReference()
        {
            //CellRef node = new CellRef(current_token);
            
            SeenCellRefs.Add(current_token.Value);

            return new CellRef(current_token);
        }

        //private Var variable()
        //{
        //    Var node = new Var(current_token);
        //    eat(TokenType.ID);
        //    return node;
        //}

        private AST empty()
        {
            return new NoOp();
        }

        private AST MaxMin()
        {
            Token token = current_token;
            eat(token.Type);

            eat(TokenType.LPAREN);
            AST arg1 = expr();
            eat(TokenType.COMMA);
            AST arg2 = expr();

            eat(TokenType.RPAREN);

            return new Clamp(token, new List<AST> { arg1, arg2 });
        }

        private AST factor()
        {
            Token token = current_token;
            if (token.Type == TokenType.INC)
            {
                eat(TokenType.INC);
                return new UnOp(token, factor());
            }
            else if (current_token.Type == TokenType.DEC)
            {
                eat(TokenType.DEC);
                return new UnOp(token, factor());
            }
            else if (current_token.Type == TokenType.INTEGER)
            {
                eat(TokenType.INTEGER);
                return new Num(token);
            }
            else if (current_token.Type == TokenType.LPAREN)
            {
                eat(TokenType.LPAREN);
                AST node = expr();
                eat(TokenType.RPAREN);
                return node;
            }
            /*else if (current_token.Type == TokenType.ID)
            {
                AST node = variable();
                return node;
            }*/
            else if (current_token.Type == TokenType.CELLREFERENCE)
            {
                AST node = cellReference();
                eat(TokenType.CELLREFERENCE);
                return node;
            }
            else if (current_token.Type == TokenType.MAX || current_token.Type == TokenType.MIN)
            {
                return MaxMin();
            }
            return empty();
        }

        public AST term()
        {
            AST node = factor();

            while (multiplicativeOps.Contains(current_token.Type))
            {
                Token token = current_token;

                if (token.Type == TokenType.MULTIPLY)
                {
                    eat(token.Type);
                }
                else if (token.Type == TokenType.DIVIDE)
                {
                    eat(token.Type);
                }
                else if (token.Type == TokenType.EXPONENTIATION)
                {
                    eat(token.Type);
                }
                node = new BinOp(node, token, factor());
            }
            return node;
        }

        public AST expr()
        {
            AST node = term();

            while (additiveOps.Contains(current_token.Type))
            {
                Token token = current_token;

                if (token.Type == TokenType.PLUS)
                {
                    eat(token.Type);
                }
                else if (token.Type == TokenType.MINUS)
                {
                    eat(token.Type);
                }
                node = new BinOp(node, token, term());
            }
            return node;
        }


    }
}
