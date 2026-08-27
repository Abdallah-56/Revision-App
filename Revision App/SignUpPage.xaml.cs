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

        // takes you back to log in page when clicked
        private void AlreadyHaveAccount_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new LogInPage());
        }

        // Signs you up
        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            // Registration logic will go here
        }
    }
}
