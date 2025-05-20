using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;

namespace Order_Now
{
    public partial class Form_Bestellen : Form
    {
        string filePath;
        DateTime dt;
        CultureInfo ci; 


        public Form_Bestellen(string FilePath, DateTime Dt)
        {
            InitializeComponent();
            this.filePath = FilePath;
            this.dt = Dt;
        }

        private void Bestellen_Load(object sender, EventArgs e)
        {
            axAcroPDF1.src = filePath;
            axAcroPDF1.setZoom(60);
            ci = new CultureInfo("de-DE");
            groupBox1.Text = "Bestellung vom " + dt.ToString("dd", ci) + ". " + dt.ToString("MMMM", ci) + " " + dt.Year + " - " + dt.ToString("HH", ci) + ":" + dt.ToString("mm", ci);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("schauflers@me.com");
            mailMessage.Subject = "-: Bestellung vom " + "[DATE]";
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = "<span style='font-size: 12pt; color: black;'>Sehr geehrtes -Team,</br>hiermit sende ich meine Bestellung</span>";

            //mailMessage.Attachments.Add(new Attachment("C://Myfile.pdf"));
            mailMessage.Attachments.Add(new Attachment(filePath));

            var filename = "C://mymessage.eml";

            //save the MailMessage to the filesystem
            mailMessage.Save(filename);
            

            //Open the file with the default associated application registered on the local machine
            Process.Start(filename);

            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }

    public static class MailUtility
    {
        //Extension method for MailMessage to save to a file on disk
        public static void Save(this MailMessage message, string filename, bool addUnsentHeader = true)
        {
            using (var filestream = File.Open(filename, FileMode.Create))
            {
                if (addUnsentHeader)
                {
                    var binaryWriter = new BinaryWriter(filestream);
                    //Write the Unsent header to the file so the mail client knows this mail must be presented in "New message" mode
                    binaryWriter.Write(System.Text.Encoding.UTF8.GetBytes("X-Unsent: 1" + Environment.NewLine));
                }

                var assembly = typeof(SmtpClient).Assembly;
                var mailWriterType = assembly.GetType("System.Net.Mail.MailWriter");

                // Get reflection info for MailWriter contructor
                var mailWriterContructor = mailWriterType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(Stream) }, null);

                // Construct MailWriter object with our FileStream
                var mailWriter = mailWriterContructor.Invoke(new object[] { filestream });

                // Get reflection info for Send() method on MailMessage
                var sendMethod = typeof(MailMessage).GetMethod("Send", BindingFlags.Instance | BindingFlags.NonPublic);

                sendMethod.Invoke(message, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { mailWriter, true, true }, null);

                // Finally get reflection info for Close() method on our MailWriter
                var closeMethod = mailWriter.GetType().GetMethod("Close", BindingFlags.Instance | BindingFlags.NonPublic);

                // Call close method
                closeMethod.Invoke(mailWriter, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { }, null);
            }
        }
    }
}