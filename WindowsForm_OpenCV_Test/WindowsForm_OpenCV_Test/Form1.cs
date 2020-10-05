using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsForm_OpenCV_Test
{
    public partial class Form1 : Form
    {
        VideoCapture _capture;

        Snake _snake;

        CircleF[] _circlesDetection;

        public Form1()
        {
            InitializeComponent();

            _capture = new VideoCapture();

            _capture.SetCaptureProperty(CapProp.FrameWidth, 1280);
            _capture.SetCaptureProperty(CapProp.FrameHeight, 720);
            _capture.SetCaptureProperty(CapProp.Fps, 30);

            _snake = new Snake();

            Application.Idle += Streaming;

            _circlesDetection = new CircleF[] { };

            
        }


        private void Streaming(object sender, EventArgs e)
        {
            var rawImg = _capture.QueryFrame().ToImage<Bgr, byte>().Flip(FlipType.Horizontal);

            var imgMaskGreen = rawImg.Convert<Hsv, byte>().InRange(new Hsv(32, 52, 30), new Hsv(98, 255, 255));

            imgMaskGreen = imgMaskGreen.Erode(9);
            imgMaskGreen = imgMaskGreen.Dilate(7);





            
            if (_circlesDetection.Length == 0)
            {
                _circlesDetection = CvInvoke.HoughCircles(imgMaskGreen, HoughModes.Gradient, 3, 10, 25, 100);
            }
            else
            {
                var tempCircles = CvInvoke.HoughCircles(imgMaskGreen, HoughModes.Gradient, 3, 10, 25, 100);

                if (tempCircles.Length != 0)
                {
                    if (Math.Abs(_circlesDetection[0].Center.X - tempCircles[0].Center.X) > 1 &&
                         Math.Abs(_circlesDetection[0].Center.Y - tempCircles[0].Center.Y) > 1)
                    {
                        _circlesDetection = tempCircles;
                    }
                }
            }
            


            /*
            var tempCircles = CvInvoke.HoughCircles(imgMaskGreen, HoughModes.Gradient, 3, 10, 25, 100);
            if (tempCircles.Length != 0)
                _circlesDetection = tempCircles;
            */



            _snake.DoStep(_circlesDetection, rawImg);

            imageBox1.Image = rawImg;

        



            /*
            if (_circlesDetection.Length != 0)
            {
                rawImg.Draw(_circlesDetection[0], new Bgr(255, 0, 0), 3);
                rawImg.Draw(new Cross2DF(_circlesDetection[0].Center, 3, 3), new Bgr(0, 0, 255), 3);
                label_area.Text = "yes";
                imageBox1.Image = rawImg;
            }
            else
            {
                label_area.Text = "no";
                imageBox1.Image = rawImg;

            }
            */



        }

    }
}
