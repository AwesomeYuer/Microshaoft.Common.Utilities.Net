namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    public enum AccessMemberStateEnum
    {
        All,
        CanRead,
        CanWrite
    }
    public static partial class TypeHelper
    {
        public static bool IsAnonymousType(this Type @this)
        {
            return
                Attribute
                    .IsDefined
                        (
                            @this
                            , typeof(CompilerGeneratedAttribute)
                            , false
                        )
                &&
                    @this
                        .Name
                        .Contains
                            ("AnonymousType")
                &&
                    (
                        @this.Name.StartsWith("<>")
                        ||
                        @this.Name.StartsWith("VB$")
                    )
                &&
                    (
                        @this.Attributes
                        &
                        TypeAttributes.NotPublic
                    )
                    ==
                    TypeAttributes.NotPublic;
        }

        public static List<Type> GetTypeAndBaseTypes(this Type @this)
        {
            List<Type> r = new List<Type>();
            if (@this != null)
            {
                r
                    .Add(@this);
                r
                    .AddRange(GetBaseTypes(@this));
            }
            return r;
        }

        public static List<Type> GetBaseTypes(this Type @this)
        {
            List<Type> r = new List<Type>();
            if (@this != null)
            {
                if 
                    (
                        @this.BaseType != null
                        &&
                        @this.BaseType != typeof(object)
                    )
                {
                    r
                        .Add(@this.BaseType);
                    r
                        .AddRange(GetBaseTypes(@this.BaseType));
                }
            }
            return r;
        }

        public static List<MemberInfo> GetAllInterfaceMembers(this Type @this)
        {
            if (!@this.IsInterface)
            {
                throw new Exception("Expected interface, found: " + @this);
            }
            var pending = new Stack<Type>();
            pending.Push(@this);
            var r = new List<MemberInfo>();
            while (pending.Count > 0)
            {
                var current = pending.Pop();
                r.AddRange(current.GetMembers());
                if (current.BaseType != null)
                {
                    pending.Push(current.BaseType);
                }
                foreach (var x in current.GetInterfaces())
                {
                    pending.Push(x);
                }
            }
            return r;
        }

        public static ConstructorInfo GetCtorByParameterInterfaceType
                                (
                                    this Type @this
                                    , params Type[] ctorParaTypes
                                )
        {
            foreach (var ctor in @this.GetConstructors())
            {
                var ctorParas = ctor.GetParameters();
                if (ctorParas.Length == ctorParaTypes.Length)
                {
                    bool isTrue = true;
                    for (int i = 0; i < ctorParaTypes.Length; i++)
                    {
                        if (ctorParas[i].ParameterType != ctorParaTypes[i])
                        {
                            isTrue = false;
                            break;
                        }

                    }
                    if (isTrue)
                        return ctor;
                }
            }
            return null;
        }

        public static ConstructorInfo GetDefaultNoArgCtorOrAppointTypeCtor
                                (
                                    this Type @this
                                    , Type ctorParaTypes = null
                                )
        {
            foreach (var ctor in @this.GetConstructors())
            {
                var ctorParas = ctor.GetParameters();
                if (ctorParas.Length == 0)
                    return ctor;//no args
                if (ctorParaTypes != null && ctorParas.Length == 1)
                {
                    if (ctorParas[0].ParameterType == ctorParaTypes)
                        return ctor;
                }
            }
            return null;
        }

        public static ConstructorInfo GetAppointTypeCtor(this Type @this, Type ctorParaTypes)
        {
            foreach (var ctor in @this.GetConstructors())
            {
                var ctorParas = ctor.GetParameters();
                if 
                    (
                        ctorParas.Length == 1
                        &&
                        ctorParas[0].ParameterType == ctorParaTypes
                    )
                {
                    return ctor;
                }
            }
            return null;
        }

        public static bool HasEmptyCtor(this Type @this)
        {
            if (@this.IsInterface)
            {
                return false;
            }
            foreach (var ctor in @this.GetConstructors())
            {
                var ctorParas = ctor.GetParameters();
                if (ctorParas.Length == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsWrongKey(this Type @this)
        {
            if
                (
                    !@this.IsPrimitive
                    &&
                    !@this.IsEnum
                    &&
                    @this != typeof(string)
                    &&
                    @this != typeof(Guid)
                    &&
                    @this != typeof(DateTime)
                )
            {
                return true;
            }
            return false;
        }
        public static object GetDefaultValue(this Type @this)
        {
            if (@this.IsValueType)
            {
                return
                    Activator
                        .CreateInstance
                            (
                                @this
                            );
            }
            return null;
        }
    }
    public class MemberExtension
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public MemberInfo MemberInfo { get; set; }
        public FieldInfo FieldInfo { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public bool IsProperty { get; set; }
        public int OrderNum { get; set; }

        public MemberExtension(PropertyInfo propertyInfo)
        {
            MemberInfo = propertyInfo;
            PropertyInfo = propertyInfo;
            Name = propertyInfo.Name;
            IsProperty = true;
            Type = propertyInfo.PropertyType;
        }

        public MemberExtension(FieldInfo fieldInfo)
        {
            MemberInfo = fieldInfo;
            FieldInfo = fieldInfo;
            Name = fieldInfo.Name;
            IsProperty = false;
            Type = fieldInfo.FieldType;
        }
    }
    public class CharTries
    {
        public char Val;
        public MemberExtension Member;
        public bool IsPeak;
        public bool IsValue;
        public CharTries Parent;
        public List<CharTries> Childrens;

        public CharTries()
        {
            Childrens = new List<CharTries>();
            IsValue = false;
        }

        public void Insert(string text, MemberExtension member)
        {
            CharTries charTries = this;
            foreach (var c in text)
            {
                var @case = charTries
                                .Childrens
                                .Find(e => e.Val == c);
                if (@case == null)
                {
                    @case = new CharTries()
                    {
                        Val = c
                        , Parent = charTries
                    };
                    charTries
                        .Childrens
                        .Add(@case);
                }
                charTries = @case;
            }
            charTries.IsValue = true;
            charTries.Member = member;
        }
    }
}