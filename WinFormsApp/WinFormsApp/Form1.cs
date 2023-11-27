using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WinFormsApp
{
    public partial class Form1 : Form
    {
        private readonly object databaseLock = new object();
        private CancellationTokenSource cancellationTokenSource;
        private bool isFormClosing = false;
        public Form1()
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
        }
        private void StartBackgroundProcess()
        {
            try
            {
                
                MySqlConnection connection = new MySqlConnection("datasource=localhost;port=3306;username=root;password=orkun123");
                MySqlCommand command = connection.CreateCommand();

                connection.Open();

                int loopCounter = 0; 
                int maxLoops = 100;

                while (loopCounter < maxLoops && !isFormClosing)
                {
                    lock (databaseLock)
                    {
                       
                        MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT * FROM my_database.my_table", connection);
                        DataSet ds = new DataSet();
                        adapter.Fill(ds, "my_table");
                        int currentValue;
                        foreach (DataRow row in ds.Tables["my_table"].Rows)
                        {

                            currentValue = Convert.ToInt32(row["integer_column"]);
                            currentValue = currentValue + 1;

                            string updateQuery = "UPDATE  my_database.my_table SET integer_column = @newValue";
                            MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection);

                            int newValue = currentValue;
                            updateCommand.Parameters.AddWithValue("@newValue", newValue);

                            string uupdateQuery = "UPDATE  my_database.my_table SET string_column = @newString";
                            MySqlCommand uupdateCommand = new MySqlCommand(uupdateQuery, connection);

                            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
                            string strNumber = Convert.ToString(currentThreadId);

                            StringBuilder sb = new StringBuilder();

                            
                            sb.Append("Thread id : ");
                            sb.Append(strNumber);
                            
                            uupdateCommand.Parameters.AddWithValue("@newString", sb);

                            updateCommand.ExecuteNonQuery();
                            uupdateCommand.ExecuteNonQuery();



                        }

                     
                        dataGridView1.Invoke((MethodInvoker)delegate
                        {
                            dataGridView1.DataSource = ds.Tables["my_table"];
                            dataGridView1.Refresh(); 
                        });

                        Task.Delay(100).Wait();
                        loopCounter++;
                    }
                }
                connection.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void Form1_Load(object sender, EventArgs e)
        {

            cancellationTokenSource = new CancellationTokenSource();
            Thread Thread1 = new Thread(new ThreadStart(StartBackgroundProcess));
            Thread1.Start();

            Thread Thread2 = new Thread(new ThreadStart(StartBackgroundProcess));
            Thread2.Start();



        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isFormClosing = true;
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel(); 
            }
        }


    }
}
