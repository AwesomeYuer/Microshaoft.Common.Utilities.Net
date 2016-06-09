namespace Microshaoft
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public static partial class ExpressionHelper
    {
        public static Expression<Func<TTarget, bool>> CreateMemberEqualsToExpression<TTarget>
            (
                string memberName
                , object equalsTo
            )
        {
            var dataParameter = Expression.Parameter(typeof(TTarget), "data");
            var targetMemberType = typeof(TTarget);
            var targetMemberParameter = Expression.Parameter(targetMemberType, "member");
            var memberInfo = targetMemberType
                                    .GetMember
                                        (
                                            memberName
                                            //, MemberTypes.Field | MemberTypes.Property
                                            //, BindingFlags.Default
                                        ).Single();
            var targetMember = Expression.MakeMemberAccess(dataParameter, memberInfo);
            var body = Expression.Equal(targetMember, Expression.Constant(equalsTo));
            var lambda = Expression.Lambda<Func<TTarget, bool>>(body, dataParameter);
            return lambda;
        }
    }
}