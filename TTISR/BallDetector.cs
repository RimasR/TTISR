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