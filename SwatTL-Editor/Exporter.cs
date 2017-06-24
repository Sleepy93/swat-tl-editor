using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
			br.BaseStream.Position = 0x28;
			if (br.ReadInt32() == 0)
			{
				br.BaseStream.Position = 0xCA;
			}
			else
			{
				br.BaseStream.Position = 0x490;
			}
			while (br.PeekChar() > -1)
			{
				ushort x, y, z;
				br.ReadByte();
				x = br.ReadUInt16();
				y = br.ReadUInt16();
				z = br.ReadUInt16();
				sw.WriteLine(string.Format("v {0}.{1} {2}.{3} {4}.{5}",
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