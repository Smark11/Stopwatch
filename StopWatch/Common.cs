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

        #region "App Opened"

        //This proc will add 1 to the number of times the app has been opened and return that value and save that value
        public int AppOpened()
        {
            int returnValue = 0;
            string settingValue = string.Empty;
            IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;

            try
            {
                if (appSettings.Contains("Stopwatch-AppOpenedCount"))
                {
                    settingValue=GetSetting("Stopwatch-AppOpenedCount");
                    returnValue = Convert.ToInt16(settingValue)+1;
                    SaveSettings("Stopwatch-AppOpenedCount", returnValue.ToString());
                }
                else   //has not been opened yet so intitialize as first time being opened
                {
                    SaveSettings("Stopwatch-AppOpenedCount", "1");
                    returnValue = 1;
                }
            }
            catch (Exception)
            {;
                return 0;
            }
            return returnValue;
        }


        public string HasAppBeenRated()
        {
            string returnValue = string.Empty;
            IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;
            try
            {
                if (appSettings.Contains("Stopwatch-AppRated"))
                {
                    returnValue = GetSetting("Stopwatch-AppRated");
                }
                else
                {
                    SaveSettings("Stopwatch-AppRated", "No");
                    returnValue = "No";
                }
            }
            catch (Exception)
            {
                return "No";
            }
            return returnValue;
        }

        #endregion "App Opened"

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
