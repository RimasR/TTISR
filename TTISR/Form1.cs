using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace TTISR
{
    public partial class Form1 : Form
    {
        private VideoCapture _videoCapture = null;
        private BallDetector detector = new BallDetector();
        double dp = 3;
        double minDist = 200.0;
        double param1 = 1.0;
        double param2 = 30.0;
        double minRadius = 0;
        double maxRadius = 0;
        double min = 90;
        double max = 120;
        double sat = 10;
        double min1 = 9;
        double max1 = 14;
        double sat1 = 10;

        public Form1()
        {
            InitializeComponent();
            trackBar1.Value = (int)dp;
            trackBar2.Value = (int)minDist;
            trackBar3.Value = (int)param1;
            trackBar4.Value = (int)param2;
            trackBar5.Value = (int)minRadius;
            trackBar6.Value = (int)maxRadius;
            trackBar7.Value = (int)min;
            trackBar8.Value = (int)max;
            trackBar9.Value = (int)sat;
            trackBar10.Value = (int)min1;
            trackBar11.Value = (int)max1;
            trackBar12.Value = (int)sat1;
        }

        private void DetectIllegalServing(Mat image)
        {
            Mat final = image.Clone();
            Mat contours = GetBallContours(image);

            Mat handContours = GetHandContours(image);
            Image<Bgr, byte> hand = GetPalm(handContours);
            CircleF[] circles = CvInvoke.HoughCircles(contours, HoughType.Gradient, dp, minDist, param1, param2);
            if(circles.Length > 0)
                foreach (var circle in circles)
                {
                CvInvoke.Circle(final, Point.Round(circle.Center), (int)circle.Radius, new Bgr(Color.Red).MCvScalar, 3);
                }

            label7.BeginInvoke(new MethodInvoker(delegate { label7.Text = $"Circle count: {circles.Length}"; }));

            pictureBox2.Image = contours.Bitmap;
            pictureBox3.Image = hand.Bitmap;
            pictureBox4.Image = final.Bitmap;
        }

        public Mat GetBallContours(Mat image)
        {
            var copy = new Mat();
            CvInvoke.GaussianBlur(image, copy, new Size(5, 5), 1.5, 1.5);
            var mask = new Mat();
            bool useUMat;
            using (InputOutputArray ia = mask.GetInputOutputArray())
                useUMat = ia.IsUMat;

            using (IImage hsv = useUMat ? (IImage)new UMat() : (IImage)new Mat())
            using (IImage s = useUMat ? (IImage)new UMat() : (IImage)new Mat())
            {
                CvInvoke.CvtColor(copy, hsv, ColorConversion.Bgr2Hsv);
                CvInvoke.ExtractChannel(hsv, mask, 0);
                CvInvoke.ExtractChannel(hsv, s, 1);

                using (ScalarArray lower = new ScalarArray(min))
                using (ScalarArray upper = new ScalarArray(max))
                    CvInvoke.InRange(mask, lower, upper, mask);
                //CvInvoke.BitwiseNot(mask, mask);

                //s is the mask for saturation of at least 10, this is mainly used to filter out white pixels
                CvInvoke.Threshold(s, s, sat, 255, ThresholdType.Binary);
                CvInvoke.BitwiseAnd(mask, s, mask, null);

            }

            //Use Dilate followed by Erode to eliminate small gaps in some contour.
            CvInvoke.Dilate(mask, mask, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            CvInvoke.Erode(mask, mask, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);

            return mask;
        }

        public Mat GetHandContours(Mat image)
        {
            var copy = new Mat();
            CvInvoke.GaussianBlur(image, copy, new Size(5, 5), 1.5, 1.5);
            var mask = new Mat();
            bool useUMat;
            using (InputOutputArray ia = mask.GetInputOutputArray())
                useUMat = ia.IsUMat;

            using (IImage hsv = useUMat ? (IImage)new UMat() : (IImage)new Mat())
            using (IImage s = useUMat ? (IImage)new UMat() : (IImage)new Mat())
            {
                CvInvoke.CvtColor(copy, hsv, ColorConversion.Bgr2Hsv);
                CvInvoke.ExtractChannel(hsv, mask, 0);
                CvInvoke.ExtractChannel(hsv, s, 1);

                using (ScalarArray lower = new ScalarArray(min1))
                using (ScalarArray upper = new ScalarArray(max1))
                    CvInvoke.InRange(mask, lower, upper, mask);
                //CvInvoke.BitwiseNot(mask, mask);

                //s is the mask for saturation of at least 10, this is mainly used to filter out white pixels
                CvInvoke.Threshold(s, s, sat1, 255, ThresholdType.Binary);
                CvInvoke.BitwiseAnd(mask, s, mask, null);

            }

            //Use Dilate followed by Erode to eliminate small gaps in some contour.
            CvInvoke.Dilate(mask, mask, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            CvInvoke.Erode(mask, mask, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);

            return mask;
        }

        public Image<Bgr, byte> GetPalm(Mat mask)
        {
            int width = mask.Width;
            int height = mask.Height;
            var temp = new Mat();
            var result = mask.ToImage<Bgr, byte>();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            VectorOfPoint biggestContour = new VectorOfPoint();
            CvInvoke.FindContours(mask, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
            if (contours.Size > 0)
            {
                biggestContour = contours[0];
                for (int i = 0; i < contours.Size; i++)
                {
                    if (contours[i].Size > biggestContour.Size)
                    {
                        biggestContour = contours[i];
                    }
                }
            }
            if(biggestContour.Size != 0)
            {
                //Gaunam rankos konturus
                CvInvoke.ApproxPolyDP(biggestContour, biggestContour, 0.00000001, false);
                var points = biggestContour.ToArray();
                VectorOfInt hull = new VectorOfInt();
                //find the palm hand area using convexitydefect
                CvInvoke.ConvexHull(biggestContour, hull, true);
                var box = CvInvoke.MinAreaRect(biggestContour);
                Mat defects = new Mat();
                CvInvoke.ConvexityDefects(biggestContour, hull, defects);

                if (!defects.IsEmpty)
                {
                    //Data from Mat are not directly readable so we convert it to Matrix<>
                    Matrix<int> m = new Matrix<int>(defects.Rows, defects.Cols, defects.NumberOfChannels);
                    defects.CopyTo(m);

                    for (int i = 0; i < m.Rows; i++)
                    {
                        int startIdx = m.Data[i, 0];
                        int endIdx = m.Data[i, 1];
                        Point startPoint = points[startIdx];
                        Point endPoint = points[endIdx];
                        //draw  a line connecting the convexity defect start point and end point in thin red line
                        CvInvoke.Line(result, startPoint, endPoint, new MCvScalar(0, 0, 255));
                    }
                }

            }

            return result;
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(_videoCapture == null)
            {
                _videoCapture = new VideoCapture(0);
            }

            _videoCapture.ImageGrabbed += _videoCapture_ImageGrabbed;
            _videoCapture.Start();
        }

        private void _videoCapture_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                Mat m = new Mat();
                _videoCapture.Retrieve(m);
                pictureBox1.Image = m.ToImage<Bgr, byte>().Bitmap;
                DetectIllegalServing(m);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(_videoCapture != null)
            {
                _videoCapture.Stop();
            }
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(_videoCapture != null)
            {
                _videoCapture.Pause();
            }
        }

        private void startToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if(_videoCapture == null)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Video Files |*.mp4";
                if(ofd.ShowDialog() == DialogResult.OK)
                {

                    _videoCapture = new VideoCapture(ofd.FileName);
                }
            }
            _videoCapture.ImageGrabbed += _videoCapture_ImageGrabbed1;
            _videoCapture.Start();
        }

        private void _videoCapture_ImageGrabbed1(object sender, EventArgs e)
        {
            try
            {
                Mat m = new Mat();
                _videoCapture.Retrieve(m);
                pictureBox1.Image = m.ToImage<Bgr, byte>().Bitmap;
                DetectIllegalServing(m);
                Thread.Sleep((int)_videoCapture.GetCaptureProperty(CapProp.Fps));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void stopToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if(_videoCapture != null)
            {
                _videoCapture.Stop();
            }
        }

        private void pauseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_videoCapture != null)
            {
                _videoCapture.Pause();
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            dp = trackBar1.Value;
            label8.Text = dp.ToString();
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            minDist = trackBar2.Value;
            label9.Text = minDist.ToString();
        }

        private void trackBar3_ValueChanged(object sender, EventArgs e)
        {
            param1 = trackBar3.Value;
            label10.Text = param1.ToString();
        }

        private void trackBar4_ValueChanged(object sender, EventArgs e)
        {
            param2 = trackBar4.Value;
            label11.Text = param2.ToString();
        }

        private void trackBar5_ValueChanged(object sender, EventArgs e)
        {
            minRadius = trackBar5.Value;
            label12.Text = minRadius.ToString();
        }

        private void trackBar6_ValueChanged(object sender, EventArgs e)
        {
            maxRadius = trackBar6.Value;
            label13.Text = maxRadius.ToString();
        }

        private void trackBar7_ValueChanged(object sender, EventArgs e)
        {
            min = trackBar7.Value;
            label17.Text = min.ToString();
        }

        private void trackBar8_ValueChanged(object sender, EventArgs e)
        {
            max = trackBar8.Value;
            label18.Text = max.ToString();
        }

        private void trackBar9_ValueChanged(object sender, EventArgs e)
        {
            sat = trackBar9.Value;
            label19.Text = sat.ToString();
        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void trackBar10_ValueChanged(object sender, EventArgs e)
        {
            min1 = trackBar10.Value;
            label24.Text = min1.ToString();
        }

        private void trackBar11_ValueChanged(object sender, EventArgs e)
        {
            max1 = trackBar11.Value;
            label25.Text = max1.ToString();
        }

        private void trackBar12_ValueChanged(object sender, EventArgs e)
        {
            sat1 = trackBar12.Value;
            label26.Text = sat1.ToString();
        }
    }
}