using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace Julia_Set_Circulator_GIF_Maker
{
    class Frame
    {
        public Bitmap FrameImage { get; private set; }
        private int frameNumber;
        private static float im;
        private static float re;
        private const int Max = 33;
        public const float baseLog = 1.1f;
        public static int numFrames { get; private set; }
        public Frame(int frameNumber)
        {
            this.frameNumber = frameNumber;
        }
        public static void Initialise(float re, float im, int Delay)
        {
            Frame.im = im;
            Frame.re = re;
            numFrames = (int)(3.5f * Math.Pow(Delay, baseLog) - 80);
        }
        public void MakeBitmap()
        {
            Bitmap b = new Bitmap(600, 600);
            ComplexNumber C = new ComplexNumber((float)(re * Math.Sin(frameNumber * 2 * Math.PI / (float)numFrames)), (float)(im * Math.Cos(frameNumber * 2 * Math.PI / (float)numFrames)));
            for (int i = 0; i < b.Width; i++)
            {
                for (int j = 0; j < b.Height; j++)
                {
                    ComplexNumber Z = new ComplexNumber(i * 4 / (float)b.Width - 2, j * 4 / (float)b.Height - 2);
                    int k = 0;
                    while (k < Max && Z.Magnitude < 4)
                    {
                        Z = Z * Z + C;
                        k++;
                    }

                    Color Colour;
                    if (k == Max)
                    {
                        Colour = Color.Black;
                    }
                    else
                    {
                        Colour = ColorUtils.HsvToRgb(Math.Sqrt((float)k / Max), 255, (255 * im * re == 0) ? 255 : 255 * im * re);
                    }



                    b.SetPixel(i, j, Colour);
                }
            }
            FrameImage = b;
        }
    }
}
