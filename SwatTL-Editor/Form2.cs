using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                listBox1.Items.Add(drv[i] + " - " + tmp[i].DriveType);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;

            frm.LoadPath(drv[listBox1.SelectedIndex]);
            Close();
        }
    }
}
