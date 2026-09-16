using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Revision_App.Models;
using Revision_App.Services;

namespace Revision_App
{
    public partial class SignUpPage : Page
    {
        public SignUpPage()
        {
            InitializeComponent();
        }

        // takes you back to log in page when clicked
        private void AlreadyHaveAccount_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new LogInPage());
        }

        // sign in button click
        private async void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            // takes all input from text boxes
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = CreatePasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            //checks if all fields are filled
            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Please complete all fields.");
                return;
            }

            // checks if email is valid
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Please enter a valid email address.");
                return;
            }

            // checks if password is valid
            if (password.Length < 8 ||
                !Regex.IsMatch(password, @"[A-Z]") ||
                !Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
            {
                MessageBox.Show(
                    "Password must be at least 8 characters and contain " +
                    "a capital letter and a symbol.");
                return;
            }

            // checks if passwords match
            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            try
            {
                // ensures client connection is initialized
                if (SupabaseService.Client == null)
                {
                    await SupabaseService.InitializeAsync();
                }

                // registers user in supabase auth
                var authSession = await SupabaseService.Client!.Auth.SignUp(email, password);

                if (authSession?.User == null)
                {
                    MessageBox.Show("Sign up failed. Please try again.");
                    return;
                }

                // saves profile details into the custom database table
                var newUser = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    XP = 0,
                    AuthUserID = authSession.User.Id
                };

                await SupabaseService.Client.From<User>().Insert(newUser);

                MessageBox.Show("Account created successfully!");
                NavigationService.Navigate(new LogInPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error signing up: {ex.Message}");
            }
        }
    }
}