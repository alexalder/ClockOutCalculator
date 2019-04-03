using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Uwp.Notifications; // Notifications library
using System;
using Windows.Storage;
using Windows.UI.Notifications;
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

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            dateTimePicker1.Time = (TimeSpan)localSettings.Values["picker1"];
            dateTimePicker2.Time = (TimeSpan)localSettings.Values["picker2"];
            dateTimePicker3.Time = (TimeSpan)localSettings.Values["picker3"];
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
    }
}
