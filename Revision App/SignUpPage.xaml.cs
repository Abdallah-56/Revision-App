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
    public partial class SignUpPage : Page
    {
        public SignUpPage()
        {
            InitializeComponent();
        }

        // Navigates back to the log-in page when purple text is clicked
        private void AlreadyHaveAccount_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new LogInPage());
        }

        // Triggers when the Sign Up button is pressed
        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            // Registration logic will go here
        }
    }
}
