using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;


using Emgu.CV.Structure;
//using emgucv_image;

namespace emgucv_image
{
    class emgucv_image
    {
        static void Main(string[] args)
        {



            ///control features/////////////////////////////////////////

            /////binary percentage///////

            Int32 biper = 20;

            /////cluster number///////

            int numCentroids = 2;

            /////retrieve threshold///

            double relimit = 0.96;

            /////error fix parameter when retrieve (lower the weight of the white part)////////

            double Fixpara =0.0005;

            ////K-Means cross out parameter///////////////////////////////////////////

            double ratiolimit = 0.90;

            ///Plot exmaple images/////////////////////////////////////////////


            Image<Bgr, Byte> frame = new Image<Bgr, Byte>("E:\\USYD\\MR\\cracks\\crack1.jpg");

            kmeans kmeansresults = new kmeans();

            Image<Gray, Byte> grayframeini = turnbinary(frame, biper);

            Image<Gray, Byte> grayframe = reducenoise(grayframeini);

            double[][] rawdata = turndatalist(grayframe);

            double[] avg = kmeansresults.Runkmeans(rawdata, numCentroids);

            double avgextent = avgdistanceextent(avg, grayframe);


            ////////show an example/////////////////////////////////////////////////////////
            //String win1 = "Test Window"; //The name of the window
            //CvInvoke.NamedWindow(win1); //Create the window using the specific name

            //Image<Bgr, Byte> frameex = new Image<Bgr, Byte>("E:\\USYD\\MR\\cracks\\crack1.jpg");

            //Image<Gray, Byte> grayimage = turngray(frameex, biper); 

            //Image<Gray, Byte> image = turnbinary(frameex, biper);

            //Image<Gray, Byte> BGRResult2 = reducenoise(image);

            //CvInvoke.Imshow(win1, grayimage); //Show the image

            //CvInvoke.WaitKey(0);

            //CvInvoke.DestroyWindow(win1); //Destory the window

            //image.Save("crack1.jpg");


            ///////////////////////////////////////////////////////////////////

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            int grayStep = 2;


            double[] histogram = colorHistogram(grayframe, grayStep, Fixpara);

            double all = countHistogram(histogram);

            double[] Phistogram = PHistogram(histogram, all);


            for (int i = 1; i < 26; i++)
            {

                string path = "E:\\USYD\\MR\\cracks\\crack" + i.ToString() + ".jpg";



                string name = "crack" + i.ToString() + ".jpg";


                Console.WriteLine("for " + name);

                Image<Bgr, Byte> framere = new Image<Bgr, Byte>(path);

                Image<Gray, Byte> grayframereini = turnbinary(framere, biper);


                Image<Gray, Byte> grayframere = reducenoise(grayframereini);



                double[][] rawdatare = turndatalist(grayframere);

                double[] avgre = kmeansresults.Runkmeans(rawdatare, numCentroids);

                double avgextentre = avgdistanceextent(avgre, grayframere);



                double[] histogramre = colorHistogram(grayframere, grayStep, Fixpara);
                double allre = countHistogram(histogramre);
                double[] Phistogramre = PHistogram(histogramre, allre);

                double BD = BDHistogram(Phistogram, Phistogramre);



                double ratio = avgextentre / avgextent;

                if (BD > relimit && ratio > ratiolimit)
                {
                    Console.WriteLine(name + "is a similar iamge and the similarity is "+BD.ToString()+".");

                }


            }





        }



        /////support functions///////////////////////////



        static double[] colorHistogram(Image<Gray, Byte> img, int grayStep, double Fixpara)
        {
            double[] histogram = new double[grayStep];

            byte[,,] data = img.Data;

            for (int i = 0; i < img.Rows; i++)
            {

                for (int j = 0; j < img.Cols; j++)
                {

                    int gray = data[i, j, 0];

                    histogram[gray / (256 / grayStep)]++;





                }

            }

            double[] fixedhistogram = new double[grayStep];
            fixedhistogram[0] = histogram[0];
            fixedhistogram[1] = Fixpara * histogram[1];

            return fixedhistogram;
        }

        static double countHistogram(double[] colorHistogram)
        {

            double all = 0;

            for (int k = 0; k < colorHistogram.GetLength(0); k++)
            {

                all = all + colorHistogram[k];

            }


            return all;
        }

