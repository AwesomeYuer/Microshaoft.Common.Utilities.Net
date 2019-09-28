namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Linq;
    public class LamadaExtention<TModel>
                                where TModel : new()
    {
        private List<Expression> _expressions = null;
        private ParameterExpression _parameterExpression = null;

        public LamadaExtention()
        {
            _expressions = new List<Expression>();
            _parameterExpression = Expression.Parameter(typeof(TModel), "x");
        }

        //只读属性，返回生成的Lamada
        public Expression<Func<TModel, bool>> Lamada
        {
            get
            {
                return GetLambda();
            }
        }

        /// <summary>
        /// 字符串Contains筛选
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <param name="memberValue"></param>
        public void Contains
                        (
                            Expression<Func<TModel, string>> memberExpression
                            , object memberValue
                        )
        {
            var expression
                            = Expression
                                    .Call
                                        (
                                            memberExpression.Body
                                            , typeof(string)
                                                    .GetMethod("Contains")
                                            , Expression
                                                    .Constant(memberValue)
                                        );
            _expressions
                    .Add(expression);
        }

        /// <summary>
        /// 等于
        /// </summary>
        /// <param name="expProperty"></param>
        /// <param name="strValue"></param>
        public void Equal
                        (
                            Expression<Func<TModel, object>> memberExpression
                            , object memberValue
                        )
        {
            var member = GetMemberExpression(memberExpression);
            Expression expression
                            = Expression
                                    .Equal
                                        (
                                            member
                                            , Expression
                                                    .Constant
                                                        (
                                                            memberValue
                                                            , member.Type
                                                        )
                                        );
            _expressions.Add(expression);
        }

        /// <summary>
        /// 小于
        /// </summary>
        /// <param name="expProperty"></param>
        /// <param name="strValue"></param>
        public void LessThan
            (
                 Expression<Func<TModel, object>> memberExpression
                 , object memberValue
             )
        {
            var member = GetMemberExpression(memberExpression);
            Expression expression
                            = Expression
                                    .LessThan
                                        (
                                            member
                                            , Expression
                                                    .Constant
                                                        (
                                                            memberValue
                                                            , member.Type
                                                        )
                                        );
            _expressions.Add(expression);
        
        }

        /// <summary>
        /// 小于等于
        /// </summary>
        /// <param name="expProperty"></param>
        /// <param name="strValue"></param>
        public void LessThanOrEqual
                        (
                            Expression<Func<TModel, object>> memberExpression
                            , object memberValue
                        )
        {
            var member = GetMemberExpression(memberExpression);
            Expression expression
                            = Expression
                                    .LessThanOrEqual
                                        (
                                            member
                                            , Expression
                                                    .Constant
                                                        (
                                                            memberValue
                                                            , member.Type
                                                        )
                                        );
            _expressions.Add(expression);
        }


        /// <summary>
        /// 大于
        /// </summary>
        /// <param name="expProperty"></param>
        /// <param name="strValue"></param>
        public void GreaterThan
                        (
                            Expression<Func<TModel, object>> memberExpression
                            , object memberValue
                        )
        {
            var member = GetMemberExpression(memberExpression);
            Expression expression
                            = Expression
                                    .GreaterThan
                                        (
                                            member
                                            , Expression
                                                    .Constant
                                                        (
                                                            memberValue
                                                            , member.Type
                                                        )
                                        );
            _expressions.Add(expression);
        }

        /// <summary>
        /// 大于等于
        /// </summary>
        /// <param name="expProperty"></param>
        /// <param name="strValue"></param>
        public void GreaterThanOrEqual
                        (
                            Expression<Func<TModel, object>> memberExpression
                            , object memberValue
                        )
        {
            var member = GetMemberExpression(memberExpression);
            Expression expression
                            = Expression
                                    .GreaterThanOrEqual
                                        (
                                            member
                                            , Expression
                                                    .Constant
                                                        (
                                                            memberValue
                                                            , member.Type
                                                        )
                                        );
            _expressions.Add(expression);
        }
        private Expression<Func<TModel, bool>> GetLambda()
        {
            Expression whereExpression = null;
            foreach (var expression in _expressions)
            {
                if (whereExpression == null)
                {
                    whereExpression = expression;
                }
                else
                {
                    whereExpression = Expression.And(whereExpression, expression);
                }
            }
            Expression<Func<TModel, bool>> lambda = null;
            if (whereExpression == null)
            {
                lambda = null;
            }
            else
            {
                lambda
                    = Expression
                            .Lambda<Func<TModel, bool>>
                                    (
                                        whereExpression
                                        , _parameterExpression
                                    );
            }
            return
                lambda;
        }

        //得到MemberExpression
        private MemberExpression GetMemberExpression(Expression<Func<TModel, object>> memberExpression)
        {
            var memberName = memberExpression
                                .Body
                                .ToString()
                                .Split
                                    (
                                        "(.)".ToCharArray()
                                        , StringSplitOptions.RemoveEmptyEntries
                                    )
                                .Last();
            MemberExpression member = Expression
                                            .PropertyOrField(_parameterExpression, memberName);
            return member;
        }

        public void CreateWhereCompareExpressions(string memberName, object memberValue, CompareExpressionType compareExpressionType)
        {
            Expression expression = null;
            MemberExpression member = Expression.PropertyOrField(_parameterExpression, memberName);
            if (compareExpressionType == CompareExpressionType.Contains)
            {
                expression = Expression.Call(member, typeof(string).GetMethod("Contains"), Expression.Constant(memberValue));
            }
            else if (compareExpressionType == CompareExpressionType.Equal)
            {
                expression = Expression.Equal(member, Expression.Constant(memberValue, member.Type));
            }
            else if (compareExpressionType == CompareExpressionType.LessThan)
            {
                expression = Expression.LessThan(member, Expression.Constant(memberValue, member.Type));
            }
            else if (compareExpressionType == CompareExpressionType.LessThanOrEqual)
            {
                expression = Expression.LessThanOrEqual(member, Expression.Constant(memberValue, member.Type));
            }
            else if (compareExpressionType == CompareExpressionType.GreaterThan)
            {
                expression = Expression.GreaterThan(member, Expression.Constant(memberValue, member.Type));
            }
            else if (compareExpressionType == CompareExpressionType.GreaterThanOrEqual)
            {
                expression = Expression.GreaterThanOrEqual(member, Expression.Constant(memberValue, member.Type));
            }
            _expressions.Add(expression);
            //return expression;
            //
        }

        //针对Or条件的表达式
        public void GetExpression(string memberName, List<object> lstValue)
        {
            Expression expression = null;
            MemberExpression member = Expression.PropertyOrField(_parameterExpression, memberName);
            foreach (var oValue in lstValue)
            {
                if (expression == null)
                {
                    expression = Expression.Equal(member, Expression.Constant(oValue, member.Type));
                }
                else
                {
                    expression = Expression.Or(expression, Expression.Equal(member, Expression.Constant(oValue, member.Type)));
                }
            }


            _expressions.Add(expression);
        }

        public enum CompareExpressionType
        {
            Contains,//like
            Equal,//等于
            LessThan,//小于
            LessThanOrEqual,//小于等于
            GreaterThan,//大于
            GreaterThanOrEqual//大于等于
        }
    }
}
namespace ConsoleApplication
{
    using System;
    using System.Linq;
    using Microshaoft;
    /// <summary>
    /// Class1 的摘要说明。
    /// </summary>
    public class ProgramTest

    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        //[STAThread]
        private class UserModelDto
        {
            public string UserName;
            public DateTime? BrithDate;

        }

        static void Main112(string[] args)
        {
            var oLamadaExtention = new LamadaExtention<UserModelDto>();
            oLamadaExtention.Equal(x => x.UserName, "张三");
            oLamadaExtention.LessThan(x => x.BrithDate, DateTime.Now);
            //var lstRes = UserManager.Find(oLamadaExtention.lamada).ToList();

            //new LamadaExtention<UserModelDto>()
            //           .CreateWhereCompareExpressions
            //                (

            //                );
                        

            //oLamadaExtention.GetExpression("USER_NAME", username, ExpressionType.Contains);

            

            //
            // TODO: 在此处添加代码以启动应用程序
            //
            Console.WriteLine("Hello World");
            Console.WriteLine(Environment.Version.ToString());
        }

    }


}