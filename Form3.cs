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
    public partial class Form3 : Form
    {
        private int _selectedArticle_ID, _selectedArticle_ArtNr;
        private string _selectedArticle_Artikel, _selectedArticle_Barcode;
        private decimal _selectedArticle_EinkaufsPreis, _selectedArticle_VerkaufsPreis;

        private readonly Form1 _form1;

        public Form3(int _SelectedArticle_ID, int _SelectedArticle_ArtNr, string _SelectedArticle_Artikel, decimal _SelectedArticle_EinkaufsPreis, decimal _SelectedArticle_VerkaufsPreis, string _SelectedArticle_Barcode, Form1 _Form1)
        {
            InitializeComponent();

            this._selectedArticle_ID = _SelectedArticle_ID;
            this._selectedArticle_ArtNr = _SelectedArticle_ArtNr;
            this._selectedArticle_Artikel = _SelectedArticle_Artikel;
            this._selectedArticle_EinkaufsPreis = _SelectedArticle_EinkaufsPreis;
            this._selectedArticle_VerkaufsPreis = _SelectedArticle_VerkaufsPreis;
            this._selectedArticle_Barcode = _SelectedArticle_Barcode;
            this._form1 = _Form1;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            numericUpDown1.Value = _selectedArticle_ArtNr;
            textBox1.Text = _selectedArticle_Artikel;
            numericUpDown2.Value = _selectedArticle_EinkaufsPreis;
            numericUpDown3.Value = _selectedArticle_VerkaufsPreis;
            numericUpDown4.Value = Convert.ToDecimal(_selectedArticle_Barcode);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _selectedArticle_ArtNr = Convert.ToInt32(numericUpDown1.Value);
            _selectedArticle_Artikel = textBox1.Text;
            _selectedArticle_EinkaufsPreis = Convert.ToDecimal(numericUpDown2.Value);
            _selectedArticle_VerkaufsPreis = Convert.ToDecimal(numericUpDown3.Value);
            _selectedArticle_Barcode = Convert.ToString(numericUpDown4.Value);

            _form1.updateArticleInPool(_selectedArticle_ID, _selectedArticle_ArtNr, _selectedArticle_Artikel, _selectedArticle_EinkaufsPreis, _selectedArticle_VerkaufsPreis, _selectedArticle_Barcode);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
