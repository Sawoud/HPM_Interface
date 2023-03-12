using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using System.IO.Ports;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Threading;
using System.Security.Cryptography;

namespace HPM_Interface
{
    public partial class Form1 : Form
    {
        
        SerialPort mySerialPort = new SerialPort("COM3");


        /*Global Values*/
        DataTable dt1 = new DataTable();
        string path = Path.GetPathRoot(Environment.SystemDirectory)+"Database.txt";
        //string localfilepath = ;

        #region UART
        /// <summary>
        /// Intilization of the UART connection
        /// </summary>
        public void UART_INIT()
        {
            /*Serial intilization*/
            mySerialPort.BaudRate = 115200;
            mySerialPort.Parity = Parity.None;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.DataBits = 8;
            mySerialPort.Handshake = Handshake.None;

            mySerialPort.WriteTimeout = 500;

        }
        /// <summary>
        /// THe below file takes each byte of data from the device, converts it to a string, then write it to a file
        /// </summary>
        public void UART_File_Read() {
            DataRow newRow = dt1.NewRow();
            newRow[0] = Txt_Domain.Text.ToString();
            newRow[1] = Text_UID.Text.ToString();
            newRow[2] = Text_Password.Text.ToString();
            dt1.Rows.Add(newRow);
            string textt = "";
            lb_loading.Show();
            Boolean first_line = true;
            int count = 0; 
            mySerialPort.Write("+");
            for (int i = 0; i < 5; i++) {
                if (Convert.ToChar(mySerialPort.ReadChar()).ToString() == "+") { break; }
                if(i == 4) { return; }
            }

            int milliseconds = 2;
            //Thread.Sleep(milliseconds);
            string FileString = "";
            while (true)
            {
                //mySerialPort.Write("send");
                //Thread.Sleep(milliseconds);
                try
                {
                    textt = Convert.ToChar(mySerialPort.ReadChar()).ToString(); // Wait for data reception
                    if (textt == "*") { break; }
                }
                catch (TimeoutException Ex)//Catch Time out Exception
                {
                    mySerialPort.Close();
                    Environment.Exit(0);
                }
                FileString = FileString + textt;

                lb_loading.Text = "loading to " + count;
            }
            File.WriteAllText(path, FileString);
            lb_loading.Hide();

        }

        /// <summary>
        /// This line sends every byte of data to the device
        /// </summary>
        public void UART_Send_Lines()
        {
            //mySerialPort.Open();
            int milliseconds = 1;
            mySerialPort.Write("-");
            for (int i = 0; i < 5; i++)
            {
                if (Convert.ToChar(mySerialPort.ReadChar()).ToString() == "-") { break; }
                if (i == 4) { return; }
            }
            byte[] FileBytes = System.IO.File.ReadAllBytes(path);
                for (int i = 0; i < FileBytes.Length; i++)
                {
                //Thread.Sleep(milliseconds);
                mySerialPort.Write(Convert.ToChar(FileBytes[i]).ToString());
            }
            //Thread.Sleep(milliseconds);
            mySerialPort.Write("*");
            dt1.Rows.Clear();
            read_file();
            dataGridView1.Refresh();
            this.dataGridView1.Sort(this.dataGridView1.Columns["DOMAIN"], ListSortDirection.Ascending);
        }

