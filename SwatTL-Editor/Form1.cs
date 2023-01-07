using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SwatTL_Editor
{
	public partial class Form1 : Form
	{
		string path;
		string[] files;
		Dictionary<string, Extensions> types;
		string tmp_filename;
		int archive_idx;

		public Form1()
		{
			InitializeComponent();
			types = new Dictionary<string, Extensions>()
			{
				{"ANM",new Extensions("Animation file",ANM,Export_Generic,"ANM")},
				{"AWF",new Extensions("Audio Wave Format",AWF,Export_Generic,"WAV")},
				{"BIK",new Extensions("Bink Video",BIK,Export_Generic,"MP4")},
				{"BIN",new Extensions("Binary file",BIN,Export_Generic,"BIN")},
				{"CUT",new Extensions("Cutscene",CUT,Export_Generic,"CUT")},
				{"DIX",new Extensions("Dialogue file",DIX,Export_Generic,"XML")},
				{"DOB",new Extensions("Model file",DOB,Export_OBJ,"OBJ")},
				{"FXO",new Extensions("Shader file",FXO,Export_Generic,"FXO")},
				{"LMP",new Extensions("Game Archive",LMP,Export_LMP,"LMP")},
				{"LUA",new Extensions("LUA Script",LUA,Export_Generic,"LUA")},
				{"LVH",new Extensions("Map data file",LVH,Export_Generic,"LVH")},
				{"LVS",new Extensions("Map scripting file",LVS,Export_Generic,"LVS")},
				{"MP3",new Extensions("MPEG-3 Audio",MP3,Export_Generic,"MP3")},
				{"PMF",new Extensions("PSP Movie file",PMF,Export_Generic,"MP4")},
				{"PNG",new Extensions("PNG Image",PNG,Export_PNG,"PNG")},
				{"PRX",new Extensions("PSP Module file",PRX,Export_Generic,"PRX")},
				{"SFO",new Extensions("PSP Game Parameter file",SFO,Export_Generic,"SFO")},
				{"SWV",new Extensions("Shockwave Flash",SWV,Export_Generic,"FLV")},
				{"TEX",new Extensions("Texture",TEX,Export_PNG,"PNG")},
				{"TGA",new Extensions("Targa Image",TGA,Export_Generic,"TGA")},
				{"ZVH",new Extensions("Zone data file",ZVH,Export_Generic,"ZVH")},
				{"ZVS",new Extensions("Zone scripting file",ZVS,Export_Generic,"ZVS")},
			};
		}

		protected override void OnShown(EventArgs e)
		{
			openPathToolStripMenuItem_Click(null, null);
		}

		private void openPathToolStripMenuItem_Click(object sender, EventArgs e)
		{
			new Form2(this).ShowDialog();
		}

		public void LoadPath(string path, bool subdir = false, bool supfile = false)
		{
            SearchOption op = SearchOption.TopDirectoryOnly;
			DirectoryInfo parent = Directory.GetParent(path);

            if (subdir && parent != null)
				op = SearchOption.AllDirectories;


            var files = Directory.GetFiles(path, "*.*", op);

            if (supfile)
			{
                var allowedExtensions = types.Keys.ToArray();
                files = files.Where(file => allowedExtensions.Any(Path.GetExtension(file).ToUpper().EndsWith)).ToArray();
            }


            if (parent != null)
                path = parent.FullName;

            this.path = path;

            for (int i = 0; i < files.Length; i++)
				files[i] = files[i].Substring(path.Length + 1);

			treeView1.Nodes.Clear();
            PopulateTreeView(treeView1, files, '\\');
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

			if (treeView.Nodes.Count > 0)
				treeView.Nodes[0].Expand();

        }

		private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
		{
            try
			{
				if (wmp != null) wmp.controls.stop();

				string tmp = Path.Combine(path, e.Node.FullPath);
                string key = Path.GetExtension(tmp).ToUpper().Replace(".", "");

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
			if (wmp != null) wmp.controls.stop();

			Stream str = new MemoryStream(_files[listBox1.SelectedIndex].Data);
			string tmp = (string)listBox1.SelectedItem;
			string key = tmp.Substring(tmp.LastIndexOf('.') + 1);
			key = key.ToUpper();
			if (types.ContainsKey(key))
				types[key].handler.Invoke(str);
		}

		private void mainToolStripMenuItem_Click(object sender, EventArgs e)
		{
			tmp_filename = null;
			if (sender == mainToolStripMenuItem)
			{
				if (treeView1.SelectedNode != null)
					tmp_filename = treeView1.SelectedNode.Text;
				archive_idx = -1;
			}
			else
			{
				if ((archive_idx = listBox1.SelectedIndex) != -1)
					tmp_filename = (string)listBox1.SelectedItem;
			}
			if (tmp_filename == null)
			{
				MessageBox.Show("Nothing selected.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			string key = tmp_filename.Substring(tmp_filename.LastIndexOf('.') + 1);
			key = key.ToUpper();
			Extensions ext;
			if (types.ContainsKey(key))
				ext = types[key];
			else
			{
				MessageBox.Show("Unknown file format.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			if (key == "LMP")
			{
				sfd.Filter = "All files|*.*";
				sfd.FileName = "Select directory to save";
			}
			else
			{
				sfd.Filter = ext.title + "|*." + ext.exp;
				sfd.FileName = Path.GetFileNameWithoutExtension(tmp_filename);
			}
			if (sfd.ShowDialog() == DialogResult.OK)
			{
				ext.conv.Invoke();
			}
		}
	}
}
