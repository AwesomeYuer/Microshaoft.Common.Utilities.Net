

namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
 
    using System.ComponentModel.DataAnnotations.Schema;
    public static class Mapper<TSource, TTarget> where TSource : class where TTarget : class
    {
        public readonly static Func<TSource, TTarget> Map;

        static Mapper()
        {
            if (Map == null)
                Map = GetMap();
        }

        private static Func<TSource, TTarget> GetMap()
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            var parameterExpression = Expression.Parameter(sourceType, "p");
            var memberInitExpression = GetExpression(parameterExpression, sourceType, targetType);

            var lambda = Expression.Lambda<Func<TSource, TTarget>>(memberInitExpression, parameterExpression);
            return lambda.Compile();
        }

        /// <summary>
        /// 根据转换源和目标获取表达式树
        /// </summary>
        /// <param name="parameterExpression">表达式参数p</param>
        /// <param name="sourceType">转换源类型</param>
        /// <param name="targetType">转换目标类型</param>
        /// <returns></returns>
        private static MemberInitExpression GetExpression(Expression parameterExpression, Type sourceType, Type targetType)
        {
            var memberBindings = new List<MemberBinding>();
            foreach (var targetItem in targetType.GetProperties().Where(x => x.PropertyType.IsPublic && x.CanWrite))
            {
                var sourceItem = sourceType.GetProperty(targetItem.Name);

                //判断实体的读写权限
                if (sourceItem == null || !sourceItem.CanRead || sourceItem.PropertyType.IsNotPublic)
                    continue;

                //标注NotMapped特性的属性忽略转换
                if (sourceItem.GetCustomAttribute<NotMappedAttribute>() != null)
                    continue;

                var propertyExpression = Expression.Property(parameterExpression, sourceItem);

                //判断都是class 且类型不相同时
                if (targetItem.PropertyType.IsClass && sourceItem.PropertyType.IsClass && targetItem.PropertyType != sourceItem.PropertyType)
                {
                    if (targetItem.PropertyType != targetType)//防止出现自己引用自己无限递归
                    {
                        var memberInit = GetExpression(propertyExpression, sourceItem.PropertyType, targetItem.PropertyType);
                        memberBindings.Add(Expression.Bind(targetItem, memberInit));
                        continue;
                    }
                }

                if (targetItem.PropertyType != sourceItem.PropertyType)
                    continue;

                memberBindings.Add(Expression.Bind(targetItem, propertyExpression));
            }
            return Expression.MemberInit(Expression.New(targetType), memberBindings);
        }
    }
}
