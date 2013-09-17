using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Sitecore.Xml;

namespace Sitecore.SharedModules.ePub.Configuration
{
    public class SettingsSection : ISettingsSection
    {
        /// <summary>
        /// The settings list
        /// </summary>
        private readonly Dictionary<string, string> settingsList = new Dictionary<string, string>();

        /// <summary>
        /// Adds to settings list.
        /// </summary>
        /// <param name="node">The node.</param>
        public void AddToSettingsList(XmlNode node)
        {
            this.settingsList.Add(XmlUtil.GetAttribute("name", node).ToLower(), XmlUtil.GetAttribute("value", node));
        }

        /// <summary>
        /// Gets the int setting.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public int GetIntSetting(string key, int defaultValue)
        {
            int result;
            if (this.settingsList.ContainsKey(key.ToLower()) && int.TryParse(this.settingsList[key.ToLower()], out result))
                return result;
            else
                return defaultValue;
        }

        /// <summary>
        /// Gets the setting.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public string GetSetting(string key, string defaultValue)
        {
            if (this.settingsList.ContainsKey(key.ToLower()))
                return this.settingsList[key.ToLower()];
            else
                return defaultValue;
        }

        /// <summary>
        /// Gets the setting.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetSetting(string key)
        {
            return this.GetSetting(key.ToLower(), string.Empty);
        }

        /// <summary>
        /// Gets the bool setting.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <returns></returns>
        public bool GetBoolSetting(string key, bool defaultValue)
        {
            bool result;
            if (this.settingsList.ContainsKey(key.ToLower()) && bool.TryParse(this.settingsList[key.ToLower()], out result))
                return result;
            else
                return defaultValue;
        }
    }
}
