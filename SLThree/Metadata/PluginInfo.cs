using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SLThree.Metadata
{
    public interface IPluginInfo
    {
        /// <summary>
        /// Название модуля
        /// </summary>
        string Name { get; }
        /// <summary>
        /// True если сборка модуля регистрируется и доступна конструкциями SLThree
        /// </summary>
        bool Insert { get; }
    }

    public interface IDescription
    {
        string Description { get; }
        string ChangeLog { get; }
    }

    public interface ILanguageProvider
    {
        /// <summary>
        /// Издание языка
        /// </summary>
        string Edition { get; }
        /// <summary>
        /// Парсер языка
        /// </summary>
        IParser Parser { get; }
        /// <summary>
        /// Реставратор языка
        /// </summary>
        IRestorator Restorator { get; }
    }

    public interface ILocalizationPlugin
    {
        IDictionary<string, IDictionary<string, string>> Strings { get; }
    }

    public interface IApiPlugin
    {
        KeyValuePair<string, Type>[] NewTypes { get; }
    }

    public interface ISerializator
    {
        byte[] Serialize(ExecutionContext.IExecutable executable);
        ExecutionContext.IExecutable Deserialize(byte[] bytes);
    }

    /// <summary>
    /// Специальный атрибут, которым помечаются метаданные языка SLThree.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SLThreeLanguageAttribute : Attribute
    {
        public SLThreeLanguageAttribute() { }
    }

    public sealed class Plugin : IPluginInfo
    {
        public class LanguageProvider : ILanguageProvider
        {
            public string Name { get; internal set; }
            public string Version { get; internal set; }
            public string Edition { get; internal set; }
            public IParser Parser { get; internal set; }
            public IRestorator Restorator { get; internal set; }
            public ISerializator Serializator { get; internal set; }
        }

        [Flags]
        public enum PluginType
        {
            None = 0,
            Description = 0x1,
            Localization = 0x2,
            Language = 0x4,
            API = 0x8,
        }

        public string Name { get; private set; }
        public Assembly Assembly { get; private set; }
        public bool AssemblyRegistred => DotnetEnvironment.RegistredAssemblies.Contains(Assembly);
        public PluginType Type { get; private set; }
        public string Version { get; private set; }

        public bool HasDescription => Type.HasFlag(PluginType.Description);
        public IDescription Description { get; private set; }
        public bool IsLocalization => Type.HasFlag(PluginType.Localization);
        public ILocalizationPlugin Localization { get; private set; }
        public bool IsLanguage => Type.HasFlag(PluginType.Language);
        public LanguageProvider Language { get; private set; }
        public bool IsApi => Type.HasFlag(PluginType.API);
        public IApiPlugin Api { get; private set; }

        private static readonly List<Plugin> addedPlugins = new List<Plugin>();
        public static Plugin[] AddedPlugins => addedPlugins.ToArray();
        private static string GetVersionOf(string path)
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(path).ProductVersion;
            var index = fileVersionInfo.IndexOf('+');
            var getted = index != -1 ? fileVersionInfo.Substring(0, index) : fileVersionInfo;
            return getted;
        }
        private static void Apply(Plugin plugin)
        {
            if (plugin.IsLocalization)
                ApplyLocalization(plugin.Localization);
            if (plugin.IsApi)
                ApplyApi(plugin.Api);
        }
        private static void ApplyLocalization(ILocalizationPlugin localizationPlugin)
        {
            foreach (var x in localizationPlugin.Strings)
            {
                var locale = Locale.RegistredLocales[x.Key];
                foreach (var str in x.Value)
                    locale.Strings[str.Key] = str.Value;
            }
        }
        private static void ApplyApi(IApiPlugin apiPlugin)
        {
            foreach (var x in apiPlugin.NewTypes)
                DotnetEnvironment.SystemTypes[x.Key] = x.Value;
        }
        internal static Plugin AddOrGetPlugin(string path, Assembly assembly, bool versionFromFile)
        {
            if (addedPlugins.TryGetAny(x => x.Assembly.Location == assembly.Location, out var pl)) return pl;

            string GetVersion()
            {
                if (versionFromFile) return GetVersionOf(path);
                else return assembly.GetName().Version.ToString(4);
            }
            var version = GetVersion();

            var pluginInfoType = assembly.GetTypes().FirstOrDefault(x => x.GetInterfaces().Contains(typeof(IPluginInfo)));
            if (pluginInfoType == null) return null;

            var pluginInfo = Activator.CreateInstance(pluginInfoType) as IPluginInfo;
            var plugin = new Plugin()
            {
                Name = pluginInfo.Name,
                Version = version,
                Assembly = assembly,
            };

            if (pluginInfo is IDescription description)
            {
                plugin.Description = description;
                plugin.Type |= PluginType.Description;
            }

            if (pluginInfo is ILocalizationPlugin localization)
            {
                plugin.Localization = localization;
                plugin.Type |= PluginType.Localization;
            }

            if (pluginInfo is ILanguageProvider language)
            {
                var isSLThree = pluginInfoType.GetCustomAttribute<SLThreeLanguageAttribute>() != null;
                plugin.Language = new LanguageProvider()
                {
                    Name = isSLThree ? "SLThree" : plugin.Name,
                    Version = version,
                    Edition = language.Edition,
                    Parser = language.Parser,
                    Restorator = language.Restorator,
                    Serializator = new Serializator(),
                };
                plugin.Type |= PluginType.Language;
            }

            if (pluginInfo is IApiPlugin api)
            {
                plugin.Api = api;
                plugin.Type |= PluginType.API;
            }

            // APPLY

            if (pluginInfo.Insert)
                DotnetEnvironment.RegistredAssemblies.Add(assembly);
            Apply(plugin);

            addedPlugins.Add(plugin);
            return plugin;
        }

        public static Assembly[] IgnoredAssemblies = new Assembly[]
        {
            typeof(BaseExpression).Assembly,
        };

        public override string ToString() => $"{Name} {Type}";

        /// <summary>
        /// Добавить плагин-сборку по пути файла. Версия будет соответствовать версии файла.
        /// </summary>
        public static Plugin AddOrGetPlugin(string path)
        {
            return AddOrGetPlugin(path, Assembly.LoadFrom(path), true);
        }
        /// <summary>
        /// Добавить плагин-сборку. Версия будет соответствовать версии сборки.
        /// </summary>
        public static Plugin AddOrGetPlugin(Assembly assembly)
        {
            return AddOrGetPlugin(assembly.Location, assembly, false);
        }
        public static Plugin[] CollectPluginsByPath(string path)
        {
            return Directory.GetFiles((path), "*.dll", SearchOption.AllDirectories).Select(x => AddOrGetPlugin(x)).Where(x => x != null).ToArray();
        }
        public static Plugin[] CollectPlugins() => CollectPluginsByPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

        bool IPluginInfo.Insert => AssemblyRegistred;
    }
}
