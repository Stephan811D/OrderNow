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
    public partial class Form4 : Form
    {
        private int _selectedArticle_ID, _selectedArticle_Anz, _selectedArticle_ArtNr;
        private string _selectedArticle_Artikel, _selectedArticle_Barcode;
        private decimal _selectedArticle_Preis;

        private readonly Form1 _form1;

        public Form4(int _SelectedArticle_ID, int _SelectedArticle_Anz, int _SelectedArticle_ArtNr, string _SelectedArticle_Artikel, decimal _SelectedArticle_Preis, string _SelectedArticle_Barcode, Form1 _Form1)
        {
            InitializeComponent();

            this._selectedArticle_ID = _SelectedArticle_ID;
            this._selectedArticle_Anz = _SelectedArticle_Anz;
            this._selectedArticle_ArtNr = _SelectedArticle_ArtNr;
            this._selectedArticle_Artikel = _SelectedArticle_Artikel;
            this._selectedArticle_Preis = _SelectedArticle_Preis;
            this._selectedArticle_Barcode = _SelectedArticle_Barcode;
            this._form1 = _Form1;
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            numericUpDown1.Value = _selectedArticle_Anz;
            numericUpDown2.Value = _selectedArticle_ArtNr;
            textBox1.Text = _selectedArticle_Artikel;
            numericUpDown3.Value = _selectedArticle_Preis;
            numericUpDown4.Value = Convert.ToDecimal(_selectedArticle_Barcode);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _selectedArticle_Anz = Convert.ToInt32(numericUpDown1.Value);
            _selectedArticle_ArtNr = Convert.ToInt32(numericUpDown2.Value);
            _selectedArticle_Artikel = textBox1.Text;
            _selectedArticle_Preis = Convert.ToDecimal(numericUpDown3.Value);
            _selectedArticle_Barcode = Convert.ToString(numericUpDown4.Value);

            _form1.updateArticleInOrder(_selectedArticle_ID, _selectedArticle_Anz, _selectedArticle_ArtNr, _selectedArticle_Artikel, _selectedArticle_Preis, _selectedArticle_Barcode);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
