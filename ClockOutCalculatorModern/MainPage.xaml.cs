using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Uwp.Notifications; // Notifications library
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ClockOutCalculatorModern
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        TimeSpan clockOut;
        ToastNotifier notificationManager = ToastNotificationManager.CreateToastNotifier();
        ScheduledToastNotification toast;
        List<TimePicker> timePickers = new List<TimePicker>();

        public MainPage()
        {
            this.InitializeComponent();
            timePickers.Add(dateTimePicker1);
            timePickers.Add(dateTimePicker2);
            timePickers.Add(dateTimePicker3);
            InitializePickers();
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow
        .Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => GetFromWebAsync());
            SetupStartup();
        }

        private void InitializePickers()
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            foreach (TimePicker p in timePickers)
            {
                if (localSettings.Values["picker" + (timePickers.IndexOf(p) + 1)] != null)
                    p.Time = (TimeSpan)localSettings.Values["picker" + (timePickers.IndexOf(p) + 1)];
            }
                
        }

        private void dateTimePicker1_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (CalculateClockOff())
            {
                SetupToast();
                SaveTime((TimePicker)sender, "picker1");
            }
                
        }

        private void dateTimePicker2_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (CalculateClockOff())
            {
                SetupToast();
                SaveTime((TimePicker)sender, "picker2");
            }
        }

        private void dateTimePicker3_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (CalculateClockOff())
            {
                SetupToast();
                SaveTime((TimePicker)sender, "picker3");
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

        private void SetupToast()
        {
            if (toast != null)
                notificationManager.RemoveFromSchedule(toast);
            // In a real app, these would be initialized with actual data
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
                //Actions = actions,

                // Arguments when the user taps body of toast
                Launch = new QueryString()
                {
                    { "action", "viewConversation" },
                    { "conversationId", conversationId.ToString() }
                }.ToString()
            };
            //var toast = new ToastNotification(toastContent.GetXml());

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

        private async Task<bool> GetFromWebAsync()
        {
            //progressBar.IsEnabled = true;
            progressBar.IsIndeterminate = true;
            bool res = false;
            //Insert your own method here
            List<TimeSpan> times = await WebLoader.GetFromWebAsync();
            if (times.Any())
            {
                for (int i = 0; i < times.Count; i++)
                    timePickers[i].Time = times[i];
                res = true;
            }
            //progressBar.IsEnabled = false;
            progressBar.IsIndeterminate = false;
            return res;
        }


        async private void SetupStartup()
        {
            StartupTask startupTask = await StartupTask.GetAsync("MyStartupId");
            //requestResult.Text = startupTask.State.ToString();
            switch (startupTask.State)
            {
                case StartupTaskState.Disabled:
                    // Task is disabled but can be enabled.
                    StartupTaskState newState = await startupTask.RequestEnableAsync();
                    //requestResult.Text = newState.ToString();
                    //Debug.WriteLine("Request to enable startup, result = {0}", newState);
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
                    //Debug.WriteLine(
                    //"Startup disabled by group policy, or not supported on this device");
                    break;
                case StartupTaskState.Enabled:
                    //Debug.WriteLine("Startup is enabled.");
                    break;
            }
        }

        private async void refreshButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var task = await GetFromWebAsync();
        }
    }
}
