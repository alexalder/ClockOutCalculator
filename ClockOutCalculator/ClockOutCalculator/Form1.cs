using System;
using System.IO;
using System.Windows.Forms;

using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

namespace ClockOutCalculator
{
    public partial class ClockOutCalculator : Form
    {

        public ClockOutCalculator()
        {
            InitializeComponent();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            CalculateClockOff();
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            CalculateClockOff();
        }

        private void dateTimePicker3_ValueChanged(object sender, EventArgs e)
        {
            CalculateClockOff();
        }

        private void CalculateClockOff()
        {
            DateTime clockOut;
            double lunchTime = CalculateLunch();
            
            if (lunchTime > 1)
            {
                clockOut = dateTimePicker1.Value.AddHours(8 + lunchTime);
                lunchLabel.Text = "1:" + Convert.ToInt32((lunchTime - 1) * 60).ToString("D2");
            }
            else
            {
                clockOut = dateTimePicker1.Value.AddHours(9);
                lunchLabel.Text = "1:00";
            }
                
            clockOutLabel.Text = clockOut.Hour.ToString("D2") + ":" + clockOut.Minute.ToString("D2");
        }

        private double CalculateLunch()
        {
            if (dateTimePicker3)
            return dateTimePicker3.Value.Subtract(dateTimePicker2.Value).TotalHours;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Get a toast XML template
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);

            // Fill in the text elements
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            for (int i = 0; i < stringElements.Count; i++)
            {
                stringElements[i].AppendChild(toastXml.CreateTextNode("Line " + i));
            }

            // Specify the absolute path to an image
            //String imagePath = "file:///" + Path.GetFullPath("toastImageAndText.png");
            //XmlNodeList imageElements = toastXml.GetElementsByTagName("image");
            //imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

            // Create the toast and attach event listeners
            ToastNotification toast = new ToastNotification(toastXml);
            //toast.Activated += ToastActivated;
            //toast.Dismissed += ToastDismissed;
            toast.Failed += ToastFailed;
            

            // Show the toast. Be sure to specify the AppUserModelId on your application's shortcut!
            ToastNotificationManager.CreateToastNotifier(System.Diagnostics.Process.GetCurrentProcess().ProcessName).Show(toast);
        }

        void ToastFailed(ToastNotification not, ToastFailedEventArgs args)
        {
            Console.WriteLine("Toast Failed");
        }
    }
}
