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
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using Microsoft.Phone.Tasks;
using System.IO;
using System.Windows.Media;
using Windows.ApplicationModel.Store;
using Common.IsolatedStoreage;
using Common.Utilities;

namespace StopWatch
{

    public partial class MainPage : INotifyPropertyChanged
    {
        DispatcherTimer _timer;
        TimeSpan _adjustment = new TimeSpan(0, 0, 0);
        DateTime _dateTimeLastStart;
        string _isRunning = "No";

        TimeSpan _lastSplitTime = new TimeSpan(0, 0, 0);

        public event PropertyChangedEventHandler PropertyChanged;

      
        // Constructor
        public MainPage()
        {
            string hasAppBeenRated = string.Empty;

            InitializeComponent();

            //5th, 10th, 15th time prompt, 20th time ok only to rate, never prompt them again after they rate.
            Rate.RateTheApp(AppResources.RateTheAppQuestion,AppResources.RateTheAppPrompt,AppResources.RateAppHeader);

            hasAppBeenRated = Rate.HasAppBeenRated();
            if (hasAppBeenRated.ToUpper() == "YES")
            {
                AdvertisingVisibility = Visibility.Collapsed;
            }
            else
            {
                AdvertisingVisibility = Visibility.Visible;
            }

            // Sample code to localize the ApplicationBar
            BuildLocalizedApplicationBar(hasAppBeenRated);
       
            App.gStopWatch = new Stopwatch();
            _isRunning = IsStopWatchRunning();

            SetLockScreenSetting();
            StopwatchTimesCollection = new ObservableCollection<StopwatchTimes>();

            LoadLapAndSplitData();

            MyAdControl.ErrorOccurred += MyAdControl_ErrorOccurred;

            //Need to determine what/if any adjustment should be made to clock
            //For example, if clock was paused previously we want to start at last clock value with clock paused
            //For example, if clock was running previously we want to add time that has accrued since app was shot down

            if ((IS.GetSettingStringValue("Stopwatch-DateTimeLastStart") == string.Empty) || (_isRunning.ToUpper() == "NO"))
            {
                if (IS.GetSettingStringValue("Stopwatch-DateTimeLastStart") == string.Empty)
                {
                    _adjustment = new TimeSpan();
                }
                else
                {
                    if (IS.GetSettingStringValue("Stopwatch-LastValue") == string.Empty)
                    {
                        _adjustment = new TimeSpan();
                    }
                    else
                    {
                        _adjustment = TimeSpan.Parse(IS.GetSettingStringValue("Stopwatch-LastValue"));
                    }
                }
            }
            else  //clock was running when it was last opened, so add accrued time
            {
                _dateTimeLastStart = DateTime.Parse(IS.GetSettingStringValue("Stopwatch-DateTimeLastStart"));
                _adjustment = TimeSpan.Parse(IS.GetSettingStringValue("Stopwatch-LastValue")) + (DateTime.Now - _dateTimeLastStart);
            }

            _timer = new DispatcherTimer();
            _timer.Tick += new EventHandler(Timer_Tick);
            _timer.Interval = new TimeSpan(0, 0, 0);
            _timer.Start();

            if (_isRunning.ToUpper() == "YES")
            {
                Mode = AppResources.StartText;
                Start.Background = new SolidColorBrush(Colors.Green);
                StartTimer();
            }
            else
            {
                if (_adjustment == new TimeSpan(0, 0, 0))
                {
                    Mode = AppResources.StartText;
                    Start.Background = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    Mode = AppResources.ResumeText;
                    Start.Background = new SolidColorBrush(Colors.Green);
                }
            }

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

        private TimeSpan _clockValue;
        public TimeSpan ClockValue
        {
            get { return _clockValue; }
            set
            {
                _clockValue = value;
                NotifyPropertyChanged("ClockValue");
            }
        }

        private String _clockValueString;
        public String ClockValueString
        {
            get { return _clockValueString; }
            set
            {
                _clockValueString = value;
                NotifyPropertyChanged("ClockValueString");
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

        private ObservableCollection<StopwatchTimes> _stopwatchTimesCollection;
        public ObservableCollection<StopwatchTimes> StopwatchTimesCollection
        {
            get { return _stopwatchTimesCollection; }
            set
            {
                _stopwatchTimesCollection = value;
                NotifyPropertyChanged("StopwatchTimesCollection");
            }
        }

        private string _mode;
        public string Mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                NotifyPropertyChanged("Mode");

                if (_mode == AppResources.StartText)
                {
                    this.Start.IsEnabled = true;
                    this.Reset.IsEnabled = false;
                    this.Lap.IsEnabled = false;
                    IS.SaveSetting("Stopwatch-IsRunning", "No");
                }
                else if (_mode == AppResources.PauseText)
                {
                    this.Start.IsEnabled = true;
                    this.Reset.IsEnabled = true;
                    this.Lap.IsEnabled = true;
                    IS.SaveSetting("Stopwatch-IsRunning", "Yes");
                }
                else if (_mode == AppResources.ResumeText)
                {
                    this.Start.IsEnabled = true;
                    this.Reset.IsEnabled = true;
                    this.Lap.IsEnabled = false;
                    IS.SaveSetting("Stopwatch-IsRunning", "No");
                }
                else if (_mode == AppResources.ExceedText)
                {
                    App.gStopWatch.Stop();
                    ClockValue = new TimeSpan(0, 0, 0);
                    this.Start.IsEnabled = false;
                    this.Reset.IsEnabled = true;
                    this.Lap.IsEnabled = false;
                    IS.SaveSetting("Stopwatch-IsRunning", "No");
                };
            }
        }
        #endregion "Properties"

        #region "Events"

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (ClockValue >= new TimeSpan(9, 0, 0))
            {
                if (Mode != AppResources.ExceedText)
                {
                    MessageBox.Show(AppResources.MaxTimeExceededMessage);
                    Mode = AppResources.ExceedText;
                }
            }
            else if (Mode != AppResources.ExceedText)
            {
                ClockValue = App.gStopWatch.Elapsed + _adjustment;
                ClockValueString = ClockValue.ToString(@"hh\:mm\:ss\.ff");
                IS.SaveSetting("Stopwatch-LastValue", ClockValue.ToString());
            }
        }

