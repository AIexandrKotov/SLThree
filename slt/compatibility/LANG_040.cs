using SLThree;
using SLThree.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using static slt.Program;

namespace slt
{
    public static class LANG_040
    {
        public static bool Supports { get; internal set; }
        internal static void Init()
        {
            IChooserType = SLThreeAssembly.GetType("System.Collections.IChooser");
            IChanceChooserType = SLThreeAssembly.GetType("System.Collections.IChanceChooser");
            IEqualchanceChooserType = SLThreeAssembly.GetType("System.Collections.IEqualchanceChooser");
            SLThreeExtensions = SLThreeAssembly.GetType("SLThree.Extensions.SLThreeExtensions");
            ToDynamicPercentsMethod = SLThreeExtensions.GetMethod("ToDynamicPercents");
            IChanceChooser_Values = IChanceChooserType.GetProperty("Values");
            IEqualchanceChooser_Values = IEqualchanceChooserType.GetProperty("Values");
            ContextWrapDecoration = SLThreeAssembly.GetType("SLThree.ExecutionContext+ContextWrap").GetField("Decoration");
            ContextWrapDecoration.SetValue(null, (Func<object, object>)GetOutput);
            ContextName = SLThreeAssembly.GetType("SLThree.ExecutionContext").GetField("Name");

            SystemTypes = SLThreeAssembly.GetType("SLThree.UsingStatement").GetProperty("SystemTypes").GetValue(null).Cast<Dictionary<string, Type>>();
            RegisterNewSystemTypes();
        }

        public static void RegisterNewSystemTypes()
        {
            foreach (var x in Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.FullName.StartsWith("slt.sys.")).ToDictionary(x => x.Name, x => x))
            {
                SystemTypes.Add(x.Key, x.Value);
            }
        }

        public static string NameOfContext(ExecutionContext context)
        {
            return (string)ContextName.GetValue(context);
        }

        private static Type IChooserType;
        private static Type IChanceChooserType;
        private static Type IEqualchanceChooserType;
        private static Type SLThreeExtensions;
        private static PropertyInfo IChanceChooser_Values;
        private static PropertyInfo IEqualchanceChooser_Values;
        private static FieldInfo ContextWrapDecoration;
        private static FieldInfo ContextName;
        public static Dictionary<string, Type> SystemTypes { get; private set; }
        private static IList<(object, double)> GET_ChanceChooser_Values(object value)
        {
            return IChanceChooser_Values.GetValue(value) as IList<(object, double)>;
        }
        private static IList<object> GET_EqualchanceChooser_Values(object value)
        {
            return IEqualchanceChooser_Values.GetValue(value) as IList<object>;
        }
        private static MethodInfo ToDynamicPercentsMethod;
        private static string ToDynamicPercents(this double value)
        {
            return ToDynamicPercentsMethod.Invoke(null, new object[1] { value }) as string;
        }

        private static bool IsIChooser(Type type)
        {
            return type.GetInterfaces().Contains(IChooserType);
        }

        private static bool IsIChanceChooserType(Type type)
        {
            return type.GetInterfaces().Contains(IChanceChooserType);
        }

        private static bool IsIEqualchanceChooserType(Type type)
        {
            return type.GetInterfaces().Contains(IEqualchanceChooserType);
        }

        public static object GetChoosersOutput(object value)
        {
            if (value == null) return value;
            value = LANG_030.GetChoosersOutput(value);
            var type = value.GetType();
            if (IsIChooser(type))
            {
                if (IsIChanceChooserType(type))
                {
                    var values = GET_ChanceChooser_Values(value);
                    value = values.Count > 10
                        ? $"({values.Take(10).Select(x => $"{x.Item1}: {ToDynamicPercents(x.Item2)}").JoinIntoString(" \\ ")}...)"
                        : $"({values.Select(x => $"{x.Item1}: {ToDynamicPercents(x.Item2)}").JoinIntoString(" \\ ")})";
                }
                else if (IsIEqualchanceChooserType(type))
                {
                    var values = GET_EqualchanceChooser_Values(value);
                    value = values.Count > 10
                        ? $"({values.Take(10).JoinIntoString(" \\ ")}...)"
                        : $"({values.JoinIntoString(" \\ ")})";
                }
            }
            return value;
        }
    }
}
