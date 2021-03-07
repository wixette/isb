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
            public Type Type { get; private init; }
            public bool IsBuiltInLib => this.Type.Equals(typeof(ISB.Lib.BuiltIn));
            public object Instance { get; private init; }

            public Dictionary<string, MethodInfo> Functions { get; private init; }
            public Dictionary<string, PropertyInfo> Properties { get; private init; }

            public Lib(Type libClass)
            {
                this.Type = libClass;
                this.Instance = Activator.CreateInstance(libClass);
                this.Functions = new Dictionary<string, MethodInfo>();
                this.Properties = new Dictionary<string, PropertyInfo>();
            }

            public void AddFunc(MethodInfo f)
            {
                this.Functions[f.Name.ToLower()] = f;
            }

            public void AddProperty(PropertyInfo p)
            {
                this.Properties[p.Name.ToLower()] = p;
            }
        }

        private Dictionary<string, Lib> Libs { get; init; }
        private string builtInLibName = typeof(ISB.Lib.BuiltIn).Name;

        public Libraries()
        {
            this.Libs = new Dictionary<string, Lib>();
            this.AutoLoadStandardLibs();
        }

        public bool HasProperty(string libName, string propertyName)
            => this.Libs.ContainsKey(libName.ToLower()) &&
                this.Libs[libName.ToLower()].Properties.ContainsKey(propertyName.ToLower());

        public bool IsPropertyWritable(string libName, string propertyName)
            => this.HasProperty(libName, propertyName) &&
                this.GetProperty(libName, propertyName).SetMethod.IsPublic;

        public bool HasFunction(string libName, string functionName)
            => this.Libs.ContainsKey(libName.ToLower()) &&
                this.Libs[libName.ToLower()].Functions.ContainsKey(functionName.ToLower());

        public bool HasBuiltInFunction(string functionName)
            => this.HasFunction(this.builtInLibName, functionName);

        public int GetArgumentNumber(string libName, string functionName)
            => this.HasFunction(libName, functionName) ?
                this.GetFunction(libName, functionName).GetParameters().Length : 0;

        public int GetArgumentNumber(string functionName)
            => this.GetArgumentNumber(this.builtInLibName, functionName);

        public bool HasReturnValue(string libName, string functionName)
            => this.HasFunction(libName, functionName) ?
                !this.GetFunction(libName, functionName).ReturnType.Equals(typeof(void)) : false;

        public bool HasReturnValue(string functionName)
            => this.HasReturnValue(this.builtInLibName, functionName);

        public BaseValue GetPropertyValue(string libName, string propertyName)
            => this.HasProperty(libName, propertyName) ?
                (BaseValue)this.GetProperty(libName, propertyName).GetValue(this.GetInstance(libName)) :
                StringValue.Empty;

        public void SetPropertyValue(string libName, string propertyName, BaseValue value)
        {
            if (this.HasProperty(libName, propertyName))
                this.GetProperty(libName, propertyName).SetValue(this.GetInstance(libName), value);
        }

        public BaseValue InvokeFunction(string libName, string functionName, object[] parameters)
            => this.HasFunction(libName, functionName) ?
                (BaseValue)this.GetFunction(libName, functionName).Invoke(this.GetInstance(libName), parameters) :
                null;

        public BaseValue InvokeFunction(string functionName, object[] parameters)
            => this.InvokeFunction(this.builtInLibName, functionName, parameters);

        private PropertyInfo GetProperty(string libName, string propertyName)
            => this.Libs[libName.ToLower()].Properties[propertyName.ToLower()];

        private MethodInfo GetFunction(string libName, string functionName)
            => this.Libs[libName.ToLower()].Functions[functionName.ToLower()];

        private object GetInstance(string libName)
            => this.Libs[libName.ToLower()].Instance;

        private static bool IsOverride(MethodInfo method)
            => !method.Equals(method.GetBaseDefinition());

        private static bool IsDerivedTypeOfBaseValue(Type t)
            => t.Equals(typeof(ISB.Runtime.BaseValue)) || t.IsSubclassOf(typeof(ISB.Runtime.BaseValue));

        private static bool IsAcceptableMethod(MethodInfo m)
        {
            if (!m.ReturnType.Equals(typeof(void)) && !IsDerivedTypeOfBaseValue(m.ReturnType))
                return false;

            foreach (var param in m.GetParameters())
            {
                if (!IsDerivedTypeOfBaseValue(param.ParameterType) ||
                        param.IsOut || param.IsOptional || param.IsRetval)
                    return false;
            }
            return true;
        }

        private static bool IsAcceptableProperty(PropertyInfo p)
            => IsDerivedTypeOfBaseValue(p.PropertyType);

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
                Lib lib = new Lib(libClass);

                var methodQuery = libClass.GetMethods().Where(
                    m => m.IsPublic && !m.IsSpecialName && !IsOverride(m) && !excludedNames.Contains(m.Name));
                foreach (var m in methodQuery.ToList())
                {
                    if (IsAcceptableMethod(m))
                        lib.AddFunc(m);
                }

                // The built-in library has no properties.
                if (!libClass.Equals(builtInClass))
                {
                    var propertyQuery = libClass.GetProperties().Where(p => p.GetMethod.IsPublic);
                    foreach (var p in propertyQuery.ToList())
                    {
                        if (IsAcceptableProperty(p))
                            lib.AddProperty(p);
                    }
                }

                this.Libs[lib.Type.Name.ToLower()] = lib;
            }
        }
    }
}