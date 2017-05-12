using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using AutoModelMapping.Helpers;

namespace AutoModelMapping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string DefaultConnectionString => ConfigurationManager.ConnectionStrings["defaultdb"].ConnectionString;
        private string ConnectionString { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnGetTables_OnClick(object sender, RoutedEventArgs e)
        {
            ConnectionString = string.IsNullOrEmpty(TxtConnectionString.Text) ? DefaultConnectionString : TxtConnectionString.Text;

            var dbTables = GetTables(ConnectionString);
            TxtTables.Items.Clear();
            foreach (var dbTable in dbTables.OrderBy(x=>x))
            {
                 TxtTables.Items.Add(string.Format("{0}{1}", dbTable, Environment.NewLine));
            }
        }

        public static List<string> GetTables(string connectionString)
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                DataTable schema = connection.GetSchema("Tables");
                List<string> TableNames = new List<string>();
                foreach (DataRow row in schema.Rows)
                {
                    TableNames.Add(row[2].ToString().Replace("\r\n", string.Empty));
                }

                return TableNames;
            }
        }

        private void CbSelectAllTables_OnClick(object sender, RoutedEventArgs e)
        {
            if (CbSelectAllTables.IsChecked.GetValueOrDefault(false))
            {
                TxtTables.SelectAll();
            }
            else
            {
                TxtTables.UnselectAll();
            }
        }

        private void btnGetModels_OnClick(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                #region ModelGeneration
                foreach (var item in TxtTables.SelectedItems)
                {
                    var table = item.ToString().Replace("\r\n", string.Empty);
                    string beginClass = string.Format("namespace {0}{1}{2}{3}\tpublic class {4}{5}\t{6}\t{7}", TxtNameSpace.Text, Environment.NewLine, "{", Environment.NewLine, table.TablePrettify(), Environment.NewLine, "{", Environment.NewLine);
                    StringBuilder sb = new StringBuilder();

                    using (SqlCommand command = new SqlCommand(string.Format("select top 1 * from {0}", table), connection))
                    {
                        DataTable dt = new DataTable();
                        dt.Load(command.ExecuteReader());
                        foreach (DataColumn dataColumn in dt.Columns)
                        {
                            sb.Append(SqlHelp.NetModel(dataColumn.ColumnName, SqlHelp.Convert(Convert.ToString(dataColumn.DataType), dataColumn)));
                        }
                    }

                    string endClass = String.Format("\t{0}{1}{2}", "}", Environment.NewLine, "}");
                    var finalClass = string.Format("{0}{1}{2}{3}", beginClass, sb, endClass, Environment.NewLine);
                    TxtColumnType.Text += finalClass;

                    if (!string.IsNullOrWhiteSpace(TxtPathDirectory.Text))
                    {
                        if (!Directory.Exists(TxtPathDirectory.Text))
                        {
                            Directory.CreateDirectory(TxtPathDirectory.Text);
                        }

                        var fullDirectory = string.Format(@"{0}\Models\", TxtPathDirectory.Text);
                       
                        System.IO.File.WriteAllText(string.Format(@"{0}{1}.cs", fullDirectory, table.TablePrettify()), finalClass);
                    }
                }
                #endregion
                #region Mapping

                foreach (var item in TxtTables.SelectedItems)
                {
                    var table = item.ToString().Replace("\r\n", string.Empty);
                    var beginMappingClass = string.Format("{0}{1}using {2};{3}{4}namespace {5}{6}{7}{8}",
                    "using System.Data.Entity.ModelConfiguration;", Environment.NewLine, TxtNameSpace.Text,
                    Environment.NewLine, Environment.NewLine, TxtMappingNameSpace.Text, Environment.NewLine, "{", Environment.NewLine);

                    beginMappingClass += string.Format("\tpublic class {0}Map : EntityTypeConfiguration<{1}>{2}\t{3}{4}\t\tpublic {5}Map(){6}\t\t{7}{8}{9}{10}",
                        table.TablePrettify(), table.TablePrettify(), Environment.NewLine, "{" ,Environment.NewLine, table.TablePrettify(), Environment.NewLine, "{", Environment.NewLine, string.Format("\t\t\tthis.ToTable(\"{0}\", \"dbo\");", table), Environment.NewLine);

                    StringBuilder sb = new StringBuilder();

                    using (SqlCommand command = new SqlCommand(string.Format("select top 1 * from {0}", table), connection))
                    {
                        DataTable dt = new DataTable();
                        dt.Load(command.ExecuteReader());
                        //ideally the first column is the primary key
                        sb.Append(string.Format("\t\t\tthis.HasKey(x => x.{0});{1}", dt.Columns[0].ColumnName.ColumnPrettify(), Environment.NewLine));
                        foreach (DataColumn dataColumn in dt.Columns)
                        {
                            sb.Append(SqlHelp.NetMapping(dataColumn.ColumnName.ColumnPrettify(), dataColumn.ColumnName));
                        }
                    }

                    var mappingString = beginMappingClass;
                    mappingString += sb.ToString();
                    mappingString += string.Format("\t\t{0}{1}\t{2}{3}{4}", "}", Environment.NewLine, "}", Environment.NewLine, "}");

                    TxtColumnType.Text += mappingString;

                    if (!string.IsNullOrWhiteSpace(TxtPathDirectory.Text))
                    {
                        var fullDirectory = string.Format(@"{0}\Mapping\", TxtPathDirectory.Text);
                        
                        System.IO.File.WriteAllText(string.Format(@"{0}{1}Map.cs", fullDirectory, table.TablePrettify()), mappingString);
                    }
                }
                
                #endregion
                connection.Close();
            }
        }
    }
}
