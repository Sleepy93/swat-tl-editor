using System;
using System.IO;
using System.Windows.Forms;

namespace SwatTL_Editor
{
    public partial class Form2 : Form
    {
        Form1 frm;
        string[] drv;

        public Form2(Form1 frm)
        {
            InitializeComponent();
            this.frm = frm;
            DriveInfo[] tmp = DriveInfo.GetDrives();
            drv = new string[tmp.Length];
            for(int i = 0; i < tmp.Length; i++)
            {
                drv[i] = tmp[i].Name;
                comboDrive.Items.Add(drv[i] + " - " + tmp[i].DriveType);
            }
            comboDrive.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path;

            if (radio1.Checked)
                path = drv[comboDrive.SelectedIndex] + @"\PSP_GAME";
            else
                path = textBox1.Text;

            if (!Directory.Exists(path))
            {
                MessageBox.Show("Path Does not exists!", "Path Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
                

            frm.LoadPath(path, checkBox1.Checked, checkBox2.Checked);
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = Dir();
        }

        string Dir()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK)
                    return fbd.SelectedPath;
            }
            return null;
        }

        private void radio1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = !radio1.Checked;
            button2.Enabled = !radio1.Checked;
            checkBox1.Checked = radio1.Checked;
            comboDrive.Enabled = radio1.Checked;
        }
    }
}
