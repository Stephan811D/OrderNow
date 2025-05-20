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
    public partial class Bestellung_Löschen : Form
    {
        private readonly Form1 _form1;

        public Bestellung_Löschen(Form1 _Form1)
        {
            InitializeComponent();
            this._form1 = _Form1;
        }

        private void Bestellung_Löschen_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            _form1.deleteTable("bestellung");
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
