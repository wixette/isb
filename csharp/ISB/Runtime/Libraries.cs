// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ISB.Utilities;

namespace ISB.Runtime
{
    public sealed class Libraries
    {
        private sealed class Lib
        {
            public Type Type { get; private set; }
            public bool IsBuiltInLib => this.Type.Equals(typeof(ISB.Lib.BuiltIn));
            public object Instance { get; private set; }

            public SortedDictionary<string, MethodInfo> Functions { get; private set; }
            public SortedDictionary<string, PropertyInfo> Properties { get; private set; }

            public Lib(Type libClass)
            {
                this.Type = libClass;
                this.Instance = Activator.CreateInstance(libClass);
                this.Functions = new SortedDictionary<string, MethodInfo>();
                this.Properties = new SortedDictionary<string, PropertyInfo>();
            }

            public void AddFunction(MethodInfo f)
            {
                this.Functions[f.Name.ToLower()] = f;
            }

            public void AddProperty(PropertyInfo p)
            {
                this.Properties[p.Name.ToLower()] = p;
            }

            public string GetHelpStringOfFunction(string functionName)
            {
                MethodInfo function = this.Functions[functionName];
                string fullName = this.IsBuiltInLib ? function.Name : $"{this.Type.Name}.{function.Name}";
                var parameterDefs = function.GetParameters();
                List<string> parameterDesc = new List<string>();
                foreach (var parameter in parameterDefs)
                {
                    parameterDesc.Add($"{parameter.Name}");
                }
                string parametersDesc = parameterDesc.Count > 0 ? String.Join(", ", parameterDesc) : "";
                Doc attr = (Doc)function.GetCustomAttribute(typeof(Doc));
                string doc = attr == null ? "" : $" : {attr.Content}";
                return $" * {fullName}({parametersDesc}){doc}";
            }

            public string GetHelpStringOfProperty(string propertyName)
            {
                PropertyInfo property = this.Properties[propertyName];
                string fullName = $"{this.Type.Name}.{property.Name}";
                Doc attr = (Doc)property.GetCustomAttribute(typeof(Doc));
                string doc = attr == null ? "" : $" : {attr.Content}";
                return $" * {fullName}{doc}";
            }

            public string GetHelpString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"\n# {this.Type.Name}\n\n");
                foreach (string functionName in this.Functions.Keys)
                {
                    sb.Append(GetHelpStringOfFunction(functionName));
                    sb.Append('\n');
                }
                foreach (string propertyName in this.Properties.Keys)
                {
                    sb.Append(GetHelpStringOfProperty(propertyName));
                    sb.Append('\n');
                }
                return sb.ToString();
            }
        }

        private SortedDictionary<string, Lib> Libs { get; set; }

        public static string BuiltInLibName = typeof(ISB.Lib.BuiltIn).Name;

        public Libraries()
        {
            this.Libs = new SortedDictionary<string, Lib>();
            this.AutoLoadStandardLibs();
        }

        public string GetHelpString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, Lib> kvp in this.Libs)
            {
                sb.Append(kvp.Value.GetHelpString());
            }
            return sb.ToString();
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
            => this.HasFunction(BuiltInLibName, functionName);

        public int GetArgumentNumber(string libName, string functionName)
            => this.HasFunction(libName, functionName) ?
                this.GetFunction(libName, functionName).GetParameters().Length : 0;

        public int GetArgumentNumber(string functionName)
            => this.GetArgumentNumber(BuiltInLibName, functionName);

        public bool HasReturnValue(string libName, string functionName)
            => this.HasFunction(libName, functionName) ?
                !this.GetFunction(libName, functionName).ReturnType.Equals(typeof(void)) : false;

        public bool HasReturnValue(string functionName)
            => this.HasReturnValue(BuiltInLibName, functionName);

        public BaseValue GetPropertyValue(string libName, string propertyName)
            => this.HasProperty(libName, propertyName) ?
                (BaseValue)this.GetProperty(libName, propertyName).GetValue(this.GetInstance(libName)) :
                StringValue.Empty;

        public bool SetPropertyValue(string libName, string propertyName, BaseValue value)
        {
            if (!this.HasProperty(libName, propertyName))
                return false;

            var property = this.GetProperty(libName, propertyName);
            var castValue = ConvertBaseValueTo(value, property.PropertyType);
            if (castValue == null)
                return false;
            property.SetValue(this.GetInstance(libName), value);
            return true;
        }

        public bool InvokeFunction(string functionName, object[] parameters, out BaseValue retValue)
            => this.InvokeFunction(BuiltInLibName, functionName, parameters, out retValue);

        public bool InvokeFunction(string libName, string functionName, object[] parameters, out BaseValue retValue)
        {
            retValue = null;
            if (!this.HasFunction(libName, functionName))
                return false;

            var function = this.GetFunction(libName, functionName);
            if (parameters != null && parameters.Length > 0)
            {
                var parameterDefs = function.GetParameters();
                if (parameterDefs.Length != parameters.Length)
                    return false;
                List<BaseValue> castParameters = new List<BaseValue>();
                for (int i = 0; i < parameters.Length; i++)
                {
                    var castValue = ConvertBaseValueTo((BaseValue)parameters[i], parameterDefs[i].ParameterType);
                    if (castValue == null)
                        return false;
                    castParameters.Add(castValue);
                }
                retValue = (BaseValue)function.Invoke(this.GetInstance(libName), castParameters.ToArray());
            }
            else
            {
                retValue = (BaseValue)function.Invoke(this.GetInstance(libName), null);
            }
            return true;
        }

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

        private static BaseValue ConvertBaseValueTo(BaseValue value, Type targetType)
        {
            if (targetType.Equals(typeof(BaseValue)) || value.GetType().Equals(targetType))
                return value;

            if (targetType.Equals(typeof(NumberValue)))
            {
                return new NumberValue(value.ToNumber());
            }
            else if (targetType.Equals(typeof(StringValue)))
            {
                return new StringValue(value.ToDisplayString());
            }
            else if (targetType.Equals(typeof(BooleanValue)))
            {
                return new BooleanValue(value.ToBoolean());
            }
            else
            {
                // Returns null if the lib function/property accepts an ArrayValue but an object of other
                // value types is passed in.
                return null;
            }
        }

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
                        lib.AddFunction(m);
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