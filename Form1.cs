using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AOCI_1;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace AOCI_2
{
    public partial class Form1 : Form
    {
        private Image<Bgr, byte> sourceImage;
        private ImageProcessing resultImage;
        private Image<Bgr, byte> boolImage;
        int c = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файлы изображений (*.jpg,  *.jpeg,  *.jpe,  *.jfif,  *.png)  |  *.jpg;  *.jpeg;  *.jpe;  *.jfif; *.png";
            var result = openFileDialog.ShowDialog(); // открытие диалога выбора файла
            if (result == DialogResult.OK) // открытие выбранного файла
            {
                string fileName = openFileDialog.FileName;
                sourceImage = new Image<Bgr, byte>(fileName).Resize(640, 480, Inter.Linear);
                resultImage = new ImageProcessing(sourceImage);

                imageBox1.Image = sourceImage;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var copyImage = sourceImage.Copy();
            imageBox2.Image = resultImage.RGB_Split(0, copyImage).Resize(640, 480, Inter.Linear);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var copyImage = sourceImage.Copy();
            imageBox2.Image = resultImage.RGB_Split(1, copyImage).Resize(640, 480, Inter.Linear);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var copyImage = sourceImage.Copy();
            imageBox2.Image = resultImage.RGB_Split(2, copyImage).Resize(640, 480, Inter.Linear);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var copyImage = sourceImage.Copy();
            imageBox2.Image = resultImage.grayFilter(copyImage);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var copyImage = sourceImage.Copy();
            imageBox2.Image = resultImage.sepiaFilter(copyImage);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            var copyImage = sourceImage.Copy();
            resultImage.brightness = trackBar1.Value;
            imageBox2.Image = resultImage.brightnessChange(copyImage);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            var copyImage = sourceImage.Copy();
            resultImage.contrast = trackBar2.Value;
            imageBox2.Image = resultImage.contrastChange(copyImage);
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            var copyImage = sourceImage.Convert<Hsv, byte>();
            resultImage.HSVValue = trackBar3.Value;
            imageBox2.Image = resultImage.HSV(copyImage);
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            var blurImg = sourceImage.Copy();
            var copyImage = blurImg.CopyBlank();
            resultImage.blur = trackBar4.Value;
            imageBox2.Image = resultImage.Blur(copyImage, blurImg);
        }

        private void ChooseBoolImg()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файлы изображений (*.jpg,  *.jpeg,  *.jpe,  *.jfif,  *.png)  |  *.jpg;  *.jpeg;  *.jpe;  *.jfif; *.png";
            var result = openFileDialog.ShowDialog(); // открытие диалога выбора файла
            if (result == DialogResult.OK) // открытие выбранного файла
            {
                string fileName = openFileDialog.FileName;
                boolImage = new Image<Bgr, byte>(fileName).Resize(640, 480, Inter.Linear);
                //resultImage = new ImageProcessing(sourceImage);

                //imageBox1.Image = boolImage;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var copyImage = sourceImage.Copy();
            ChooseBoolImg();
            resultImage.valAdd = trackBar7.Value * 0.1;
            imageBox2.Image = resultImage.Addition(copyImage, boolImage);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            var copyImage = sourceImage.Copy();
            ChooseBoolImg();
            imageBox2.Image = resultImage.Exclusion(copyImage, boolImage);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            var copyImage = sourceImage.Copy();
            ChooseBoolImg();
            imageBox2.Image = resultImage.Intersection(copyImage, boolImage);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            var winImg = sourceImage.Copy();
            var copyImage = winImg.CopyBlank();

            int i1 = Convert.ToInt32(textBox1.Text);
            int i2 = Convert.ToInt32(textBox2.Text);
            int i3 = Convert.ToInt32(textBox3.Text);
            int i4 = Convert.ToInt32(textBox4.Text);
            int i5 = Convert.ToInt32(textBox5.Text);
            int i6 = Convert.ToInt32(textBox6.Text);
            int i7 = Convert.ToInt32(textBox7.Text);
            int i8 = Convert.ToInt32(textBox8.Text);
            int i9 = Convert.ToInt32(textBox9.Text);

            imageBox2.Image = resultImage.WinAll(winImg, copyImage, i1, i2, i3, i4, i5, i6, i7, i8, i9);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            ChooseBoolImg();
            resultImage.aq1 = trackBar5.Value;
            var aqImg = sourceImage.Copy();
            var copyImage = aqImg.CopyBlank();
            imageBox2.Image = resultImage.Aqua(aqImg, boolImage, copyImage);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            var carImg = sourceImage.Copy();
            var copyImage = new Image<Gray, byte>(sourceImage.Size);
            imageBox2.Image = resultImage.Cartoon(copyImage, carImg, sourceImage);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            var copyImage = sourceImage.Copy();
            resultImage.k = double.Parse(textBox10.Text);
            imageBox2.Image = resultImage.Scale(copyImage);
        }

        private void imageBox1_MouseClick(object sender, MouseEventArgs e)
        {
            var copyImage = sourceImage.Copy();

            int x = (int)(e.Location.X / imageBox1.ZoomScale);
            int y = (int)(e.Location.Y / imageBox1.ZoomScale);

            resultImage.pts[c] = new Point(x, y);
            c++;
            if (c >= 4)
                c = 0;

            //Point center = new Point(x, y);
            int radius = 2;
            int thickness = 2;
            var color = new Bgr(Color.Blue).MCvScalar;

            for (int i = 0; i < 4; i++)
                CvInvoke.Circle(copyImage, new Point((int)resultImage.pts[i].X, (int)resultImage.pts[i].Y), radius, color, thickness);

            imageBox1.Image = copyImage;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            var copyImage = sourceImage.Copy();
            imageBox2.Image = resultImage.Homography(copyImage);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            var copyImage = sourceImage.Copy();
            resultImage.shift = double.Parse(textBox11.Text);
            imageBox2.Image = resultImage.Shearing(copyImage);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            var copyImage = sourceImage.Copy();
            resultImage.angle = double.Parse(textBox12.Text);
            imageBox2.Image = resultImage.Rotation(copyImage);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            var copyImage = sourceImage.Copy();
            resultImage.qX = int.Parse(textBox13.Text);
            resultImage.qY = int.Parse(textBox14.Text);
            imageBox2.Image = resultImage.Reflection(copyImage);
        }
    }
}
