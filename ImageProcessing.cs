using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace AOCI_1
{
    public class ImageProcessing
    {
        public Image<Bgr, byte> sourceImage;
        public VideoCapture capture;
        public event EventHandler<ImageEventArgs> ImageProcessed;
        int frameCounter = 0;

        public class ImageEventArgs : EventArgs
        {
            public IInputArray Image { get; set; }
        }

        public ImageProcessing(Image<Bgr, byte> image)
        {
            sourceImage = image;
        }
        public ImageProcessing(string filename)
        {
            VideoProcessing(filename);
        }

        public double cannyThreshold { get; set; } = 20.0;
        public double cannyThresholdLinking { get; set; } = 30.0;
        public int Color_1 { get; set; } = 50;
        public int Color_2 { get; set; } = 150;
        public int Color_3 { get; set; } = 200;
        public int brightness { get; set; }
        public int contrast { get; set; }
        public int HSVValue { get; set; }
        public int blur { get; set; }
        public int aq1 { get; set; }
        public double valAdd { get; set; }
        public double k { get; set; }
        public PointF[] pts { get; set; } = new PointF[4];
        public double shift { get; set; }
        public double angle { get; set; }
        public int qX { get; set; }
        public int qY { get; set; }

        public Image<Bgr, byte> Processing()
        {
            Image<Gray, byte> grayImage = sourceImage.Convert<Gray, byte>();

            var tempImage = grayImage.PyrDown();
            var destImage = tempImage.PyrUp();

            Image<Gray, byte> cannyEdges = destImage.Canny(cannyThreshold, cannyThresholdLinking);

            //cannyEdges._Dilate(1);

            var cannyEdgesBgr = cannyEdges.Convert<Bgr, byte>();
            var resultImage = sourceImage.Sub(cannyEdgesBgr);

            for (int channel = 0; channel < resultImage.NumberOfChannels; channel++)
                for (int x = 0; x < resultImage.Width; x++)
                    for (int y = 0; y < resultImage.Height; y++) // обход по пискелям
                    {
                        // получение цвета пикселя
                        byte color = resultImage.Data[y, x, channel];
                        if (color <= Color_1)
                            color = 0;
                        //else if (color <= 100)
                        //    color = 25;
                        else if (color <= Color_2)
                            color = 100;
                        else if (color <= Color_3)
                            color = 200;
                        else
                            color = 255;
                        resultImage.Data[y, x, channel] = color; // изменение цвета пикселя
                    }
            return resultImage;
        }

        public Image<Bgr, byte> RGB_Split(int chan, Image<Bgr, byte> copyImage)
        {
            var channel = copyImage.Split()[1];

            Image<Bgr, byte> destImage = copyImage.CopyBlank();

            VectorOfMat vm = new VectorOfMat();

            if (chan == 0)
            {
                vm.Push(channel.CopyBlank());
                vm.Push(channel.CopyBlank());
                vm.Push(channel);
            }
            else if (chan == 1)
            {
                vm.Push(channel.CopyBlank());
                vm.Push(channel);
                vm.Push(channel.CopyBlank());
            }
            else if (chan == 2)
            {
                vm.Push(channel);
                vm.Push(channel.CopyBlank());
                vm.Push(channel.CopyBlank());
            }

            CvInvoke.Merge(vm, destImage);

            return destImage;
        }

        public Image<Gray, byte> grayFilter(Image<Bgr, byte> copyImage)
        {
            var grayImage = new Image<Gray, byte>(copyImage.Size);

            for (int y = 0; y < grayImage.Height; y++)
            {
                for (int x = 0; x < grayImage.Width; x++)
                {
                    grayImage.Data[y, x, 0] = Convert.ToByte(0.299 * copyImage.Data[y, x, 2] + 0.587 * copyImage.Data[y, x, 1] + 0.114 * copyImage.Data[y, x, 0]);
                }
            }

            return grayImage;
        }

        public Image<Bgr, byte> sepiaFilter(Image<Bgr, byte> copyImage)
        {
            for (int y = 0; y < copyImage.Height; y++)
                for (int x = 0; x < copyImage.Width; x++)
                {
                    if ((0.393 * copyImage.Data[y, x, 2] + 0.769 * copyImage.Data[y, x, 1] + 0.189 * copyImage.Data[y, x, 0]) > 255)
                    {
                        copyImage.Data[y, x, 2] = 255;
                    }
                    else
                    {
                        copyImage.Data[y, x, 2] = Convert.ToByte(0.393 * copyImage.Data[y, x, 2] + 0.769 * copyImage.Data[y, x, 1] + 0.189 * copyImage.Data[y, x, 0]);
                    }
                    if ((0.349 * copyImage.Data[y, x, 2] + 0.686 * copyImage.Data[y, x, 1] + 0.168 * copyImage.Data[y, x, 0]) > 255)
                    {
                        copyImage.Data[y, x, 1] = 255;
                    }
                    else
                    {
                        copyImage.Data[y, x, 1] = Convert.ToByte(0.349 * copyImage.Data[y, x, 2] + 0.686 * sourceImage.Data[y, x, 1] + 0.168 * copyImage.Data[y, x, 0]);
                    }
                    if ((0.272 * copyImage.Data[y, x, 2] + 0.534 * copyImage.Data[y, x, 1] + 0.131 * copyImage.Data[y, x, 0]) > 255)
                    {
                        copyImage.Data[y, x, 0] = 255;
                    }
                    else
                    {
                        copyImage.Data[y, x, 0] = Convert.ToByte(0.272 * copyImage.Data[y, x, 2] + 0.534 * copyImage.Data[y, x, 1] + 0.131 * copyImage.Data[y, x, 0]);
                    }
                }
            return copyImage;
        }

        public Image<Bgr, byte> brightnessChange(Image<Bgr, byte> copyImage)
        {
            for (int ch = 0; ch < 3; ch++)
                for (int y = 0; y < copyImage.Height; y++)
                    for (int x = 0; x < copyImage.Width; x++)
                    {
                        double color = copyImage.Data[y, x, ch] + brightness;
                        if (brightness < 0)
                        {
                            if (color < 0)
                                copyImage.Data[y, x, ch] = 0;
                            else
                                copyImage.Data[y, x, ch] = Convert.ToByte(color);
                        }
                        else
                        {
                            if ((color) > 255)
                                copyImage.Data[y, x, ch] = 255;
                            else
                                copyImage.Data[y, x, ch] = Convert.ToByte(color);
                        }
                    }

            return copyImage;
        }

        public Image<Bgr, byte> contrastChange(Image<Bgr, byte> copyImage)
        {
            for (int ch = 0; ch < 3; ch++)
                for (int y = 0; y < copyImage.Height; y++)
                    for (int x = 0; x < copyImage.Width; x++)
                    {
                        double color = copyImage.Data[y, x, ch] * contrast * 0.1;
                        if (color > 255)
                            copyImage.Data[y, x, ch] = 255;
                        else if (color < 0)
                            copyImage.Data[y, x, ch] = 0;
                        else
                            copyImage.Data[y, x, ch] = Convert.ToByte(color);
                    }

            return copyImage;
        }

        public Image<Hsv, byte> HSV(Image<Hsv, byte> copyImage)
        {
            for (int y = 0; y < copyImage.Height; y++)
                for (int x = 0; x < copyImage.Width; x++)
                {
                    copyImage.Data[y, x, 0] = (byte)HSVValue;
                }

            return copyImage;
        }

        public Image<Bgr, byte> Blur(Image<Bgr, byte> copyImage, Image<Bgr, byte> blurImg)
        {
            List<byte> l = new List<byte>();
            for (int ch = 0; ch < 3; ch++)
                for (int y = 0; y < blurImg.Height; y++)
                    for (int x = 0; x < blurImg.Width; x++)
                    {
                        l.Clear();
                        for (int i = -1; i < 2; i++)
                            for (int j = -1; j < 2; j++)
                            {
                                if (i + y > 0 && j + x > 0 && j + x < blurImg.Width && i + y < blurImg.Height)
                                {
                                    l.Add(blurImg.Data[i + y, j + x, ch]);
                                }

                            }
                        l.Sort();
                        copyImage.Data[y, x, ch] = l[l.Count / 2];
                    }

            return copyImage;
        }

        public Image<Bgr, byte> Addition(Image<Bgr, byte> copyImage, Image<Bgr, byte> addImg)
        {
            for (int ch = 0; ch < 3; ch++)
                for (int y = 0; y < copyImage.Height; y++)
                    for (int x = 0; x < copyImage.Width; x++)
                    {
                        double val = valAdd * copyImage.Data[y, x, ch] + (valAdd - 1) * addImg.Data[y, x, ch];
                        if (val > 255)
                        {
                            copyImage.Data[y, x, ch] = 255;
                            addImg.Data[y, x, ch] = 255;
                        }
                        else
                        if (val < 50)
                        {
                            copyImage.Data[y, x, ch] = 0;
                            addImg.Data[y, x, ch] = 0;
                        }
                        else
                        {
                            copyImage.Data[y, x, ch] = Convert.ToByte(val);
                        }
                    }

            return copyImage;
        }

        public Image<Bgr, byte> Exclusion(Image<Bgr, byte> copyImage, Image<Bgr, byte> exImg)
        {
            for (int ch = 0; ch < 3; ch++)
                for (int y = 0; y < copyImage.Height; y++)
                    for (int x = 0; x < copyImage.Width; x++)
                    {
                        double val = copyImage.Data[y, x, ch] - exImg.Data[y, x, ch];
                        if (val > 255)
                        {
                            copyImage.Data[y, x, ch] = 255;
                            exImg.Data[y, x, ch] = 255;
                        }
                        else
                        if (val < 0)
                        {
                            copyImage.Data[y, x, ch] = 0;
                            exImg.Data[y, x, ch] = 0;
                        }
                        else
                        {
                            copyImage.Data[y, x, ch] = Convert.ToByte(val);
                        }
                    }

            return copyImage;
        }

        public Image<Bgr, byte> Intersection(Image<Bgr, byte> copyImage, Image<Bgr, byte> intImg)
        {
            for (int ch = 0; ch < 3; ch++)
                for (int y = 0; y < intImg.Height; y++)
                    for (int x = 0; x < intImg.Width; x++)
                    {
                        if (intImg.Data[y, x, ch] > 50)
                        {
                            copyImage.Data[y, x, ch] = copyImage.Data[y, x, ch];
                        }
                        else
                        if (intImg.Data[y, x, ch] <= 50)
                        {
                            copyImage.Data[y, x, ch] = 0;
                            intImg.Data[y, x, ch] = 0;
                        }
                    }

            return copyImage;
        }

        public void Window(Image<Bgr, byte> copyImage, Image<Bgr, byte> winImg, int[,] v)
        {
            int sh = 1;
            for (int ch = 0; ch < 3; ch++)
                for (int y = sh; y < copyImage.Height - sh; y++)
                    for (int x = sh; x < copyImage.Width - sh; x++)
                    {
                        if (x == 0 | y == 0 | x == copyImage.Width - 1 | y == copyImage.Height - 1)
                        {
                            if (v[1, 1] == 6)
                            {

                            }
                            if (v[1, 1] == 9)
                            {

                            }
                        }
                        int r = 0;
                        for (int i = -1; i < 2; i++)
                            for (int j = -1; j < 2; j++)
                            {
                                r += copyImage.Data[i + y, j + x, ch] * v[i + 1, j + 1];
                            }
                        if (r > 255) r = 255;
                        if (r < 0) r = 0;
                        winImg.Data[y, x, ch] = (byte)r;
                    }
        }

        public Image<Bgr, byte> WinAll(Image<Bgr, byte> copyImage, Image<Bgr, byte> winImg, params int[] v)
        {
            int[,] w;
            w = new int[3, 3]
            {
                {v[0],  v[1], v[2]},
                {v[3], v[4], v[5]},
                {v[6],  v[7], v[8]}
            };

            Window(copyImage, winImg, w);

            return winImg;
        }

        public Image<Bgr, byte> Aqua(Image<Bgr, byte> copyImage, Image<Bgr, byte> aqImg, Image<Bgr, byte> aquaImg)
        {
            valAdd = 0.8;

            aqImg = Addition(copyImage, aqImg);
            var blurImg = copyImage.CopyBlank();

            blur = 3;

            aquaImg = Blur(blurImg, copyImage);

            for (int ch = 0; ch < 3; ch++)
                for (int y = 0; y < aquaImg.Height; y++)
                    for (int x = 0; x < aquaImg.Width; x++)
                    {
                        double color = aquaImg.Data[y, x, ch] + aq1;
                        if (brightness < 0)
                        {
                            if (color < 0)
                                aquaImg.Data[y, x, ch] = 0;
                            else
                                aquaImg.Data[y, x, ch] = Convert.ToByte(color);
                        }
                        else
                        {
                            if ((color) > 255)
                                aquaImg.Data[y, x, ch] = 255;
                            else
                                aquaImg.Data[y, x, ch] = Convert.ToByte(color);
                        }
                    }

            return aquaImg;
        }

        public Image<Bgr, byte> Cartoon(Image<Gray, byte> grayImage, Image<Bgr, byte> cartoonImage, Image<Bgr, byte> copyImage)
        {
            var blurImg = copyImage.CopyBlank();

            blur = 3;

            for (int y = 0; y < grayImage.Height; y++)
            {
                for (int x = 0; x < grayImage.Width; x++)
                {
                    grayImage.Data[y, x, 0] = Convert.ToByte(0.299 * sourceImage.Data[y, x, 2] + 0.587 * copyImage.Data[y, x, 1] + 0.114 * copyImage.Data[y, x, 0]);
                }
            }
            List<byte> l = new List<byte>();
            int sh = 1;
            int n = 3;
            var resultImage = grayImage.CopyBlank();

            for (int y = sh; y < grayImage.Height - sh; y++)
                for (int x = sh; x < grayImage.Width - sh; x++)
                {
                    l.Clear();
                    for (int i = -1; i < 2; i++)
                        for (int j = -1; j < 2; j++)
                        {
                            l.Add(grayImage.Data[i + y, j + x, 0]);
                        }
                    l.Sort();
                    resultImage.Data[y, x, 0] = l[n / 2];
                }



            resultImage = resultImage.ThresholdAdaptive(new Gray(100), AdaptiveThresholdType.MeanC, ThresholdType.Binary, 3, new Gray(0.99));

            for (int ch = 0; ch < 3; ch++)
                for (int y = 1; y < cartoonImage.Height; y++)
                    for (int x = 1; x < cartoonImage.Width; x++)
                    {
                        if (resultImage.Data[y, x, 0] > 50)
                        {
                            cartoonImage.Data[y, x, ch] = cartoonImage.Data[y, x, ch];
                        }
                        else
                        if (resultImage.Data[y, x, 0] <= 50)
                        {
                            cartoonImage.Data[y, x, ch] = resultImage.Data[y, x, 0];
                        }
                    }

            return cartoonImage;
        }

        //3 лаба

        public Image<Bgr, byte> Scale(Image<Bgr, byte> copyImage)
        {
            Image<Bgr, byte> scaledImg = new Image<Bgr, byte>((int)(copyImage.Width * k), (int)(copyImage.Height * k));
            for (int i = 0; i < copyImage.Width - 1; i++)
                for (int j = 0; j < copyImage.Height - 1; j++)
                {
                    double I = (i / k);
                    double J = (j / k);

                    double baseI = Math.Floor(I);
                    double baseJ = Math.Floor(J);

                    if (baseI >= copyImage.Width - 1) continue;
                    if (baseJ >= copyImage.Height - 1) continue;

                    double rI = I - baseI;
                    double rJ = J - baseJ;

                    double irI = 1 - rI;
                    double irJ = 1 - rJ;

                    Bgr c1 = new Bgr();
                    c1.Blue = copyImage.Data[(int)baseJ, (int)baseI, 0] * irI + copyImage.Data[(int)baseJ, (int)(baseI + 1), 0] * rI;
                    c1.Green = copyImage.Data[(int)baseJ, (int)baseI, 1] * irI + copyImage.Data[(int)baseJ, (int)(baseI + 1), 1] * rI;
                    c1.Red = copyImage.Data[(int)baseJ, (int)baseI, 2] * irI + copyImage.Data[(int)baseJ, (int)(baseI + 1), 2] * rI;

                    Bgr c2 = new Bgr();
                    c2.Blue = copyImage.Data[(int)(baseJ + 1), (int)baseI, 0] * irI + copyImage.Data[(int)(baseJ + 1), (int)(baseI + 1), 0] * rI;
                    c2.Green = copyImage.Data[(int)(baseJ + 1), (int)baseI, 1] * irI + copyImage.Data[(int)(baseJ + 1), (int)(baseI + 1), 1] * rI;
                    c2.Red = copyImage.Data[(int)(baseJ + 1), (int)baseI, 2] * irI + copyImage.Data[(int)(baseJ + 1), (int)(baseI + 1), 2] * rI;

                    Bgr c = new Bgr();
                    c.Blue = c1.Blue * irJ + c2.Blue * rJ;
                    c.Green = c1.Green * irJ + c2.Green * rJ;
                    c.Red = c1.Red * irJ + c2.Red * rJ;

                    scaledImg[j, i] = c;
                }
            return scaledImg;
        }

        public Image<Bgr, byte> Homography(Image<Bgr, byte> copyImage)
        {
            var destPoints = new PointF[]
            {
                new PointF(0, 0),
                new PointF(0, copyImage.Height - 1),
                new PointF(copyImage.Width - 1, copyImage.Height - 1),
                new PointF(copyImage.Width - 1, 0)
            };

            var homographyMatrix = CvInvoke.GetPerspectiveTransform(pts, destPoints);
            var destImage = new Image<Bgr, byte>(copyImage.Size);
            CvInvoke.WarpPerspective(copyImage, destImage, homographyMatrix, destImage.Size);

            return destImage;
        }

        public Image<Bgr, byte> Shearing(Image<Bgr, byte> copyImage)
        {
            Image<Bgr, byte> shearingImg = new Image<Bgr, byte>((int)(copyImage.Width + copyImage.Width * shift) + 1, (int)(copyImage.Height));
            for (int i = 0; i < copyImage.Width - 1; i++)
                for (int j = 0; j < copyImage.Height - 1; j++)
                {
                    int newX = (int)(i + shift * (copyImage.Height - j));
                    int newY = (int)(j);
                    shearingImg[newY, newX] = sourceImage[j, i];
                }

            return shearingImg;
        }
        public Image<Bgr, byte> Rotation(Image<Bgr, byte> copyImage)
        {
            double rad = angle * (Math.PI / 180.0);
            Image<Bgr, byte> rotationImg = new Image<Bgr, byte>((int)(copyImage.Width), (int)(copyImage.Height));
            for (int i = 0; i < copyImage.Width - 1; i++)
                for (int j = 0; j < copyImage.Height - 1; j++)
                {
                    int newX = (int)(Math.Cos(rad) * (i - copyImage.Width / 2) - Math.Sin(rad) * (j - copyImage.Height / 2) + copyImage.Width / 2);
                    int newY = (int)(Math.Sin(rad) * (i - copyImage.Width / 2) + Math.Cos(rad) * (j - copyImage.Height / 2) + copyImage.Height / 2);

                    if (newY > 0 && newX > 0 && newX < copyImage.Width && newY < copyImage.Height)
                    {
                        double baseI = Math.Floor((double)newX);
                        double baseJ = Math.Floor((double)newY);

                        if (baseI >= copyImage.Width - 1) continue;
                        if (baseJ >= copyImage.Height - 1) continue;

                        double rI = newX - baseI;
                        double rJ = newY - baseJ;

                        double irI = 1 - rI;
                        double irJ = 1 - rJ;

                        //if (newY > 0 && newX > 0 && newX < copyImage.Width && newY < copyImage.Height)
                        //{
                        //    //rotationImg[newY, newX] = sourceImage[j, i];
                        //    //rotationImg[newY, newX] = (sourceImage.Data[(int)i, (int)j, 0] * irI + sourceImage.Data[(int)i + 1, (int)j, 0] * rI) * irJ + (sourceImage.Data[(int)i, (int)j + 1, 0] * irI) * irJ;
                        //}
                        Bgr c = new Bgr();
                        Bgr c1 = new Bgr();
                        Bgr c2 = new Bgr();

                        c1.Blue = sourceImage.Data[(int)baseJ, (int)baseI, 0] * irI + sourceImage.Data[(int)baseJ, (int)baseI + 1, 0] * rI;
                        c1.Green = sourceImage.Data[(int)baseJ, (int)baseI, 1] * irI + sourceImage.Data[(int)baseJ, (int)baseI + 1, 1] * rI;
                        c1.Red = sourceImage.Data[(int)baseJ, (int)baseI, 2] * irI + sourceImage.Data[(int)baseJ, (int)baseI + 1, 2] * rI;


                        c2.Blue = sourceImage.Data[(int)baseJ + 1, (int)baseI, 0] * irI + sourceImage.Data[(int)baseJ + 1, (int)baseI + 1, 0] * rI;
                        c2.Green = sourceImage.Data[(int)baseJ + 1, (int)baseI, 1] * irI + sourceImage.Data[(int)baseJ + 1, (int)baseI + 1, 1] * rI;
                        c2.Red = sourceImage.Data[(int)baseJ + 1, (int)baseI, 2] * irI + sourceImage.Data[(int)baseJ + 1, (int)baseI + 1, 2] * rI;

                        c.Blue = c1.Blue * irJ + c2.Blue * rJ;
                        c.Green = c1.Green * irJ + c2.Green * rJ;
                        c.Red = c1.Red * irJ + c2.Red * rJ;

                        rotationImg[j, i] = c;
                    }
                }

            return rotationImg;
        }

        public Image<Bgr, byte> Reflection(Image<Bgr, byte> copyImage)
        {
            Image<Bgr, byte> reflectionImg = new Image<Bgr, byte>((int)(copyImage.Width + copyImage.Width), (int)(copyImage.Height + copyImage.Height));
            for (int i = 0; i < copyImage.Width - 1; i++)
                for (int j = 0; j < copyImage.Height - 1; j++)
                {
                    int newX = (int)(i * qX + copyImage.Width);
                    int newY = (int)(j * qY + copyImage.Height);
                    reflectionImg[newY, newX] = sourceImage[j, i];
                }

            return reflectionImg;
        }

        public Image<Bgr, byte> triangleСontours(Image<Gray, byte> copyImage)
        {
            int kernelSize = 5; // радиус размытия
            int c = 0;
            var bluredImage = copyImage.SmoothGaussian(kernelSize);

            var threshold = new Gray(80); // пороговое значение
            var color = new Gray(255); // этим цветом будут закрашены пиксели, имеющие значение > threshold
            var binarizedImage = bluredImage.ThresholdBinary(threshold, color);

            var contours = new VectorOfVectorOfPoint(); // контейнер для хранения контуров
            
            CvInvoke.FindContours(
                binarizedImage, // исходное чёрно-белое изображение
                contours, // найденные контуры
                null, // объект для хранения иерархии контуров (в данном случае не используется)
                RetrType.List, // структура возвращаемых данных (в данном случае список)
                ChainApproxMethod.ChainApproxSimple);

            var contoursImage = sourceImage.Copy(); //создание "пустой" копии исходного изображения
            //for (int i = 0; i < contours.Size; i++)
            //{
            //    var points = contours[i].ToArray();
            //    contoursImage.Draw(points, new Bgr(Color.GreenYellow), 2); // отрисовка точек
            //}

            for (int i = 0; i < contours.Size; i++)
            {
                var approxContour = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(
                contours[i], // исходный контур
                approxContour, // контур после аппроксимации
                CvInvoke.ArcLength(contours[i], true) * 0.05, // точность аппроксимации, прямо
                                                              //пропорциональная площади контура
                true); // контур становится закрытым (первая и последняя точки соединяются)
                if (approxContour.Size == 3) // если контур содержит 3 точки, то рисуется треугольник
                {
                    var points = approxContour.ToArray();
                    contoursImage.Draw(new Triangle2DF(points[0], points[1], points[2]),
                    new Bgr(Color.GreenYellow), 2);
                }
                if (CvInvoke.ContourArea(approxContour, false) > 256)
                {

                    if (approxContour.Size == 3) // если контур содержит 3 точки, то рисуется треугольник
                    {
                        var points = approxContour.ToArray();
                        contoursImage.Draw(new Triangle2DF(points[0], points[1], points[2]),
                        new Bgr(Color.GreenYellow), 2);
                        c++;
                        contoursImage = contoursImage.Resize(640, 480, Inter.Linear);
                        //obj.Add(points);
                        //listBox2.Items.Add(c);
                    }
                }
            }


            return contoursImage;
        }
        public bool squareСontoursS(Point[] points)
        {
            int delta = 10; // максимальное отклонение от прямого угла
            LineSegment2D[] edges = PointCollection.PolyLine(points, true);
            for (int i = 0; i < edges.Length; i++) // обход всех ребер контура
            {
                double angle = Math.Abs(edges[(i + 1) %
                edges.Length].GetExteriorAngleDegree(edges[i]));
                if (angle < 90 - delta || angle > 90 + delta) // если угол непрямой
                {
                    return false;
                }
            }
            return true;
        }
        public Image<Bgr, byte> squareСontours(Image<Gray, byte> copyImage)
        {
            int kernelSize = 5; // радиус размытия
            int c = 0;
            var bluredImage = copyImage.SmoothGaussian(kernelSize);

            var threshold = new Gray(120); // пороговое значение
            var color = new Gray(255); // этим цветом будут закрашены пиксели, имеющие значение > threshold
            var binarizedImage = bluredImage.ThresholdBinary(threshold, color);

            var contours = new VectorOfVectorOfPoint(); // контейнер для хранения контуров

            CvInvoke.FindContours(
                binarizedImage, // исходное чёрно-белое изображение
                contours, // найденные контуры
                null, // объект для хранения иерархии контуров (в данном случае не используется)
                RetrType.List, // структура возвращаемых данных (в данном случае список)
                ChainApproxMethod.ChainApproxSimple);

            var contoursImage = sourceImage.Copy(); //создание "пустой" копии исходного изображения

            for (int i = 0; i < contours.Size; i++)
            {
                var approxContour = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(
                contours[i], // исходный контур
                approxContour, // контур после аппроксимации
                CvInvoke.ArcLength(contours[i], true) * 0.05, // точность аппроксимации, прямо
                                                              //пропорциональная площади контура
                true); // контур становится закрытым (первая и последняя точки соединяются)
                if (approxContour.Size == 4) // если контур содержит 3 точки, то рисуется треугольник
                {
                    var points = approxContour.ToArray();
                    contoursImage.Draw(CvInvoke.MinAreaRect(approxContour), new Bgr(Color.GreenYellow), 2);
                }
                if (CvInvoke.ContourArea(approxContour, false) > 99)
                {

                    if (approxContour.Size == 4) // если контур содержит 3 точки, то рисуется треугольник
                    {
                        var points = approxContour.ToArray();
                        contoursImage.Draw(CvInvoke.MinAreaRect(approxContour), new Bgr(Color.GreenYellow), 2);
                        c++;
                        contoursImage = contoursImage.Resize(640, 480, Inter.Linear);
                        //obj.Add(points);
                        //listBox2.Items.Add(c);
                    }
                }
            }

            return contoursImage;
        }
        public Image<Bgr, byte> roundСontours(Image<Gray, byte> copyImage)
        {
            var grayImage = sourceImage.Convert<Gray, byte>();
            var bluredImage = grayImage.SmoothGaussian(9);

            List<CircleF> circles = new List<CircleF>(CvInvoke.HoughCircles(bluredImage,
                 HoughModes.Gradient,
                 1.0,
                 250,
                 100,
                 36,
                 2,
                 500));

            var resultImage = sourceImage.Copy();

            foreach (CircleF circle in circles) resultImage.Draw(circle, new Bgr(Color.Blue), 2);

            return resultImage;
        }
        //public Image<Bgr, byte> hsvСontours(Image<Gray, byte> copyImage)
        //{
        //    var hsvImage = sourceImage.Convert<Hsv, byte>(); // конвертация в HSV
        //    var hueChannel = hsvImage.Split()[0]; // выделение канала Hue
        //    byte color = 30; // соответствует желтому тону в Emgu.CV
        //    byte rangeDelta = 10; // величина разброса цвета
        //    var resultImage = hueChannel.InRange(new Gray(color - rangeDelta), new Gray(color +
        //    rangeDelta));

        //    return resultImage;
        //}
        public void StartVideoFromCam()
        {
            capture = new VideoCapture();
            capture.ImageGrabbed += ProcessFrame;
            capture.Start();
        }

        public Image<Bgr, byte> timerVideo()
        {
            var frame = capture.QueryFrame();

            sourceImage = frame.ToImage<Bgr, byte>(); //обрабатываемое изображение из функции Processing приравниваем к фрейму
            var videoImage = Processing(); //на финальное изображение накладываем фильтр, вызывая функцию

            frameCounter++;
            return videoImage;
        }

        //public Image

        public void VideoProcessing(string fileName)
        {
            capture = new VideoCapture(fileName); //берем кадры из видео
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            try
            {
                var frame = new Mat();
                capture.Retrieve(frame);
                sourceImage = frame.ToImage<Bgr, byte>();
                var result = Processing();
                ImageProcessed?.Invoke(this, new ImageEventArgs { Image = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                capture.ImageGrabbed -= ProcessFrame;
                capture.Stop();
            }
        }
    }
}