        private void Email_Click(object sender, EventArgs e)
        {
            EmailComposeTask emailCompuser;

            emailCompuser = new EmailComposeTask();
            emailCompuser.Subject = AppResources.EmailSubject;
            emailCompuser.Body = BuildEmailBody();
            emailCompuser.Show();
        }

        private void Start_Click(object sender, EventArgs e)
        {
            StartTimer();
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            App.gStopWatch.Stop();
            MessageBoxResult result = MessageBox.Show(AppResources.ResetMessage, AppResources.ResetText, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {

                App.gStopWatch.Reset();
                _adjustment = new TimeSpan();
                _lastSplitTime = new TimeSpan();
                StopwatchTimesCollection.Clear();
                IS.RemoveSetting("Stopwatch-Laps");
                IS.RemoveSetting("Stopwatch-Splits");
                Mode = AppResources.StartText;
                Start.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                App.gStopWatch.Stop();
                Mode = AppResources.ResumeText;
                Start.Background = new SolidColorBrush(Colors.Green);
            }
        }

        private void Lap_Click(object sender, EventArgs e)
        {
            string saveLaps = string.Empty;
            string saveSplits = string.Empty;
            StopwatchTimes stopwatchTimes = new StopwatchTimes();

            stopwatchTimes.ItemCount = StopwatchTimesCollection.Count + 1;
            stopwatchTimes.SplitTime = ClockValueString;
            stopwatchTimes.LapTime = (ClockValue - _lastSplitTime).ToString(@"hh\:mm\:ss\.ff");
            StopwatchTimesCollection.Insert(0, stopwatchTimes);
            _lastSplitTime = ClockValue;

            saveSplits = GetSplitData(",");
            IS.SaveSetting("Stopwatch-Splits", saveSplits);

            saveLaps = GetLapData(",");
            IS.SaveSetting("Stopwatch-Laps", saveLaps);
        }

        private void DeleteLaps_Click(object sender, EventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(AppResources.DeleteAllLapDataMessage, AppResources.DeleteLapsTitle, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                IS.RemoveSetting("Stopwatch-Laps");
                IS.RemoveSetting("Stopwatch-Splits");
                StopwatchTimesCollection.Clear();
            }
        }

        private void About_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private void Options_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Options.xaml", UriKind.Relative));
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

        private void RemoveAdvertising_Click(object sender, EventArgs e)
        {
            MessageBoxResult msgResult;
            string hasAppBeenRated = string.Empty;
            
            hasAppBeenRated = Rate.HasAppBeenRated();

            if (hasAppBeenRated.ToUpper() == "YES")
            {
                return;
            }

            msgResult = MessageBox.Show(AppResources.RemoveAdvertisingQuestion,AppResources.RateAppHeader, MessageBoxButton.OKCancel);
            if (msgResult == MessageBoxResult.OK)
            {
                MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
                marketplaceReviewTask.Show();

                IS.SaveSetting("AppRated", "Yes");
                AdvertisingVisibility = Visibility.Collapsed;
            }
            else
            {
                IS.SaveSetting("AppRated", "No");
            }           
        }

        #endregion "Events"

        #region "Methods"

        public void StartTimer()
        {
            if (Mode == AppResources.StartText)
            {
                App.gStopWatch.Start();
                Mode = AppResources.PauseText;
                Start.Background = new SolidColorBrush(Colors.Red);
            }
            else if (Mode == AppResources.PauseText)
            {
                App.gStopWatch.Stop();
                Mode = AppResources.ResumeText;
                Start.Background = new SolidColorBrush(Colors.Green);
            }
            else if (Mode == AppResources.ResumeText)
            {
                App.gStopWatch.Start();
                Mode = AppResources.PauseText;
                Start.Background = new SolidColorBrush(Colors.Red);
            };

            ClockValue = App.gStopWatch.Elapsed;
        }

        public String GetLapData(String delimiter)
        {
            String returnString = String.Empty;

            try
            {
                foreach (var item in StopwatchTimesCollection)
                {
                    returnString = returnString + item.LapTime + delimiter;
                }
            }
            catch (Exception)
            {

                return returnString;
            }
            return returnString;
        }

        public String GetSplitData(String delimiter)
        {
            String returnString = String.Empty;

            try
            {
                foreach (var item in StopwatchTimesCollection)
                {
                    returnString = returnString + item.SplitTime + delimiter;
                }
            }
            catch (Exception)
            {

                return returnString;
            }
            return returnString;
        }

        public void LoadLapAndSplitData()
        {
            string[] laps;
            string[] splits;

            laps = IS.GetSettingStringValue("Stopwatch-Laps").Split(',');
            splits = IS.GetSettingStringValue("Stopwatch-Splits").Split(',');

            for (int i = laps.Count() - 1; i >= 0; i--)
            {
                if (laps[i] != string.Empty)
                {
                    StopwatchTimes stopwatchTimes = new StopwatchTimes();
                    stopwatchTimes.ItemCount = laps.Count() - i - 1;
                    stopwatchTimes.LapTime = laps[i];
                    stopwatchTimes.SplitTime = splits[i];
                    StopwatchTimesCollection.Insert(0, stopwatchTimes);

                    _lastSplitTime = TimeSpan.Parse(stopwatchTimes.SplitTime);
                }
            }
        }

        public string BuildEmailBody()
        {
            string returnString = string.Empty;

            try
            {
                returnString = AppResources.EmailBody + "\n";
                foreach (var item in StopwatchTimesCollection)
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
        }

        public string IsStopWatchRunning()
        {
            string returnValue = string.Empty;

            try
            {
                if (IS.GetSettingStringValue("Stopwatch-IsRunning") != string.Empty)
                {
                    returnValue = IS.GetSettingStringValue("Stopwatch-IsRunning");
                }
                else
                {
                    returnValue = "No";
                }
            }
            catch (Exception)
            {
                return "No";
            }
            return returnValue;
        }

        #endregion "Methods"

        #region "Common Routines"

        // Sample code for building a localized ApplicationBar
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

            ApplicationBarMenuItem appBarMenuItem3 = new ApplicationBarMenuItem(AppResources.AppMenuItemReview);
            ApplicationBar.MenuItems.Add(appBarMenuItem3);
            appBarMenuItem3.Click += new EventHandler(Review_Click);

            ApplicationBarMenuItem appBarMenuItem4 = new ApplicationBarMenuItem(AppResources.AppMenuItemMoreApps);
            ApplicationBar.MenuItems.Add(appBarMenuItem4);
            appBarMenuItem4.Click += new EventHandler(MoreApps_Click);

            if (hasAppBeenRated.ToUpper() == "NO")
            {
                ApplicationBarMenuItem appBarMenuItem5 = new ApplicationBarMenuItem(AppResources.AppMenuItemRemoveAdvertising);
                ApplicationBar.MenuItems.Add(appBarMenuItem5);
                appBarMenuItem5.Click += new EventHandler(RemoveAdvertising_Click);
            }         
        }

        private void PhoneOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            // Switch the placement of the buttons based on an orientation change.
            if ((e.Orientation & PageOrientation.Portrait) == (PageOrientation.Portrait))
            {
                Grid.SetRow(MyAdControl, 0);
                Grid.SetRow(ContentPanel, 1);
                Grid.SetRowSpan(ContentPanel, 2);
                Grid.SetRow(ButtonPanel, 3);
                LapBorder.Visibility = Visibility.Visible;
                LapGrid.Visibility = Visibility.Visible;
            }
            // If not in portrait, move buttonList content to visible row and column.
            else
            {
                Grid.SetRow(ContentPanel, 1);
                Grid.SetRow(MyAdControl, 0);
                Grid.SetRowSpan(ContentPanel, 4);
                Grid.SetRow(ButtonPanel, 5);
                LapBorder.Visibility = Visibility.Collapsed;
                LapGrid.Visibility = Visibility.Collapsed;
            }

        }

        #endregion "Common Routines"

       
    }
}