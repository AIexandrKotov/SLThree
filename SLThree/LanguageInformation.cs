using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public sealed class LanguageInformation : ILanguageInformation
    {
        /// <summary>
        /// Название языка
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Версия языка
        /// </summary>
        public string Version { get; private set; }
        /// <summary>
        /// Издание языка
        /// </summary>
        public string Edition { get; private set; }
        /// <summary>
        /// Парсер языка
        /// </summary>
        public IParser Parser { get; private set; }
        /// <summary>
        /// Ресторатор языка
        /// </summary>
        public IRestorator Restorator { get; private set; }
        /// <summary>
        /// Сериализатор SLThree
        /// </summary>
        public ISerializator Serializator { get; private set; }

        /// <summary>
        /// Интерфейс парсера в SLThree представление
        /// </summary>
        public interface IParser
        {
            BaseStatement ParseScript(string code, string fileName);
            BaseExpression ParseExpression(string code, string fileName);
        }

        /// <summary>
        /// Интерфейс ресторатора SLThree представления в код
        /// </summary>
        public interface IRestorator
        {
            string Restore(BaseStatement statement);
            string Restore(BaseExpression expression);
        }

        /// <summary>
        /// Сериализатор
        /// </summary>
        public interface ISerializator
        {
            byte[] Serialize(ExecutionContext.IExecutable statement);
            ExecutionContext.IExecutable Deserialize(byte[] bytes);
        }

        private static Dictionary<Assembly, LanguageInformation> registred = new Dictionary<Assembly, LanguageInformation>();
        private static Dictionary<string, LanguageInformation> byLangName = new Dictionary<string, LanguageInformation>();

        /// <summary>
        /// Получить информацию о языке в сборке.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static LanguageInformation GetInformation(Assembly assembly)
        {
            if (registred.TryGetValue(assembly, out var info)) return info;
            var ret = Activator.CreateInstance(assembly.GetTypes().FirstOrDefault(x => x.BaseType == typeof(ILanguageInformation))) as LanguageInformation;
            return byLangName[ret.Name] = registred[assembly] = new LanguageInformation()
            {
                Edition = ret.Edition,
                Name = ret.Name,
                Version = ret.Version,
                Parser = ret.Parser,
                Restorator = ret.Restorator
            };
        }
        public static LanguageInformation GetForLanguage(string languageName) => byLangName.TryGetValue(languageName, out var value) ? value : null;
    }
}
