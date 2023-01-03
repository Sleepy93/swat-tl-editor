using System.Globalization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;


namespace SwatTL_Editor
{
	public partial class Form1 : Form
	{
		void Export_Generic()
		{
			if (archive_idx == -1)
			{
				File.Copy(tmp_filename, sfd.FileName);
			}
			else
			{
				File.WriteAllBytes(sfd.FileName, _files[archive_idx].Data);
			}
		}

        void Export_OBJ()
        {
            StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.Default);
            BinaryReader br;
            if (archive_idx == -1)
            {
                br = new BinaryReader(new FileStream(tmp_filename, FileMode.Open, FileAccess.Read), Encoding.Default);
            }
            else
            {
                br = new BinaryReader(new MemoryStream(_files[archive_idx].Data), Encoding.Default);
            }
            /* X,Y,Z generation OLD
            while (br.PeekChar() > -1)
            {
               // ushort x, y, z;
                br.ReadByte();
                //x = br.ReadUInt16();
                //y = br.ReadUInt16();
                //z = br.ReadUInt16();

                ushort x, y, z;
                float fx, fy, fz;

                br.ReadByte();
                x = br.ReadUInt16(); fx = (float)(x / 256.0); if (fx >= 128.0) fx -= (float)256.0;
                y = br.ReadUInt16(); fy = (float)(y / 256.0); if (fy >= 128.0) fy -= (float)256.0;
                z = br.ReadUInt16(); fz = (float)(z / 256.0); if (fz >= 128.0) fz -= (float)256.0;
                sw.WriteLine("v {0} {1} {2}", fx.ToString(System.Globalization.CultureInfo.InvariantCulture), fy.ToString(System.Globalization.CultureInfo.InvariantCulture), fz.ToString(System.Globalization.CultureInfo.InvariantCulture));
                
                /*sw.WriteLine(string.Format("v {0}.{1} {2}.{3} {4}.{5}",
                    x & 0xFF,
                    x / 0x3FFF,
                    y & 0xFF,
                    y / 0x3FFF,
                    z & 0xFF,
                    z / 0x3FFF));
                br.BaseStream.Position += 0xF;
            }
            sw.Close();
            br.Close();
            */
            
            ///// Structure of DOB models 
            uint magic = br.ReadUInt32();
            uint vTypeOfModel = br.ReadUInt32();
            //Checking the Magic and Type of the Model
            if (magic != 0x7B)
                throw new InvalidDataException("Bad magic, not a valid DOB model file!");

            // 1  - Single Object
            // 24 - MAP object
            // 25 - Character

            if (vTypeOfModel != 1 & vTypeOfModel !=24 & vTypeOfModel != 25)
                throw new InvalidDataException("Unknown type of model file, cannot load DOB model file!");

            uint vObjectOffset = br.ReadUInt32();
            uint vObjectCount = br.ReadUInt32();
            // Skip the Padding
            br.BaseStream.Position += 0xC;

            //Reading Bones and Animations
            uint vUnkDataOffset = br.ReadUInt32(); //Transformation offset obj1 + 0x00 or 0x30 gots the next object... - probably stepper of Obj
            uint vBonesCount = br.ReadUInt32();
            uint vBonesOffset = br.ReadUInt32();
            uint vAnimationCount = br.ReadUInt32();
            uint vAnimationOffset = br.ReadUInt32();
            
            var sb = new StringBuilder();
            // read vertex buffers
            for (int vBuf = 0; vBuf < vObjectCount; vBuf++)
            {
                // move to beginning of declaration
                br.BaseStream.Position = (vObjectOffset + (vBuf * 0x30)); //0x30 or 0x00

                // skip the name Paddings
                var vStartOfStructure = br.ReadInt16();
                var vObjectName = Encoding.UTF8.GetString(br.ReadBytes(0x8));
                var vUnkTransformDATA01 = br.ReadInt16();
                var vUnkTransformDATA02 = br.ReadUInt32();
                var vUnkTransformDATA03 = br.ReadUInt32();
                var vNumberOfVertex = br.ReadInt16();
                var vEndOfStructure = br.ReadInt16();
                var vUnkOtherDATA01 = br.ReadBytes(0x18);
                var vVertexOffset = br.ReadUInt32();

                // for support in blender :)
                sb.AppendFormat("o {0}", vObjectName).AppendLine();

                // read the vertices in the buffer
                for (int v = 0; v < vNumberOfVertex; v++)
                {
                    // move to beginning of vertex
                    br.BaseStream.Position = (vVertexOffset + (v * 0x16));
                    /*
                    if (vNumberOfVertex == 0)
                    {
                        throw new InvalidOperationException(String.Format("Invalid data in vertex buffer #{0} @ 0x{1:X8}, cannot read DOB model file!",
                            br.BaseStream.Position));
                    }
                    */
                    float[] coords = new float[3];

                    for (int i = 0; i < coords.Length; i++)
                    {
                        var c = (br.ReadUInt16() / 256.0f);

                        if (c >= 128.0f)
                            c -= 256.0f;

                        coords[i] = c;

                        sb.AppendFormat(CultureInfo.InvariantCulture, "v {0:F4} {1:F4} {2:F4}",
                            coords[0], coords[1], coords[2]).AppendLine();
                    }
                }
                sb.AppendLine();

                sb.AppendFormat("g {0}", vObjectName).AppendLine();
                sb.AppendLine("s off");
                
                // build the tris list :)
                for (int t = 0; t < vNumberOfVertex; t += 3)
                {
                    // best to put these here,
                    // in case we need to swap faces or something
                    var t1 = (t + 1);
                    var t2 = (t + 2);
                    var t3 = (t + 3);

                    sb.AppendFormat("f {0} {1} {2}", t1, t2, t3);
                }
            }

            sb.AppendLine();
            sb.AppendLine("#EOF");
            sw.Write(sb.ToString());

            //Closing Readers and writers
            sw.Close();
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
			{
				File.WriteAllBytes(Path.Combine(tmp, info.Filename), info.Data);
			}
		}
	}
}