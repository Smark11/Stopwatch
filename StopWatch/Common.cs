using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;

namespace StopWatch
{
    class Common
    {
        public string GetSetting(string settingName)
        {
            String returnValue = string.Empty;
            IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;

            if (appSettings.Contains(settingName))
            {
                returnValue = appSettings[settingName].ToString();
            }

            return returnValue;
        }

        public void SaveSettings(string settingName, string value)
        {
            IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;

            appSettings[settingName] = value;
            appSettings.Save();
        }

        public void RemoveSettings(string settingName)
        {
            IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;

            appSettings.Remove(settingName);
            appSettings.Save();
        }
    }
}
