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
    public partial class Form_Löschen : Form
    {
        private int _selectedArticle_ID;
        private string _selectedArticle_Artikel, _selectedTable;
        private readonly Form1 _form1;

        public Form_Löschen(string _SelectedTable, string _SelectedArticle_Artikel, int _SelectedArticle_ID, Form1 _Form1)
        {
            InitializeComponent();
            this._selectedTable = _SelectedTable;
            this._selectedArticle_Artikel = _SelectedArticle_Artikel;
            this._selectedArticle_ID = _SelectedArticle_ID;
            this._form1 = _Form1;
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            if(_selectedArticle_Artikel.Length > 30)
            {
                this.AutoSize = true;
                this.Width = this.Width + 125;
                groupBox1.AutoSize = true;
                groupBox1.Width = groupBox1.Width + 25;
                button1.Location = new Point(groupBox1.Width - 120, 106);
                button2.Location = new Point(groupBox1.Width, 106);
            }
            label1.Text = "Soll der Artikel [" + _selectedArticle_Artikel + "] wirklich gelöscht werden?";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _form1.deleteArticle(_selectedTable, _selectedArticle_ID);
            this.Close();
        }
    }
}
