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
    public partial class Version_Info : Form
    {
        public Version_Info()
        {
            InitializeComponent();
        }

        private void Version_Info_Load(object sender, EventArgs e)
        {
            label4.Text = "Icons made by Lucy G from www.flaticon.com is licensed by CC 3.0 BY";
        }
    }
}
