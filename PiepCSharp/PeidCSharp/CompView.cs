using Algorithm;
using Compression;
using NeuralNetwork;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PeidCSharp
{
    public partial class CompView : Form
    {
        Image img;
        int width, height;
        int maxHeight = 184;
        int maxWidth = 184;
        double size;
        Helper help = new Helper();
        public Bitmap sourceImage;
        public Bitmap filteredImage;
        public Bitmap originalImage;
        public Bitmap imgCluster1, imgCluster2, imgCluster3;
        private BackgroundWorker backgroundWorker;
        public Stopwatch stopWatch;

        public delegate void MyDelegate(string input);

        delegate void SetTextCallback(string text);

        public delegate void DelegateThreadFinished();

        public CompView()
        {
            InitializeComponent();
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);

            stopWatch = new Stopwatch();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Progress.Value = e.ProgressPercentage;
            epoch.Text = e.UserState as string;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled==true))
            {
                epoch.Text = "Canceled";
            }
            else if (!(e.Error==null))
            {
                epoch.Text = ("Error: " + e.Error.Message);
            }
            Progress.Enabled = false;
            btnCluster.Enabled = true;
            btnCancel.Enabled = false;
            btnCompress.Enabled = true;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            backgroundWorker.ReportProgress(0, "working...");
            filteredImage = (Bitmap)OriImage.Image.Clone();
            int numCluster = (int)numericUpDown1.Value;
            int maxIterations = (int)numericUpDown2.Value;
            double accuracy = (double)numericUpDown3.Value;

            List<ClusterPoint> points = new List<ClusterPoint>();
            for (int row = 0; row < originalImage.Width; ++row)
            {
                for (int col = 0; col < originalImage.Height; ++col)
                {
                    Color c2 = originalImage.GetPixel(row, col);
                    points.Add(new ClusterPoint(row, col, c2));
                }
            }

            List<ClusterCentroid> centroids = new List<ClusterCentroid>();

            //Create random points to use a the cluster centroids
            Random random = new Random();
            for (int i = 0; i < numCluster; i++)
            {
                int randomNumber1 = random.Next(sourceImage.Width);
                int randomNumber2 = random.Next(sourceImage.Height);
                centroids.Add(new ClusterCentroid(randomNumber1, randomNumber2, filteredImage.GetPixel(randomNumber1, randomNumber2)));
            }

            FCM alg = new FCM(points, centroids, 2, filteredImage, (int)numericUpDown1.Value);

            int k = 0;
            do
            {
                if ((backgroundWorker.CancellationPending==true))
                {
                    e.Cancel = true;
                    break;
                }else
                {
                    k++;
                    alg.J = alg.CalculateObjectiveFunction();
                    alg.CalculateClusterCentroids();
                    alg.Step();
                    double Jnew = alg.CalculateObjectiveFunction();
                    Console.WriteLine("Run method i={0} accuracy = {1} delta = {2}",k,alg.J,Math.Abs(alg.J-Jnew));
                    precision.Text = "Precision " + Math.Abs(alg.J - Jnew);

                    //Format and Display the TimeSpan Value;
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", stopWatch.Elapsed.Hours, stopWatch.Elapsed.Minutes, stopWatch.Elapsed.Seconds, stopWatch.Elapsed.Milliseconds / 10);
                    duration.Text = "Duration: " + elapsedTime;

                    SegmentedImage.Image = (Bitmap)alg.getProcessedImage;
                    backgroundWorker.ReportProgress((100 * k) / maxIterations, "Iteration " + k);
                    if (Math.Abs(alg.J - Jnew) < accuracy) break;
                }
            } while (maxIterations>k);
            Console.WriteLine("Done..");

            //Save the Segmented Image
            SegmentedImage.Image = (Bitmap)alg.getProcessedImage.Clone();
            alg.getProcessedImage.Save("SegmentedImage.png");

            //Create a new image for each cluster in order to extratc the feature
            double[,] Matrix = alg.U;
            Bitmap[] bmapArray = new Bitmap[centroids.Count];
            for (int i = 0; i < centroids.Count; i++)
            {
                bmapArray[i] = new Bitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppRgb);
            }

            for (int j = 0; j < points.Count; j++)
            {
                for (int i = 0; i < centroids.Count; i++)
                {
                    ClusterPoint p = points[j];
                    if (Matrix[j,i]==p.ClusterIndex)
                    {
                        bmapArray[i].SetPixel((int)p.X, (int)p.Y, p.OriginalPixelColor);
                    }
                }
            }

            //Save the image for each segmented Cluster
            
            for (int i = 0; i < centroids.Count; i++)
            {
                bmapArray[i].Save("Cluster" + i + ".png");
                
            }
            imgCluster1 = (Bitmap)bmapArray[0].Clone();
            imgCluster3 = (Bitmap)bmapArray[2].Clone();
            imgCluster2 = (Bitmap)bmapArray[1].Clone();
            pictClus1.Image = imgCluster1;
            pictClus2.Image = imgCluster2;
            pictClus3.Image = imgCluster3;
            
            // Resource Cleanup..
            backgroundWorker.ReportProgress(100, "Done in " + k + "iteration.");
            //getDataCluster();

            for (int i = 0; i < points.Count; i++)
            {
                points[i] = null;
            }
            for (int i = 0; i < centroids.Count; i++)
            {
                centroids[i] = null;
            }
            alg = null;
        }

        private void CompView_Load(object sender, EventArgs e)
        {

        }

        private void btnCompress_Click(object sender, EventArgs e)
        {
            getDataCluster();
            string cons1 = "";
            string cons2 = "";
            
            startCompress(maxWidth, maxHeight, OriImage, ref cons1, ref cons2);
            txtOriBit1.Text = cons1;
            txtCompBit1.Text = cons2;

            startCompress(maxWidth, maxHeight, pictClus1, ref cons1, ref cons2);
            txtOriBitClus1.Text = cons1;
            txtCompClus1.Text = cons2;

            startCompress(maxWidth, maxHeight, pictClus1,ref cons1,ref cons2);
            txtOriBitClus1.Text = cons1;
            txtCompClus1.Text = cons2;

            startCompress(maxWidth, maxHeight, pictClus2, ref cons1, ref cons2);
            txtOriBitClus2.Text = cons1;
            txtCompClus2.Text = cons2;

            startCompress(maxWidth, maxHeight, pictClus3, ref cons1, ref cons2);
            txtOriBitClus3.Text = cons1;
            txtCompClus3.Text = cons2;
        }

        private void btnCluster_Click(object sender, EventArgs e)
        {
            btnCluster.Enabled = false;
            btnCancel.Enabled = true;
            btnCompress.Enabled = true;
            stopWatch.Reset();
            stopWatch.Start();
            backgroundWorker.RunWorkerAsync();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (backgroundWorker!=null)
            {
                backgroundWorker.CancelAsync();
            }
            epoch.Text = "Aborting, Please wait.....";
        }

        private void SegmentedImage_Click(object sender, EventArgs e)
        {

        }

        private void OpenImage_Click(object sender, EventArgs e)
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

                        OriImage.Image = img;
                        btnCluster.Enabled = true;
                        originalImage = (Bitmap)OriImage.Image.Clone();
                        sourceImage = (Bitmap)OriImage.Image.Clone();
                        size = (int)new System.IO.FileInfo(dlg.FileName).Length;
                        btnCluster.Enabled = true;

                        #region Get Image Properties
                        if (OriImage.Image != null)
                        {
                            width = img.Width;
                            height = img.Height;
                            txtWidth1.Text = width.ToString();
                            txtHeight1.Text = height.ToString();
                            txtSize1.Text = size.ToString();
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

        private void getDataCluster()
        {
            try
            {
                txtHeightClus1.Text = imgCluster1.Height.ToString();
                txtWidthClus1.Text = imgCluster1.Width.ToString();

                txtHeightClus2.Text = imgCluster2.Height.ToString();
                txtWidthClus2.Text = imgCluster2.Width.ToString();


                txtHeightClus3.Text = imgCluster3.Height.ToString();
                txtWidthClus3.Text = imgCluster3.Width.ToString();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void startCompress(int maxWidth, int maxHeight,PictureBox OriImg,ref string oriBit,ref string compBit)
        {
            if (OriImg.Image!=null)
            {
                List<int> imgData = new List<int>();
                #region Get Pixel Data
                int[,] citra = new int[maxWidth, maxHeight];
                int p = citra.GetLength(0);
                int l = citra.GetLength(1);
                Bitmap bmp = new Bitmap(OriImg.Image);
                bmp = help.ResizeBitmap(bmp, maxWidth, maxHeight);
                using (bmp)
                {
                    for (int i = 0; i < p; i++)
                    {
                        for (int j = 0; j < l; j++)
                        {
                            Color clr = bmp.GetPixel(i, j);
                            citra[i, j] = clr.R;
                        }
                    }
                }
                #endregion

                #region Vektorisasi
                imgData.Clear();
                for (int i = 0; i < p; i++)
                {
                    for (int j = 0; j < l; j++)
                    {
                        imgData.Add(citra[i, j]);
                    }
                }
                #endregion

                #region Huffman Coding
                oriBit =(imgData.Count *8).ToString();
                var huffman = new Huffman<int>(imgData);
                List<int> encoding = huffman.Encode(imgData);
                compBit = (encoding.Count).ToString();
                #endregion


            }
        }
    }
}
