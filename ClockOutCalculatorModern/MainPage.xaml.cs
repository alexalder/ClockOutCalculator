using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ClockOutCalculatorModern
{
    public sealed partial class MainPage : Page
    {
        TimeSpan clockOut;
        ToastNotifier notificationManager = ToastNotificationManager.CreateToastNotifier();
        ScheduledToastNotification toast;
        List<TimePicker> timePickers = new List<TimePicker>();

        public MainPage()
        {
            this.InitializeComponent();

            InitializePickers();
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow
        .Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => GetFromWebAsync());
            SetupStartup();
            SetLaunchSize();
        }

        private void SetLaunchSize()
        {
            ApplicationView.PreferredLaunchViewSize = new Size(600, 345);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private void InitializePickers()
        {
            timePickers.Add(dateTimePicker1);
            timePickers.Add(dateTimePicker2);
            timePickers.Add(dateTimePicker3);
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            foreach (TimePicker p in timePickers)
            {
                if (localSettings.Values["dateTimePicker" + (timePickers.IndexOf(p) + 1)] != null)
                {
                    p.Time = (TimeSpan)localSettings.Values["dateTimePicker" + (timePickers.IndexOf(p) + 1)];
                }
            }

        }

        private void dateTimePicker1_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            DateChanged(sender);
        }

        private void dateTimePicker2_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            DateChanged(sender);
        }

        private void dateTimePicker3_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            DateChanged(sender);
        }

        private void DateChanged(object sender)
        {
            TimePicker picker = sender as TimePicker;
            if (CalculateClockOff())
            {
                SetupToast();
                SaveTime(picker, picker.Name);
            }
        }

        private void SaveTime(TimePicker timePicker, string pickerName)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[pickerName] = timePicker.Time;
        }

        private bool CalculateClockOff()
        {
            if (lunchLabel != null)
            {
                TimeSpan lunchTime = CalculateLunch();
                SetClockOut(lunchTime);

                SetBreak();

                return true;
            }
            return false;
        }

        private TimeSpan CalculateLunch()
        {
            if (dateTimePicker3 != null)
            {
                return dateTimePicker3.Time.Subtract(dateTimePicker2.Time);
            }
            else
            {
                return (new TimeSpan(0, 0, 0));
            }
        }

        private void SetClockOut(TimeSpan lunchTime)
        {
            if (lunchTime.TotalMinutes > 60)
            {
                clockOut = dateTimePicker1.Time.Add(lunchTime).Add(new TimeSpan(8, 0, 0));
                lunchLabel.Text = "1:" + (lunchTime.Minutes).ToString("D2");
            }
            else
            {
                clockOut = dateTimePicker1.Time.Add(new TimeSpan(9, 0, 0));
                lunchLabel.Text = "1:00";
            }

            clockOutLabel.Text = clockOut.Hours.ToString("D2") + ":" + clockOut.Minutes.ToString("D2");
        }

        private void SetBreak()
        {
            TimeSpan breakTime = dateTimePicker3.Time.Add(TimeSpan.FromTicks(clockOut.Subtract(dateTimePicker3.Time).Ticks / 2));
            breakLabel.Text = breakTime.Hours.ToString("D2") + ":" + breakTime.Minutes.ToString("D2");
        }

        private async Task<bool> GetFromWebAsync()
        {
            try
            {
                progressBar.IsIndeterminate = true;
                ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                string username = (string)localSettings.Values["username"];
                if (username == null)
                {
                    return false;
                }
                else
                {
                    PasswordVault vault = new PasswordVault();
                    string password = vault.Retrieve("ClockOutCalculator", username).Password;
                    //Insert your own method here
                    List<TimeSpan> times = await WebLoader.GetFromWebAsync(username, password);
                    for (int i = 0; i < times.Count && i < 3; i++)
                    {
                        timePickers[i].Time = times[i];
                    }
                    return true;
                }

            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                progressBar.IsIndeterminate = false;
            }
        }

        private async void OpenLoginWindow()
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Window.Current.Content = new LoginPage();
                // You have to activate the window in order to show it later.
                Window.Current.Activate();

                newViewId = ApplicationView.GetForCurrentView().Id;
            });
            bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
        }

        private void SetupToast()
        {
            if (toast != null)
            {
                notificationManager.RemoveFromSchedule(toast);
            }

            string title = "ClockOutCalculator";
            string content = "Ora di uscire!";
            string image = "https://picsum.photos/360/202?image=883";
            string logo = "ms-appx:///Assets/2048-200.png";

            // Construct the visuals of the toast
            ToastVisual visual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children =
                    {
                        new AdaptiveText()
                        {
                            Text = title
                        },

                        new AdaptiveText()
                        {
                            Text = content
                        },

                        new AdaptiveImage()
                        {
                            Source = image
                        }
                    },

                    AppLogoOverride = new ToastGenericAppLogo()
                    {
                        Source = logo,
                        HintCrop = ToastGenericAppLogoCrop.Circle
                    }
                }
            };

            int conversationId = 384928;

            ToastContent toastContent = new ToastContent()
            {
                Visual = visual,

                // Arguments when the user taps body of toast
                Launch = new QueryString()
                {
                    { "action", "viewConversation" },
                    { "conversationId", conversationId.ToString() }
                }.ToString()
            };

            DateTimeOffset toastTime = new DateTimeOffset(DateTime.Today.Add(clockOut));

            if (toastTime.CompareTo(DateTimeOffset.Now) > 0)
            {
                toast = new ScheduledToastNotification(toastContent.GetXml(), toastTime)
                {
                    Tag = "18365",
                    Group = "clockOutPosts",
                    ExpirationTime = DateTime.Today.Add(clockOut).AddHours(1)
                };

                // And create the toast notification
                notificationManager.AddToSchedule(toast);
            }
        }

        private async void SetupStartup()
        {
            StartupTask startupTask = await StartupTask.GetAsync("MyStartupId");
            switch (startupTask.State)
            {
                case StartupTaskState.Disabled:
                    // Task is disabled but can be enabled.
                    StartupTaskState newState = await startupTask.RequestEnableAsync();
                    break;
                case StartupTaskState.DisabledByUser:
                    // Task is disabled and user must enable it manually.
                    MessageDialog dialog = new MessageDialog(
                        "I know you don't want this app to run " +
                        "as soon as you sign in, but if you change your mind, " +
                        "you can enable this in the Startup tab in Task Manager.",
                        "TestStartup");
                    await dialog.ShowAsync();
                    break;
                case StartupTaskState.DisabledByPolicy:
                    break;
                case StartupTaskState.Enabled:
                    break;
            }
        }

        private async void refreshButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            bool task = await GetFromWebAsync();
        }

        private async void loginButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            OpenLoginWindow();
        }
    }
}
