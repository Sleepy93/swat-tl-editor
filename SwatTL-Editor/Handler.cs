using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SwatTL_Editor
{
    public partial class Form1 : Form
    {
        void ANM(Stream input)
        {

        }

        void AWF(Stream input)
        {

        }

        void BIK(Stream input)
        {

        }

        void BIN(Stream input)
        {

        }

        void CUT(Stream input)
        {

        }

        void DIX(Stream input)
        {

        }

        void DOB(Stream input)
        {

        }

        void FXO(Stream input)
        {

        }

        public class LMPFinfo
        {
            public string Filename { get; set; }

            public int DataOffset { get; set; }
            public int DataSize { get; set; }
            public byte[] Data { get; set; }
        }

        LMPFinfo[] _files;

        void LMP(Stream input)
        {
            listBox1.Items.Clear();

            var fileLength = input.Length;

            using (var fs = input)
            using (var f = new BinaryReader(fs, Encoding.Default))
            {
                var count = f.ReadInt32();

                if (count == 0)
                {
                    MessageBox.Show("No files present in archive!");
                    return;
                }

                _files = new LMPFinfo[count];

                for (int i = 0; i < count; i++)
                {
                    // entries begin at 0x4 (length: 0xC)
                    var entryOffset = (i * 0xC) + 4;

                    fs.Position = entryOffset;

                    // all offsets are relative to beginning of file
                    // so they're basically absolute offsets :)
                    var entryInfo = new
                    {
                        NameOffset = f.ReadInt32(),

                        DataOffset = f.ReadInt32(),
                        DataSize = f.ReadInt32()
                    };

                    // make sure offset and size are valid
                    if ((entryInfo.DataOffset + entryInfo.DataSize) > fileLength || (entryInfo.DataOffset <= ((count * 0xC) + 4)))
                    {
                        MessageBox.Show(string.Format(
@"Invalid LMP file - the entry table seems to be corrupt!

--- Detailed Information ---
Total entries: {0}

Index: {1}
Offset: {2}
Name offset \\ Data offset: 0x{3:X8} \\ 0x{4:X8}
DataSize: 0x{5:X8}", count, (i + 1), entryOffset, entryInfo.NameOffset, entryInfo.DataOffset, entryInfo.DataSize));

                        // stop reading file
                        return;
                    }

                    // get filename
                    var entryName = new StringBuilder();

                    // make sure offset is valid
                    if (entryInfo.NameOffset < fileLength && entryInfo.NameOffset < entryInfo.DataOffset)
                    {
                        // get filename
                        fs.Position = entryInfo.NameOffset;

                        while (f.PeekChar() != 0)
                            entryName.Append(f.ReadChar());
                    }
                    else
                    {
                        MessageBox.Show(string.Format("Bad filename pointer 0x{0:X8} @ 0x{1:X8}!", entryInfo.NameOffset, entryOffset));
                        entryName.AppendFormat("file_{0}", (i + 1));
                    }

                    // add to list of entries
                    byte[] dbuf = new byte[entryInfo.DataSize];
                    fs.Position = entryInfo.DataOffset;
                    fs.Read(dbuf, 0, entryInfo.DataSize);
                    _files[i] = new LMPFinfo()
                    {
                        DataOffset = entryInfo.DataOffset,
                        DataSize = entryInfo.DataSize,
                        Data = dbuf,
                        Filename = entryName.ToString()
                    };

                    var sizeStr = (entryInfo.DataSize >= 1024)
                            ? String.Format("{0} KB", (int)Math.Round(entryInfo.DataSize / 1024f))
                            : String.Format("{0:N} bytes", entryInfo.DataSize);

                    listBox1.Items.Add(_files[i].Filename);
                }
            }
        }

        void LUA(Stream input)
        {
            ShowRightSide(richTextBox1);
            richTextBox1.Text = new StreamReader(input, Encoding.Default).ReadToEnd();
        }

        void LVH(Stream input)
        {

        }

        void LVS(Stream input)
        {

        }

        WMPLib.WindowsMediaPlayer wmp;

        void MP3(Stream input)
        {
            if (wmp == null) wmp = new WMPLib.WindowsMediaPlayer();
            wmp.URL = null;
            byte[] tmp = new byte[input.Length];
            input.Read(tmp, 0, tmp.Length);
            string fname = Path.GetTempPath() + "tmpmedia.mp3";
            BinaryWriter sw = new BinaryWriter(new FileStream(fname, FileMode.Create, FileAccess.Write), Encoding.Default);
            sw.Write(tmp);
            sw.Close();
            wmp.URL = fname;
            wmp.controls.play();
        }

        void PMF(Stream input)
        {

        }

        void PNG(Stream input)
        {
            ShowRightSide(pictureBox1);
            pictureBox1.Image = new Bitmap(input);
        }

        void PRX(Stream input)
        {

        }

        void SFO(Stream input)
        {

        }

        void SWV(Stream input)
        {

        }

        Bitmap _img;

        void TEX(Stream input)
        {
            _img = null;
            pictureBox1.Image = null;
            pictureBox1.BackgroundImage = null;

            using (var fs = input)
            using (var f = new BinaryReader(fs, Encoding.Default))
            {
                int width = f.ReadUInt16(),
                    height = f.ReadUInt16(),

                    paletteSize = f.ReadUInt16(),

                    unknown = f.ReadUInt16(),
                    reserved = f.ReadInt32(),

                    paletteOffset = f.ReadUInt16();

                if (unknown == 0x4)
                {
                    var dataSize = (width * height);

                    var hasPalette = dataSize > paletteSize;

                    var dataOffset = paletteOffset + ((hasPalette) ? paletteSize : (paletteSize - dataSize));

                    // read palette
                    fs.Position = paletteOffset;

                    var paletteData = f.ReadBytes(dataOffset - paletteOffset);

                    // read image data
                    fs.Position = dataOffset;

                    var data = f.ReadBytes(dataSize);

                    _img = new Bitmap(width, height);

                    // ---- begin unswizzling code ---- \\
                    // author: Nick Woronekin (https://github.com/nickworonekin)
                    // source: https://github.com/nickworonekin/puyotools/blob/master/Libraries/GimSharp/GimTexture/GimPixelCodec.cs
                    var dest = new byte[width * height];

                    int destOffset = 0;

                    // Incorporate the bpp into the width
                    width = (width * 8) >> 3;

                    int rowblocks = (width / 16);

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int blockX = x / 16;
                            int blockY = y / 8;

                            int blockIndex = blockX + ((blockY) * rowblocks);
                            int blockAddress = blockIndex * 16 * 8;

                            dest[destOffset] = data[blockAddress + (x - blockX * 16) + ((y - blockY * 8) * 16)];
                            destOffset++;
                        }
                    }
                    // ---- end unswizzling code ---- \\

                    //-- copy raw image data to clipboard (for use in a hex editor)
                    //var sb = new StringBuilder();
                    //
                    //for (int i = 0; i < dest.Length; i++)
                    //    sb.AppendFormat("{0:X2}{1}", dest[i], ((i % 16) == 0 ? "\r\n" : " "));
                    //
                    //Clipboard.SetData(DataFormats.Text, sb.ToString());

                    fs.Position = paletteOffset;

                    Color[] palette = new Color[256];
                    ushort color;
                    //double h, s, v;
                    for (int i = 0; i < 256; i++)
                    {
                        color = f.ReadUInt16();
                        palette[i] = Color.FromArgb(
                            (byte)Clamp(((color & 0x7C00) >> 10) * 8, 0, 255),
                            (byte)Clamp(((color & 0x3E0) >> 5) * 8, 0, 255),
                            (byte)Clamp((color & 0x1F) * 8, 0, 255)
                            );
                        /*ColorToHSV(palette[i], out h, out s, out v);
                        h = (h + 160) % 360;
                        palette[i] = ColorFromHSV(h, s, v);*/
                    }
                    _img = new Bitmap(width, height);

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            var p = dest[x + (y * width)];

                            _img.SetPixel(x, y, palette[p]);
                        }
                    }


                }
                else if (unknown == 0x8080)
                {
                    fs.Position = paletteOffset;

                    var dataSize = (width * height);

                    var ch1 = new byte[dataSize];
                    var ch2 = new byte[dataSize];

                    for (int i = 0; i < dataSize; i++)
                    {
                        ch1[i] = f.ReadByte();
                        ch2[i] = f.ReadByte();
                    }

                    _img = new Bitmap(width, height);

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            var idx = x + (y * width);

                            var p = ch1[idx];
                            var a = ch2[idx];

                            _img.SetPixel(x, y, Color.FromArgb(a, p, p, p));
                        }
                    }

                    // create a checkered background
                    var checker = new Bitmap(width, height);

                    var c1 = Color.FromArgb(255, 0, 0, 0);
                    var c2 = Color.FromArgb(255, 132, 0, 132);

                    for (int y = 0; y < height / 8; y++)
                    {
                        var oddRow = ((y % 2) != 0);

                        for (int x = 0; x < width / 8; x++)
                        {
                            var oddCol = ((x % 2) != 0);

                            for (int row = 0; row < 8; row++)
                            {
                                for (int col = 0; col < 8; col++)
                                    checker.SetPixel((x * 8) + col, (y * 8) + row, (!oddRow && oddCol) ? c2 : (oddRow && !oddCol) ? c2 : c1);
                            }
                        }
                    }

                    pictureBox1.Image = _img;
                }
                else
                {
                    MessageBox.Show("Unknown texture format :(", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            ShowRightSide(pictureBox1);
            pictureBox1.Image = _img;
        }

        int Clamp(int val, int min, int max)
        {
            if (val < min) return min;
            else if (val > max) return max;
            return val;
        }

        void TGA(Stream input)
        {
            Paloma.TargaImage _img = new Paloma.TargaImage(input);
            ShowRightSide(pictureBox1);
            pictureBox1.Image = _img.Image;
        }

        void ZVH(Stream input)
        {

        }

        void ZVS(Stream input)
        {

        }
    }
}