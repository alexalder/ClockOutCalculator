using Windows.Foundation;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClockOutCalculatorModern
{
    public sealed partial class LoginPage : UserControl
    {
        public LoginPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(600, 345);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["username"] = usernameBox.Text;
            PasswordVault vault = new PasswordVault();
            PasswordCredential credential = new PasswordCredential("ClockOutCalculator", usernameBox.Text, passwordBox.Password);
            vault.Add(credential);
            Window.Current.Close();
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            Window.Current.Close();
        }
    }
}
