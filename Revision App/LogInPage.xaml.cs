using Revision_App.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Revision_App
{
    public partial class LogInPage : Page
    {
        public LogInPage()
        {
            InitializeComponent();
        }
        // takes you to sign up page when clicked
        private void CreateAccountButton_Click(
            object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SignUpPage());
        }
        //signs in the user when clicked and checks if the email and password are correct
        private async void SignInButton_Click(
            object sender, RoutedEventArgs e)
        {
            string email = UsernameTextBox.Text.Trim();
            string password = PasswordTextbox.Password;
            // Check if email and password are not empty
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter your email and password.");
                return;
            }
            // Checks if email is valid
            try
            {
                if (SupabaseService.Client == null)
                {
                    await SupabaseService.InitializeAsync();
                }
                // checks if password is correct and signs in the user
                var session = await SupabaseService.Client!
                    .Auth.SignIn(email, password);

                if (session?.User == null)
                {
                    MessageBox.Show("Login failed. Please try again.");
                    return;
                }

                NavigationService.Navigate(new DashboardPage());
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Login failed. Check your email and password, then try again.");
            }
        }
    }
}