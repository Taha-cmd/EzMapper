using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Models
{
    internal class WhereClause
    {
        public WhereClause(string leftOperand, string operation, string rightOperand)
        {
            LeftOperand = leftOperand;
            Operation = operation;
            RightOperand = rightOperand;

        }
        public string LeftOperand { get; set; }
        public string Operation { get; set; }
        public string RightOperand { get; set; }
    }
}
