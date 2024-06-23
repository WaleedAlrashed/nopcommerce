using System.Collections.Generic;
using FluentMigrator;
using Nop.Data;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Payments.Tamara.Domain;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Extensions;

namespace Nop.Plugin.Payments.Tamara.Migrations
{
    
    [NopMigration("2023-10-14 00:00:00", "Payments.Tamara 1.07. Add Pay Later message", MigrationProcessType.Update)]
    public class SchemaMigration : MigrationBase
    {

        #region Fields
        private readonly TamaraSettings _tamaraSettings;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public SchemaMigration(TamaraSettings tamaraSettings,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ISettingService settingService)
        {
            _tamaraSettings = tamaraSettings;
            _languageService = languageService;
            _localizationService = localizationService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            //locales
            var (languageId, languages) = this.GetLanguageData();

            _localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Plugins.Payments.Tamara.Fields.DisplayPayLaterMessages"] = "Display Pay Later messages",
                ["Plugins.Payments.Tamara.Fields.DisplayPayLaterMessages.Hint"] = "Determine whether to display Pay Later messages. This message displays how much the customer pays in four payments. The message will be shown next to the Tamara buttons.",
            }, languageId);


            //settings
            if (!_settingService.SettingExists(_tamaraSettings, settings => settings.DisplayPayLaterMessages))
                _tamaraSettings.DisplayPayLaterMessages = false;

            _settingService.SaveSetting(_tamaraSettings);

            Create.TableFor<TamaraPaymentTransaction>();

        }

        /// <summary>
        /// Collects the DOWN migration expressions
        /// </summary>
        public override void Down()
        {
            //nothing

        }

        #endregion
    }
}
