using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Web;

namespace Sitecore.SharedModules.ePub.Configuration
{
    public class WebCommonSettings
    {
        private readonly Database db;
        private readonly ISettingsSection settingsSection;

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        public string Database
        {
            get
            {
                return this.settingsSection.GetSetting("ePubStudio.Database", "master");
            }
        }

        /// <summary>
        /// Gets the web host.
        /// </summary>
        /// <value>
        /// The web host.
        /// </value>
        public string WebHost
        {
            get
            {
                return WebUtil.GetServerUrl();
            }
        }

        /// <summary>
        /// Gets the domain.
        /// </summary>
        /// <value>
        /// The domain.
        /// </value>
        public string Domain
        {
            get
            {
                return this.settingsSection.GetSetting("ePubStudio.Domain", "sitecore");
            }
        }

        /// <summary>
        /// Gets the standard fields.
        /// </summary>
        /// <value>
        /// The standard fields.
        /// </value>
        public string[] StandardFields
        {
            get
            {
                return this.settingsSection.GetSetting("ePubStudio.StandardFields").Split(new char[1]
        {
          '|'
        }, StringSplitOptions.RemoveEmptyEntries);
            }
        }


        /// <summary>
        /// Gets the folder icon.
        /// </summary>
        /// <value>
        /// The folder icon.
        /// </value>
        public string FolderIcon
        {
            get
            {
                return this.settingsSection.GetSetting("ePubStudio.FolderIcon");
            }
        }

        /// <summary>
        /// Gets the photo scenery icon.
        /// </summary>
        /// <value>
        /// The photo scenery icon.
        /// </value>
        public string PhotoSceneryIcon
        {
            get
            {
                return this.settingsSection.GetSetting("ePubStudio.PhotoSceneryIcon");
            }
        }

        /// <summary>
        /// Gets the fonts service.
        /// </summary>
        /// <value>
        /// The fonts service.
        /// </value>
        public string FontsService
        {
            get
            {
                return this.settingsSection.GetSetting("ePubStudio.FontsService");
            }
        }

        /// <summary>
        /// Gets the core database.
        /// </summary>
        /// <value>
        /// The core database.
        /// </value>
        public string CoreDatabase
        {
            get
            {
                return this.settingsSection.GetSetting("ePubStudio.CoreDatabase", "core");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebCommonSettings"/> class.
        /// </summary>
        /// <param name="settingsSection">The settings section.</param>
        public WebCommonSettings(ISettingsSection settingsSection)
        {
            this.settingsSection = settingsSection;
            this.db = Factory.GetDatabase("master");            
        }

        public string PublishingCachePath
        {
            get
            {
                return this.settingsSection.GetSetting("ePubStudio.PublishingCachePath", "core");
            }
        }
    }
}
