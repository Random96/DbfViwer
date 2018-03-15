using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DbfViewer.Forms
{
    public partial class DataViewForm : Form
    {
        private DataTable _dataTable;
        private string _filter = string.Empty;
        private string _sql = string.Empty;
        private string _database = string.Empty;

        public DataViewForm()
        {
            InitializeComponent();
        }

        private void filterStripButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_sql))
                SetupFilter();
            SetupSql();
        }

        private void SetupSql()
        {
            // get SQL
            using (var sqlForm = new InputMultyLineForm())
            {
                bool compleate = false;
                sqlForm.sqlTextBox.Text = _sql;
                while ( !compleate)
                {
                    if (sqlForm.ShowDialog() != DialogResult.OK)
                        return;

                    try
                    {
                        SetSql(sqlForm.sqlTextBox.Text);
                        compleate = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка установки SQL");
                    }
                }
            }
        }

        private void SetupFilter()
        {
            using (var frm = new InputText())
            {
                frm.textBox1.Text = _filter;
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var dt = _dataTable.Select(frm.textBox1.Text);
                        var newDt = _dataTable.Clone();
                        foreach (var row in dt)
                        {
                            newDt.Rows.Add(row.ItemArray);
                        }
                        dataGridView1.DataSource = newDt;
                        _filter = frm.textBox1.Text;
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message, "Ошибка");
                    }
                }
            }

        }

        internal void SetSql(string sqlString)
        {
            using (var connect = new OleDbConnection())
            {
                connect.ConnectionString = GetConnectionString(_database);
                connect.Open();

                using (var cmd = connect.CreateCommand())
                {
                    cmd.CommandText = sqlString;

                    using (var da = new OleDbDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        SetDatabase(dt);

                        _sql = sqlString;
                    }
                }
            }
        }

        internal void SetSql(string databaseName, string sqlString)
        {
            using (var connect = new OleDbConnection())
            {
                connect.ConnectionString = GetConnectionString(databaseName);
                connect.Open();

                using (var cmd = connect.CreateCommand())
                {
                    cmd.CommandText = sqlString;

                    using (var da = new OleDbDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        SetDatabase(dt);

                        _sql = sqlString;
                        _database = databaseName;
                    }
                }
            }
        }

        public void SetDatabase(DataTable data)
        {
            _dataTable = data;
            dataGridView1.DataSource = data;
            _filter = string.Empty;
        }

        internal void SetFiedName(string fileName)
        {
            var directory = System.IO.Path.GetDirectoryName(fileName);
            var shortName = System.IO.Path.GetFileName(fileName);
            if (shortName != null)
            {
                var tableName = shortName.Split('.')[0];

                Text = "Окно " + tableName;

                using (var connect = new OleDbConnection())
                {
                    connect.ConnectionString = GetConnectionString(directory);
                    connect.Open();

                    using (var cmd = connect.CreateCommand())
                    {
                        cmd.CommandText = tableName;
                        cmd.CommandType = CommandType.TableDirect;
                        using (var da = new OleDbDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            SetDatabase(dt);
                        }
                    }
                }
            }
        }

        private string GetConnectionString(string directory)
        {
            return @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + directory + ";Extended Properties=dBase IV";
        }
    }
}
