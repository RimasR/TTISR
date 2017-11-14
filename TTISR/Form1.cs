using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using HandGestureRecognition.SkinDetector;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace TTISR
{
    public partial class Form1 : Form
    {
        //ball detection fields
        private VideoCapture _videoCapture = null;
        private BallDetector detector = new BallDetector();
        private Mat _frame;
        private Mat _cannyFrame;
        private bool _captureInProgress;
        private Mat uImage = new Mat();
        private Mat pyrDown = new Mat();
        private double cannyThreshold = 180.0;
        private double circleAccumulatorThreshold = 120;

        // skin detection
        private IColorSkinDetector skinDetector;

        private Hsv hsv_min;
        private Hsv hsv_max;
        private Ycc YCrCb_min;
        private Ycc YCrCb_max;

        private Bgr white_upper = new Bgr(255, 255, 255);
        private Bgr white_lower = new Bgr(212, 243, 244);

        //private CvSeq<Point> hull;
        //private Seq<Point> filteredHull;
        //private Seq<MCvConvexityDefect> defects;
        //private MCvConvexityDefect[] defectArray;
        private Rectangle handRect;

        //private MCvBox2D box;
        private Ellipse ellip;

        public Form1()
        {

            InitializeComponent();
            hsv_min = new Hsv(0, 45, 0);
            hsv_max = new Hsv(20, 255, 255);
            YCrCb_min = new Ycc(0, 131, 80);
            YCrCb_max = new Ycc(255, 185, 135);
            CvInvoke.UseOpenCL = false;
            try
            {
                _videoCapture = new VideoCapture();
                _videoCapture.ImageGrabbed += ProcessFrame;
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }
            _frame = new Mat();
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            if (_videoCapture != null && _videoCapture.Ptr != IntPtr.Zero)
            {
                _videoCapture.Retrieve(_frame, 0);
                skinDetector = new YCrCbSkinDetector();
                Mat copy = new Mat();
                /*Image<Bgr, Byte> blurred = null;
                CvInvoke.GaussianBlur(copy, blurred, new Size(11, 11), 0);
                Mat hsv = null;
                CvInvoke.CvtColor(copy, hsv, ColorConversion.Bgr2Hsv, 0);
                *///Image<Gray, Byte> skin = copy.InRange(white_lower, white_upper);
                BallDetector.GetYellowMask(_frame, copy);
                BallDetector.GetBallContours(copy, copy);
                /*ExtractContourAndHull(skin); // SKIN - reikiamas img, apdorotas
                DrawAndComputeFingersNum();*/
                //PerformHandDetection(_frame);
                //PerformShapeDetection(_frame);
                captureImageBox.Image = _frame;
                videoProcessing.Image = copy;
            }
        }

        public void PerformHandDetection(Mat frame)
        {
            //Get image
            var img = frame.ToImage<Bgr, Byte>();

            //Convert to gray
            UMat uimage = new UMat();
            CvInvoke.CvtColor(img, uimage, ColorConversion.Bgr2Gray);

            //use pyr to remove noise
            UMat pyrDown = new UMat();
            CvInvoke.PyrDown(uimage, pyrDown);
            CvInvoke.PyrUp(pyrDown, uimage);

            //Canny and edge detection
            double cannyThresholdLinking = 200.0;
            double cannyThresh = 200.0;
            Mat cannyEdges = new Mat();
            CvInvoke.Canny(uimage, cannyEdges, cannyThresh, cannyThresholdLinking);

            _cannyFrame = new Mat();
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                VectorOfPoint biggestContour = null;
                CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                Double result1 = 0;
                Double result2 = 0;
                VectorOfPoint approxContour = new VectorOfPoint();
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    CvInvoke.ApproxPolyDP(contours[i], approxContour, CvInvoke.ArcLength(contours[i], true) * 0.05, true);
                    result1 = approxContour.Size;
                    if (result1 > result2)
                    {
                        result2 = result1;
                        biggestContour = approxContour;
                    }
                }

                if (biggestContour != null)
                {
                    Point[] pts = biggestContour.ToArray();
                    CvInvoke.Polylines(_cannyFrame, pts, true, new Bgr(Color.LimeGreen).MCvScalar, 2);
                }
            }
        }

        private void PerformShapeDetection(Mat frame)
        {
            CvInvoke.CvtColor(frame, uImage, ColorConversion.Bgr2Gray);
            uImage = frame;
            CvInvoke.PyrDown(uImage, pyrDown);
            CvInvoke.PyrUp(pyrDown, uImage);
            CircleF[] circles = CvInvoke.HoughCircles(uImage, HoughType.Gradient, 2.0, 50.0, cannyThreshold, circleAccumulatorThreshold, 5);
            Point left = new Point();
            Point right = new Point();
            foreach (CircleF circle in circles)
            {
                var x1 = circle.Center.X - circle.Radius;
                left = new Point((int)x1, (int)circle.Center.Y);
                var x2 = circle.Center.X + circle.Radius;
                right = new Point((int)x2, (int)circle.Center.Y);
                CvInvoke.Circle(frame, Point.Round(circle.Center), (int)circle.Radius, new Bgr(Color.Brown).MCvScalar, 2);
                CvInvoke.Line(frame, left, right, new Bgr(Color.YellowGreen).MCvScalar, 2);
            }
        }

        private void captureButton_Click(object sender, EventArgs e)
        {
            if (_videoCapture != null)
            {
                if (_captureInProgress)
                {
                    captureButton.Text = "Start";
                    _videoCapture.Pause();
                }
                else
                {
                    captureButton.Text = "Stop";
                    _videoCapture.Start();
                }

                _captureInProgress = !_captureInProgress;
            }
        }

        private void ExtractContourAndHull(Image<Gray, byte> skin)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(skin, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxSimple);
            VectorOfPoint biggestContour = null;
            int i = 0;
            Double Result1 = 0;
            Double Result2 = 0;
            while (i < contours.Size)
            {
                Result1 = CvInvoke.ContourArea(contours[i]);
                if (Result1 > Result2)
                {
                    Result2 = Result1;
                    biggestContour = contours[i];
                }
                i++;
            }

            if (biggestContour != null)
            {
                //currentFrame.Draw(biggestContour, new Bgr(Color.DarkViolet), 2);

                //HERE CONTINUE
                /*CvInvoke.ApproxPolyDP(biggestContour, )
                    var currentContour = biggestContour.ApproxPoly(biggestContour.Perimeter * 0.0025, storage);
                currentFrame.Draw(currentContour, new Bgr(Color.LimeGreen), 2);
                biggestContour = currentContour;

                hull = biggestContour.GetConvexHull(Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);
                box = biggestContour.GetMinAreaRect();
                PointF[] points = box.GetVertices();
                //handRect = box.MinAreaRect();
                //currentFrame.Draw(handRect, new Bgr(200, 0, 0), 1);

                Point[] ps = new Point[points.Length];
                for (int i = 0; i < points.Length; i++)
                    ps[i] = new Point((int)points[i].X, (int)points[i].Y);

                currentFrame.DrawPolyline(hull.ToArray(), true, new Bgr(200, 125, 75), 2);
                currentFrame.Draw(new CircleF(new PointF(box.center.X, box.center.Y), 3), new Bgr(200, 125, 75), 2);

                //ellip.MCvBox2D= CvInvoke.cvFitEllipse2(biggestContour.Ptr);
                //currentFrame.Draw(new Ellipse(ellip.MCvBox2D), new Bgr(Color.LavenderBlush), 3);

                PointF center;
                float radius;
                //CvInvoke.cvMinEnclosingCircle(biggestContour.Ptr, out  center, out  radius);
                //currentFrame.Draw(new CircleF(center, radius), new Bgr(Color.Gold), 2);

                //currentFrame.Draw(new CircleF(new PointF(ellip.MCvBox2D.center.X, ellip.MCvBox2D.center.Y), 3), new Bgr(100, 25, 55), 2);
                //currentFrame.Draw(ellip, new Bgr(Color.DeepPink), 2);

                //CvInvoke.cvEllipse(currentFrame, new Point((int)ellip.MCvBox2D.center.X, (int)ellip.MCvBox2D.center.Y), new System.Drawing.Size((int)ellip.MCvBox2D.size.Width, (int)ellip.MCvBox2D.size.Height), ellip.MCvBox2D.angle, 0, 360, new MCvScalar(120, 233, 88), 1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, 0);
                //currentFrame.Draw(new Ellipse(new PointF(box.center.X, box.center.Y), new SizeF(box.size.Height, box.size.Width), box.angle), new Bgr(0, 0, 0), 2);

                filteredHull = new Seq<Point>(storage);
                for (int i = 0; i < hull.Total; i++)
                {
                    if (Math.Sqrt(Math.Pow(hull[i].X - hull[i + 1].X, 2) + Math.Pow(hull[i].Y - hull[i + 1].Y, 2)) > box.size.Width / 10)
                    {
                        filteredHull.Push(hull[i]);
                    }
                }

                defects = biggestContour.GetConvexityDefacts(storage, Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);

                defectArray = defects.ToArray();*/
            }
        }

        private void DrawAndComputeFingersNum()
        {
            /*int fingerNum = 0;

            #region hull drawing

            //for (int i = 0; i < filteredHull.Total; i++)
            //{
            //    PointF hullPoint = new PointF((float)filteredHull[i].X,
            //                                  (float)filteredHull[i].Y);
            //    CircleF hullCircle = new CircleF(hullPoint, 4);
            //    currentFrame.Draw(hullCircle, new Bgr(Color.Aquamarine), 2);
            //}

            #endregion hull drawing

            #region defects drawing

            for (int i = 0; i < defects.Total; i++)
            {
                PointF startPoint = new PointF((float)defectArray[i].StartPoint.X,
                                                (float)defectArray[i].StartPoint.Y);

                PointF depthPoint = new PointF((float)defectArray[i].DepthPoint.X,
                                                (float)defectArray[i].DepthPoint.Y);

                PointF endPoint = new PointF((float)defectArray[i].EndPoint.X,
                                                (float)defectArray[i].EndPoint.Y);

                LineSegment2D startDepthLine = new LineSegment2D(defectArray[i].StartPoint, defectArray[i].DepthPoint);

                LineSegment2D depthEndLine = new LineSegment2D(defectArray[i].DepthPoint, defectArray[i].EndPoint);

                CircleF startCircle = new CircleF(startPoint, 5f);

                CircleF depthCircle = new CircleF(depthPoint, 5f);

                CircleF endCircle = new CircleF(endPoint, 5f);

                //Custom heuristic based on some experiment, double check it before use
                if ((startCircle.Center.Y < box.center.Y || depthCircle.Center.Y < box.center.Y) && (startCircle.Center.Y < depthCircle.Center.Y) && (Math.Sqrt(Math.Pow(startCircle.Center.X - depthCircle.Center.X, 2) + Math.Pow(startCircle.Center.Y - depthCircle.Center.Y, 2)) > box.size.Height / 6.5))
                {
                    fingerNum++;
                    currentFrame.Draw(startDepthLine, new Bgr(Color.Green), 2);
                    //currentFrame.Draw(depthEndLine, new Bgr(Color.Magenta), 2);
                }

                currentFrame.Draw(startCircle, new Bgr(Color.Red), 2);
                currentFrame.Draw(depthCircle, new Bgr(Color.Yellow), 5);
                //currentFrame.Draw(endCircle, new Bgr(Color.DarkBlue), 4);
            }

            #endregion defects drawing

            MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_DUPLEX, 5d, 5d);
            currentFrame.Draw(fingerNum.ToString(), ref font, new Point(50, 150), new Bgr(Color.White));*/
        }
    }
}