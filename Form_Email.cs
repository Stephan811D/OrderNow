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
    public partial class Form_Email : Form
    {
        public Form_Email()
        {
            InitializeComponent();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.Text = "<span style='font-size: 12pt; color: black;'>Sehr geehrtes -Team,</br>hiermit sende ich meine Bestellung</span>";
        }
    }
}
