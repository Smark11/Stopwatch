using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using StopWatch.Resources;
using Common.IsolatedStoreage;

namespace StopWatch
{
    public partial class CountdownOptions : PhoneApplicationPage
    {
        public CountdownOptions()
        {

            InitializeComponent();

            GetLockScreenSetting();
            GetCountdownAlarmSetting();
        }

        #region "Events"

        //OFF Lock screen is not disabled
        private void toggleLockScreen_Checked(object sender, RoutedEventArgs e)
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
            IS.SaveSetting("Stopwatch-LockScreen", "Enabled");
            toggleLockScreen.Content = AppResources.Enabled;
        }

        //ON Lock screen is disabled
        private void toggleLockScreen_Unchecked(object sender, RoutedEventArgs e)
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            IS.SaveSetting("Stopwatch-LockScreen", "Disabled");
            toggleLockScreen.Content = AppResources.Disabled;
        }

        private void togglePlayAlarm_Checked(object sender, RoutedEventArgs e)
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
            IS.SaveSetting("Stopwatch-CountdownAlarm", "Enabled");
            togglePlayAlarm.Content = AppResources.Enabled;
        }

        private void togglePlayAlarm_Unchecked(object sender, RoutedEventArgs e)
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            IS.SaveSetting("Stopwatch-CountdownAlarm", "Disabled");
            togglePlayAlarm.Content = AppResources.Disabled;
        }

        private void toggleDefaultStartTime_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void toggleDefaultStartTime_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        #endregion "Events"

        #region "Methods"

        private void GetLockScreenSetting()
        {
            string lockScreenValue = string.Empty;

            if (IS.GetSettingStringValue("Stopwatch-LockScreen") == string.Empty)
            {
                toggleLockScreen.IsChecked = false;
                toggleLockScreen.Content = AppResources.Disabled;
            }
            else
            {
                lockScreenValue = IS.GetSettingStringValue("Stopwatch-LockScreen");
                if (lockScreenValue == "Enabled")
                {
                    toggleLockScreen.IsChecked = true;
                    toggleLockScreen.Content = AppResources.Enabled;
                }
                else
                {
                    toggleLockScreen.IsChecked = false;
                    toggleLockScreen.Content = AppResources.Disabled;
                }
            }
        }

        private void GetCountdownAlarmSetting()
        {
            string countdownAlarmValue = string.Empty;

            if (IS.GetSettingStringValue("Stopwatch-CountdownAlarm") == string.Empty)
            {
                togglePlayAlarm.IsChecked = false;
                togglePlayAlarm.Content = AppResources.Disabled;
            }
            else
            {
                countdownAlarmValue = IS.GetSettingStringValue("Stopwatch-CountdownAlarm");
                if (countdownAlarmValue == "Enabled")
                {
                    togglePlayAlarm.IsChecked = true;
                    togglePlayAlarm.Content = AppResources.Enabled;
                }
                else
                {
                    togglePlayAlarm.IsChecked = false;
                    togglePlayAlarm.Content = AppResources.Disabled;
                }
            }
        }
        #endregion "Methods"
    }
}