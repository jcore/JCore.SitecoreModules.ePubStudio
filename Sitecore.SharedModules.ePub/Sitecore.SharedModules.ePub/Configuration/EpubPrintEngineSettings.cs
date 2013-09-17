using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedModules.ePub.Configuration
{
    public class EpubPrintEngineSettings
    {
        private readonly ISettingsSection settingsSection;

        public bool StoreMastersInDb
        {
            get
            {
                return this.settingsSection.GetBoolSetting("ePubStudio.StoreMastersInDB", true);
            }
        }

        
        public EpubPrintEngineSettings(ISettingsSection settingsSection)
        {
            this.settingsSection = settingsSection;
        }
    }
}
