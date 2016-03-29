namespace Microshaoft
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    public class DynamicMemberAccessor
    {
		public static Action<TTarget, TMember> CreateSetter<TTarget, TMember>(string memberName)
        {
            var targetType = typeof(TTarget);
            var targetParameter = Expression.Parameter(targetType, "target");
            var targetMemberType = typeof(TMember);
            var targetMemberParameter = Expression.Parameter(targetMemberType, "member");
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
                isStatic = propertyInfo.GetSetMethod().IsStatic;
            }
            if (isStatic)
            {
                targetParameter = null;
            }
            var targetMember = Expression.MakeMemberAccess(targetParameter, memberInfo);
            var targetMemberConverter = Expression.Convert(targetMemberParameter, targetMember.Type);
            var body = Expression.Assign(targetMember, targetMemberConverter);
            Expression<Action<TTarget, TMember>> lambda = null;
            //if (!isStatic)
            //{
            lambda = Expression.Lambda<Action<TTarget, TMember>>(body, targetParameter, targetMemberParameter);
            //}
            //else
            //{
            //    lambda = Expression.Lambda<Action<TTarget, TMember>>(body, targetParameter, targetMemberParameter);
            //}
            return lambda.Compile();
        }
        public static Action<object, object> CreateSetter(Type targetType, string memberName)
        {
            //var targetType = typeof(TTarget);
            var targetParameter = Expression.Parameter(targetType, "target");
            var targetMemberType = typeof(object);
            var targetMemberParameter = Expression.Parameter(targetMemberType, "member");
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
                isStatic = propertyInfo.GetSetMethod().IsStatic;
            }
            if (isStatic)
            {
                targetParameter = null;
            }
            var targetMember = Expression.MakeMemberAccess(targetParameter, memberInfo);
            var targetMemberConverter = Expression.Convert(targetMemberParameter, targetMember.Type);
            var body = Expression.Assign(targetMember, targetMemberConverter);
            Expression<Action<object, object>> lambda = null;
            //if (!isStatic)
            //{
            lambda = Expression.Lambda<Action<object, object>>(body, targetParameter, targetMemberParameter);
            //}
            //else
            //{
            //    lambda = Expression.Lambda<Action<TTarget, TMember>>(body, targetParameter, targetMemberParameter);
            //}
            return lambda.Compile();
        }
        public static Func<TTarget, TMember> CreateGetter<TTarget, TMember>(string memberName)
        {
            var targetType = typeof(TTarget);
            var targetParameter = Expression.Parameter(targetType, "target");
            var targetMember = Expression.PropertyOrField(targetParameter, memberName);
            var memberType = typeof(TMember);
            var body = Expression.Convert(targetMember, memberType);
            var lambda = Expression.Lambda<Func<TTarget, TMember>>(body, targetParameter);
            return lambda.Compile();
        }
        public static Func<object, object> CreateGetter(Type targetType, string memberName)
        {
            var targetParameter = Expression.Parameter(targetType, "target");
            var targetMember = Expression.PropertyOrField(targetParameter, memberName);
            var memberType = typeof(object);
            var body = Expression.Convert(targetMember, memberType);
            var lambda = Expression.Lambda<Func<object, object>>(body, targetParameter);
            return lambda.Compile();
        }
    }
}