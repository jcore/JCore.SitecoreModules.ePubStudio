using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sitecore.SharedModules.ePub.Configuration
{
    public interface ISettingsSection
    {
        void AddToSettingsList(XmlNode node);

        int GetIntSetting(string key, int defaultValue);

        string GetSetting(string key, string defaultValue);

        string GetSetting(string key);

        bool GetBoolSetting(string key, bool defaultValue);
    }
}
