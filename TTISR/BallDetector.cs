using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Collections;
using System.Drawing;
using System;

namespace TTISR
{
    public class BallDetector
    {
        private Queue queue;
        public BallDetector()
        {
            queue = new Queue();
        }

        public VectorOfVectorOfPoint GetBallContours(Image<Bgr, byte> image)
        {
            Image<Gray, byte> imgout = image.Convert<Gray, byte>().Not().ThresholdBinary(new Gray(50), new Gray(255));
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            CvInvoke.FindContours(imgout, contours, hier, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            return contours;
        }




        /*
        public static void GetYellowMask(IInputArray image, IInputOutputArray mask)
        {
            var copy = (IImage)new Mat();
            CvInvoke.GaussianBlur(image, copy, new Size(11, 11), 0);
            var hsv = (IImage)new Mat();
            CvInvoke.CvtColor(image, hsv, ColorConversion.Bgr2Hsv);

            using (ScalarArray lower = new ScalarArray(new MCvScalar(90, 69, 55)))
            using (ScalarArray upper = new ScalarArray(new MCvScalar(255, 236, 171)))
                CvInvoke.InRange(hsv, lower, upper, mask);
            CvInvoke.Erode(mask, mask, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            CvInvoke.Dilate(mask, mask, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
        }

        public static void GetBallContours(IInputOutputArray image, IInputOutputArray outputImage)
        {

            
            outputImage = image;
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(image, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
            VectorOfPoint biggestContour = new VectorOfPoint();
            if (contours.Size > 0) {
                biggestContour = contours[0];
                for (int i = 0; i < contours.Size; i++)
                {
                    if(contours[i].Size > biggestContour.Size)
                    {
                        biggestContour = contours[i];
                        var moments = CvInvoke.Moments(biggestContour);
                        var coords = CvInvoke.MinEnclosingCircle(biggestContour);
                        Point center = new Point((int)(moments.M10 / moments.M00), (int)(moments.M01 / moments.M00));
                        //CvInvoke.Circle(outputImage, center, 5, new MCvScalar(0, 255, 255), 1);
                        //CvInvoke.Circle(outputImage, Point.Round(coords.Center), (int)coords.Radius, new Bgr(Color.Brown).MCvScalar, 2);
                    }
                }
            }

            CvInvoke.DrawContours(outputImage, contours, -1, new MCvScalar(255, 0, 0));
        }*/
    }
}