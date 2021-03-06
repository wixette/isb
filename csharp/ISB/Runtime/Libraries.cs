// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ISB.Runtime
{
    public sealed class Libraries
    {
        private sealed class Lib
        {
            public string Name { get; private init; }
            public Dictionary<string, Function> Funcs { get; private init; }
            public Dictionary<string, Property> Properties { get; private init; }

            public bool IsBuiltInLib => this.Name == typeof(ISB.Lib.BuiltIn).Name;

            public Lib(string name)
            {
                this.Name = name;
                this.Funcs = new Dictionary<string, Function>();
                this.Properties = new Dictionary<string, Property>();
            }

            public void AddFunc(Function f)
            {
                this.Funcs[f.Name] = f;
            }

            public void AddProperty(Property p)
            {
                this.Properties[p.Name] = p;
            }
        }

        private sealed class Function
        {
            public string Name { get; private init; }
            public Type ReturnType { get; private init; }
            public IReadOnlyList<Type> ParameterTypes { get; private init; }

            public Function(string name, Type returnType, IReadOnlyList<Type> parameterTypes)
            {
                this.Name = name;
                this.ReturnType = returnType;
                this.ParameterTypes = parameterTypes;
            }
        }

        private sealed class Property
        {
            public string Name { get; private init; }
            public Type Type { get; private init; }
            public bool IsWritable { get; private init; }

            public Property(string name, Type type, bool isWritable)
            {
                this.Name = name;
                this.Type = type;
                this.IsWritable = isWritable;
            }
        }

        private Dictionary<string, Lib> Libs { get; init; }

        public Libraries()
        {
            this.Libs = new Dictionary<string, Lib>();
            this.AutoLoadStandardLibs();
        }

        public bool IsPropertyExist(string libName, string propertyName)
        {
            // TODO
            return true;
        }

        public bool IsPropertyWritable(string libName, string propertyName)
        {
            // TODO
            return true;
        }

        public bool IsFunctionExist(string libName, string functionName)
        {
            // TODO
            return true;
        }

        public int GetFunctionArgumentNumber(string libName, string functionName)
        {
            // TODO
            return 0;
        }

        private static bool IsOverride(MethodInfo method)
            => !method.Equals(method.GetBaseDefinition());

        private static bool IsDerivedTypeOfBaseValue(Type t)
            => t.Equals(typeof(ISB.Runtime.BaseValue)) || t.IsSubclassOf(typeof(ISB.Runtime.BaseValue));

        private static bool IsAcceptableMethod(MethodInfo m, out Type returnType, out List<Type> parameterTypes)
        {
            returnType = null;
            parameterTypes = new List<Type>();
            if (!m.ReturnType.Equals(typeof(void)) && !IsDerivedTypeOfBaseValue(m.ReturnType))
                return false;
            else
                returnType = m.ReturnType;

            foreach (var param in m.GetParameters())
            {
                if (!IsDerivedTypeOfBaseValue(param.ParameterType) ||
                        param.IsOut || param.IsOptional || param.IsRetval)
                    return false;
                else
                    parameterTypes.Add(param.ParameterType);
            }
            return true;
        }

        private static bool IsAcceptableProperty(PropertyInfo p, out Type propertyType)
        {
            propertyType = null;
            if (!IsDerivedTypeOfBaseValue(p.PropertyType))
                return false;
            else
                propertyType = p.PropertyType;
            return true;
        }

        private void AutoLoadStandardLibs()
        {
            Type builtInClass = typeof(ISB.Lib.BuiltIn);
            var classQuery = builtInClass.Assembly.GetTypes()
                .Where(t => t.IsClass && t.IsPublic && t.Namespace == builtInClass.Namespace);
            var libClasses = classQuery.ToList();

            HashSet<string> excludedNames = new HashSet<string>(
                new string[] { "GetType" }
            );

            foreach (var libClass in libClasses)
            {
                Lib lib = new Lib(libClass.Name);

                var methodQuery = libClass.GetMethods().Where(
                    m => m.IsPublic && !m.IsSpecialName && !IsOverride(m) && !excludedNames.Contains(m.Name));
                foreach (var m in methodQuery.ToList())
                {
                    if (IsAcceptableMethod(m, out Type returnType, out List<Type> parameterTypes))
                        lib.AddFunc(new Function(m.Name, returnType, parameterTypes));
                }

                var propertyQuery = libClass.GetProperties().Where(p => p.GetMethod.IsPublic);
                foreach (var p in propertyQuery.ToList())
                {
                    if (IsAcceptableProperty(p, out Type propertyType))
                        lib.AddProperty(new Property(p.Name, p.PropertyType, p.SetMethod.IsPublic));
                }

                this.Libs[lib.Name] = lib;
            }
        }
    }
}