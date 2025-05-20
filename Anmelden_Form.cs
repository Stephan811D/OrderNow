using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Order_Now
{
    public partial class Anmelden_Form : Form
    {
        bool online = false;

        BindingSource bs;

        Form1 _form1;

        public Anmelden_Form(Form1 _Form1)
        {
            InitializeComponent();

            this._form1 = _Form1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Anmelden_Form_Load(object sender, EventArgs e)
        {
            textBox1.Text = _form1.Server;
            numericUpDown1.Value = Convert.ToDecimal(_form1.Port);
            textBox3.Text = _form1.Benutzer;
            textBox4.Text = _form1.ToInsecureString(_form1.DecryptString(_form1.Passwort));
            comboBox1.Text = _form1.Datenbank;

            bs = new BindingSource();


            if (_form1.Server != "" && _form1.Port != 0 && _form1.Benutzer != "" && _form1.Passwort != "")
            {
                textBox1.Enabled = false;
                textBox3.Enabled = false;
                textBox4.Enabled = false;
                numericUpDown1.Enabled = false;
                button1.Text = "Abmelden";
                comboBox1.Enabled = true;

                bs.DataSource = _form1.getDatabases();

                if (bs != null)
                {
                    comboBox1.DataSource = bs;
                    comboBox1.SelectedItem = _form1.Datenbank;
                }

                online = true;
            }
            else
            {
                textBox1.Enabled = true;
                textBox3.Enabled = true;
                textBox4.Enabled = true;
                numericUpDown1.Enabled = true;
                comboBox1.Enabled = false;

                online = false;
            }
            _form1.CloseConnection();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (online == true) //Klick = Abmeldung
            {
                textBox1.Enabled = true;
                textBox3.Enabled = true;
                textBox4.Enabled = true;
                numericUpDown1.Enabled = true;
                comboBox1.Enabled = false;
                button1.Text = "Anmelden";

                if(comboBox1.Items.Count > 0)
                {
                    comboBox1.DataSource = null;
                    comboBox1.Items.Clear();
                }

                online = false;
            }
            else //Klick = Anmeldung
            {
                textBox1.Enabled = false;
                textBox3.Enabled = false;
                textBox4.Enabled = false;
                numericUpDown1.Enabled = false;
                comboBox1.Enabled = true;

                button1.Text = "Abmelden";

                _form1.Server = textBox1.Text.ToString();
                _form1.Port = Convert.ToInt32(numericUpDown1.Value);
                _form1.Benutzer = textBox3.Text.ToString();
                _form1.Passwort = _form1.EncryptString(_form1.ToSecureString(textBox4.Text.ToString()));

                _form1.Initialize(_form1.Server, _form1.Port, _form1.Benutzer, _form1.Passwort);

                bs.DataSource = _form1.getDatabases();

                if (bs != null)
                {
                    comboBox1.DataSource = bs;
                }

                online = true;
            }
        }   

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && numericUpDown1.Value != 0 && textBox3.Text != "" && textBox4.Text != "" && comboBox1.Text != "")
            {
                Properties.Settings.Default["Server"] = textBox1.Text.ToString();
                Properties.Settings.Default["Port"] = Convert.ToInt32(numericUpDown1.Value);
                Properties.Settings.Default["Benutzer"] = textBox3.Text.ToString();
                Properties.Settings.Default["Passwort"] = _form1.EncryptString(_form1.ToSecureString(textBox4.Text.ToString()));
                Properties.Settings.Default["Datenbank"] = comboBox1.Text.ToString();

                _form1.Datenbank = comboBox1.Text.ToString();

                _form1.Initialize(_form1.Server, _form1.Port, _form1.Benutzer, _form1.Passwort);
                _form1.OpenConnection();
                _form1.LoadGridViews();
                _form1.setdataGridViewStyle();

                if (_form1.succesfulLoad == true)
                {
                    Properties.Settings.Default.Save();
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Es konnte nicht gespeichert werden, da nicht alle Felder ausgefüllt wurden.", "Order Now - Fehler beim Speichern", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
