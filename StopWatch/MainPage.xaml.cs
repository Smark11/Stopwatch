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
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using Microsoft.Phone.Tasks;
using System.Windows.Media;
using Windows.ApplicationModel.Store;
using Common.IsolatedStoreage;
using Common.Utilities;

namespace StopWatch
{

    public partial class MainPage : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        // Constructor
        public MainPage()
        {
            string hasAppBeenRated = string.Empty;

            InitializeComponent();

            AdvertisingVisibility = Visibility.Visible;

            //5th, 10th, 15th time prompt, 20th time ok only to rate, never prompt them again after they rate.
            Rate.RateTheApp(AppResources.RateTheAppQuestion, AppResources.RateTheAppPrompt, AppResources.RateAppHeader);

            hasAppBeenRated = Rate.HasAppBeenRated();
            if (hasAppBeenRated.ToUpper() == "YES")
            {
                EnableCountdown();
            }
            else
            {
                DisableCountdown();
            }

            BuildLocalizedApplicationBar(hasAppBeenRated);

            SetLockScreenSetting();

            MyAdControl.ErrorOccurred += MyAdControl_ErrorOccurred;

            this.DataContext = this;
        }

        void MyAdControl_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            Console.WriteLine(e.Error);
        }

        #region "Properties"
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private Visibility _advertisingVisibility;
        public Visibility AdvertisingVisibility
        {
            get { return _advertisingVisibility; }
            set
            {
                _advertisingVisibility = value;
                NotifyPropertyChanged("AdvertisingVisibility");
            }
        }

        private String _pivotName;
        public String PivotName
        {
            get
            {
                _pivotName = (pivotControl.SelectedItem as PivotItem).Header as string;
                return _pivotName.ToUpper();
            }
        }

        #endregion "Properties"

        #region "Events"

        private void pivotControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Need to change the appbar and menu item captions depending on wich pivot
            //Options should either be Stopwatch options or Countdown options
            foreach (ApplicationBarMenuItem item in ApplicationBar.MenuItems)
            {
                if (item.Text.Contains("Options"))
                {
                    if (PivotName == "STOPWATCH")
                    {
                        item.Text = AppResources.StopWatchOptions;
                    }
                    else
                    {
                        item.Text = AppResources.CountdownOptions;
                    }
                }
            }

            foreach (ApplicationBarIconButton item in ApplicationBar.Buttons)
            {

            }
        }

        private void Email_Click(object sender, EventArgs e)
        {
            EmailComposeTask emailCompuser;
            string emailBody = string.Empty;

            switch (PivotName)
            {
                case "STOPWATCH":
                    emailBody = BuildEmailBodyStopwatch();
                    break;
                case "COUNTDOWN":
                    emailBody = string.Empty;
                    break;
            }

            emailCompuser = new EmailComposeTask();
            emailCompuser.Subject = AppResources.EmailSubject;
            emailCompuser.Body = emailBody;
            emailCompuser.Show();
        }

        private void DeleteLaps_Click(object sender, EventArgs e)
        {
            switch (PivotName)
            {
                case "STOPWATCH":
                    MessageBoxResult result = MessageBox.Show(AppResources.DeleteAllLapDataMessage, AppResources.DeleteLapsTitle, MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        IS.RemoveSetting("Stopwatch-Laps");
                        IS.RemoveSetting("Stopwatch-Splits");
                        this.stopwatchControl.StopwatchTimesCollection.Clear();
                    }

                    break;
                case "COUNTDOWN":
                    break;
            }
        }

        private void Options_Click(object sender, EventArgs e)
        {
            switch (PivotName)
            {
                case "STOPWATCH":
                    NavigationService.Navigate(new Uri("/StopwatchOptions.xaml", UriKind.Relative));
                    break;
                case "COUNTDOWN":
                    NavigationService.Navigate(new Uri("/CountdownOptions.xaml", UriKind.Relative));
                    break;
            }
        }

        private void About_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private void Review_Click(object sender, EventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        private void MoreApps_Click(object sender, EventArgs e)
        {
            MarketplaceSearchTask marketplaceSearchTask = new MarketplaceSearchTask();

            marketplaceSearchTask.SearchTerms = "KLBCreations";
            marketplaceSearchTask.Show();
        }

        private void AddCountdown_Click(object sender, EventArgs e)
        {
            MessageBoxResult msgResult;
            string hasAppBeenRated = string.Empty;

            hasAppBeenRated = Rate.HasAppBeenRated();

            if (hasAppBeenRated.ToUpper() == "YES")
            {
                return;
            }

            msgResult = MessageBox.Show(AppResources.AddCountdownQuestion, AppResources.RateAppHeader, MessageBoxButton.OKCancel);
            if (msgResult == MessageBoxResult.OK)
            {
                MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
                marketplaceReviewTask.Show();

                IS.SaveSetting("AppRated", "Yes");
                EnableCountdown();

                foreach (ApplicationBarMenuItem item in ApplicationBar.MenuItems)
                {
                    if (item.Text == AppResources.AppMenuItemAddCountdown)
                    {
                        //Disable it now that it is no longer needed, next time app is started it will not be even loaded as a menu option
                        item.IsEnabled = false;
                    }
                }

            }
            else
            {
                IS.SaveSetting("AppRated", "No");
            }
        }

        #endregion "Events"

        #region "Methods"

        private void SetLockScreenSetting()
        {
            string lockScreenValue = string.Empty;

            if (IS.GetSettingStringValue("Stopwatch-LockScreen") != string.Empty)
            {
                lockScreenValue = IS.GetSettingStringValue("Stopwatch-LockScreen");
                if (lockScreenValue == "Enabled")
                {
                    PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
                }
                else
                {
                    PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
                }
            }
            else //if lockscreen setting not set then default to disabling lockscreen
            {
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
                IS.SaveSetting("Stopwatch-LockScreen", "Disabled");
            }
        }

        public string BuildEmailBodyStopwatch()
        {
            string returnString = string.Empty;

            try
            {
                returnString = AppResources.EmailBody + "\n";
                foreach (var item in this.stopwatchControl.StopwatchTimesCollection)
                {
                    returnString = returnString + item.ItemCount + '/' + item.SplitTime + '/' + item.LapTime + "\n";
                }
            }
            catch (Exception)
            {

                return returnString;
            }
            return returnString;
        }

        private void EnableCountdown()
        {
            this.pivotCountdown.IsEnabled = true;
            this.countdownControl.btnCountdownHowTo.Visibility = Visibility.Collapsed;
        }

        private void DisableCountdown()
        {
            this.pivotCountdown.IsEnabled = false;
            this.countdownControl.btnCountdownHowTo.Visibility = Visibility.Visible;
        }



        #endregion "Methods"

        #region "Common Routines"

        private void BuildLocalizedApplicationBar(string hasAppBeenRated)
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 1.0;
            ApplicationBar.IsVisible = true;
            ApplicationBar.IsMenuEnabled = true;

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/feature.email.png", UriKind.Relative));
            appBarButton.Text = AppResources.AppBarEmailButton;
            ApplicationBar.Buttons.Add(appBarButton);
            appBarButton.Click += new EventHandler(Email_Click);

            ApplicationBarIconButton appBarButton2 = new ApplicationBarIconButton(new Uri("/Assets/cancel.png", UriKind.Relative));
            appBarButton2.Text = AppResources.AppBarClearLapsButton;
            ApplicationBar.Buttons.Add(appBarButton2);
            appBarButton2.Click += new EventHandler(DeleteLaps_Click);

            // Create a new menu item with the localized string from AppResources.
            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppMenuItemOptions);
            ApplicationBar.MenuItems.Add(appBarMenuItem);
            appBarMenuItem.Click += new EventHandler(Options_Click);

            ApplicationBarMenuItem appBarMenuItem2 = new ApplicationBarMenuItem(AppResources.AppMenuItemAbout);
            ApplicationBar.MenuItems.Add(appBarMenuItem2);
            appBarMenuItem2.Click += new EventHandler(About_Click);

            ApplicationBarMenuItem appBarMenuItem4 = new ApplicationBarMenuItem(AppResources.AppMenuItemMoreApps);
            ApplicationBar.MenuItems.Add(appBarMenuItem4);
            appBarMenuItem4.Click += new EventHandler(MoreApps_Click);

            if (hasAppBeenRated.ToUpper() == "NO")
            {
                ApplicationBarMenuItem appBarMenuItem5 = new ApplicationBarMenuItem(AppResources.AppMenuItemAddCountdown);
                ApplicationBar.MenuItems.Add(appBarMenuItem5);
                appBarMenuItem5.Click += new EventHandler(AddCountdown_Click);
            }
        }

        private void PhoneOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            

            switch (PivotName)
            {
                case "STOPWATCH":
                    // Switch the placement of the buttons based on an orientation change.
                    if ((e.Orientation & PageOrientation.Portrait) == (PageOrientation.Portrait))
                    {
                        Grid.SetRow(MyAdControl, 0);
                        Grid.SetRow(this.stopwatchControl.ContentPanel, 1);
                        Grid.SetRowSpan(this.stopwatchControl.ContentPanel, 2);
                        Grid.SetRow(this.stopwatchControl.ButtonPanel, 3);
                        this.stopwatchControl.LapBorder.Visibility = Visibility.Visible;
                        this.stopwatchControl.LapGrid.Visibility = Visibility.Visible;
                    }
                    // If not in portrait, move buttonList content to visible row and column.
                    else
                    {
                        Grid.SetRow(MyAdControl, 0);
                        Grid.SetRow(this.stopwatchControl.ContentPanel, 1);
                        Grid.SetRowSpan(this.stopwatchControl.ContentPanel, 2);
                        Grid.SetRow(this.stopwatchControl.ButtonPanel, 4);
                        this.stopwatchControl.LapBorder.Visibility = Visibility.Collapsed;
                        this.stopwatchControl.LapGrid.Visibility = Visibility.Collapsed;                     
                    }
                    break;
                case "COUNTDOWN":
                    // Switch the placement of the buttons based on an orientation change.
                    if ((e.Orientation & PageOrientation.Portrait) == (PageOrientation.Portrait))
                    {
                        Grid.SetRow(MyAdControl, 0);
                        Grid.SetRow(this.countdownControl.ContentPanel, 1);
                        Grid.SetRowSpan(this.countdownControl.ContentPanel, 2);
                        Grid.SetRow(this.countdownControl.ButtonPanel, 3);
                        this.countdownControl.LapBorder.Visibility = Visibility.Visible;
                        this.countdownControl.LapGrid.Visibility = Visibility.Visible;
                    }
                    // If not in portrait, move buttonList content to visible row and column.
                    else
                    {
                        Grid.SetRow(MyAdControl, 0);
                        Grid.SetRow(this.countdownControl.ContentPanel, 1);
                        Grid.SetRowSpan(this.countdownControl.ContentPanel, 2);
                        Grid.SetRow(this.countdownControl.ButtonPanel, 4);
                        this.countdownControl.LapBorder.Visibility = Visibility.Collapsed;
                        this.countdownControl.LapGrid.Visibility = Visibility.Collapsed;
                    }
                    break;
            }
        }

        #endregion "Common Routines"


    }
}