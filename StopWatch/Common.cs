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

        #region "General"

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

        #endregion "General"

        #region "InstallDate"

        public void SaveInstallDate(string settingName, DateTime installDate)
        {
            IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;

            appSettings[settingName] = installDate;
            appSettings.Save();
        }

        public DateTime GetInstallDate(string settingName)
        {
            DateTime returnValue = DateTime.Now;
            IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;

            if (appSettings.Contains(settingName))
            {
                returnValue = (DateTime)appSettings[settingName];
            }
            else
            {
                //no install date so save one
                SaveInstallDate("Stopwatch-InstallDate", DateTime.Now);
            }

            return returnValue;
        }

        public int GetDaySinceInstalled()
        {
            int returnValue = 0;
            IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;
            try
            {
                if (appSettings.Contains("Stopwatch-InstallDate"))
                {
                    DateTime installDate = GetInstallDate("Stopwatch-InstallDate");

                    TimeSpan daysTimespan = DateTime.Now - installDate;

                    returnValue = daysTimespan.Days;
                }
                else
                {
                    //no install date so save one
                    SaveInstallDate("Stopwatch-InstallDate", DateTime.Now);
                }

                return returnValue;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        #endregion "InstallDate"
    }
}
