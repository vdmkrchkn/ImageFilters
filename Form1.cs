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

namespace Filters
{
    public partial class Form1 : Form
    {        
        bool mode; // способ фильтрации - T - частично, F - целиком
        Point pStart, pCur, pNull, pMin, pMax; // точки выделения области
        List<Point> border;
        Pen pen;
        Image srcImage;

        public Form1()
        {
            InitializeComponent();
            openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            this.mode = false;
            this.pen = Pens.Black;
            this.pNull = new Point(int.MaxValue, 0);
        }
        // преобразование изображения, заданного Bitmap, в матрицу его яркостей
        void convertBitmap(Bitmap bmp, ref HSV[,] arr)
        {            
            for (int i = 0; i < bmp.Height; ++i)
                for (int j = 0; j < bmp.Width; ++j)
                {                                      
                    arr[i, j] = new HSV(bmp.GetPixel(j, i));
                    arr[i, j].Value *= 100;                   
                }           
        }
        // преобразование яркости изображения, заданного Bitmap
        void convertBitmap(ref Bitmap bmp, HSV[,] arr)
        {
            for (int i = 1; i < bmp.Height-1; ++i)
                for (int j = 1; j < bmp.Width-1; ++j)
                    try
                    {
                        arr[i, j].Value /= 100.0;
                        bmp.SetPixel(j, i, arr[i, j].toRGB());
                    }
                    catch(ArgumentException ae)
                    {
                        Console.WriteLine(ae.Message + " (" + i + "," + j + ")");
                        return;
                    }
        }
        // свёртка матрицы с ядром фильтра
        void convolution(int[,] kernel, ref HSV[,] arr)
        {
            int rMax = int.MinValue;
            int rMin = int.MaxValue;
            double[,] laplasian = new double[arr.GetLength(0), arr.GetLength(1)];
            for (int i = pMin.Y + 1; i < pMax.Y - 1; ++i)
                for (int j = pMin.X + 1; j < pMax.X - 1; ++j)
                {
                    int r = 0; // отклик ядра
                    for(int s = 0; s < kernel.GetLength(0); ++s)
                        for (int t = 0; t < kernel.GetLength(1); ++t)
                            r += kernel[s,t] * (int)arr[i + s - 1, j + t - 1].Value;
                    if (r > rMax)
                        rMax = r;
                    if (r < rMin)
                        rMin = r;                    
                    laplasian[i, j] = r;
                }
            //Console.WriteLine("min={0},max={1}",rMin,rMax);            
            //double vMax = double.MinValue;
            //double vMin = double.MaxValue;
            for (int i = pMin.Y + 1; i < pMax.Y - 1; ++i)
                for (int j = pMin.X + 1; j < pMax.X - 1; ++j)
                {                    
                    laplasian[i, j] -= rMin;
                    arr[i, j].Value = laplasian[i, j] /= (rMax - rMin) / 100.0;                     
                    //if (laplasian[i, j] > vMax)
                    //    vMax = laplasian[i, j];
                    //if (laplasian[i, j] < vMin)
                    //    vMin = laplasian[i, j];                    
                }
            //Console.WriteLine("min={0},max={1}",vMin,vMax);            
        }
        //
        void getBorderBounds(List<Point> lp)
        {
            if (lp.Count > 0)
            {
                pMin = new Point(lp.Min(p => p.X), lp.Min(p => p.Y));
                if (pMin.X < 0) pMin.X = 0;
                if (pMin.Y < 0) pMin.Y = 0;
                pMax = new Point(lp.Max(p => p.X), lp.Max(p => p.Y));
                //if (x2 > bmp.Width) x2 = bmp.Width;                                                
                //if (y2 > bmp.Height) y2 = bmp.Height;                
            }
        }
        // являются ли точки смежными в 8 связной области
        static bool isAdjacent(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) <= 1 && Math.Abs(p1.Y - p2.Y) <= 1;
        }

        private void open1_Click(object sender, EventArgs e)
        {
            pStart = pNull;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string s = openFileDialog1.FileName;
                try
                {
                    srcImage = new Bitmap(s);
                    Graphics g = Graphics.FromImage(srcImage);
                    g.Dispose();
                    this.reset1_Click(null, EventArgs.Empty);
                }
                catch
                {
                    MessageBox.Show("File " + s + " has a wrong format", "Error");
                    return;
                }
                Text = "Photoshop plugin - " + s;
                saveFileDialog1.FileName = s;
                openFileDialog1.FileName = "";
            }
        }

        private void close1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void improvement1_Click(object sender, EventArgs e)
        {
            Bitmap bmp;
            try
            {
                bmp = new Bitmap(srcImage);
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Для преобразования инициализируйте изображение.");
                return;
            }
            HSV[,] brArr = new HSV[bmp.Height, bmp.Width];
            convertBitmap(bmp, ref brArr);
            int[,] maskRight = 
            {
                { 0, -1,  0},
                {-1,  5, -1},
                { 0, -1,  0}
            };

            int[,] maskDiag = 
            {
                {-1, -1, -1},
                {-1,  9, -1},
                {-1, -1, -1}
            };
            if(radioButton1.Checked)
                convolution(maskRight, ref brArr);
            else
                convolution(maskDiag, ref brArr);
            convertBitmap(ref bmp, brArr);            
            pictureBox2.Image = bmp;
        }

        private void reset1_Click(object sender, EventArgs e)
        {
            mode = false;
            pMin = new Point(0,0);
            pMax = new Point(srcImage.Width,srcImage.Height);            
            try
            {
                if (pictureBox1.Image != null)
                    pictureBox1.Image.Dispose();
                if (pictureBox2.Image != null)
                    pictureBox2.Image.Dispose();
                pictureBox1.Image = pictureBox2.Image = srcImage.Clone() as Image;
            }
            catch
            {
                return;
            }
        }

        private void glass1_Click(object sender, EventArgs e)
        {
            Bitmap bmp;
            try
            {
                bmp = new Bitmap(srcImage);
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Для преобразования инициализируйте изображение.");
                return;
            }                        
            Random rnd = new Random();
            for (int i = pMin.Y + 5; i < pMax.Y - 5; ++i)
                for (int j = pMin.X + 5; j < pMax.X - 5; ++j)                
                    bmp.SetPixel(j, i, bmp.GetPixel(j + (int)((rnd.NextDouble() - 0.5) * 10), i + (int)((rnd.NextDouble() - 0.5) * 10)));                
            pictureBox2.Image = bmp;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            border = new List<Point>();
            if (e.Button == MouseButtons.Left)
            {
                pCur = pStart = e.Location;                
                border.Add(pCur);
            }
            else
                this.reset1_Click(null, EventArgs.Empty);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (pStart == pNull)
                return;
            if (e.Button == MouseButtons.Left)
            {
                try
                {
                    Graphics g = Graphics.FromImage(pictureBox1.Image);
                    g.DrawLine(pen, pCur, e.Location);
                    g.Dispose();
                    pictureBox1.Invalidate();
                    pCur = e.Location;
                    border.Add(pCur);
                }
                catch
                {
                    return;
                }                         
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isAdjacent(pStart, pCur))
            {
                mode = e.Button == MouseButtons.Left;
                this.getBorderBounds(border);
            }
            else
            {
                MessageBox.Show("Неверное выделена область. Попробуйте ещё раз.");
                this.reset1_Click(null, EventArgs.Empty);
            }
        }                      
    }
}
