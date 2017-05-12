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
        private static string DefaultConnectionString => ConfigurationManager.ConnectionStrings["defaultdb"].ConnectionString;
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
                 TxtTables.Items.Add($"{dbTable}{Environment.NewLine}");
            }
        }

        public static List<string> GetTables(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var schema = connection.GetSchema("Tables");
                var tableNames = new List<string>();
                foreach (DataRow row in schema.Rows)
                {
                    tableNames.Add(row[2].ToString().Replace("\r\n", string.Empty));
                }

                return tableNames;
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
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                #region ModelGeneration
                foreach (var item in TxtTables.SelectedItems)
                {
                    var table = item.ToString().Replace("\r\n", string.Empty);
                    var beginClass = $"namespace {TxtNameSpace.Text}{Environment.NewLine}{{{Environment.NewLine}\tpublic class {table.TablePrettify()}{Environment.NewLine}\t{{\t{Environment.NewLine}";
                    var sb = new StringBuilder();

                    using (var command = new SqlCommand($"select top 1 * from {table}", connection))
                    {
                        var dt = new DataTable();
                        dt.Load(command.ExecuteReader());
                        foreach (DataColumn dataColumn in dt.Columns)
                        {
                            sb.Append(SqlHelp.NetModel(dataColumn.ColumnName, SqlHelp.Convert(Convert.ToString(dataColumn.DataType), dataColumn)));
                        }
                    }

                    var endClass = $"\t}}{Environment.NewLine}}}";
                    var finalClass = $"{beginClass}{sb}{endClass}{Environment.NewLine}";
                    TxtColumnType.Text += finalClass;

                    if (!string.IsNullOrWhiteSpace(TxtPathDirectory.Text))
                    {
                        var modelDirectory = $@"{TxtPathDirectory.Text}\Models\";

                        if (!Directory.Exists(modelDirectory))
                        {
                            Directory.CreateDirectory(modelDirectory);
                        }

                        File.WriteAllText($@"{modelDirectory}{table.TablePrettify()}.cs", finalClass);
                    }
                }
                #endregion
                #region Mapping

                foreach (var item in TxtTables.SelectedItems)
                {
                    var table = item.ToString().Replace("\r\n", string.Empty);
                    var beginMappingClass = $"using System.Data.Entity.ModelConfiguration;{Environment.NewLine}using {TxtNameSpace.Text};{Environment.NewLine}{Environment.NewLine}namespace {TxtMappingNameSpace.Text}{Environment.NewLine}{{{Environment.NewLine}";

                    beginMappingClass += $"\tpublic class {table.TablePrettify()}Map : EntityTypeConfiguration<{table.TablePrettify()}>{Environment.NewLine}\t{{{Environment.NewLine}\t\tpublic {table.TablePrettify()}Map(){Environment.NewLine}\t\t{{{Environment.NewLine}{string.Format("\t\t\tthis.ToTable(\"{0}\", \"dbo\");", table)}{Environment.NewLine}";

                    var sb = new StringBuilder();

                    using (var command = new SqlCommand($"select top 1 * from {table}", connection))
                    {
                        var dt = new DataTable();
                        dt.Load(command.ExecuteReader());
                        //ideally the first column is the primary key
                        sb.Append($"\t\t\tthis.HasKey(x => x.{dt.Columns[0].ColumnName.ColumnPrettify()});{Environment.NewLine}");
                        foreach (DataColumn dataColumn in dt.Columns)
                        {
                            sb.Append(SqlHelp.NetMapping(dataColumn.ColumnName.ColumnPrettify(), dataColumn.ColumnName));
                        }
                    }

                    var mappingString = beginMappingClass;
                    mappingString += sb.ToString();
                    mappingString += $"\t\t}}{Environment.NewLine}\t}}{Environment.NewLine}}}";

                    TxtColumnType.Text += mappingString;

                    if (!string.IsNullOrWhiteSpace(TxtPathDirectory.Text))
                    {
                        var mappingDirectory = $@"{TxtPathDirectory.Text}\Mapping\";

                        if (!Directory.Exists(mappingDirectory))
                        {
                            Directory.CreateDirectory(mappingDirectory);
                        }

                        File.WriteAllText($@"{mappingDirectory}{table.TablePrettify()}Map.cs", mappingString);
                    }
                }
                
                #endregion
                connection.Close();
            }
        }
    }
}
