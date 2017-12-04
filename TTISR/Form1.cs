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
        private Mat _frame;
        private Mat _ballContourFrame;
        private bool _captureInProgress;

        public Form1()
        {
            InitializeComponent();
            /*CvInvoke.UseOpenCL = false;
            try
            {
                _videoCapture = new VideoCapture();
                _videoCapture.ImageGrabbed += ProcessFrame;
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            };
            _frame = new Mat();
            _ballContourFrame = new Mat();*/
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            if (_videoCapture != null && _videoCapture.Ptr != IntPtr.Zero)
            {
                _videoCapture.Retrieve(_frame, 0);
                _ballContourFrame = _frame;

                DetectIllegalServing(_ballContourFrame.ToImage<Bgr, byte>());
                Mat m = new Mat();
                

                /*captureImageBox.Image = _frame;
                ballProcessing.Image = _ballContourFrame;*/
                
            }
        }

        private void captureButton_Click(object sender, EventArgs e)
        {
            /*if (_videoCapture != null)
            {
                if (_captureInProgress)
                {
                    captureButton.Text = "Start";
                    _videoCapture.Stop();
                }
                else
                {
                    captureButton.Text = "Stop";
                    _videoCapture.Start();
                }

                _captureInProgress = !_captureInProgress;
            }*/
        }

        private void DetectIllegalServing(Image<Bgr, byte> image)
        {
            detector.GetBallContours(image);
            
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
                DetectIllegalServing(m.ToImage<Bgr, byte>());
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
                DetectIllegalServing(m.ToImage<Bgr, byte>());
                Thread.Sleep((int)_videoCapture.GetCaptureProperty(CapProp.Fps)*2);
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
    }
}