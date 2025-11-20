using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace MyExcelMauiLab1
//{
//    public class Cell //: Interpreter
//    {
//        public int? Value;
//        public string Expression;
//        public string ErrorMessage;

//        public List<Cell> Precedents = new();   // cells this depends on
//        public List<Cell> Dependents = new();   // cells that depend on this (optional)


//        public Cell(string expr)
//        {
//            Expression = expr;
//            ErrorMessage = "";
//        }

//        public Cell() => (Value, Expression, ErrorMessage) = (0, "\0", "");

//        public int? CalculateExpr(string expression, SpreadsheetLogic spreadSheet)
//        {
//            this.Expression = expression;

//            if (string.IsNullOrWhiteSpace(expression))
//            {
//                this.Value = null;
//                return this.Value;
//            }

//            int parsedInteger;
//            if (int.TryParse(expression, out parsedInteger))
//            {
//                this.Value = parsedInteger;
//            }
//            else
//            {
//                Lexer lexer = new Lexer(expression);
//                Parser parser = new Parser(lexer);
//                AST parseSyntTree = parser.Parse();
//                List<string> referencedCells = parser.SeenCellRefs;
//                Interpreter interpreter = new Interpreter(parser, spreadSheet);


//                this.Value = interpreter.Interpret();
//            }

//            return this.Value;
//        }



//    }
//}

using System.Collections.Generic;

namespace MyExcelMauiLab1
{
    public class Cell
    {
        public int? Value { get; set; }
        public string Expression { get; set; }
        public string ErrorMessage { get; set; }


        public List<string> ReferencedCellNames { get; private set; } = new List<string>();

        public Cell(string expr)
        {
            Expression = expr;
            ErrorMessage = "";
        }

        public Cell() : this("") { }

        public void SetReferences(List<string> refs)
        {
            ReferencedCellNames = refs;
        }
    }
}
