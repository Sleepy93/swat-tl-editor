using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SwatTL_Editor
{
    public partial class Form1 : Form
    {
        string path;
        string[] files;
        Dictionary<string, string> types = new Dictionary<string, string>()
        {
            {"","Raw file"},
            { "ANM","Animation file"},
            { "AWF","Audio Wave Format" },
            { "BIK","Bink VIdeo"},
            { "BIN","Binary file"},
            { "CUT","Cutscene"},
            { "DIX","Dialogue file"},
            { "DOB","Model file" },
            { "FXO","Shader file"},
            { "LMP","Game Archive"},
            { "LUA","LUA Script"},
            { "LVH","Map model"},
            { "LVS","Map scripting file"},
            { "MP3","MPEG-3 Audio"},
            { "PMF","PSP Movie file"},
            { "PNG","PNG Image"},
            { "PRX","PSP Module file"},
            { "SFO","PSP Game Parameter file"},
            { "SWV","Shockwave Flash"},
            { "TEX","Texture"},
            { "TGA","Targa Image"},
            { "ZVH","Unknown"},
            { "ZVS","Unknown"}
        };

        public Form1()
        {
            InitializeComponent();
        }

        private void openPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                path = fbd.SelectedPath + "\\PSP_GAME\\";
                if (Directory.Exists(path))
                {
                    files = Directory.GetFiles(path);

                }
            }
        }
    }
}
