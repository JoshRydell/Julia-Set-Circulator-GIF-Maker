using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AnimatedGif;
using System.IO;

namespace Julia_Set_Circulator_GIF_Maker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ClientSize = new Size(500, 600);
            MaximumSize = Size;
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Text = "Julia Set Circulator GIF Maker";

            Img = new PictureBox();
            Img.Location = new Point(0, 0);
            Img.Size = new Size(Width, Width);
            Img.SizeMode = PictureBoxSizeMode.Zoom;
            Img.BackColor = Color.Gray;
            Controls.Add(Img);

            Button Create = new Button();
            Create.Size = new Size(50, 35);
            Create.Text = "Create GIF";
            Create.Location = new Point(10, 520);
            Create.Click += new EventHandler(MakeGIF);
            Controls.Add(Create);

            Button Save = new Button();
            Save.Size = Create.Size;
            Save.Text = "Save GIF";
            Save.Location = new Point(10 + Create.Location.X + Create.Width, 520);
            Save.Click += new EventHandler(SaveGif);
            Controls.Add(Save);

            Re = new TextBox();
            Re.Location = new Point(10 + Save.Location.X + Save.Width, 520);
            Re.Size = new Size(ClientSize.Width - Re.Location.X - 10, 10);
            Re.TextChanged += new EventHandler(CheckFormatStartNums);
            Controls.Add(Re);

            Im = new TextBox();
            Im.Location = new Point(10 + Save.Location.X + Save.Width, Re.Location.Y + Re.Height + 5);
            Im.Size = new Size(ClientSize.Width - Re.Location.X - 10, 10);
            Im.TextChanged += new EventHandler(CheckFormatStartNums);
            Controls.Add(Im);

            Progress = new ProgressBar();
            Progress.Location = new Point(Create.Location.X, Create.Location.Y + Create.Height + 2);
            Progress.Size = new Size(Save.Location.X + Save.Width - Create.Location.X, 10);
            Progress.Minimum = 0;
            Controls.Add(Progress);

            Delay = new TrackBar();
            Delay.Location = new Point(Im.Location.X, Im.Location.Y + Im.Height + 2);
            Delay.Size = new Size(Im.Width, Im.Height);
            Delay.SetRange((int)Math.Log(10,Frame.baseLog), (int)Math.Log(150, Frame.baseLog));
            Delay.ValueChanged += new EventHandler(DelayChange);
            
            Controls.Add(Delay);

            RotsPerSecond = new Label();
            RotsPerSecond.Location = new Point(Progress.Location.X, Im.Location.Y + Im.Height + 2);
            Controls.Add(RotsPerSecond);

            Delay.Value = (Delay.Maximum + Delay.Minimum) / 2;
        }
        TrackBar Delay;
        PictureBox Img;
        Label RotsPerSecond;
        TextBox Re;
        TextBox Im;

        public static ProgressBar Progress;
        private bool CheckAccurateInput(string input, float bottom, float top)
        {
            if (float.TryParse(input, out float num))
            {
                if (bottom <= num && top >= num)
                {
                    return true;
                }
            }

            return false;
        }
        private void CheckFormatStartNums(object sender, EventArgs e)
        {
            TextBox send = (TextBox)sender;

            if (!(send.Text == "" || send.Text == "-") && !CheckAccurateInput(send.Text, -1, 1))
            {
                MessageBox.Show("Input must be between -1 and 1");
                send.Text = send.Text.Substring(0, send.Text.Length - 1);
            }
        }
        private void SaveGif(object sender, EventArgs e)
        {
            if (Img.Image != null)
            {
                SaveFileDialog fd = new SaveFileDialog();
                fd.Title = "Save GIF";
                fd.Filter = "GIF (*.gif)|*.gif";
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    Img.Image.Save(fd.FileName);
                }
            }
        }
        private void DelayChange(object sender, EventArgs e)
        {
            double num = (Math.Pow(Delay.Maximum + Delay.Minimum - Delay.Value, Frame.baseLog) * (Math.Pow(Delay.Maximum + Delay.Minimum - Delay.Value, Frame.baseLog) * 3.5f - 80) / 1000f);
            RotsPerSecond.Text = "RPM: " + Math.Round((Math.Pow(Delay.Maximum + Delay.Minimum - Delay.Value, Frame.baseLog) * (Math.Pow(Delay.Maximum + Delay.Minimum - Delay.Value, Frame.baseLog) * 3.5f - 80) / 1000f),2).ToString();
        }
        
        private async void MakeGIF(object sender, EventArgs e)
        {
            if (Im.Text == "" || Re.Text == "" || Im.Text == "-" || Re.Text == "-")
            {
                return;
            }
            Frame.Initialise(float.Parse(Re.Text), float.Parse(Im.Text), Delay.Value);
            Progress.Maximum = Frame.numFrames;
            MemoryStream ms = new MemoryStream();
            AnimatedGifCreator gif = new AnimatedGifCreator(ms, Delay.Value);
            Frame[] Frames = new Frame[Frame.numFrames];
            Task[] Threads = new Task[Frames.Length];
            

            for(int i = 0; i < Frames.Length; i++)
            {
                Frames[i] = new Frame(i);
            }
            
            
            for (int i = 0; i < Frames.Length; i++)
            {
                Threads[i] = new Task(Frames[i].MakeBitmap);
                Threads[i].Start();
            }

            for(int i = 0; i < Frames.Length; i++)
            {
                await Threads[i];
                Progress.Value++;
            }
            for(int i = 0; i < Frames.Length; i++)
            {
                gif.AddFrame(Frames[i].FrameImage);
            }

            Progress.Value = 0;
            Img.Image = Image.FromStream(ms);
        }
    }
    class ComplexNumber
    {
        public float Re;
        public float Im;
        public ComplexNumber(float Re, float Im)
        {
            this.Re = Re;
            this.Im = Im;
        }
        public float Magnitude
        {
            get { return (float)Math.Sqrt(Re * Re + Im * Im); }
        }
        public static ComplexNumber operator *(ComplexNumber a, ComplexNumber b)
        {
            return new ComplexNumber(a.Re * b.Re - a.Im * b.Im, a.Im * b.Re + a.Re * b.Im);
        }
        public static ComplexNumber operator +(ComplexNumber a, ComplexNumber b)
        {
            return new ComplexNumber(a.Re + b.Re, a.Im + b.Im);
        }
    }
    public class ColorUtils
    {
        public static Color HsvToRgb(double h, double s, double v)
        {
            int hi = (int)Math.Floor(h / 60.0) % 6;
            double f = (h / 60.0) - Math.Floor(h / 60.0);

            double p = v * (1.0 - s);
            double q = v * (1.0 - (f * s));
            double t = v * (1.0 - ((1.0 - f) * s));

            Color ret;

            switch (hi)
            {
                case 0:
                    ret = ColorUtils.GetRgb(v, t, p);
                    break;
                case 1:
                    ret = ColorUtils.GetRgb(q, v, p);
                    break;
                case 2:
                    ret = ColorUtils.GetRgb(p, v, t);
                    break;
                case 3:
                    ret = ColorUtils.GetRgb(p, q, v);
                    break;
                case 4:
                    ret = ColorUtils.GetRgb(t, p, v);
                    break;
                case 5:
                    ret = ColorUtils.GetRgb(v, p, q);
                    break;
                default:
                    ret = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
                    break;
            }
            return ret;
        }
        public static Color GetRgb(double r, double g, double b)
        {
            return Color.FromArgb(255, (byte)(r * 255.0), (byte)(g * 255.0), (byte)(b * 255.0));
        }
    }
}
