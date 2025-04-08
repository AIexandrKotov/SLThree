## [EN]
Plugins are ready-made .NET libraries loaded in Runtime into the SLThree interpreter. The following restrictions apply to plugins:

- Plugins must be of the same version.NET, which is the same as the interpreter, for example .NET 8.0

- Plugins must contain a class implementing IPluginInfo as well as implementations specifying the purpose of the plugin: ILanguageProvider for programming languages for the SLThree object model, ILocalizationPlugin for localizations, IApiPlugin for api plugins (classes from NewTypes will be available in priority in `using`)

## [RU]
Плагины — готовые .NET-библиотеки, загружаемые в Runtime в интерпретатор SLThree. На плагины накладываются следующие ограничения:

- Плагины должны быть той же версии .NET, что и интерпретатор, например .NET 8.0

- Плагины должны содержать класс, реализующий IPluginInfo а также реализации конкретизирующие назначение плагина: ILanguageProvider для языков программирования под объектную модель SLThree, ILocalizationPlugin для локализаций, IApiPlugin для апи-плагинов (классы из NewTypes будут доступны в приоритетном порядке по `using`)