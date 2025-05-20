using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BarcodeLib;
using System.IO;
using System.Net;
using MySql.Data.MySqlClient;

namespace Order_Now
{
    public partial class Form2 : Form
    {
        public int _newArticle_ArtNr;
        public string _newArticle_Artikel, _newArticle_Barcode;
        public decimal _newArticle_EinkaufsPreis, _newArticle_VerkaufsPreis;

        private readonly Form1 _form1;

        public Form2(Form1 _Form1)
        {
            InitializeComponent();

            this._form1 = _Form1;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.ActiveControl = numericUpDown1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _newArticle_ArtNr = Convert.ToInt32(numericUpDown1.Value);
            _newArticle_Artikel = textBox1.Text;
            _newArticle_EinkaufsPreis = numericUpDown2.Value;
            _newArticle_VerkaufsPreis = numericUpDown3.Value;
            _newArticle_Barcode = textBox2.Text;

            _form1.addArticleToPool(_newArticle_ArtNr, _newArticle_Artikel, _newArticle_EinkaufsPreis, _newArticle_VerkaufsPreis, _newArticle_Barcode);

            this.Close();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown3.Value = numericUpDown2.Value;
        }

        private void generateBarcodeEAN13(string barcode)
        {
            try
            {
                BarcodeLib.Barcode b = new BarcodeLib.Barcode();
                pictureBox1.Image = b.Encode(BarcodeLib.TYPE.EAN13, barcode, Color.Black, Color.White, 326, 91);
            }
            catch (Exception e)
            {
                MessageBox.Show("Der eingegebene Barcode ist nicht gültig!");
                e = null;
            }
        }

        //private void generateBarcodeEAN8(string barcode)
        //{
        //    try
        //    {
        //        BarcodeLib.Barcode b = new BarcodeLib.Barcode();
        //        pictureBox1.Image = b.Encode(BarcodeLib.TYPE.EAN8, barcode, Color.Black, Color.White, 326, 91);
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show("Der eingegebene Barcode ist nicht gültig!");
        //    }
        //}

        private string GetHtmlCode()
        {
            try
            {
                string url = "https://www.google.com/search?q=" + textBox2.Text.ToString() + "&tbm=isch";
                string data = "";

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Accept = "text/html, application/xhtml+xml, */*";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";

                var response = (HttpWebResponse)request.GetResponse();

                using (Stream dataStream = response.GetResponseStream())
                {
                    if (dataStream == null)
                    {
                        pictureBox2.Image = pictureBox2.ErrorImage;
                        return "";
                    }

                    using (var sr = new StreamReader(dataStream))
                    {
                        data = sr.ReadToEnd();
                    }
                }
                return data;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                pictureBox2.Image = pictureBox2.ErrorImage;
                return null; 
            }
            
        }

        private List<string> GetUrl(string html)
        {
            var urls = new List<string>();

            int ndx = html.IndexOf("\"ou\"", StringComparison.Ordinal);

            while (ndx >= 0)
            {
                ndx = html.IndexOf("\"", ndx + 4, StringComparison.Ordinal);
                ndx++;
                int ndx2 = html.IndexOf("\"", ndx, StringComparison.Ordinal);
                string url = html.Substring(ndx, ndx2 - ndx);
                urls.Add(url);
                ndx = html.IndexOf("\"ou\"", ndx2, StringComparison.Ordinal);
            }
            return urls;
        }

        private byte[] GetImage(string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                var response = (HttpWebResponse)request.GetResponse();

                using (Stream dataStream = response.GetResponseStream())
                {
                    if (dataStream == null)
                        return null;
                    using (var sr = new BinaryReader(dataStream))
                    {
                        byte[] bytes = sr.ReadBytes(100000000);
                        return bytes;
                    }
                }

                return null;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
                return null;
            }

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            getImage();
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
            getImage();
        }

        private void getImage()
        {
            try
            {
                if (textBox2.Text.ToString().Length == 12)
                {

                    pictureBox2.Image = Image.FromFile("../../Icons/loader.gif");

                    generateBarcodeEAN13(textBox2.Text.ToString());

                    string html = GetHtmlCode();
                    List<string> urls = GetUrl(html);
                    var rnd = new Random();

                    if (urls.Count == 0)
                    {
                        pictureBox2.Image = pictureBox2.ErrorImage;
                    }
                    else
                    {
                        byte[] image = GetImage(urls[0].ToString());
                        using (var ms = new MemoryStream(image))
                        {
                            pictureBox2.Image = Image.FromStream(ms);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