        static double[] PHistogram(double[] colorHistogram, double all)
        {
            double[] Phistogram = new double[colorHistogram.GetLength(0)];

            for (int k = 0; k < colorHistogram.GetLength(0); k++)
            {

                Phistogram[k] = colorHistogram[k] / all;

            }

            return Phistogram;
        }

        static double BDHistogram(double[] PHistogram1, double[] PHistogram2)
        {

            double BD = 0;


            for (int k = 0; k < PHistogram1.GetLength(0); k++)
            {

                BD = BD + Math.Sqrt(PHistogram1[k] * PHistogram2[k]);

            }

            return BD;
        }




        static Image<Gray, Byte> turnbinary(Image<Bgr, Byte> frame, Int32 biper)
        {
            //Image<Bgr, Byte> frame = new Image<Bgr, Byte>("E:\\USYD\\MR\\crack\\crack1.jpg");

            byte[,,] data = frame.Data;

            Image<Gray, Byte> grayframe = new Image<Gray, Byte>(frame.Width, frame.Height);


            byte[,,] graydata = grayframe.Data;

            Byte max = 0;
            Byte min = 255;


            for (int i = 0; i < frame.Rows; i++)
            {
                for (int j = 0; j < frame.Cols; j++)
                {

                    graydata[i, j, 0] = (byte)((11 * data[i, j, 0] + 59 * data[i, j, 1] + 30 * data[i, j, 2]) / 100);

                    if (graydata[i, j, 0] > max)
                    {
                        max = graydata[i, j, 0];
                    }

                    if (graydata[i, j, 0] < min)
                    {
                        min = graydata[i, j, 0];
                    }

                }
            }




            //Console.WriteLine("The maximum value is " + max.ToString());
            //Console.WriteLine("The minimum value is " + min.ToString());

            Int32 Binarythreshold = (max - min) * biper / 100 + min;
            //Console.WriteLine("The threshold value is " + Binarythreshold.ToString());


            for (int i = 0; i < grayframe.Rows; i++)
            {
                for (int j = 0; j < grayframe.Cols; j++)
                {

                    if (graydata[i, j, 0] > Binarythreshold)
                    {
                        graydata[i, j, 0] = (byte)255;
                    }
                    else
                    {
                        graydata[i, j, 0] = (byte)0;
                    }


                }
            }

            return grayframe;




        }


        static Image<Gray, Byte> grayturnbinary(Image<Gray, Byte> frame, Int32 biper)
        {
            //Image<Bgr, Byte> frame = new Image<Bgr, Byte>("E:\\USYD\\MR\\crack\\crack1.jpg");

            byte[,,] graydata = frame.Data;


            Byte max = 0;
            Byte min = 255;


            for (int i = 0; i < frame.Rows; i++)
            {
                for (int j = 0; j < frame.Cols; j++)
                {

                    //graydata[i, j, 0] = (byte)((11 * data[i, j, 0] + 59 * data[i, j, 1] + 30 * data[i, j, 2]) / 100);

                    if (graydata[i, j, 0] > max)
                    {
                        max = graydata[i, j, 0];
                    }

                    if (graydata[i, j, 0] < min)
                    {
                        min = graydata[i, j, 0];
                    }

                }
            }


            //Console.WriteLine("The maximum value is " + max.ToString());
            //Console.WriteLine("The minimum value is " + min.ToString());

            Int32 Binarythreshold = (max - min) * biper / 100 + min;
            //Console.WriteLine("The threshold value is " + Binarythreshold.ToString());


            for (int i = 0; i < frame.Rows; i++)
            {
                for (int j = 0; j < frame.Cols; j++)
                {

                    if (graydata[i, j, 0] > Binarythreshold)
                    {
                        graydata[i, j, 0] = (byte)255;
                    }
                    else
                    {
                        graydata[i, j, 0] = (byte)0;
                    }


                }
            }

            return frame;




        }

