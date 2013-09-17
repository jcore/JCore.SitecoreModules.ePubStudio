using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.SharedModules.ePub.Configuration
{
    public class ConfigHandler
    {
        private static WebCommonSettings commonSettings;
        private static EpubPrintEngineSettings printStudioEngineSettings;

        public static WebCommonSettings CommonSettings
        {
            get
            {
                if (ConfigHandler.commonSettings != null)
                    return ConfigHandler.commonSettings;
                XmlNode configNode = Factory.GetConfigNode("epubstudio/common");
                Assert.IsNotNull((object)configNode, "epubstudio/common");
                return ConfigHandler.commonSettings = new WebCommonSettings(Factory.CreateObject<ISettingsSection>(configNode));
            }
        }

        public static EpubPrintEngineSettings EpubPrintEngineSettings
        {
            get
            {
                if (ConfigHandler.printStudioEngineSettings != null)
                    return ConfigHandler.printStudioEngineSettings;
                XmlNode configNode = Factory.GetConfigNode("epubstudio/printstudioengine");
                Assert.IsNotNull((object)configNode, "epubstudio/printstudioengine");
                return ConfigHandler.printStudioEngineSettings = new EpubPrintEngineSettings(Factory.CreateObject<ISettingsSection>(configNode));
            }
        }
    }
}
