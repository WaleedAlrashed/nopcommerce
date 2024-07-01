using System.Collections.Generic;
using FluentMigrator;
using Nop.Data;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Payments.Tabby;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Extensions;

namespace Nop.Plugin.Payments.Tabby.Migrations
{
    [NopMigration("2024-06-24 00:00:00", "Payments.Tabby 1.00. Add localization resources", MigrationProcessType.Installation)]
    public class TabbyMigration : MigrationBase
    {
        #region Fields

        private readonly TabbySettings _tabbySettings;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public TabbyMigration(TabbySettings tabbySettings,
                              ILanguageService languageService,
                              ILocalizationService localizationService,
                              ISettingService settingService)
        {
            _tabbySettings = tabbySettings;
            _languageService = languageService;
            _localizationService = localizationService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public override void Up()
        {
            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            // Locales
            var (languageId, languages) = this.GetLanguageData();

            _localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["plugins.payments.tabby.fields.configuration"] = "Tabby Payment Configuration",
                ["plugins.payments.tabby.fields.publickey"] = "Public Key",
                ["plugins.payments.tabby.fields.secretkey"] = "Secret Key"
            }, languageId);

            // Settings (if needed, you can add default settings here)
            if (!_settingService.SettingExists(_tabbySettings, settings => settings.PublicKey))
            {
                _tabbySettings.PublicKey = string.Empty;
                _tabbySettings.SecretKey = string.Empty;
                _settingService.SaveSetting(_tabbySettings);
            }
        }

        public override void Down()
        {
            // Optionally remove the locale resources
            var resources = new[]
            {
                "plugins.payments.tabby.fields.configuration",
                "plugins.payments.tabby.fields.publickey",
                "plugins.payments.tabby.fields.secretkey"
            };
            _localizationService.DeleteLocaleResources(resources);
        }

        #endregion
    }
}
