using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NeuralNetwork;
using Compression;
namespace PeidCSharp
{
    public partial class MainView : Form
    {
        Image img;
        int width, height;
        int maxHeight =100;
        int maxWidth =100;
        double size;
        List<int> imgData = new List<int>();
        Helper help = new Helper();
        public MainView()
        {
            InitializeComponent();
        }

        private void OpenCitra_Click(object sender, EventArgs e)
        {
            try
            {
                #region Open Image
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "Open Image |PiedCharp v1";
                    dlg.Filter = "BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png|TIFF|*.tif;*.tiff|"
                                + "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        //OriPicture.Image = new Bitmap(dlg.FileName);
                        img = Image.FromFile(dlg.FileName);

                        #region Preprocessing
                        img = help.Resize(img, maxWidth, maxHeight);
                        #endregion

                        OriPicture.Image = img;
                        size = (int)new System.IO.FileInfo(dlg.FileName).Length;

                        #region Get Image Properties
                        if (OriPicture.Image != null)
                        {
                            width = img.Width;
                            height = img.Height;
                            txtWidth.Text = width.ToString();
                            txtHeight.Text = height.ToString();
                            txtSize.Text = size.ToString();
                        }
                        #endregion
                    }
                }
                #endregion
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Process_Click(object sender, EventArgs e)
        {
                if (OriPicture.Image != null)
                { 

                #region 1.Get Value Pixel
                int[,] citra = new int[maxWidth,maxHeight];
                int p = citra.GetLength(0);
                int l = citra.GetLength(1);
                Bitmap bmp = new Bitmap(img);
                bmp = help.ResizeBitmap(bmp, maxWidth, maxHeight);
                using (bmp)
                    {
                        for (int i = 0; i <p; i++)
                        {
                            for (int j = 0; j <l; j++)
                            {
                                Color clr = bmp.GetPixel(i, j);
                            citra[i, j] = clr.R;
                            }
                        }
                    help.ShowData(citra, true);
                    }

                #endregion

                #region 2.Vektorisasi
                for (int i = 0; i < p; i++)
                {
                    for (int j = 0; j < l; j++)
                    {
                        //vektor[k++] = citra[i, j];
                        imgData.Add(citra[i,j]);
                    }
                }
                #endregion

                #region 3.Huffman Coding
                Console.WriteLine("Data Asli");
                foreach (int item in imgData)
                {
                    Console.Write("{0} ", item);
                }
                Console.WriteLine("\nJumlah Bit Data Asli {0} x 8 = {1} bit", imgData.Count, imgData.Count * 8);
                var huffman = new Huffman<int>(imgData);

                Console.WriteLine("\nData Encode");
                List<int> encoding = huffman.Encode(imgData);
                foreach (int item in encoding)
                {
                    Console.Write("{0} ", item);
                }
                Console.WriteLine("\nJumlah Bit Data Encoding {0}", encoding.Count);

                #region 4.Comparation
                OriBit.Text = (imgData.Count * 8).ToString();
                CompBit.Text = (encoding.Count).ToString();
                #endregion

                List<int> decoding = huffman.Decode(encoding);

                Console.WriteLine("\nData Decoding");
                foreach (int item in decoding)
                {
                    Console.Write("{0} ", item);
                }
                Console.WriteLine("\n\nDetail Compressed");

                var ints = new HashSet<int>(imgData);
                foreach (int c in ints)
                {
                    encoding = huffman.Encode(c);
                    Console.Write("{0} : ", c);
                    foreach (int bit in encoding)
                    {
                        Console.Write("{0}", bit);
                    }
                    Console.WriteLine();
                }
                #endregion

                }
        }
    }
}
