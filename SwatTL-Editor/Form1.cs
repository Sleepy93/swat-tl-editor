using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SwatTL_Editor
{
    public partial class Form1 : Form
    {
        string path;
        string[] files;
        Dictionary<string, Extensions> types;

        public Form1()
        {
            InitializeComponent();
            types = new Dictionary<string, Extensions>()
            {
                {"ANM",new Extensions("Animation file",ANM)},
                {"AWF",new Extensions("Audio Wave Format",AWF)},
                {"BIK",new Extensions("Bink Video",BIK)},
                {"BIN",new Extensions("Binary file",BIN)},
                {"CUT",new Extensions("Cutscene",CUT)},
                {"DIX",new Extensions("Dialogue file",DIX)},
                {"DOB",new Extensions("Model file",DOB)},
                {"FXO",new Extensions("Shader file",FXO)},
                {"LMP",new Extensions("Game Archive",LMP)},
                {"LUA",new Extensions("LUA Script",LUA)},
                {"LVH",new Extensions("Map data file",LVH)},
                {"LVS",new Extensions("Map scripting file",LVS)},
                {"MP3",new Extensions("MPEG-3 Audio",MP3)},
                {"PMF",new Extensions("PSP Movie file",PMF)},
                {"PNG",new Extensions("PNG Image",PNG)},
                {"PRX",new Extensions("PSP Module file",PRX)},
                {"SFO",new Extensions("PSP Game Parameter file",SFO)},
                {"SWV",new Extensions("Shockwave Flash",SWV)},
                {"TEX",new Extensions("Texture",TEX)},
                {"TGA",new Extensions("Targa Image",TGA)},
                {"ZVH",new Extensions("Zone data file",ZVH)},
                {"ZVS",new Extensions("Zone scripting file",ZVS)},
            };
            //Open Context menu
            ContextMenu cm = new ContextMenu();
            cm.MenuItems.Add("Export").Click += picturebox_RClcik;
            pictureBox1.ContextMenu = cm;
        }

        private void picturebox_RClcik(object sender, EventArgs e)
        {
            string tmp = treeView1.SelectedNode.Text;
            sfd.FileName = tmp.Substring(tmp.LastIndexOf('\\') + 1);
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image.Save(sfd.FileName);
            }
        }

        private void openPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Form2(this).ShowDialog();
        }

        public void LoadPath(string input)
        {
            path = input + "\\PSP_GAME\\";
            if (Directory.Exists(path))
            {
                files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    files[i] = files[i].Substring(path.Length);
                }
                PopulateTreeView(treeView1, files, '\\');
            }
        }

        private static void PopulateTreeView(TreeView treeView, string[] paths, char pathSeparator)
        {
            TreeNode lastNode = null;
            string subPathAgg;
            foreach (string path in paths)
            {
                subPathAgg = string.Empty;
                foreach (string subPath in path.Split(pathSeparator))
                {
                    subPathAgg += subPath + pathSeparator;
                    TreeNode[] nodes = treeView.Nodes.Find(subPathAgg, true);
                    if (nodes.Length == 0)
                        if (lastNode == null)
                            lastNode = treeView.Nodes.Add(subPathAgg, subPath);
                        else
                            lastNode = lastNode.Nodes.Add(subPathAgg, subPath);
                    else
                        lastNode = nodes[0];
                }
                lastNode = null; // This is the place code was changed
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                if (wmp != null) wmp.controls.stop();

                TreeNode node = e.Node;
                string tmp = node.Text;
                node = node.Parent;
                while (node != null)
                {
                    tmp = node.Text + "\\" + tmp;
                    node = node.Parent;
                }
                string key = tmp.Substring(tmp.LastIndexOf('.') + 1);
                key = key.ToUpper();
                tmp = path + tmp;
                if (types.ContainsKey(key))
                    types[key].handler.Invoke(new FileStream(tmp, FileMode.Open, FileAccess.Read));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + " - " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowRightSide(Control ctrl)
        {
            richTextBox1.Text = null;
            pictureBox1.Image = null;
            listView2.Items.Clear();
            foreach (Control c in splitContainer1.Panel2.Controls)
            {
                c.Visible = c == ctrl;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            if(wmp != null) wmp.controls.stop();

            Stream str = new MemoryStream(_files[listBox1.SelectedIndex].Data);
            string tmp = (string)listBox1.SelectedItem;
            string key = tmp.Substring(tmp.LastIndexOf('.') + 1);
            key = key.ToUpper();
            if (types.ContainsKey(key))
                types[key].handler.Invoke(str);
        }
    }
}