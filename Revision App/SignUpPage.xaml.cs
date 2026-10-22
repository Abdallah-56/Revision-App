using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        // Return to the login page
        private void AlreadyHaveAccount_MouseDown(
            object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new LogInPage());
        }

        // Register a new account
        private async void SignUpButton_Click(
            object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = CreatePasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Please complete all fields.");
                return;
            }

            // Validate email
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Please enter a valid email address.");
                return;
            }

            // Validate password
            if (password.Length < 8 ||
                !Regex.IsMatch(password, @"[A-Z]") ||
                !Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
            {
                MessageBox.Show(
                    "Password must be at least 8 characters and contain " +
                    "a capital letter and a symbol.");
                return;
            }

            // Check password confirmation
            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            try
            {
                // Initialise Supabase if necessary
                if (SupabaseService.Client == null)
                {
                    await SupabaseService.InitializeAsync();
                }

                var client = SupabaseService.Client;

                if (client == null)
                {
                    MessageBox.Show("Could not connect to Supabase.");
                    return;
                }

                // Create the authentication account

                var authSession = await SupabaseService.Client!.Auth.SignUp(
                    email,
                    password,
                    new Supabase.Gotrue.SignUpOptions
                    {
                        Data = new Dictionary<string, object>
                        {
                           { "first_name", firstName },
                           { "last_name", lastName }
                        }
                    }
                );

                if (authSession?.User == null)
                {
                    MessageBox.Show("Sign up failed. Please try again.");
                    return;
                }

                MessageBox.Show("Account created successfully!");
                NavigationService.Navigate(new LogInPage());


                // Check the returned user ID and session status
                string authUserId = authSession.User.Id;
                bool hasSession =
                    !string.IsNullOrWhiteSpace(authSession.AccessToken);

                if (!hasSession)
                {
                    MessageBox.Show(
                        "Your authentication account was created, but " +
                        "Supabase did not return an active session.\n\n" +
                        "The profile cannot be inserted using an RLS policy " +
                        "that requires auth.uid() to match AuthUserID.\n\n" +
                        "Check the authentication session before retrying.");
                    return;
                }

               

                // Insert the profile into public.Users
                await client.From<User>().Insert(newUser);

                MessageBox.Show("Account created successfully!");

                // Return to the login page
                NavigationService.Navigate(new LogInPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error signing up:\n{ex.Message}",
                    "Sign-up error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
