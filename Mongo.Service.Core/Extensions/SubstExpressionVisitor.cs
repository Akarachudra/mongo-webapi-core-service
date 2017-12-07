using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mongo.Service.Core.Extensions
{

    internal class SubstExpressionVisitor : ExpressionVisitor
    {
        public Dictionary<Expression, Expression> Subst = new Dictionary<Expression, Expression>();

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Expression newValue;
            if (this.Subst.TryGetValue(node, out newValue))
            {
                return newValue;
            }

            return node;
        }
    }
}