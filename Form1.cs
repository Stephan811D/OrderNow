using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.IO;
using System.Reflection;
using iTextSharp.text.pdf;
using iTextSharp.text;
using iTextSharp.text.pdf.draw;
using System.Security;
using System.Security.Cryptography;
using System.Drawing.Drawing2D;
using System.Threading;

namespace Order_Now
{
    public partial class Form1 : Form
    {
        //Settings
        public string Server;
        public int Port;
        public string Benutzer;
        public string Passwort;
        public string Datenbank;

        //Connection
        private MySqlConnection connection;
        private string connectionString;

        //GridView

        private MySqlDataAdapter myDataAdapter;
        private MySqlCommandBuilder mySQLCommandBuilder;
        private DataSet datasetArtikel, datasetBestellung;

        public int? selectedRowArtikel = 0, selectedRowOrder;

        public bool succesfulLoad = false;

        //Order
        public decimal totalOrder;

        //Filter
        public BindingSource bs;

        //Selected
        private string _selectedArtikel_ID, _selectedArtikel_ArtNr, _selectedArtikel_Artikel, _selectedArtikel_Einkaufspreis, _selectedArtikel_Verkaufspreis, _selectedArtikel_Barcode;
        private string _selectedBestellung_ID, _selectedBestellung_Anzahl, _selectedBestellung_ArtNr, _selectedBestellung_Artikel, _selectedBestellung_Preis, _selectedBestellung_Barcode;

        //Form Login
        Anmelden_Form anmelde_form;

        //Form Add
        Form f2;

        //Form Update
        Form f3, f4;

        //Form Delete
        Form_Löschen f5;
        Bestellung_Löschen bestellung_löschen;

        //Form
        Version_Info vi;

        //Security
        static byte[] entropy = System.Text.Encoding.Unicode.GetBytes("$OrderNow");


        public int currentSelection = 0;
        public Form1()
        {
            ///Thread t = new Thread(new ThreadStart(SplashStart));
            //Thread 
            //t.Start();
            //Thread.Sleep(3000);
            InitializeComponent();
            //t.Abort();
        }

        public void SplashStart()
        {
            //Application.Run(new Form_LoadScreen());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //WindowState = FormWindowState.Maximized;

            //Disabled Function
            toolStripDropDownButton3.Enabled = false;

            loadSettings();

            try
            {
                if (Server != "" && Port != 0 && Benutzer != "" && Passwort != "")
                {
                    LoadData();
                    dataGridView1.Sort(this.dataGridView1.Columns["ArtNr"], ListSortDirection.Ascending);
                    dataGridView2.Sort(this.dataGridView2.Columns["ArtNr"], ListSortDirection.Ascending);
                    setSettings();
                    CloseConnection();
                }
                else
                {
                    anmelde_form = new Anmelden_Form(this);
                    anmelde_form.Show();
                }
                dataGridView1.Select();
                dataGridView2.Select();
            }
            catch (Exception eFormLoad)
            {
                MessageBox.Show(eFormLoad.ToString(), "Order Now - Fehler bei der Initialisierung", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            f2 = new Form2(this);
            f2.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1 != null && dataGridView1.SelectedRows.Count > 0 && dataGridView1.CurrentCell != null)
            {
                int _selectedArticle_ID, _selectedArticle_ArtNr;
                string _selectedArticle_Artikel, _selectedArticle_Barcode;
                decimal _selectedArticle_einkaufsPreis, _selectedArticle_verkaufsPreis;

                _selectedArticle_ID = Convert.ToInt32(dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[0].Value);
                _selectedArticle_ArtNr = Convert.ToInt32(dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[1].Value);
                _selectedArticle_Artikel = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[2].Value.ToString();
                _selectedArticle_einkaufsPreis = Convert.ToDecimal(dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[3].Value);
                _selectedArticle_verkaufsPreis = Convert.ToDecimal(dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[4].Value);
                _selectedArticle_Barcode = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[5].Value.ToString();

                f3 = new Form3(_selectedArticle_ID, _selectedArticle_ArtNr, _selectedArticle_Artikel, _selectedArticle_einkaufsPreis, _selectedArticle_verkaufsPreis, _selectedArticle_Barcode, this);
                f3.Show();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0 && dataGridView2.SelectedRows.Count > 0 && dataGridView2.CurrentCell != null)
            {
                int _selectedArticle_ID, _selectedArticle_Anz, _selectedArticle_ArtNr;
                string _selectedArticle_Artikel, _selectedArticle_Barcode;
                decimal _selectedArticle_Preis;

                _selectedArticle_ID = Convert.ToInt32(dataGridView2.Rows[dataGridView2.CurrentCell.RowIndex].Cells[0].Value);
                _selectedArticle_Anz = Convert.ToInt32(dataGridView2.Rows[dataGridView2.CurrentCell.RowIndex].Cells[1].Value);
                _selectedArticle_ArtNr = Convert.ToInt32(dataGridView2.Rows[dataGridView2.CurrentCell.RowIndex].Cells[2].Value);
                _selectedArticle_Artikel = dataGridView2.Rows[dataGridView2.CurrentCell.RowIndex].Cells[3].Value.ToString();
                _selectedArticle_Preis = Convert.ToDecimal(dataGridView2.Rows[dataGridView2.CurrentCell.RowIndex].Cells[4].Value);
                _selectedArticle_Barcode = dataGridView2.Rows[dataGridView2.CurrentCell.RowIndex].Cells[5].Value.ToString();

                f4 = new Form4(_selectedArticle_ID, _selectedArticle_Anz, _selectedArticle_ArtNr, _selectedArticle_Artikel, _selectedArticle_Preis, _selectedArticle_Barcode, this);
                f4.Show();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //Löschen-Button: Bestellung

            if (dataGridView2 != null && dataGridView2.Rows.Count > 0 && dataGridView2.CurrentCell != null)
            {
                int _selectedArticle_ID;
                string _selectedArticle_Artikel;
                string _selectedTable = "bestellung";

                _selectedArticle_ID = Convert.ToInt32(dataGridView2.Rows[dataGridView2.CurrentCell.RowIndex].Cells[0].Value);
                _selectedArticle_Artikel = dataGridView2.Rows[dataGridView2.CurrentCell.RowIndex].Cells[3].Value.ToString();

                f5 = new Form_Löschen(_selectedTable, _selectedArticle_Artikel, _selectedArticle_ID, this);
                f5.Show();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Löschen-Button: Artikel

            if (dataGridView1 != null && dataGridView1.Rows.Count > 0 && dataGridView1.CurrentCell != null)
            {
                int _selectedArticle_ID;
                string _selectedArticle_Artikel;
                string _selectedTable = "artikel";

                _selectedArticle_ID = Convert.ToInt32(dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[0].Value);
                _selectedArticle_Artikel = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[2].Value.ToString();

                f5 = new Form_Löschen(_selectedTable, _selectedArticle_Artikel, _selectedArticle_ID, this);
                f5.Show();
            }
        }

        //DataGridView Selection
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1 != null && dataGridView1.Rows.Count > 0 && dataGridView1.CurrentCell != null)
            {
                dataGridView1_Selection(e);

                if (dataGridView1.SelectedRows.Count > 0)
                {
                    button2.Enabled = true;
                    button3.Enabled = true;
                    button5.Enabled = true;
                    numericUpDown1.Enabled = true;
                }
                else
                {
                    button2.Enabled = false;
                    button3.Enabled = false;
                    button5.Enabled = false;
                    numericUpDown1.Enabled = false;
                }
            }
        }
        private void dataGridView1_Selection(EventArgs e)
        {
            if (e != null)
            {
                //Label: ID
                _selectedArtikel_ID = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[0].Value.ToString();
                //label13.Text = _selectedArtikel_ID;

                //Label: ArtNr
                _selectedArtikel_ArtNr = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[1].Value.ToString();
                label3.Text = _selectedArtikel_ArtNr;

                //Label: Artikel
                _selectedArtikel_Artikel = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[2].Value.ToString();
                //label5.Text = _selectedArtikel_Artikel;
                label4.Text = _selectedArtikel_Artikel;


                if (_selectedArtikel_Artikel.Length >= 25)
                {
                    label4.Font = new System.Drawing.Font("Verdana", 10, FontStyle.Bold);
                }

                else {
                    label4.Font = new System.Drawing.Font("Verdana", 12, FontStyle.Bold);
                }

                //Label: Einkaufspreis
                _selectedArtikel_Einkaufspreis = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[3].Value.ToString();
                label7.Text = _selectedArtikel_Einkaufspreis + "€";

                //Label: Verkaufspreis
                _selectedArtikel_Verkaufspreis = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[4].Value.ToString();
                label9.Text = _selectedArtikel_Verkaufspreis + "€";

                //Label: Barcode
                _selectedArtikel_Barcode = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[5].Value.ToString();
                label11.Text = _selectedArtikel_Barcode;
                generateBarcodeEAN13(_selectedArtikel_Barcode);
            }
        }
        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView2 != null && dataGridView2.Rows.Count > 0 && dataGridView2.CurrentCell != null)
            {
                dataGridView2_Selection(e);

                if (dataGridView2.SelectedRows.Count > 0)
                {
                    button4.Enabled = true;
                    button6.Enabled = true;
                    button7.Enabled = true;
                }
                else
                {
                    button4.Enabled = false;
                    button6.Enabled = false;
                    button7.Enabled = false;
                }
            }
        }
        private void dataGridView2_Selection(EventArgs e)
        {
            if (e != null)
            {
                //Label: Anzahl
                _selectedBestellung_Anzahl = dataGridView2.Rows[dataGridView2.CurrentCell.RowIndex].Cells[1].Value.ToString();
                label18.Text = _selectedBestellung_Anzahl;

                //Label: ArtNr
                _selectedBestellung_ArtNr = dataGridView2.Rows[dataGridView2.CurrentCell.RowIndex].Cells[2].Value.ToString();
                label20.Text = _selectedBestellung_ArtNr;

                //Label: Artikel
                _selectedBestellung_Artikel = dataGridView2.Rows[dataGridView2.CurrentCell.RowIndex].Cells[3].Value.ToString();
                panel7.Text = _selectedBestellung_Artikel;

                //Label: Preis
                _selectedBestellung_Preis = dataGridView2.Rows[dataGridView2.CurrentCell.RowIndex].Cells[4].Value.ToString();
                label25.Text = _selectedBestellung_Preis + "€";

                //Label: Barcode
                _selectedBestellung_Barcode = dataGridView2.Rows[dataGridView2.CurrentCell.RowIndex].Cells[5].Value.ToString();
                label29.Text = _selectedBestellung_Barcode;

                //Label: Summe
                label21.Text = (Convert.ToInt32(_selectedBestellung_Anzahl) * decimal.Parse(_selectedBestellung_Preis)).ToString() + "€";
            }
        }

