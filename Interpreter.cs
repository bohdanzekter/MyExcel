using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MyExcelMauiLab1
{
    public class Interpreter
    {
        private readonly Parser parser;
        private SpreadsheetLogic spreadSheet;

        public Dictionary<string, int> Variables { get; } = new Dictionary<string, int>();


        public Interpreter(Parser parser, SpreadsheetLogic spreadSheet)
        {
            this.parser = parser;
            this.spreadSheet = spreadSheet;
        }

        public int Interpret()
        {
            AST tree = parser.Parse();
            return Visit(tree);
        }

        private int Visit(AST node)
        {
            return VisitNode((dynamic)node);
        }

        private int VisitNode(Num node)
        {
            return node.Value;
        }

        private int VisitNode(Clamp node)
        {
            TokenType tokType = node.token.Type;
            List<int> argValues = new List<int>();
            foreach (var arg in node.Arguments)
            {
                argValues.Add(Visit(arg));
            }

            if (tokType == TokenType.MAX)
            {
                return argValues.Max();
            }
            if (tokType == TokenType.MIN)
            {
                return argValues.Min();
            }
            return -1;
        }

        private int VisitNode(UnOp node)
        {
            TokenType op = node.Op.Type;
            int exprValue = Visit(node.Expr);
            
            if (op == TokenType.INC)
            {
                return exprValue + 1;
            }
            else if (op == TokenType.DEC)
            {
                return exprValue - 1;
            }
            return exprValue;
        }

        //private int VisitNode(Var node)
        //{
        //    string varName = node.Name;
        //    if (Variables.ContainsKey(varName))
        //    {
        //        return Variables[varName];
        //    }
        //    return 0;
        //}

        private int? VisitNode(CellRef node)
        {
            int? num = spreadSheet.GetCellValueByName(node.Name);
            if (num == null)
            {
                return 0;
            }
            
            return spreadSheet.GetCellValueByName(node.Name);
        }


        private int VisitNode(NoOp node)
        {
            return 0;
        }

        private int VisitNode(BinOp node)
        {
            int leftValue = Visit(node.Left);
            int rightValue = Visit(node.Right);

            switch (node.Op.Type)
            {
                case TokenType.PLUS:
                    return leftValue + rightValue;
                case TokenType.MINUS:
                    return leftValue - rightValue;
                case TokenType.MULTIPLY:
                    return leftValue * rightValue;
                case TokenType.DIVIDE:
                    if (rightValue == 0)
                        throw new DivideByZeroException("Can't divide by zero.");
                    return leftValue / rightValue;
                case TokenType.EXPONENTIATION:
                    return (int)Math.Pow(leftValue, rightValue);
                default:
                    throw new NotSupportedException($"Operator {node.Op.Type} not supported.");
            }
        }

        private int VisitNode(object node)
        {
            throw new Exception($"Немає методу VisitNode для типу {node.GetType().Name}");
        }
    }
}