        static Image<Gray, Byte> reducenoise (Image<Gray, Byte> image)
        {
            Int32 biperagain = 75;

            float mid = 0.7F;
            float margin = (1 - mid) / 8;

            float[,] matrixKernel = new float[3, 3] { {margin, margin, margin },
             { margin, mid, margin },
             { margin, margin, margin } };


            //float[,] matrixKernel = new float[7, 7] { { 0.02F, 0.02F, 0.02F, 0.02F, 0.02F, 0.02F, 0.02F},
            //                                         { 0.02F, 0.02F, 0.02F, 0.02F, 0.02F, 0.02F, 0.02F},
            //                                          { 0.02F, 0.02F, 0.02F, 0.02F, 0.02F, 0.02F, 0.02F},
            //                                         { 0.02F, 0.02F, 0.02F, 0.02F, 0.02F, 0.02F, 0.02F},
            //                                         { 0.02F, 0.02F, 0.02F, 0.02F, 0.02F, 0.02F, 0.02F},
            //                                        { 0.02F, 0.02F, 0.02F, 0.02F, 0.02F, 0.02F, 0.02F},
            //                                        { 0.02F, 0.02F, 0.02F, 0.02F, 0.02F, 0.02F, 0.02F} };

            //float[,] matrixKernel = new float[3, 3] {{ 0,-1, 0 },{-1, 5,-1 },{ 0,-1, 0 }};


            ConvolutionKernelF matrix = new ConvolutionKernelF(matrixKernel);
            //Image<Bgr, float> result = new Image<Bgr, float>(image.Width, image.Height);
            Image<Gray, float> result = new Image<Gray, float>(image.Width, image.Height);
            CvInvoke.Filter2D(image, result, matrix, new Point(0, 0));
            //Image<Bgr, Byte> BGRResult = result.Convert<Bgr, Byte>();
            //Image<Bgr, Byte> BGRResult = result.ConvertScale<byte>(1, 0);
            Image<Gray, Byte> BGRResult = result.ConvertScale<byte>(1, 0);


            Image<Gray, Byte> BGRResult2 = grayturnbinary(BGRResult, biperagain);

            return BGRResult2;
        }


        //static Image<Gray, Byte> reducenoise(Image<Gray, Byte> image)

        static long notwhitepointsnum (Image<Gray, Byte> image)
        {
            Int32 grayStep = 2;
            double Fixpara = 1;

            double[] histogram = colorHistogram(image, grayStep, Fixpara);

            double pointsnumD = histogram[0];

            long pointsnum = (long) pointsnumD;

            
            //pointsnum -= 1;
            //Console.WriteLine(pointsnum);

            return pointsnum;
        }



        static double[][] turndatalist(Image<Gray, Byte> image)
        {

            long pointsum = notwhitepointsnum(image);

            //Console.WriteLine("===================="+pointsum.ToString());

            byte[,,] graydata = image.Data;

            var rawData = new double[pointsum][];


            long Pnumber = 0; 

            for (int i = 0; i < image.Rows; i++)
            {
                for (int j = 0; j < image.Cols; j++)
                {

                    if (graydata[i, j, 0] == 0)
                    {
                        rawData[Pnumber]  = new[] { (double) i, (double) j };
                        //Console.WriteLine("==" + Pnumber.ToString());
                        Pnumber++;
                    }


                }
            }

            //Console.WriteLine("====================" + Pnumber.ToString());

            return rawData;

        }


        static double avgdistanceextent(double[] avg, Image<Gray, Byte> frame)
        {
            
            //double[] distanceratio = new double[avg.Length];

            double sum = 0;
            double count = 0;

            for (int i = 0; i < avg.Length; i++)
            {
                if (avg[i] != 0)
                {
                    sum += avg[i] / (double)(frame.Rows + frame.Cols);
                    count += 1;

                }

            }

            //Console.WriteLine(sum);
            //Console.WriteLine(count);
            double avgextent = sum / count;

            return avgextent;

        }


        static Image<Gray, Byte> turngray(Image<Bgr, Byte> frame, Int32 biper)
        {
            //Image<Bgr, Byte> frame = new Image<Bgr, Byte>("E:\\USYD\\MR\\crack\\crack1.jpg");

            byte[,,] data = frame.Data;

            Image<Gray, Byte> grayframe = new Image<Gray, Byte>(frame.Width, frame.Height);


            byte[,,] graydata = grayframe.Data;

            Byte max = 0;
            Byte min = 255;


            for (int i = 0; i < frame.Rows; i++)
            {
                for (int j = 0; j < frame.Cols; j++)
                {

                    graydata[i, j, 0] = (byte)((11 * data[i, j, 0] + 59 * data[i, j, 1] + 30 * data[i, j, 2]) / 100);


                }
            }

            return grayframe;




        }














    }
}

