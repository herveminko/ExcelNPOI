using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Shapes;
using TerritoryHelperWinClient;

namespace TerritoryHelperWPFApp
{
    /// <summary>
    /// Interaktionslogik für InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        Hashtable publishersAssignments;

        public InputDialog(Hashtable assignments)
        {
            this.publishersAssignments = assignments;
            InitializeComponent();
        }


        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string message = textArea.Text.Trim();
            bool success;


            foreach (string mail in publishersAssignments.Keys)
            {
                DataTable territorries = (DataTable)publishersAssignments[mail];
                List<string> territoriesStrings = new List<string>();
                foreach (DataRow row in territorries.Rows)
                {
                    territoriesStrings.Add(row[0].ToString());
                }
                MailUtilities mailer = new MailUtilities();
                success = mailer.sendFreeTextMail(mail, territoriesStrings, message);

            }

            message = "Courriels envoyés aux proclamateurs";
            MessageBoxResult result = await Task.Run(() => MessageBox.Show(message, "INFO", MessageBoxButton.OK, MessageBoxImage.Information));
            this.Close();
        }




    }
}
