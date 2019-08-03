namespace Microshaoft
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    public static partial class DynamicExpressionTreeHelper
    {
		public static Action<TTarget, TMember> CreateMemberSetter<TTarget, TMember>(string memberName)
        {
            var targetType = typeof(TTarget);
            var target = Expression.Parameter(targetType, "target");
            var targetMemberType = typeof(TMember);
            var targetMemberParameter = Expression.Parameter(targetMemberType, "member");
            var memberInfo = targetType
                                    .GetMember
                                        (
                                            memberName
                                            //, MemberTypes.Field | MemberTypes.Property
                                            //, BindingFlags.Default
                                        ).Single();
            //not support static member because bug
            var isStatic = false;
            if (memberInfo is FieldInfo)
            {
                var fieldInfo = memberInfo as FieldInfo;
                isStatic = fieldInfo.IsStatic;
            }
            else if (memberInfo is PropertyInfo)
            {
                var propertyInfo = memberInfo as PropertyInfo;
                isStatic = propertyInfo.GetGetMethod().IsStatic;
            }
            if (isStatic)
            {
                target = null;
            }
            var targetMember = Expression.MakeMemberAccess(target, memberInfo);
            //var targetMemberConverter = Expression.Convert(targetMemberParameter, targetMember.Type);
            var body = Expression.Assign(targetMember, targetMemberParameter);
            //if (!isStatic)
            //{
            Expression<Action<TTarget, TMember>>
                lambda = Expression
                                .Lambda
                                    <Action<TTarget, TMember>>
                                        (
                                            body
                                            , target
                                            , targetMemberParameter
                                        );
            //}
            //else
            //{
            //not support static member because bug
            //    lambda = Expression.Lambda<Action<TTarget, TMember>>(body, targetParameter, targetMemberParameter);
            //}
            return lambda.Compile();
        }

        public static Action<object, object> CreateMemberSetter(Type targetType, string memberName)
        {
            var objectTargetType = typeof(object);
            var targetParameter = Expression.Parameter(objectTargetType, "objectTarget");
            var memberValue = Expression.Parameter(objectTargetType, "memberValue");
            var target = Expression.Convert(targetParameter, targetType);
            var memberInfo = targetType
                                    .GetMember
                                        (
                                            memberName
                                            //, MemberTypes.Field | MemberTypes.Property
                                            //, BindingFlags.Default
                                        ).Single();
            var isStatic = false;
            if (memberInfo is FieldInfo)
            {
                var fieldInfo = memberInfo as FieldInfo;
                isStatic = fieldInfo.IsStatic;
            }
            else if (memberInfo is PropertyInfo)
            {
                var propertyInfo = memberInfo as PropertyInfo;
                isStatic = propertyInfo.GetGetMethod().IsStatic;
            }
            if (isStatic)
            {
                targetParameter = null;
            }
            var targetMember = Expression.MakeMemberAccess(target, memberInfo);
            
            var castMemberValue = Expression.Convert(memberValue, targetMember.Type);
            var assign = Expression.Assign(targetMember, castMemberValue);
            var lambda = Expression
                            .Lambda<Action<object, object>>
                                (
                                    assign
                                    , targetParameter
                                    , memberValue
                                );
            return lambda.Compile();
        }
        
        public static Func<TTarget, TMember> CreateMemberGetter<TTarget, TMember>(string memberName)
        {
            var targetType = typeof(TTarget);
            var targetParameter = Expression.Parameter(targetType, "target");
            var targetMember = Expression.PropertyOrField(targetParameter, memberName);
            var memberType = typeof(TMember);
            var body = Expression.Convert(targetMember, memberType);
            var lambda = Expression.Lambda<Func<TTarget, TMember>>(body, targetParameter);
            return lambda.Compile();
        }
        public static Func<object, object> CreateMemberGetter(Type targetType, string memberName)
        {
            var objectTargetType = typeof(object);
            var target = Expression.Parameter(objectTargetType, "target");
            var castTarget = Expression.Convert(target, targetType);
            var targetMember = Expression.PropertyOrField(castTarget, memberName);
            var castMemberValue = Expression.Convert(targetMember, objectTargetType);
            var lambda = Expression
                            .Lambda<Func<object, object>>
                                (
                                    castMemberValue
                                    , target
                                );
            return lambda.Compile();
            //var targetParameter = Expression.Parameter(targetType, "target");
            //var targetMember = Expression.PropertyOrField(targetParameter, memberName);
            //var memberType = typeof(object);
            //var body = Expression.Convert(targetMember, memberType);
            //var lambda = Expression.Lambda<Func<object, object>>(body, targetParameter);
            //return lambda.Compile();
        }
    }
}