        public Boolean pin_check() {
            mySerialPort.Write("P");
            for (int i = 0; i < 5; i++)
            {
                if (Convert.ToChar(mySerialPort.ReadChar()).ToString() == "P") { break; }
                if (i == 4) { return false; }
            }
            if (Convert.ToChar(mySerialPort.ReadChar()).ToString() == "1") { return true; }
            else { return false; }
            
        }
        #endregion
        public Form1()
        {
            InitializeComponent();
            try { mySerialPort.Open(); }catch { }
            this.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            UART_INIT();

            read_file();
            lb_loading.Hide();
            dataGridView1.DataSource = dt1;
            this.dataGridView1.Sort(this.dataGridView1.Columns["DOMAIN"], ListSortDirection.Ascending);

            try { UART_File_Read(); }
            catch { }
            foreach (string line in File.ReadAllLines(path))
            {
                string[] pin = line.Split(';');
                txt_oldPin.Text = pin[0].ToString();
            }
            this.dataGridView1.Columns["PASSWORD"].Visible = false;
            this.dataGridView1.Columns[0].Width = 120;
            this.dataGridView1.Columns[1].Width = 180;
            dataGridView1.Rows.RemoveAt(0);

        }
        /// <summary>
        /// This reads the CSV dile stored on the desktop
        /// </summary>
        public void read_file() {
            Boolean first_line = true;
            /*
            myserialPort.DataReceived();
            mySerialPort.Close();
            */

            var file_contents = File.ReadAllLines(path);
            //file_contents = mySerialPort.DataReceived
            foreach (string line in file_contents)
            {
                if (first_line)
                {
                    DataRow dr1 = dt1.NewRow();
                    string[] data1 = line.Split(';');

                    for (int i = 0; i < data1.Length; i++) { try { dt1.Columns.Add(data1[i], typeof(string)); } catch { } }
                    first_line = false;
                }
                else
                {
                    DataRow dr = dt1.NewRow();

                    string[] data = line.Split(';');

                    for (int i = 0; i < data.Length; i++)
                    {
                        try { dr[i] = data[i]; }
                        catch { }
                    }
                    if ((dr[0] == ";") || (dr[0] == "") || (dr[0] == "*")) { }
                    else { dt1.Rows.Add(dr); }
                }
            }
        }

