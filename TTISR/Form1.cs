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
        double minDist = 160.0;
        double param1 = 50.0;
        double param2 = 120.0;
        double minRadius = 0;
        double maxRadius = 0;
        double min = 9;
        double max = 14;
        double sat = 10;

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
        }

        private void DetectIllegalServing(Mat image)
        {
            Mat final = image.Clone();
            Mat contours = GetBallContours(image);

            Mat handContours = GetHandContours(image);
            CircleF[] circles = CvInvoke.HoughCircles(contours, HoughType.Gradient, dp, minDist, param1, param2);
            foreach (var circle in circles)
            {
                CvInvoke.Circle(final, Point.Round(circle.Center), (int)circle.Radius, new Bgr(Color.Red).MCvScalar, 3);
            }
            label7.BeginInvoke(new MethodInvoker(delegate { label7.Text = $"Circle count: {circles.Length}"; }));

            pictureBox2.Image = contours.Bitmap;
            pictureBox3.Image = handContours.Bitmap;
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

                using (ScalarArray lower = new ScalarArray(90))
                using (ScalarArray upper = new ScalarArray(120))
                    CvInvoke.InRange(mask, lower, upper, mask);
                //CvInvoke.BitwiseNot(mask, mask);

                //s is the mask for saturation of at least 10, this is mainly used to filter out white pixels
                CvInvoke.Threshold(s, s, 10, 255, ThresholdType.Binary);
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

                using (ScalarArray lower = new ScalarArray(9))
                using (ScalarArray upper = new ScalarArray(14))
                    CvInvoke.InRange(mask, lower, upper, mask);
                //CvInvoke.BitwiseNot(mask, mask);

                //s is the mask for saturation of at least 10, this is mainly used to filter out white pixels
                CvInvoke.Threshold(s, s, 10, 255, ThresholdType.Binary);
                CvInvoke.BitwiseAnd(mask, s, mask, null);

            }

            //Use Dilate followed by Erode to eliminate small gaps in some contour.
            CvInvoke.Dilate(mask, mask, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            CvInvoke.Erode(mask, mask, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);

            return mask;
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
    }
}