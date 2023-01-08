using System.IO;
using System.Text;
using System.Windows.Forms;


namespace SwatTL_Editor
{
	public partial class Form1 : Form
	{
		void Export_Generic()
		{
			if (archive_idx == -1)
				File.Copy(tmp_filename, sfd.FileName);
			else
				File.WriteAllBytes(sfd.FileName, _files[archive_idx].Data);
		}

        void Export_OBJ()
        {
            BinaryReader br;
            if (archive_idx == -1)
                br = new BinaryReader(new FileStream(Path.Combine(path,tmp_filename), FileMode.Open, FileAccess.Read), Encoding.Default);
            else
                br = new BinaryReader(new MemoryStream(_files[archive_idx].Data), Encoding.Default);
            
            ///// Structure of DOB models 
            uint magic   = br.ReadUInt32();
            uint numMesh = br.ReadUInt32();
            
            //Checking the Magic and Type of the Model
            if (magic != 0x7B)
                throw new InvalidDataException("Bad magic, not a valid DOB model file!");
            //there are some with other magic. Already fixed, I'll fix it later.

            uint ObjOfs   = br.ReadUInt32();
            uint ObjCount = br.ReadUInt32();
            br.BaseStream.Position += 0xC;// Skip unk
            uint bonesOfs = br.ReadUInt32();


            //Reading Bones and Animations
            if (bonesOfs > 0)
            {
                br.BaseStream.Position = bonesOfs;
                uint bonesCount = br.ReadUInt32();
                uint matOfs     = br.ReadUInt32();
                uint animCount  = br.ReadUInt32();
                uint animOfs    = br.ReadUInt32();
            }


            StringBuilder sb = new StringBuilder("#by Durik256 git: https://github.com/Sleepy93/swat-tl-editor\n");

            // move to beginning of declaration
            br.BaseStream.Position = ObjOfs;

            // read vertex buffers
            int f = 0;//counter all indices
            for (int i = 0; i < ObjCount; i++)
            {
                br.ReadInt16();//HH
                var objName = Encoding.UTF8.GetString(br.ReadBytes(0x8));
                var meshID = br.ReadByte();
                var numBones = br.ReadByte();
                var bonesIndex = br.ReadBytes(8);
                var numVertex = br.ReadInt16();
                br.ReadInt16();//HH
                br.BaseStream.Position += 24;
                var VertexOfs = br.ReadUInt32();

                long curPos = br.BaseStream.Position;
                br.BaseStream.Position = VertexOfs;

                sb.AppendLine($"g {objName}");

                //vert block (32768/64=512)
                for (int v = 0; v < numVertex; v++)
                {
                    if (numBones > 0)//stride > 14
                        br.BaseStream.Position += 8;//weight

                    sb.AppendLine($"vt {br.ReadInt16() / 512f} {br.ReadInt16() / 512f}".Replace(',', '.'));//uvs
                    br.BaseStream.Position += 4;//normal?

                    sb.AppendLine($"v {br.ReadInt16() / 512f} {br.ReadInt16() / 512f} {br.ReadInt16() / 512f}".Replace(',', '.'));//vert
                }

                //generate faces
                sb.AppendLine($"usemtl default");
                for (int idx = 1; idx < numVertex - 1; idx++)
                {
                    int a = idx + f;
                    int b = idx + 1 + f;
                    int c = idx + 2 + f;

                    if (idx % 2 == 0)
                        sb.AppendLine($"f {a}/{a} {b}/{b} {c}/{c}");
                    else
                        sb.AppendLine($"f {c}/{c} {b}/{b} {a}/{a}");
                }
                f += numVertex;
                
                br.BaseStream.Position = curPos;//return to read obj
            }

            File.WriteAllText(Path.ChangeExtension(sfd.FileName, ".obj"), sb.ToString());

            //Closing Readers
            br.Close();
            MessageBox.Show("Model file Exported Successfully");
        }

        void Export_PNG()
		{
			pictureBox1.Image.Save(sfd.FileName);
		}

		void Export_LMP()
		{
			string tmp = Path.GetDirectoryName(sfd.FileName);
			foreach (LMPFinfo info in _files)
				File.WriteAllBytes(Path.Combine(tmp, info.Filename), info.Data);
		}
	}
}