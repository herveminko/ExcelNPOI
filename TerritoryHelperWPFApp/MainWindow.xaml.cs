using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TerritoryHelperWinClient;
using Newtonsoft.Json.Linq;
using InterestedExcelParser;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace TerritoryHelperWPFApp
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Hashtable publishersAssignments;
        DataTable publishers;
        DataTable assignments;
        DataTable allAddresses;

        public MainWindow()
        {
            InitializeComponent();
            loadAssignmentsFromDisk();
            loadTerritoriesFromDisk();
            loadPublishersFromDisk();

            loadAllAddressesFromDisk();
            //Thread childThread = new Thread(loadAllAddressesFromDisk);
            //childThread.Start();
        }

        private string GetTerritoryHelperServerUrl()
        {
            return  System.Configuration.ConfigurationSettings.AppSettings["territory-helper-url"];
        }


        private async void PublishersDownload_Click(object sender, RoutedEventArgs e)
        {
            string servicePath = System.Configuration.ConfigurationSettings.AppSettings["service-path-publishers"];

            TerritoryHelperWebSkeleton helper = new TerritoryHelperWebSkeleton();
            try
            {
                await helper.GetTerritoryHelperPublishersAsync(GetTerritoryHelperServerUrl(), servicePath);
                string message = "La liste des proclamateurs a bien été téléchargée du serveur web. ";
                MessageBoxResult result = await Task.Run(() => MessageBox.Show(message, "INFO", MessageBoxButton.OK, MessageBoxImage.Information));
                loadPublishersFromDisk();
            }
            catch (HttpRequestException ex)
            {
                string message = "Echec du téléchargement de la liste des proclamateurs. Verifiez votre connection internet ou l'accessibilité du serveur: " + GetTerritoryHelperServerUrl();
                MessageBoxResult result = await Task.Run(() => MessageBox.Show(message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error));
            }

            

               //switch (result)
            //{
            //    case MessageBoxResult.None:
            //        break;
            //    case MessageBoxResult.Yes:
            //        // do something
            //        break;
            //    case MessageBoxResult.No:
            //        // do something
            //        break;
            //    default:
            //        break;
            //}
        }

        private void Datagrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Adding 1 to make the row count start at 1 instead of 0
            // as pointed out by daub815
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private async void MailWork_Click(object sender, RoutedEventArgs e)
        {
            string message;
  

            foreach (string mail in publishersAssignments.Keys)
            {
                DataTable territorries = (DataTable)publishersAssignments[mail];
                List<string> territoriesStrings = new List<string>();
                foreach (DataRow row in territorries.Rows)
                {
                    territoriesStrings.Add(row[0].ToString());
                }
                MailUtilities mailer = new MailUtilities();
                mailer.sendTerritoryWorkRequestMail(mail, territoriesStrings);
            }

            message = "Courriels (Travail) envoyés aux proclamateurs";
            MessageBoxResult result = await Task.Run(() => MessageBox.Show(message, "INFO", MessageBoxButton.OK, MessageBoxImage.Information));

        }

        private async void TerritoriesCardsDownload_Click(object sender, RoutedEventArgs e)
        {
            string exportFormat = TerritoriesFormat.Text;
            TerritoryHelperWebSkeleton helper = new TerritoryHelperWebSkeleton();
            string servicePath = System.Configuration.ConfigurationSettings.AppSettings["service-path-territories"]; 
            if (exportFormat.Contains("KML"))
            {
                servicePath = servicePath.Replace("[FORMAT]", "GoogleEarth");
                exportFormat = "kml";
            }
            else
            {
                servicePath = servicePath.Replace("[FORMAT]", "GeoJson");
                servicePath = servicePath.Replace("Export", "ExportAsync");
                exportFormat = "json";
            }
            try {
                await helper.GetTerritoryHelperTerritoriesCardsAsync(GetTerritoryHelperServerUrl(), servicePath, exportFormat);
                string message = "Les limites géographiques des territoires de l'assemblée ont été téléchargées du serveur web (Format = " + exportFormat + ")";
                MessageBoxResult result = await Task.Run(() => MessageBox.Show(message, "INFO", MessageBoxButton.OK, MessageBoxImage.Information));

                loadTerritoriesFromDisk();
            }
            catch (HttpRequestException ex)
            {
                string message = "Echec du téléchargement de la liste des territoires. Verifiez votre connection internet ou l'accessibilité du serveur: " + GetTerritoryHelperServerUrl();
                MessageBoxResult result = await Task.Run(() => MessageBox.Show(message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error));
            }

         }



        private async void AddressesDownload_Click(object sender, RoutedEventArgs e)
        {
            string servicePath = System.Configuration.ConfigurationSettings.AppSettings["service-path-addresses"];

            TerritoryHelperWebSkeleton helper = new TerritoryHelperWebSkeleton();
            try
            {
                await helper.GetTerritoryHelperAddressesAsync(GetTerritoryHelperServerUrl(), servicePath);
                string message = "Les adresses des territoires ont bien été téléchargées du serveur web. ";
                MessageBoxResult result = await Task.Run(() => MessageBox.Show(message, "INFO", MessageBoxButton.OK, MessageBoxImage.Information));
                //CreateTerritoriesAddressFiles();
            }
            catch (HttpRequestException ex)
            {
                string message = "Echec du téléchargement des addresses des territoires. Verifiez votre connection internet ou l'accessibilité du serveur: " + GetTerritoryHelperServerUrl();
                MessageBoxResult result = await Task.Run(() => MessageBox.Show(message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error));
            }

            loadAllAddressesFromDisk();
        }

        private void CreateTerritoriesAddressFiles()
        {
            TerritoryHelperFileParser parser = new TerritoryHelperFileParser();
            parser.ParseTerritoryHelperInterestedFile(null);
            List<int> territoriesWithAddresses = parser.GetAllTerritoryNumbersHavingLocations();
            foreach (int number in territoriesWithAddresses)
            {
                parser.CreateTerritoryAddressesFile(number);
            }
        }


        private async void AssignmentsDownload_Click(object sender, RoutedEventArgs e)
        {             
            string servicePath = System.Configuration.ConfigurationSettings.AppSettings["service-path-assignments"];

            TerritoryHelperWebSkeleton helper = new TerritoryHelperWebSkeleton();
            try {
                await helper.GetTerritoryHelperTerritoriesAssignmentsAsync(GetTerritoryHelperServerUrl(), servicePath);
                string message = "Les attributions de territoires ont bien été téléchargées du serveur web. ";
                MessageBoxResult result = await Task.Run(() => MessageBox.Show(message, "INFO", MessageBoxButton.OK, MessageBoxImage.Information));
                loadAssignmentsFromDisk();
            }
            catch (HttpRequestException ex)
            {
                string message = "Echec du téléchargement des attributions de territoires. Verifiez votre connection internet ou l'accessibilité du serveur: " + GetTerritoryHelperServerUrl();
                MessageBoxResult result = await Task.Run(() => MessageBox.Show(message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error));
            }
            

         
        }

        private void TerritoriesData_SelectedCellsChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                // find row for the first selected item
                DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(e.AddedItems[0]);
                if (row != null)
                {
                    DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(row);
                    // find grid cell object for the cell with index 0
                    DataGridCell cellTerritoryNumber = presenter.ItemContainerGenerator.ContainerFromIndex(1) as DataGridCell;

                    // Trying to get the mail (third column)
                    string territoryNumber = ((TextBlock)cellTerritoryNumber.Content).Text;
                    List<TerritoryAddress> addresses = new List<TerritoryAddress>();
                    if (allAddresses != null)
                    {
                        foreach (DataRow row2 in allAddresses.Rows)
                        {
                            string currentRowTerritoryNumber = row2["Numéro de territoire"].ToString();
                            string logement = row2["Logement"].ToString();
                            if (currentRowTerritoryNumber == territoryNumber && logement != null && logement.Length > 0) {
                                TerritoryAddress add = new TerritoryAddress();
                                add.TerritoryNumber = territoryNumber;
                                add.Address = row2["Adresse"].ToString();
                                add.Details = row2["Logement"].ToString();
                                add.Status = row2["Statut"].ToString();
                                add.Language = row2["Langue"].ToString();
                                add.Remark = row2["Notes"].ToString();

                                addresses.Add(add);
                            }
                        }
                    }
                   
                    territoryAddressesData.FilteredItemsSource = addresses;
                }
            }

        }

        private void PublishersData_SelectedCellsChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                // find row for the first selected item
                DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(e.AddedItems[0]);
                if (row != null)
                {
                    DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(row);
                    // find grid cell object for the cell with index 0
                    DataGridCell cellEmail = presenter.ItemContainerGenerator.ContainerFromIndex(2) as DataGridCell;

                    // Trying to get the mail (third column)
                    string key = ((TextBlock)cellEmail.Content).Text;
                    if (key.Length == 0) { 
                        DataGridCell cellFirstName = presenter.ItemContainerGenerator.ContainerFromIndex(0) as DataGridCell;
                        DataGridCell cellLastName = presenter.ItemContainerGenerator.ContainerFromIndex(1) as DataGridCell;

                        key = ((TextBlock)cellFirstName.Content).Text + " " + ((TextBlock)cellLastName.Content).Text;
                    }


                    DataTable publishersTerritories = (DataTable)publishersAssignments[key];
                    if (publishersTerritories == null)
                    {
                        publishersTerritories = new DataTable("Liste de territoires");
                        DataRow newRow = publishersTerritories.NewRow();

                        string cellType = "System.String";
                        DataColumn newHeader = new DataColumn("Liste des Territoires du proclamateur", System.Type.GetType(cellType));
                        publishersTerritories.Columns.Add(newHeader);
                    }

                    publishersAssignmentsData.ItemsSource = publishersTerritories.DefaultView;
                }
            }
        }


        static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null) child = GetVisualChild<T>(v);
                if (child != null) break;
            }
            return child;
        }

        public void loadAssignmentsFromDisk()
        {
            assignments = UiHelper.loadAssignmentsFromDisk();
            if (assignments != null)
            {
                publishersAssignments = UiHelper.getPublishersAssignments(assignments);
                //assignmentsData.ItemsSource = assignments.DefaultView;

                List<TerritoryAssignment> assignmentsList = new List<TerritoryAssignment>();
                foreach (DataRow row in assignments.Rows)
                {
                    TerritoryAssignment assign = new TerritoryAssignment();
                    assign.TerritoryNumber = row["Numéro de Territoire"].ToString();
                    assign.TerritoryType = row["Type de territoire"].ToString();
                    assign.TerritoryDescription = row["Titre de territoire"].ToString();
                    assign.PhoneNumber = row["Téléphone"].ToString();
                    assign.PublisherName = row["Nom du proclamateur"].ToString();
                    assign.Email = row["Email"].ToString();
                    assign.Remarks = row["Notes"].ToString();
  
                    if (row["Attribués"].ToString().Length > 0)
                    {
                            assign.AssignDate = (DateTime)row["Attribués"];
                    } 

                    if (row["Restitués"].ToString().Length > 0)
                    {
                            assign.ReturnDate = DateTime.Parse(row["Restitués"].ToString());
                    }
                    assignmentsList.Add(assign);                    
                   
                }
                assignmentsData.FilteredItemsSource = assignmentsList;
            }

        }

        public void loadPublishersFromDisk()
        {
            publishers = UiHelper.loadPublishersFromDisk();
            if (publishers != null)
            {
                //publishersData.DataContext = publishers.DefaultView;
                // publishersData.ItemsSource = publishers.DefaultView;

                List<Publisher> publishersList = new List<Publisher>();
                foreach (DataRow row in publishers.Rows)
                {
                    Publisher pub = new Publisher();
                    pub.FirstName = row["Prénom"].ToString();
                    pub.LastName = row["Nom"].ToString();
                    pub.Email = row["Email"].ToString();
                    pub.PhoneNumber = row["Téléphone"].ToString();
                    pub.Role = row["Rôle"].ToString();
                    if (row["Dernière mise à jour"].ToString().Length > 0) {                       
                        pub.LastUpdate = Convert.ToDateTime(row["Dernière mise à jour"]);                       
                    }
                    if (row["Date de création"].ToString().Length > 0)
                    {
                        pub.CreationDate = (DateTime)row["Date de création"];
                    }
                    publishersList.Add(pub);
                }
               publishersData.FilteredItemsSource = publishersList;
            }
        }

        public void loadTerritoriesFromDisk() {
            List<Territory> territories = UiHelper.loadTerritoriesFromDisk();
            territoriesData.FilteredItemsSource = territories;
            territoryAddressesData.FilteredItemsSource = new List<TerritoryAddress>();
        }

        public void loadAllAddressesFromDisk()
        {
            allAddresses = UiHelper.loadAllAddressesFromDisk();
            UpdateAddressesData();
        }

        public void UpdateAddressesData()
        {
            List<TerritoryAddress> addresses = new List<TerritoryAddress>();
            if (allAddresses != null)
            {
                foreach (DataRow row in allAddresses.Rows)
                {
                    string logement = row["Logement"].ToString();

                    string type = row["Type de territoire"].ToString();
                    if (logement != null && logement.Length > 0 && !type.Equals("P") && !type.Equals("C"))
                    //if (logement != null && logement.Length > 0)
                    {
                        TerritoryAddress add = new TerritoryAddress();
                        add.TerritoryNumber = row["Numéro de territoire"].ToString();
                        add.Address = row["Adresse"].ToString();
                        add.Details = row["Logement"].ToString();

                        string status = row["Statut"].ToString();
                        add.Language = row["Langue"].ToString();
                        add.Remark = row["Notes"].ToString();
                        add.Status = status; // "Ne voulant Plus être Visité"

                        addresses.Add(add);
                    }

                }
            }

            allAddressesData.FilteredItemsSource = addresses;
        }

        private void CreateTerritoriesFiles_Click(object sender, RoutedEventArgs e)
        {
            Action onCompleted = () =>
            {
                //On complete action
                string message = "Creation des fichiers d'adresses des territoires terminée dans ";
                MessageBox.Show(message, "INFO", MessageBoxButton.OK, MessageBoxImage.Information);
            };

            var thread = new Thread(
              () =>
              {
                  try
                  {
                      CreateTerritoriesAddressFiles();
                  }
                  finally
                  {
                      onCompleted();
                  }
              });
            thread.Start();
        }

        private void MenuItemExportAddresses_Click(object sender, RoutedEventArgs e)
        {
            //Get the clicked MenuItem
            MenuItem menuItem = (MenuItem)sender;
            //Get the ContextMenu to which the menuItem belongs
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;

            //Find the placementTarget
            DataGrid dataGrid = (DataGrid)contextMenu.PlacementTarget;

            dataGrid.SelectionMode = DataGridSelectionMode.Extended;
            dataGrid.SelectAllCells();
            dataGrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            ApplicationCommands.Copy.Execute(null, dataGrid);
            dataGrid.UnselectAllCells();
            dataGrid.SelectionMode = DataGridSelectionMode.Single;

            string textResult = (string)System.Windows.Clipboard.GetData(System.Windows.DataFormats.Text);
            string csvResult = (string)System.Windows.Clipboard.GetData(System.Windows.DataFormats.CommaSeparatedValue);


            String timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            string exportName = "exported_addresses";

            File.AppendAllText(exportName + "_" + timeStamp + ".txt", textResult, UnicodeEncoding.UTF8);
            File.AppendAllText(exportName + "_" + timeStamp + ".csv", csvResult, UnicodeEncoding.UTF8);

            string message = "Exportation des adresses terminée";
            MessageBox.Show(message, "INFO", MessageBoxButton.OK, MessageBoxImage.Information);

        }
    }
}
