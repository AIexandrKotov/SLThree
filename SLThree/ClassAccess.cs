using SLThree.Extensions;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SLThree
{
    public class ClassAccess
    {
        public Type Name;
        public ClassAccess(Type name)
        {
            Name = name;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Name.GetTypeString()} {{");
            //var methods = Name.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var static_methods = Name.GetMethods(BindingFlags.Public | BindingFlags.Static);
            //var fields = Name.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var static_fields = Name.GetFields(BindingFlags.Public | BindingFlags.Static);
            //var props = Name.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var static_props = Name.GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var x in static_fields)
            {
                sb.AppendLine($"    {(x.IsInitOnly?"const ":"")}{x.FieldType.GetTypeString()} {x.Name};");
            }
            /*if (!Name.IsSealed && !Name.IsAbstract)
            foreach (var x in fields)
            {
                sb.AppendLine($"    {x.Name};");
            }*/
            foreach (var x in static_props)
            {
                sb.Append($"    {x.PropertyType.GetTypeString()} {x.Name} {{ ");
                if (x.GetMethod != null) sb.Append("get; ");
                if (x.SetMethod != null) sb.Append("set; ");
                sb.AppendLine("}");
            }
            /*if (!Name.IsSealed && !Name.IsAbstract)
                foreach (var x in props)
            {
                sb.Append($"    {x.Name} {{ ");
                if (x.GetMethod != null) sb.Append("get; ");
                if (x.SetMethod != null) sb.Append("set; ");
                sb.AppendLine("}");
            }*/
            foreach (var x in static_methods)
            {
                if (x.Name.StartsWith("get_") || x.Name.StartsWith("set_")) continue;
                sb.Append($"    {x.ReturnType.GetTypeString()} {x.Name}");
                if (x.IsGenericMethodDefinition)
                {
                    sb.Append($"<{x.GetGenericArguments().Select(a => a.Name).JoinIntoString(", ")}>");
                }
                sb.Append("(");
                sb.Append(x.GetParameters().ConvertAll(p => p.ParameterType.GetTypeString()).JoinIntoString(", "));
                sb.AppendLine(");");
            }
            /*if (!Name.IsSealed && !Name.IsAbstract)
                foreach (var x in methods)
            {
                if (x.Name.StartsWith("get_") || x.Name.StartsWith("set_")) continue;
                sb.Append($"    {x.Name}(");
                sb.Append(x.GetParameters().ConvertAll(p => p.ParameterType.GetTypeString()).JoinIntoString(", "));
                sb.AppendLine(");");
            }*/

            sb.AppendLine("}");

            return sb.ToString();
        }

        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
        public sealed class DocAttribute : Attribute
        {
            public readonly string Description;

            public DocAttribute(string description)
            {
                Description = description;
            }
        }
    }
}
