using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Threading;

namespace PasswordManager
{
    public partial class Form1 : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private static JObject data;
        private static System.Timers.Timer timer;
        public Form1()
        {
           // key = null;
            InitializeComponent();
            // Create a simple tray menu with only one item.
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Password Manager";
            trayIcon.Icon = new Icon("key.ico");

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
            trayIcon.Click += new EventHandler(trayIcon_Click);
            data = JObject.Parse(File.ReadAllText(@"data.json"));
        }
     
        private void displayData()
        {
            DataTable myDataTable = new DataTable();
            dataGridView1.DataSource = myDataTable;
            //adding Columns
            myDataTable.Columns.Add("Title", typeof(string));
            myDataTable.Columns.Add("Username", typeof(string));
            JArray entries = (JArray)data["entries"];

            for (int i = 0; i < entries.Count; i++)
            {
                myDataTable.Rows.Add(entries[i]["red"].ToString(), entries[i]["yellow"].ToString());
            }

        }
      
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = true;
            Visible = false;
            ShowInTaskbar = false;
        }

        private void trayIcon_Click(object sender, System.EventArgs e)
        {
            Visible = true;
            ShowInTaskbar = true;
            displayData();
            searchBox.Focus();
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window. 
            ShowInTaskbar = false; // Remove from taskbar.
            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            string input = "";
            ShowNewInputDialog(ref input);
            displayData();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            string input = "";
            ShowDeleteInputDialog(ref input);
            displayData();
        }

        private void search(object sender, EventArgs e)
        {
            if(searchBox.Text.Equals("")) { displayData(); return;  }
            DataTable myDataTable = new DataTable();
            dataGridView1.DataSource = myDataTable;
            //adding Columns
            myDataTable.Columns.Add("Title", typeof(string));
            myDataTable.Columns.Add("Username", typeof(string));
            JArray entries = (JArray)data["entries"];
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i]["red"].ToString().Contains(searchBox.Text))
                {
                    myDataTable.Rows.Add(entries[i]["red"].ToString(), entries[i]["yellow"].ToString());
                }
            }  
        }
        private static byte[] getByte(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }
        private static string getString(byte[] b)
        {
            return Encoding.UTF8.GetString(b);
        }

        private static DialogResult ShowNewInputDialog(ref string input)
        {
            Size size = new Size(400, 80);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = "Enter details";

            Label label = new Label();
            label.Text = "Title:";
            label.Location = new Point(5, 5);
            TextBox textBox = new TextBox();
            textBox.Size = new Size(size.Width/2, 23);
            textBox.Location = new Point(80, 5);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);
            inputBox.Controls.Add(label);

            Label labelb = new Label();
            labelb.Text = "Username:";
            labelb.Location = new Point(5, 30);
            TextBox textBoxb = new TextBox();
            textBoxb.Size = new Size(size.Width/2, 23);
            textBoxb.Location = new Point(80, 30);
            textBoxb.Text = input;
            inputBox.Controls.Add(textBoxb);
            inputBox.Controls.Add(labelb);

            Label labelc = new Label();
            labelc.Text = "Password:";
            labelc.Location = new Point(5, 55);
            TextBox textBoxc = new TextBox();
            textBoxc.Size = new Size(size.Width/2, 23);
            textBoxc.Location = new Point(80, 55);
            textBoxc.Text = input;
            inputBox.Controls.Add(textBoxc);
            inputBox.Controls.Add(labelc);

            Button okButton = new Button();
            okButton.DialogResult = DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new Point(300, 5);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new Point(300, 30);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            if (result.ToString().Equals("Cancel")) { inputBox.Close(); return result; }
            string title = textBox.Text;
            string user = textBoxb.Text;
            string password = textBoxc.Text;
            if (title.Equals("") || user.Equals("") || password.Equals(""))
            {
                MessageBox.Show("One or more entries blank, aborting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return result;
            }
            byte[] plaintext = getByte(password);
            byte[] entropy = new byte[20];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(entropy);
            }

            byte[] ciphertext = ProtectedData.Protect(plaintext, entropy,
                DataProtectionScope.CurrentUser);
            JArray entries = (JArray)data["entries"];
            JObject x = new JObject(
                    new JProperty("red", title),
                    new JProperty("yellow", user),
                    new JProperty("green", Base64Encode(ciphertext)),
                    new JProperty("blue", Base64Encode(entropy)));
            entries.Add(x);

            File.WriteAllText(@"data.json", data.ToString());
            return result;
        }

        private static DialogResult ShowDeleteInputDialog(ref string input)
        {
            Size size = new Size(400, 35);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = "Enter details";

            Label label = new Label();
            label.Text = "Title:";
            label.Location = new Point(5, 5);
            TextBox textBox = new TextBox();
            textBox.Size = new Size(size.Width / 2 - 10, 23);
            textBox.Location = new Point(45, 5);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);
            inputBox.Controls.Add(label);

            Button okButton = new Button();
            okButton.DialogResult = DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new Point(240, 5);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new Point(320, 5);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            // delete here 
            string title = textBox.Text;
            if(result.ToString().Equals("Cancel")) { inputBox.Close(); return result; }
            if(title.Equals(""))
            {
                MessageBox.Show("Cannot be blank, aborting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return result;
            }

            JArray entries = (JArray)data["entries"];
            for (int i = 0; i < entries.Count; i++) {
                if (entries[i]["red"].ToString() == title)
                {
                   entries.Remove(entries[i]);
                }
            }
            File.WriteAllText(@"data.json", data.ToString());
            return result;
        }
        private static DialogResult ShowInputDialogKey(ref string input)
        {
            Size size = new Size(200, 70);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;

            TextBox textBox = new TextBox();
            textBox.Size = new Size(size.Width - 10, 23);
            textBox.Location = new Point(5, 5);
            textBox.Text = input;
            textBox.PasswordChar = '*';
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new Point(size.Width - 80, 39);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;
            inputBox.Text = "Enter key";

            DialogResult result = inputBox.ShowDialog();
            if (result.ToString().Equals("Cancel")) { System.Environment.Exit(1); }
            input = textBox.Text;
            if (input == "")
            {
                MessageBox.Show("Key cannot be blank.", "Error");
                ShowInputDialogKey(ref input);

            }
            // make key
            System.Diagnostics.Debug.WriteLine("Key enter");
            return result;
        }
        static public void deleteClipboardText()
        {
            try
            {
                Thread th = new Thread(new ThreadStart(clearClipboardText));
                th.SetApartmentState(ApartmentState.STA);
                th.Start();
                th.Join();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        static public void clearClipboardText()
        {
            Clipboard.Clear();
        }
        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            deleteClipboardText();
        }
        public static byte[] Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return base64EncodedBytes;
        }
        public static string Base64Encode(byte[] plainText)
        {
            return System.Convert.ToBase64String(plainText);
        }

        private void getData(string c, string e)
        {
            byte[] plaintext = ProtectedData.Unprotect(Base64Decode(c), Base64Decode(e), DataProtectionScope.CurrentUser);
            System.Diagnostics.Debug.WriteLine(getString(plaintext));
            Clipboard.SetText(getString(plaintext));
        }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string title =  dataGridView1.CurrentRow.Cells["Title"].Value.ToString();
            JArray entries = (JArray)data["entries"];
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i]["red"].ToString() == title)
                {
                    Visible = false;
                    timer = new System.Timers.Timer();
                    timer.Interval = 5000;
                    timer.Elapsed += OnTimedEvent;
                    timer.AutoReset = false;
                    timer.Enabled = true;
                    getData(entries[i]["green"].ToString(), entries[i]["blue"].ToString());
                }
            }
        }
    }
}