        public DataTable GetContentAsDataTable(bool IgnoreHideColumns = false)
        {
            try
            {
                if (dataGridView2.ColumnCount == 0) return null;
                DataTable dtSource = new DataTable();

                dtSource.Columns.Add("ArtNr", typeof(int));
                dtSource.Columns.Add("Bezeichnung", typeof(string));
                dtSource.Columns.Add("Menge", typeof(int));
                dtSource.Columns.Add("Einzelpreis", typeof(decimal));
                dtSource.Columns.Add("Preis", typeof(decimal));

                if (dtSource.Columns.Count == 0) return null;

                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    DataRow row1;

                    row1 = dtSource.NewRow();
                    row1["ArtNr"] = row.Cells[2].Value.ToString();
                    row1["Bezeichnung"] = row.Cells[3].Value.ToString();
                    row1["Menge"] = row.Cells[1].Value.ToString();
                    row1["Einzelpreis"] = row.Cells[4].Value.ToString();
                    row1["Preis"] = (Convert.ToDecimal(row.Cells[1].Value) * Convert.ToDecimal(row.Cells[4].Value)).ToString();

                    dtSource.Rows.Add(row1);
                }

                
                DataView dv = dtSource.DefaultView;
                dv.Sort = "ArtNr asc";
                DataTable sortedDT = dv.ToTable();
     
                return sortedDT;

            }
            catch { return null; }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            createPDF();
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            filterArtikel(1, 2, 5, textBox1, dataGridView1);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            filterArtikel(1, 2, 5, textBox1, dataGridView1);
        }

        private void textBox2_Click(object sender, EventArgs e)
        {
            filterArtikel(2, 3, 5, textBox2, dataGridView2);
        }

        private void AnmeldenToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            anmelde_form = new Anmelden_Form(this);
            anmelde_form.Show();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            filterArtikel(2, 3, 5, textBox2, dataGridView2);
        }

        private void filterArtikel(int spalte1, int spalte2, int spalte3, TextBox tx, DataGridView dgv)
        {
            bs = new BindingSource();
            bs.DataSource = dgv.DataSource;
            bs.Filter = string.Format("CONVERT(" + dgv.Columns[spalte1].DataPropertyName + ", System.String) like '%" + tx.Text.Replace("'", "''") + "%' OR CONVERT(" + dgv.Columns[spalte2].DataPropertyName + ", System.String) like '%" + tx.Text.Replace("'", "''") + "%' OR CONVERT(" + dgv.Columns[spalte3].DataPropertyName + ", System.String) like '%" + tx.Text.Replace("'", "''") + "%'");
            dgv.DataSource = bs;
        }
        private void generateBarcodeEAN13(string barcode)
        {
            try
            {
                BarcodeLib.Barcode b = new BarcodeLib.Barcode();
                pictureBox1.Image = b.Encode(BarcodeLib.TYPE.EAN13, barcode, Color.Black, Color.White, 260, 69);
            }
            catch (Exception e)
            {
                pictureBox1.Image = pictureBox1.ErrorImage;
                MessageBox.Show("Der eingegene Barcode ist nicht gülitg!");
            }
        }


        private void button5_Click(object sender, EventArgs e)
        {
            string artikel, barcode;
            int anzahl, artNr;
            decimal preis;

            anzahl = Convert.ToInt32(numericUpDown1.Value);

            if (anzahl != 0 && dataGridView1 != null && dataGridView1.CurrentCell != null)
            {
                artNr = Convert.ToInt32(dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[1].Value.ToString());
                artikel = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[2].Value.ToString();
                preis = Convert.ToDecimal(dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[3].Value.ToString());
                barcode = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[5].Value.ToString();

                addArticleToOrder(anzahl, artNr, artikel, preis, barcode);
                LoadGridViews();
            }
        }

        public void loadSettings()
        {
            Server = Properties.Settings.Default["Server"].ToString();
            Port = Convert.ToInt32(Properties.Settings.Default["Port"]);
            Benutzer = Properties.Settings.Default["Benutzer"].ToString();
            Passwort = Properties.Settings.Default["Passwort"].ToString();
            Datenbank = Properties.Settings.Default["Datenbank"].ToString();
        }
        public void LoadData()
        {
            Initialize(Server, Port, Benutzer, Passwort);
            OpenConnection();
            LoadGridViews();
        }
        public void setSettings()
        {
            setdataGridViewStyle();
            getOrderDetails();
        }
        public void Initialize(string server, int port, string username, string password)
        {
            try
            {
                string passwort = ToInsecureString(DecryptString(password));
                connectionString = "datasource=" + server + ";" + "port=" + port + ";" + "username=" + username + ";" + "password=" + passwort + ";";

                connection = new MySqlConnection(connectionString);

                toolStripLabel1.Text = "Server: " + server;
                toolStripLabel5.Text = "Datenbank: " + Datenbank;

                //BindingSource bs = new BindingSource();
                //bs.DataSource = getDatabases();
                //toolStripLabel2.ComboBox.DataSource = bs;
            }
            catch (Exception eInitialize)
            {
                MessageBox.Show(eInitialize.ToString());
            }
        }
        public bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Es konnte keine Verbindung zum Server hergestellt werden.");
                        break;
                    case 1045:
                        MessageBox.Show("Es wurden falsche Login-Daten verwendet.", "Order Now - Falsche Login-Daten", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case 1042:
                        MessageBox.Show("Es konnte keine Verbindung zum Server hergestellt werden.", "Order Now - Keine Verbindung", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        MessageBox.Show(ex.ToString());
                        break;
                }
                return false;
            }
        }
        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public List<string> getDatabases()
        {
            string query1 = "SHOW DATABASES;"; // WHERE Table like 'artikel';";
            List<string> databases = new List<string>();

            MySqlDataReader Reader;

            try
            {
                if (this.OpenConnection() == true)
                {
                    MySqlCommand cmd1 = new MySqlCommand(query1, connection);
                    Reader = cmd1.ExecuteReader();

                    while (Reader.Read())
                    {
                        string row = "";
                        for (int i = 0; i < Reader.FieldCount; i++)
                        {
                            row += Reader.GetValue(i).ToString();
                            databases.Add(row);
                        }
                    }

                    CloseConnection();
                    return databases;
                }
                else
                {
                    OpenConnection();
                    return null;
                }
            }
            catch (Exception eNoOrderDatabases)
            {
                MessageBox.Show(eNoOrderDatabases.ToString(), "Order Now - Datenbanken konnten nicht geladen werden", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        private void versionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            vi = new Version_Info();
            vi.Show();
        }

        private void bestellungLöschenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bestellung_löschen = new Bestellung_Löschen(this);
            bestellung_löschen.Show();
        }

        private void groupBox1_Paint(object sender, PaintEventArgs e)
        {
            //GroupBox box = sender as GroupBox;
            //DrawGroupBox(box, e.Graphics, Color.Black, Color.Black, groupBox1.Width, groupBox1.Height, 30);
        }

        private void groupBox2_Paint(object sender, PaintEventArgs e)
        {
            //GroupBox box = sender as GroupBox;
            //DrawGroupBox(box, e.Graphics, Color.Black, Color.Black, groupBox2.Width, groupBox2.Height, 30);
        }

        private void bestellenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            createPDF();
        }

        private void DrawGroupBox(GroupBox box, Graphics g, Color textColor, Color borderColor, int Width, int Height, int Offset)
        {
            if (box != null)
            {
                Brush textBrush = new SolidBrush(textColor);
                Brush borderBrush = new SolidBrush(borderColor);
                Pen borderPen = new Pen(borderBrush);
                SizeF strSize = g.MeasureString(box.Text, box.Font);
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(box.ClientRectangle.X,
                                               box.ClientRectangle.Y + (int)(strSize.Height / 2),
                                               box.ClientRectangle.Width - 1,
                                               box.ClientRectangle.Height - (int)(strSize.Height / 2) - 1);

                // Clear text and border
                g.Clear(this.BackColor);

                // Draw text
                g.DrawString(box.Text, box.Font, textBrush, box.Padding.Left, 0);

                // Drawing Border
                //Left
                g.DrawLine(borderPen, rect.Location, new Point(rect.X, rect.Y + rect.Height));
                //Right
                g.DrawLine(borderPen, new Point(rect.X + rect.Width, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height));
                //Bottom
                g.DrawLine(borderPen, new Point(rect.X, rect.Y + rect.Height), new Point(rect.X + rect.Width, rect.Y + rect.Height));
                //Top1
                g.DrawLine(borderPen, new Point(rect.X, rect.Y), new Point(rect.X + box.Padding.Left, rect.Y));
                //Top2
                g.DrawLine(borderPen, new Point(rect.X + box.Padding.Left + (int)(strSize.Width), rect.Y), new Point(rect.X + rect.Width, rect.Y));

                if (Width != 0 && Height != 0)
                {
                    LinearGradientBrush linGrBrush = new LinearGradientBrush(
                    new Point(0, Offset - 1),
                    new Point(0, Height),
                    Color.FromArgb(0, 0, 0, 0),
                    Color.FromArgb(255, 230, 230, 230));

                    float[] relativeIntensities = { 0f, 0.4f, 0.5f, 1f, 1f };
                    float[] relativePositions = { 0f, 0.1f, 0.2f, 0.4f, 1.0f };

                    //Create a Blend object and assign it to linGrBrush.
                    Blend blend = new Blend();
                    blend.Factors = relativeIntensities;
                    blend.Positions = relativePositions;
                    linGrBrush.Blend = blend;

                    System.Drawing.Rectangle gradient_rectangle = new System.Drawing.Rectangle(1, Offset, Width - 2, Height - Offset - 1);
                    g.FillRectangle(linGrBrush, gradient_rectangle);
                }
            }
        }

        private void eMailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_Email Form_Email = new Form_Email();
            Form_Email.Show();
        }
        public void LoadGridViews()
        {
            try
            {
                dataGridView1.DefaultCellStyle.SelectionBackColor = Color.DarkRed;
                dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;

                myDataAdapter = new MySqlDataAdapter();
                datasetArtikel = new DataSet();
                datasetBestellung = new DataSet();

                myDataAdapter.SelectCommand = new MySqlCommand("select * from " + Datenbank + ".artikel", connection);
                mySQLCommandBuilder = new MySqlCommandBuilder(myDataAdapter);
                myDataAdapter.Fill(datasetArtikel);

                myDataAdapter.SelectCommand = new MySqlCommand("select * from " + Datenbank + ".bestellung", connection);
                myDataAdapter.Fill(datasetBestellung);

                dataGridView1.DataSource = datasetArtikel.Tables[0];
                dataGridView1.Columns["ID"].Visible = false;
                dataGridView2.DataSource = datasetBestellung.Tables[0];
                dataGridView2.Columns["ID"].Visible = false;

                if (selectedRowArtikel != null)
                {
                    dataGridView1.Sort(this.dataGridView1.Columns["ArtNr"], ListSortDirection.Ascending);
                    dataGridView1.Rows[Convert.ToInt32(selectedRowArtikel)].Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = Convert.ToInt32(selectedRowArtikel);
                }
                if (selectedRowOrder != null && dataGridView2.RowCount > 0)
                {
                    dataGridView2.Rows[Convert.ToInt32(selectedRowOrder)].Selected = true;
                    dataGridView2.FirstDisplayedScrollingRowIndex = Convert.ToInt32(selectedRowOrder);
                }
                succesfulLoad = true;
            }
            catch (Exception eLoadGridViews)
            {
                MessageBox.Show("Die ausgewählte Datenbank enthält nicht die erforderten Tabellen (Artikel & Bestellungen).", "Order Now - Keine Verbindung zur Datenbank", MessageBoxButtons.OK, MessageBoxIcon.Error);
                succesfulLoad = false;
            }

            CloseConnection();
        }
        public void setdataGridViewStyle()
        {
            //dataGridView-Style

            if (dataGridView1.RowCount != 0 && dataGridView2.RowCount != 0)
            {
                dataGridView1.DefaultCellStyle.Font = new System.Drawing.Font("Verdana", 12);
                dataGridView1.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Verdana", 13);

                dataGridView1.Columns[0].HeaderText = "ID";
                dataGridView1.Columns[1].HeaderText = "ArtNr.";
                dataGridView1.Columns[2].HeaderText = "Artikel";
                dataGridView1.Columns[3].HeaderText = "EK (€)";
                dataGridView1.Columns[4].HeaderText = "VK (€)";
                dataGridView1.Columns[5].HeaderText = "Barcode";

                dataGridView1.Columns[1].Width = 75;
                dataGridView1.Columns[2].Width = 510;
                dataGridView1.Columns[3].Width = 90;
                dataGridView1.Columns[4].Width = 90;
                dataGridView1.Columns[5].Width = 150;

                dataGridView2.DefaultCellStyle.Font = new System.Drawing.Font("Verdana", 12);
                dataGridView2.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Verdana", 13);

                dataGridView2.Columns[0].HeaderText = "ID";
                dataGridView2.Columns[1].HeaderText = "Anzahl";
                dataGridView2.Columns[2].HeaderText = "ArtNr.";
                dataGridView2.Columns[3].HeaderText = "Artikel";
                dataGridView2.Columns[4].HeaderText = "Preis (€)";
                dataGridView2.Columns[5].HeaderText = "Barcode";

                dataGridView2.Columns[1].Width = 80;
                dataGridView2.Columns[2].Width = 85;
                dataGridView2.Columns[3].Width = 510;
                dataGridView2.Columns[4].Width = 115;
                dataGridView2.Columns[5].Width = 150;
            }  
        }
        public void getOrderDetails()
        {
            string query1 = "SELECT Count(*) FROM " + Datenbank + ".bestellung";
            string query2 = "SELECT SUM(Anzahl * Preis) FROM " + Datenbank + ".bestellung";
            int countProducts = -1;

            try
            {
                if (this.OpenConnection() == true)
                {
                    MySqlCommand cmd1 = new MySqlCommand(query1, connection);
                    countProducts = int.Parse(cmd1.ExecuteScalar() + "");
                    label30.Text = countProducts.ToString();

                    MySqlCommand cmd2 = new MySqlCommand(query2, connection);

                    if (cmd2.ExecuteScalar().ToString() == "")
                    {
                        label31.Text = "0,00 €";

                        label18.Text = "-";
                        label20.Text = "-";
                        label21.Text = "-";
                        label25.Text = "-";
                        label29.Text = "-";

                        button4.Enabled = false;
                        button6.Enabled = false;
                        button7.Enabled = false;
                    }
                    else
                    {
                        button4.Enabled = true;
                        button6.Enabled = true;
                        button7.Enabled = true;
                        totalOrder = decimal.Parse(cmd2.ExecuteScalar().ToString());
                        label31.Text = totalOrder.ToString() + "€";
                    }
                }
                CloseConnection();
            }
            catch (Exception eNoOrderDetails)
            {
                MessageBox.Show("Die Bestelldetails konnten nicht geladen werden.", "Order Now - Bestelldetails konnten nicht geladen werden", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void addArticleToOrder(int Anzahl, int ArtNr, string Artikel, decimal Preis, string Barcode)
        {
            string query1 = "SELECT COUNT(*) FROM " + Datenbank + ".bestellung WHERE ArtNr = '" + ArtNr + "';";
            string query2 = "INSERT INTO " + Datenbank + ".bestellung (Anzahl, ArtNr, Artikel, Preis, Barcode) VALUES (" + Anzahl + ", " + ArtNr + ", '" + Artikel + "', " + Preis.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ", " + Barcode + ");";
            string query3 = "UPDATE " + Datenbank + ".bestellung SET Anzahl = Anzahl + '" + Anzahl + "' WHERE ArtNr = '" + ArtNr + "';";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd1 = new MySqlCommand(query1, connection);

                if (cmd1.ExecuteScalar().ToString() == "0")
                {
                    MySqlCommand cmd2 = new MySqlCommand(query2, connection);
                    cmd2.ExecuteNonQuery();
                }
                else
                {
                    MySqlCommand cmd3 = new MySqlCommand(query3, connection);
                    cmd3.ExecuteNonQuery();
                }
            }
            getSelectedRow(dataGridView1, ArtNr);

            CloseConnection();
            getOrderDetails();
        }
        public void deleteArticle(string table, int ID)
        {
            string query = "DELETE FROM " + Datenbank + "." + table + " WHERE ID=" + ID + ";";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.ExecuteNonQuery();

                this.CloseConnection();
            }

            if (table == "bestellung")
            {
                getOrderDetails();

            }
            LoadGridViews();
            CloseConnection();
        }
        public void deleteTable(string table)
        {
            string query = "DELETE FROM " + Datenbank + "." + table + ";";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.ExecuteNonQuery();

                this.CloseConnection();
            }

            if (table == "bestellung")
            {
                getOrderDetails();
            }
            LoadGridViews();
            CloseConnection();
        }
        public void updateArticleInOrder(int ID, int Anzahl, int ArtNr, string Artikel, decimal Preis, string Barcode)
        {
            string query = "UPDATE " + Datenbank + ".bestellung SET Anzahl = '" + Anzahl + "', ArtNr = '" + ArtNr + "', Artikel = '" + Artikel + "', Preis = '" + Preis.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "', Barcode = '" + Barcode + "' WHERE ID=" + ID + ";";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.ExecuteNonQuery();

                this.CloseConnection();
            }

            selectedRowOrder = dataGridView1.CurrentCell.RowIndex;

            getOrderDetails();
            LoadGridViews();
            CloseConnection();
        }
        public void updateArticleInPool(int ID, int ArtNr, string Artikel, decimal EinkaufsPreis, decimal VerkaufsPreis, string Barcode)
        {
            string query = "UPDATE " + Datenbank + ".artikel SET ArtNr = '" + ArtNr + "', Artikel = '" + Artikel + "', EinkaufsPreis = '" + EinkaufsPreis.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "', VerkaufsPreis = '" + VerkaufsPreis.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "', Barcode = '" + Barcode + "' WHERE ID=" + ID + ";";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.ExecuteNonQuery();

                this.CloseConnection();
            }

            selectedRowArtikel = dataGridView1.CurrentCell.RowIndex;

            LoadGridViews();
            CloseConnection();
        }
        public void addArticleToPool(int ArtNr, string Artikel, decimal EinkaufsPreis, decimal VerkaufsPreis, string Barcode)
        {
            string query1 = "SELECT COUNT(*) FROM " + Datenbank + ".artikel WHERE ArtNr = '" + ArtNr + "';";
            string query2 = "INSERT INTO " + Datenbank + ".artikel (ArtNr, Artikel, Einkaufspreis, Verkaufspreis, Barcode) VALUES (" + ArtNr + ", '" + Artikel + "', " + EinkaufsPreis.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ", " + VerkaufsPreis.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ", " + Barcode + ");";
            string query3 = "UPDATE " + Datenbank + ".artikel SET ArtNr = '" + ArtNr + "', Artikel = '" + Artikel + "', Einkaufspreis = '" + EinkaufsPreis.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "', Verkaufspreis = '" + VerkaufsPreis.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "', Barcode = '" + Barcode + "' WHERE ArtNr = '" + ArtNr + "';";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd1 = new MySqlCommand(query1, connection);

                if (cmd1.ExecuteScalar().ToString() == "0")
                {
                    MySqlCommand cmd2 = new MySqlCommand(query2, connection);
                    cmd2.ExecuteNonQuery();
                }
                else
                {
                    MessageBox.Show("Es war bereits ein Artikel mit dieser Artikelnummer in der Datenbank angelegt. Der Artikel wurde aktualisiert.", "Order Now - Artikel wurde aktualisiert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    MySqlCommand cmd3 = new MySqlCommand(query3, connection);
                    cmd3.ExecuteNonQuery();
                }

                this.CloseConnection();

            }

            LoadGridViews();
            CloseConnection();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[1].Value.ToString().Equals(ArtNr.ToString()))
                {
                    selectedRowArtikel = row.Index;
                    LoadGridViews();
                    break;
                }
            }
        }
        public string EncryptString(System.Security.SecureString input)
        {
            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
              System.Text.Encoding.Unicode.GetBytes(ToInsecureString(input)),
              entropy,
              System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }
        public SecureString DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                  Convert.FromBase64String(encryptedData),
                  entropy,
                  System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return new SecureString();
            }
        }
        public SecureString ToSecureString(string input)
        {
            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }
        public string ToInsecureString(SecureString input)
        {
            string returnValue = string.Empty;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }
        public void createPDF()
        {

            DateTime dt = DateTime.Now;
            CultureInfo ci = new CultureInfo("de-DE");

            string filename = "Bestellung_vom_" + dt.ToString("dd", ci) + "." + dt.ToString("MM", ci) + "." + dt.ToString("yyyy", ci) + "_" + dt.Hour + "-" + dt.Minute + "-" + dt.Second + ".pdf";
            string folderPath = @"C:\PDFs\";

            iTextSharp.text.Font Small = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 9f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.DARK_GRAY);
            iTextSharp.text.Font Header = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.DARK_GRAY);
            iTextSharp.text.Font Normal = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 11f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.DARK_GRAY);
            iTextSharp.text.Font Total = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12f, iTextSharp.text.Font.BOLD | iTextSharp.text.Font.UNDERLINE, BaseColor.DARK_GRAY);


            //Creating iTextSharp Table from the DataTable data
            PdfPTable pdfTable = new PdfPTable(dataGridView2.ColumnCount - 2);
            pdfTable.DefaultCell.Padding = 5;
            pdfTable.WidthPercentage = 80;
            pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfTable.DefaultCell.BorderWidth = 1;

            DataTable dataTable = new DataTable();
            dataTable = GetContentAsDataTable(true);

            List<PdfPTable> Tables_Artikel = new List<PdfPTable>();

            PdfPTable table = new PdfPTable(dataTable.Columns.Count);
            table.WidthPercentage = 100;

            PdfPTable testtable = new PdfPTable(dataTable.Columns.Count);


            int[] intTblWidth = { 8, 47, 8, 10, 10 };
            table.SetWidths(intTblWidth);

            Paragraph company_adress_header, own_adress_header;

            //Exporting to PDF
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            using (FileStream stream = new FileStream(folderPath + filename, FileMode.Create))
            {
                //Document pdfDoc = new Document(PageSize.A4, 25, 25, 30, 30);

                using (Document pdfDoc = new Document(PageSize.A4, 25, 25, 30, 30))
                {
                    try
                    {
                        // step 2
                        PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, stream);
                        pdfWriter.PageEvent = new ITextEvents();

                        //open the stream 
                        pdfDoc.Open();

                        pdfDoc.Add(new Paragraph("\n\n\n\n\n\n"));

                        PdfPTable adresses = new PdfPTable(2);
                        int[] intTblWidth2 = { 5, 4 };
                        adresses.SetWidths(intTblWidth2);

                        adresses.WidthPercentage = 100;
                        adresses.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                        company_adress_header = new Paragraph("Empfänger:", Header);
                        own_adress_header = new Paragraph("Rechnungs- & Lieferadresse:", Header);

                        adresses.AddCell(company_adress_header);
                        adresses.AddCell(own_adress_header);

                        var company_adress = new Paragraph();
                        company_adress.Add("- GmbH\n");
                        company_adress.Add("- Straße\n");
                        company_adress.Add("- Location\n");
                        company_adress.Add("Country\n");
                        company_adress.Font = Normal;
                        company_adress.SetLeading(13f, 0f);

                        PdfPCell company_adress_Cell = new PdfPCell();
                        company_adress_Cell.AddElement(company_adress);
                        company_adress_Cell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                        adresses.AddCell(company_adress_Cell);

                        var own_adress = new Paragraph();
                        own_adress.Add("-\n");
                        own_adress.Add("-\n");
                        own_adress.Add("-\n");
                        own_adress.Add("-\n");
                        own_adress.Font = Normal;
                        own_adress.SetLeading(13f, 0f);

                        PdfPCell own_adress_Cell = new PdfPCell();
                        own_adress_Cell.AddElement(own_adress);
                        own_adress_Cell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                        adresses.AddCell(own_adress_Cell);

                        pdfDoc.Add(adresses);


                        pdfDoc.Add(new Paragraph("\n\n"));

                        pdfDoc.Add(new Paragraph("Bestellung vom " + dt.ToString("dd", ci) + ". " + dt.ToString("MMMM", ci) + " " + dt.Year, Header));

                        pdfDoc.Add(new Paragraph("\n"));
                        pdfDoc.Add(new Paragraph("Hiermit bestellen wir bei Ihnen folgende Artikel gemäß Ihrer Preisliste:", Normal));
                        pdfDoc.Add(new Paragraph("\n"));

                        #region if Count <= 15 
                        if (dataTable.Rows.Count <= 15)
                        {
                            //Header
                            for (int k = 0; k < dataTable.Columns.Count; k++)
                            {
                                PdfPCell cell = new PdfPCell(new Phrase(dataTable.Columns[k].ColumnName));

                                cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                                cell.VerticalAlignment = PdfPCell.ALIGN_CENTER;
                                cell.FixedHeight = 20f;
                                cell.BackgroundColor = new iTextSharp.text.BaseColor(240, 240, 240);

                                table.AddCell(cell);
                            }

                            //Rest
                            for (int k = 0; k < dataTable.Rows.Count; k++)
                            {
                                for (int j = 0; j < dataTable.Columns.Count; j++)
                                {
                                    PdfPCell cell = new PdfPCell(new Phrase(dataTable.Rows[k][j].ToString()));

                                    switch (j)
                                    {
                                        case 0:
                                            cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                                            break;
                                        case 1:
                                            cell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                                            cell.PaddingLeft = 5f;
                                            break;
                                        case 2:
                                            cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                                            break;
                                        default:
                                            cell.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                                            cell.PaddingRight = 5f;
                                            break;
                                    }

                                    cell.VerticalAlignment = PdfPCell.ALIGN_CENTER;
                                    cell.FixedHeight = 20f;

                                    table.AddCell(cell);
                                }
                            }

                            //Sum
                            PdfPCell cell_sum_label = new PdfPCell(new Phrase("Gesamtpreis: ", Header));
                            cell_sum_label.Colspan = 4;
                            cell_sum_label.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                            cell_sum_label.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                            cell_sum_label.Padding = 5f;
                            cell_sum_label.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            table.AddCell(cell_sum_label);

                            PdfPCell cell_sum = new PdfPCell(new Phrase(totalOrder.ToString() + "€", Header));
                            cell_sum.Colspan = 1;
                            cell_sum.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                            cell_sum.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                            cell_sum.Padding = 5f;
                            cell_sum.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            table.AddCell(cell_sum);

                            pdfDoc.Add(table);

                            pdfDoc.Add(new Paragraph("\n"));
                            pdfDoc.Add(new Paragraph("Wir bitten um schnellstmögliche Lieferung.\n\nMit freundlichen Grüßen\n\n-", Normal));
                        }
                        #endregion
                        #region if Count > 15 && <= 18

                        if (dataTable.Rows.Count > 15 && dataTable.Rows.Count < 18)
                        {
                            //Header
                            for (int k = 0; k < dataTable.Columns.Count; k++)
                            {
                                PdfPCell cell = new PdfPCell(new Phrase(dataTable.Columns[k].ColumnName));

                                cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                                cell.VerticalAlignment = PdfPCell.ALIGN_CENTER;
                                cell.FixedHeight = 20f;
                                cell.BackgroundColor = new iTextSharp.text.BaseColor(240, 240, 240);

                                table.AddCell(cell);
                            }

                            //Rest
                            for (int k = 0; k < dataTable.Rows.Count; k++)
                            {
                                for (int j = 0; j < dataTable.Columns.Count; j++)
                                {
                                    PdfPCell cell = new PdfPCell(new Phrase(dataTable.Rows[k][j].ToString()));

                                    switch (j)
                                    {
                                        case 0:
                                            cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                                            break;
                                        case 1:
                                            cell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                                            cell.PaddingLeft = 5f;
                                            break;
                                        case 2:
                                            cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                                            break;
                                        default:
                                            cell.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                                            cell.PaddingRight = 5f;
                                            break;
                                    }

                                    cell.VerticalAlignment = PdfPCell.ALIGN_CENTER;
                                    cell.FixedHeight = 20f;

                                    table.AddCell(cell);
                                }
                            }

                            //Sum
                            PdfPCell cell_sum_label = new PdfPCell(new Phrase("Gesamtpreis: ", Header));
                            cell_sum_label.Colspan = 4;
                            cell_sum_label.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                            cell_sum_label.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                            cell_sum_label.Padding = 5f;
                            cell_sum_label.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            table.AddCell(cell_sum_label);

                            PdfPCell cell_sum = new PdfPCell(new Phrase(totalOrder.ToString() + "€", Header));
                            cell_sum.Colspan = 1;
                            cell_sum.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                            cell_sum.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                            cell_sum.Padding = 5f;
                            cell_sum.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            table.AddCell(cell_sum);

                            pdfDoc.Add(table);

                            pdfDoc.Add(new Paragraph("Wir bitten um schnellstmögliche Lieferung.\nMit freundlichen Grüßen\n-", Normal));
                        }
                        #endregion
                        #region if Count > 18

                        if (dataTable.Rows.Count > 18)
                        {
                            int current_Row = 0;
                            int current_Row_BigTable = 0;

                            decimal zwischensumme = 0;

                            #region

                            //Header
                            for (int k = 0; k < dataTable.Columns.Count; k++)
                            {
                                PdfPCell cell = new PdfPCell(new Phrase(dataTable.Columns[k].ColumnName));

                                cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                                cell.VerticalAlignment = PdfPCell.ALIGN_CENTER;
                                cell.FixedHeight = 20f;
                                cell.BackgroundColor = new iTextSharp.text.BaseColor(240, 240, 240);

                                table.AddCell(cell);
                            }

                            //Rest
                            for (int k = 0; k < 18; k++)
                            {
                                current_Row++;
                                for (int j = 0; j < dataTable.Columns.Count; j++)
                                {
                                    PdfPCell cell = new PdfPCell(new Phrase(dataTable.Rows[k][j].ToString()));

                                    switch (j)
                                    {
                                        case 0:
                                            cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                                            break;
                                        case 1:
                                            cell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                                            cell.PaddingLeft = 5f;
                                            break;
                                        case 2:
                                            cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                                            break;
                                        case 4:
                                            cell.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                                            cell.PaddingRight = 5f;
                                            zwischensumme = zwischensumme + Math.Round(Convert.ToDecimal(dataTable.Rows[k][4].ToString()), 2);
                                            break;
                                        default:
                                            cell.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                                            cell.PaddingRight = 5f;
                                            break;
                                    }

                                    cell.VerticalAlignment = PdfPCell.ALIGN_CENTER;
                                    cell.FixedHeight = 20f;

                                    table.AddCell(cell);
                                }
                            }

                            //Sum
                            PdfPCell cell_sum_label = new PdfPCell(new Phrase("Zwischensumme: ", Header));
                            cell_sum_label.Colspan = 4;
                            cell_sum_label.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                            cell_sum_label.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                            cell_sum_label.Padding = 5f;
                            cell_sum_label.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            table.AddCell(cell_sum_label);

                            PdfPCell cell_sum = new PdfPCell(new Phrase(zwischensumme + "€", Header));
                            cell_sum.Colspan = 1;
                            cell_sum.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                            cell_sum.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                            cell_sum.Padding = 5f;
                            cell_sum.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            table.AddCell(cell_sum);

                            pdfDoc.Add(table);
                            pdfDoc.NewPage();
                            pdfDoc.Add(new Paragraph("\n\n\n\n\n"));

                            #endregion

                            table.Rows.Clear();

                            decimal tables_amount = Decimal.Divide(Convert.ToDecimal(dataTable.Rows.Count), 28);
                            tables_amount = Math.Ceiling(tables_amount);

                            for (int i = 0; i < tables_amount; i++)
                            {
                                if (current_Row == dataTable.Rows.Count)
                                {
                                    break;
                                }

                                //Header
                                for (int k = 0; k < dataTable.Columns.Count; k++)
                                {
                                    PdfPCell cell = new PdfPCell(new Phrase(dataTable.Columns[k].ColumnName));

                                    cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                                    cell.VerticalAlignment = PdfPCell.ALIGN_CENTER;
                                    cell.FixedHeight = 20f;
                                    cell.BackgroundColor = new iTextSharp.text.BaseColor(240, 240, 240);

                                    table.AddCell(cell);
                                }

                                //Rest
                                for (int k = 0; k < 28; k++)
                                {
                                    for (int j = 0; j < dataTable.Columns.Count; j++)
                                    {
                                        PdfPCell cell = new PdfPCell(new Phrase(dataTable.Rows[current_Row][j].ToString()));

                                        switch (j)
                                        {
                                            case 0:
                                                cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                                                break;
                                            case 1:
                                                cell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                                                cell.PaddingLeft = 5f;
                                                break;
                                            case 2:
                                                cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                                                break;
                                            case 4:
                                                cell.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                                                cell.PaddingRight = 5f;
                                                decimal value = Math.Round(Convert.ToDecimal(dataTable.Rows[current_Row][4].ToString()), 2);
                                                zwischensumme = zwischensumme + value;
                                                break;
                                            default:
                                                cell.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                                                cell.PaddingRight = 5f;
                                                break;
                                        }

                                        cell.VerticalAlignment = PdfPCell.ALIGN_CENTER;
                                        cell.FixedHeight = 20f;

                                        table.AddCell(cell);
                                    }

                                    current_Row++;
                                    current_Row_BigTable++;

                                    if (current_Row == dataTable.Rows.Count)
                                    {
                                        //Sum
                                        PdfPCell cell_sum_label2 = new PdfPCell(new Phrase("Gesamtsumme: ", Header));
                                        cell_sum_label2.Colspan = 4;
                                        cell_sum_label2.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                                        cell_sum_label2.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                                        cell_sum_label2.Padding = 5f;
                                        cell_sum_label2.Border = iTextSharp.text.Rectangle.NO_BORDER;
                                        table.AddCell(cell_sum_label2);

                                        PdfPCell cell_sum2 = new PdfPCell(new Phrase(totalOrder.ToString() + "€", Total));
                                        cell_sum2.Colspan = 1;
                                        cell_sum2.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                                        cell_sum2.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                                        cell_sum2.Padding = 5f;
                                        cell_sum2.Border = iTextSharp.text.Rectangle.NO_BORDER;
                                        table.AddCell(cell_sum2);

                                        pdfDoc.Add(table);
                                        break;
                                    }

                                    if (k == 27)
                                    {
                                        //Sum
                                        PdfPCell cell_sum_label2 = new PdfPCell(new Phrase("Zwischensumme: ", Header));
                                        cell_sum_label2.Colspan = 4;
                                        cell_sum_label2.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                                        cell_sum_label2.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                                        cell_sum_label2.Padding = 5f;
                                        cell_sum_label2.Border = iTextSharp.text.Rectangle.NO_BORDER;
                                        table.AddCell(cell_sum_label2);

                                        PdfPCell cell_sum2 = new PdfPCell(new Phrase(zwischensumme + "€", Header));
                                        cell_sum2.Colspan = 1;
                                        cell_sum2.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                                        cell_sum2.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                                        cell_sum2.Padding = 5f;
                                        cell_sum2.Border = iTextSharp.text.Rectangle.NO_BORDER;
                                        table.AddCell(cell_sum2);

                                        pdfDoc.Add(table);
                                        pdfDoc.NewPage();
                                        pdfDoc.Add(new Paragraph("\n\n\n\n\n"));
                                        table.Rows.Clear();

                                        current_Row_BigTable = 0;
                                    }
                                }
                            }

                            if (current_Row_BigTable <= 23)
                            {
                                pdfDoc.Add(new Paragraph("\nWir bitten um schnellstmögliche Lieferung.\n\nMit freundlichen Grüßen\n\n-", Normal));
                            }
                            if (current_Row_BigTable > 23 && current_Row_BigTable < 26)
                            {
                                pdfDoc.Add(new Paragraph("\nWir bitten um schnellstmögliche Lieferung.\nMit freundlichen Grüßen\n-", Normal));
                            }
                            if (current_Row_BigTable >= 26)
                            {
                                pdfDoc.NewPage();
                                pdfDoc.Add(new Paragraph("\n\n\n\n\n"));
                                pdfDoc.Add(new Paragraph("Wir bitten um schnellstmögliche Lieferung.\n\nMit freundlichen Grüßen\n\n-", Normal));
                            }
                        }
                        #endregion

                        pdfDoc.Close();
                        stream.Close();
                    }
                    catch (Exception ex)
                    {
                        //handle exception
                    }
                    finally
                    {
                    }
                }
            }

            Form_Bestellen bs = new Form_Bestellen(folderPath + filename, dt);
            bs.Show();


        }
        public void getSelectedRow(DataGridView dgv, int ArtNr)
        {
            if (dgv == dataGridView1)
            {
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.Cells[1].Value.ToString().Equals(ArtNr.ToString()))
                    {
                        //bs.Filter = null;
                        //dgv.DataSource = bs;
                        selectedRowArtikel = row.Index;
                        break;
                    }
                }
            }
            else
            {
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.Cells[2].Value.ToString().Equals(ArtNr.ToString()))
                    {
                        selectedRowOrder = row.Index;
                        break;
                    }
                }
            }
        }
    }
    public class ITextEvents : PdfPageEventHelper
    {
        // This is the contentbyte object of the writer
        PdfContentByte cb;

        // we will put the final number of pages in a template
        PdfTemplate headerTemplate, footerTemplate;

        // this is the BaseFont we are going to use for the header / footer
        BaseFont bf = null;

        // This keeps track of the creation time
        DateTime PrintTime = DateTime.Now;

        #region Fields
        private string _header;
        #endregion

        #region Properties
        public string Header
        {
            get { return _header; }
            set { _header = value; }
        }
        #endregion

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                PrintTime = DateTime.Now;
                bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                cb = writer.DirectContent;
                headerTemplate = cb.CreateTemplate(100, 100);
                footerTemplate = cb.CreateTemplate(100, 100);


            }
            catch (DocumentException de)
            {
            }
            catch (System.IO.IOException ioe)
            {
            }
        }

        public override void OnEndPage(iTextSharp.text.pdf.PdfWriter writer, iTextSharp.text.Document document)
        {
            base.OnEndPage(writer, document);

            iTextSharp.text.Font Small = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 9f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.DARK_GRAY);
            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance("../../PDF_Images/Logo.png");
            iTextSharp.text.Image bottom = iTextSharp.text.Image.GetInstance("../../PDF_Images/Bottom.png");

            String text = "Seite " + writer.PageNumber + " von ";

            //Add paging to header
            {
                cb.BeginText();
                cb.SetTextMatrix(document.PageSize.GetRight(200), document.PageSize.GetTop(45));
                cb.EndText();
                cb.AddTemplate(headerTemplate, document.PageSize.GetRight(200), document.PageSize.GetTop(45));
                
            }
            //Add paging to footer
            {
                cb.BeginText();
                cb.SetFontAndSize(bf, 12);
                cb.SetTextMatrix(document.PageSize.GetRight(90), document.PageSize.GetBottom(85));
                cb.ShowText(text);
                cb.EndText();
                bottom.SetAbsolutePosition(0, document.PageSize.GetBottom(0));
                bottom.ScalePercent(24f);
                cb.AddImage(bottom);
                float len = bf.GetWidthPoint(text, 12);
                cb.AddTemplate(footerTemplate, document.PageSize.GetRight(90) + len, document.PageSize.GetBottom(85));
            }


            PdfPTable top = new PdfPTable(2);

            float[] widths = new float[] { 1f, 1f };
            top.SetWidths(widths);

            var headertext = new Paragraph();
            headertext.Add("-\n");
            headertext.Add("-\n");
            headertext.Add("Tel.: -\n");
            headertext.Add("E-Mail: -\n");
            headertext.Add("Web: -");
            headertext.SpacingAfter = 10f;
            headertext.Font = Small;
            headertext.SetLeading(0.2f, 1.05f);

            PdfPCell headertext_Cell = new PdfPCell();
            headertext_Cell.AddElement(headertext);
            headertext_Cell.Border = iTextSharp.text.Rectangle.NO_BORDER;

            PdfPCell logo_Cell = new PdfPCell();
            logo_Cell.AddElement(logo);
            logo.ScalePercent(30f);
            //logo.Border = iTextSharp.text.Rectangle.NO_BORDER;
            logo_Cell.Border = 0;
            logo_Cell.HorizontalAlignment = Element.ALIGN_RIGHT;
            logo_Cell.VerticalAlignment = Element.ALIGN_MIDDLE;

            headertext_Cell.HorizontalAlignment = Element.ALIGN_CENTER;
            headertext_Cell.VerticalAlignment = Element.ALIGN_MIDDLE;

            headertext_Cell.Border = 0;
            logo_Cell.Border = 0;

            top.AddCell(headertext_Cell);
            top.AddCell(logo_Cell);

            top.TotalWidth = document.PageSize.Width - 50;
            top.WidthPercentage = 70;

            //call WriteSelectedRows of PdfTable. This writes rows from PdfWriter in PdfTable
            //first param is start row. -1 indicates there is no end row and all the rows to be included to write
            //Third and fourth param is x and y position to start writing
            top.WriteSelectedRows(0, -1, 25, document.PageSize.Height - 30, writer.DirectContent);
            //set pdfContent value

            //Move the pointer and draw line to separate header section from rest of page
            cb.MoveTo(25, document.PageSize.Height - 90);
            cb.LineTo(document.PageSize.Width - 25, document.PageSize.Height - 90);
            cb.SetLineWidth(0.5f);
            cb.Stroke();

            //Move the pointer and draw line to separate footer section from rest of page
            //cb.MoveTo(40, document.PageSize.GetBottom(50));
            //cb.LineTo(document.PageSize.Width - 40, document.PageSize.GetBottom(50));
            //cb.Stroke();
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            headerTemplate.BeginText();
            headerTemplate.SetFontAndSize(bf, 12);
            headerTemplate.SetTextMatrix(0, 0);
            headerTemplate.EndText();

            footerTemplate.BeginText();
            footerTemplate.SetFontAndSize(bf, 12);
            footerTemplate.SetTextMatrix(0, 0);
            footerTemplate.ShowText((writer.PageNumber).ToString());
            footerTemplate.EndText();
        }
    }
}




