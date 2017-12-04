using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;

namespace TTISR
{
    public class HandDetector
    {
        public HandDetector()
        {
            
        }
        public static void GetSkinMask(IInputArray image, IInputOutputArray mask)
        {
            var copy = (IImage)new Mat();
            CvInvoke.GaussianBlur(image, copy, new Size(11, 11), 0);
            var hsv = (IImage)new Mat();
            CvInvoke.CvtColor(image, hsv, ColorConversion.Bgr2Hsv);
            using (ScalarArray lower = new ScalarArray(new MCvScalar(20, 100, 100)))
            using (ScalarArray upper = new ScalarArray(new MCvScalar(100, 255, 255)))
                CvInvoke.InRange(hsv, lower, upper, mask);
            CvInvoke.Erode(mask, mask, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            CvInvoke.Dilate(mask, mask, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
        }

        public static void GetSkinContours(IInputOutputArray image, IInputOutputArray outputImage)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(image, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
            VectorOfPoint biggestContour = new VectorOfPoint();
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
            var moments = CvInvoke.Moments(biggestContour);
            var coords = CvInvoke.MinEnclosingCircle(biggestContour);
            Point center = new Point((int)(moments.M10 / moments.M00), (int)(moments.M01 / moments.M00));
            outputImage = image;
            CvInvoke.Circle(outputImage, center, 5, new MCvScalar(0, 255, 255), 1);
            CvInvoke.Circle(outputImage, Point.Round(coords.Center), (int)coords.Radius, new Bgr(Color.Brown).MCvScalar, 2);



        }
    }
}