        private void Btn_Add_Click(object sender, EventArgs e)
        {
            //this.dataGridView1.Rows.Add(Txt_Domain.ToString(), Text_UID.ToString(), Text_Password.ToString());
            //this.dataGridView1.Rows.Insert(0,Txt_Domain.ToString(), Text_UID.ToString(), Text_Password.ToString());
            DataRow newRow = dt1.NewRow();
            newRow[0] = Txt_Domain.Text.ToString();
            newRow[1] = Text_UID.Text.ToString();
            newRow[2] = Text_Password.Text.ToString();
            dt1.Rows.Add(newRow);
            SaveTodB();


        }
        public byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }
        public byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            string input = "a7ffc6f8bf1ed76651c14756a061d662f580ff4de43b49fa82d80a4b80f8434a";
            byte[] decryptedBytes = null;
            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }
        public string EncryptText (string password)
        {
            // Get the bytes of the string
            string input = "a7ffc6f8bf1ed76651c14756a061d662f580ff4de43b49fa82d80a4b80f8434a";
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            string result = Convert.ToBase64String(bytesEncrypted);

            return result;
        }

        public string DecryptText(string password)
        {
            // Get the bytes of the string
            string input = "a7ffc6f8bf1ed76651c14756a061d662f580ff4de43b49fa82d80a4b80f8434a";
            byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            string result = Encoding.UTF8.GetString(bytesDecrypted);

            return result;
        }
        
        public void SaveTodB() {
            this.dataGridView1.Sort(this.dataGridView1.Columns["DOMAIN"], ListSortDirection.Ascending);
            using (TextWriter tw = new StreamWriter(path))
            {
                tw.Write("DOMAIN;UID;PASSWORD\r\n");
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView1.Columns.Count; j++)
                    {
                        tw.Write($"{dataGridView1.Rows[i].Cells[j].Value.ToString()}");
                        tw.Write(";");
                    }
                    tw.WriteLine();
                }
            }
            dt1.Rows.Clear();
            /*
             byte[] writeBuffer = File.ReadAllBytes("filename.txt");
             port.Write(writeBuffer, 0, writeBuffer.Length);
             */
            try { UART_Send_Lines(); }
            catch { }
            read_file();
            dataGridView1.Refresh();
            this.dataGridView1.Sort(this.dataGridView1.Columns["DOMAIN"], ListSortDirection.Ascending);
        }

        private void Btn_Edit_Click(object sender, EventArgs e)
        {
            string message = "Are you sure you want to edit the selected row? \nThis action can not be undone";
            string title = "Caution";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result == DialogResult.Yes)
            {
                this.dataGridView1.SelectedRows[0].Cells[0].Value = Txt_Domain.Text.ToString();
                this.dataGridView1.SelectedRows[0].Cells[1].Value = Text_UID.Text.ToString();
                this.dataGridView1.SelectedRows[0].Cells[2].Value = Text_Password.Text.ToString();
                MessageBox.Show("Operation Succesful !", "Success");
                SaveTodB();
            }
            else { MessageBox.Show("Operation Aborted", "Canceled"); }


        }

        private void Btn_Hide_Click(object sender, EventArgs e)
        {

        }

        private void Btn_Del_Click(object sender, EventArgs e)
        {
            string message = "Are you sure you want to delete the selected row? \nThis action can not be undone !";
            string title = "Warning";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, title, buttons);
            if(result == DialogResult.Yes)
            {
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    dataGridView1.Rows.RemoveAt(row.Index);
                }
                MessageBox.Show("Operation Succesful !", "Success");
                SaveTodB();
            }
            else { MessageBox.Show("Operation Aborted", "Canceled"); }



        }



        private void Btn_GenPass_Click(object sender, EventArgs e)
        {
            string charchters = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
            string special_charchters = Txt_Chars.Text.ToString();
            Random random = new Random((int)DateTime.Now.Ticks);
            string randompass = "";
            Boolean match = false;
            int[] special_char_locations = new int[1];
            Random rnd = new Random();
            try {
           Array.Resize(ref special_char_locations, rnd.Next(Int32.Parse(Txt_SopCharNum.Text[0].ToString()), Int32.Parse(Txt_SopCharNum.Text[2].ToString())) );
            for (int j = 0; j < special_char_locations.Length; j++) {
                    int loc = rnd.Next(0, Int32.Parse(Txt_PassLen.Text.ToString()));
                    special_char_locations[j] = loc;
                }
            }
            
            catch { special_char_locations[0] = -1;}
            
            for (int i = 0; i < Int32.Parse(Txt_PassLen.Text.ToString()); i++) {
                for (int j = 0; j < special_char_locations.Length; j++) {
                    if (special_char_locations[j] == i) { match = true; break; }
                    else { };
                }
                if (match) { 
                    randompass += special_charchters[random.Next(0, special_charchters.Length - 1)]; }
                else { randompass += charchters[random.Next(0, charchters.Length - 1)]; }
                match = false;
            }
            Txt_GenPass.Text = randompass;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {

           DataGridViewRow row = this.dataGridView1.SelectedRows[0];
            //string PIN = Microsoft.VisualBasic.Interaction.InputBox("Please Input your PIN","PIN","****");
            MessageBox.Show("Please Look at the HPMKey");
            try
            {
                if (pin_check())
                {
                    Txt_Domain.Text = row.Cells["DOMAIN"].Value.ToString();
                    Text_UID.Text = row.Cells["UID"].Value.ToString();
                    Text_Password.Text = row.Cells["PASSWORD"].Value.ToString();
                }
                else
                {
                    MessageBox.Show("PIN Entry Failed/Terminated");
                }
            }
            catch { }
            /*
            if (PIN == "7531") {
                Txt_Domain.Text = row.Cells["DOMAIN"].Value.ToString();
                Text_UID.Text = row.Cells["UID"].Value.ToString();
                Text_Password.Text = row.Cells["PASSWORD"].Value.ToString();
            }
            else {
                MessageBox.Show("PIN Entry Failed/Terminated");
            }

             */

        }

        private void txt_NewPin_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_NewPin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
            (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
        }

        private void Btn__clipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(Txt_GenPass.Text);
        }

        private void Btn_SaveToDevice_Click(object sender, EventArgs e)
        {
            UART_Send_Lines();
        }
    }
}
