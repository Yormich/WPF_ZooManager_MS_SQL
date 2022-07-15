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
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace WPF_Zoo_Manager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        SqlConnection sqlConnection;    
        public MainWindow()
        {
            InitializeComponent();

           ConnectionStringSettingsCollection connectionStringSettingsCollection = ConfigurationManager.ConnectionStrings;
            string Connection = ConfigurationManager.ConnectionStrings["WPF_Zoo_Manager.Properties.Settings.TutorialDBConnectionString"].ConnectionString;
            sqlConnection = new SqlConnection(Connection);
            ShowZoos();
            ShowAnimals();
        }

        private void ShowZoos()
        {
            try
            {
                string query = "select * from Zoo";

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, sqlConnection);

                using (sqlDataAdapter)
                {
                    DataTable zooTable = new DataTable();

                    sqlDataAdapter.Fill(zooTable);

                    //which info should be shown
                    listZoos.DisplayMemberPath = "Location";
                    //which value should be delivered, when an item is selected
                    listZoos.SelectedValuePath = "Id";
                    //reference to the data the listbox shhould populate
                    listZoos.ItemsSource = zooTable.DefaultView;
                } 
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void ShowAssociatedAnimals()
        {
            try
            {
                string query = "SELECT * FROM Animal a inner join ZooAnimal za ON a.Id = za.AnimalId WHERE za.ZooId = @ZooId";

                SqlCommand sqlCommand = new SqlCommand(query,sqlConnection);

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);

                using (sqlDataAdapter)
                {
                    sqlCommand.Parameters.AddWithValue("@ZooId", listZoos.SelectedValue);

                    DataTable animalTable = new DataTable();

                    sqlDataAdapter.Fill(animalTable);

                    //which info should be shown
                   listAssociatedAnimals.DisplayMemberPath = "Name";
                    //which value should be delivered, when an item is selected
                    listAssociatedAnimals.SelectedValuePath = "Id";
                    //reference to the data the listbox shhould populate
                    listAssociatedAnimals.ItemsSource = animalTable.DefaultView;
                }
            }
            catch (Exception e)
            {
               // MessageBox.Show(e.Message);
            }
        }

        private void listZoos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowAssociatedAnimals();
           
            ShowSelectedZooInTextBox();
        }

        private void ShowSelectedZooInTextBox()
        {
            try
            {
                string query = "SELECT Location FROM Zoo WHERE Id = @ZooId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("ZooId", listZoos.SelectedValue);

                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand);
                using(dataAdapter)
                {
                    DataTable zooTable = new DataTable();
                    dataAdapter.Fill(zooTable);

                    MainTextBox.Text = zooTable.Rows[0]["Location"].ToString();
                }
            }
            catch (Exception exc)
            {
                //MessageBox.Show(exc.Message);
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void ShowAnimals()
        {
            try
            {
                string query = "SELECT * FROM Animal";
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, sqlConnection);
                using(sqlDataAdapter)
                {
                    DataTable animalTable = new DataTable();
                    sqlDataAdapter.Fill(animalTable);

                    listAnimals.DisplayMemberPath = "Name";
                    listAnimals.SelectedValuePath = "Id";
                    listAnimals.ItemsSource = animalTable.DefaultView;
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void DeleteZoo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string query = "DELETE FROM Zoo WHERE id = @ZooId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("ZooId", listZoos.SelectedValue);
                sqlCommand.ExecuteScalar();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
            finally
            {
                sqlConnection.Close();
                ShowZoos();
            }
        }

        private void AddZoo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string query = "INSERT INTO Zoo VALUES(@Location)";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                if (MainTextBox.Text.Length > 0 && MainTextBox.Text.Length <=50)
                {
                    sqlCommand.Parameters.AddWithValue("Location", MainTextBox.Text);
                    sqlCommand.ExecuteScalar();
                }
                else
                {
                    throw new ArgumentException($"Wrong Length Of Zoo Name: {MainTextBox.Text.Length} (Must be between 1 and 50)");
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
            finally
            {
                sqlConnection.Close();
                ShowZoos();
            }
        }

        private void AddAnimalToZoo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string query = "INSERT INTO ZooAnimal VALUES(@ZooId, @AnimalId)";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("ZooId", listZoos.SelectedValue);
                sqlCommand.Parameters.AddWithValue("AnimalId",listAnimals.SelectedValue);
                sqlCommand.ExecuteScalar();
                
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
            finally
            {
                sqlConnection.Close();
                ShowAssociatedAnimals();
            }
        }

        private void DeleteAnimalFromZoo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string query = "DELETE FROM ZooAnimal WHERE ZooAnimal.ZooId = @ZooId AND ZooAnimal.AnimalID = @AnimalId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("ZooId", listZoos.SelectedValue);
                sqlCommand.Parameters.AddWithValue("AnimalId", listAssociatedAnimals.SelectedValue);
                sqlCommand.ExecuteScalar();

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
            finally
            {
                sqlConnection.Close();
                ShowAssociatedAnimals();
            }
        }

        private void DeleteAnimal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string query = "DELETE FROM Animal WHERE Id = @AnimalId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("AnimalId", listAnimals.SelectedValue);
                sqlCommand.ExecuteScalar();

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
            finally
            {
                sqlConnection.Close();
                ShowAnimals();
                ShowAssociatedAnimals();
            }
        }

        private void AddAnimal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string query = "INSERT INTO Animal VALUES(@Name)";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                if(MainTextBox.Text.Length > 0 && MainTextBox.Text.Length <=50)
                {
                    sqlCommand.Parameters.AddWithValue("Name", MainTextBox.Text);
                    sqlCommand.ExecuteScalar();
                }
                else
                {
                    throw new ArgumentException($"Wrong Length Of Zoo Name: {MainTextBox.Text.Length} (Must be between 1 and 50)");
                }

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
            finally
            {
                sqlConnection.Close();
                ShowAnimals();
            }
        }

        private void listAnimals_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowSelectedAnimalInTextBox();
        }

        private void ShowSelectedAnimalInTextBox()
        {
            try
            {
                string query = "SELECT Name FROM Animal WHERE Id = @AnimalId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("AnimalId", listAnimals.SelectedValue);

                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand);
                using (dataAdapter)
                {

                    DataTable animalTable = new DataTable();
                    dataAdapter.Fill(animalTable);

                    MainTextBox.Text = animalTable.Rows[0]["Name"].ToString();
                }
            }
            catch (Exception exc)
            {
            //    MessageBox.Show(exc.Message);
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void UpdateZoo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string query = "UPDATE Zoo SET Location = @NewLocation WHERE Id = @ZooId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("ZooId", listZoos.SelectedValue);
                if (MainTextBox.Text.Length > 0 && MainTextBox.Text.Length<=50)
                {
                    sqlCommand.Parameters.AddWithValue("NewLocation", MainTextBox.Text);
                    sqlCommand.ExecuteScalar();

                }
                else
                {
                    throw new ArgumentException($"Wrong Length Of Zoo Name: {MainTextBox.Text.Length} (Must be between 1 and 50)");
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
            finally
            {
                sqlConnection.Close();
                ShowZoos();
            } 
        }

        private void UpdateAnimal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string query = "UPDATE Animal SET Name = @NewName WHERE Id = @AnimalId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("AnimalId", listAnimals.SelectedValue);
                if (MainTextBox.Text.Length > 0 && MainTextBox.Text.Length <= 50)
                {
                    sqlCommand.Parameters.AddWithValue("NewName", MainTextBox.Text);
                    sqlCommand.ExecuteScalar();

                }
                else
                {
                    throw new ArgumentException($"Wrong Length Of Zoo Name: {MainTextBox.Text.Length} (Must be between 1 and 50)");
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
            finally
            {
                sqlConnection.Close();
                ShowAnimals();
                ShowAssociatedAnimals();
            }
        }
    }
}
