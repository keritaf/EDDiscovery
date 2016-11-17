﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ExtendedControls
{
    public class DrawnPanel : Panel
    {
        // Back, Fore color used
        public Color MouseOverColor { get; set; } = Color.White;
        public Color MouseSelectedColor { get; set; } = Color.Green;

        public enum ImageType { Close, Minimize, Gripper, EDDB, Ross, InverseText, Move, Text, None };

        public ImageType ImageSelected { get; set; } = ImageType.Close;
        public Image DrawnImage { get; set; } = null;                                   // if not set, an image is drawn . Use None below for a image only
        public ImageAttributes DrawnImageAttributes = null;                             // Image override (colour etc) 
        public string ImageText { get; set; } = null;       // for Text Type
        public int MarginSize { get; set; } = 4;                    // margin around icon, 0 =auto, -1 = zero

        #region Public Functions
        public void Captured()                                     // if doing the move capture stuff on this panel, call this
        {
            mousecapture = true;
            Invalidate();
        }

        public bool IsCaptured { get { return mousecapture; } }

        public void SetDrawnBitmapRemapTable( ColorMap[] remap )
        {
            ImageAttributes ia = new ImageAttributes();
            ia.SetRemapTable(remap, ColorAdjustType.Bitmap);
            DrawnImageAttributes = ia;
        }

        #endregion

        #region Implementation

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if ( DrawnImage != null )
            {
                if (DrawnImageAttributes != null)
                    e.Graphics.DrawImage(DrawnImage, new Rectangle(0, 0, DrawnImage.Width, DrawnImage.Height), 0, 0, DrawnImage.Width, DrawnImage.Height, GraphicsUnit.Pixel, DrawnImageAttributes);
                else
                    e.Graphics.DrawImage(DrawnImage, new Point(0, 0));
            }

            if (ImageSelected != ImageType.None)
            {
                int msize = (MarginSize == -1) ? 0 : ((MarginSize > 0) ? MarginSize : ClientRectangle.Height / 6);
                Color pc = (Enabled) ? ((mousedown || mousecapture) ? MouseSelectedColor : ((mouseover) ? MouseOverColor : this.ForeColor)) : Average(this.ForeColor, this.BackColor, 0.25F);
                //Console.WriteLine("Enabled" + Enabled + " Mouse over " + mouseover + " mouse down " + mousedown);

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Pen p1 = new Pen(pc, 1.0F);
                Pen p2 = new Pen(pc, 2.0F);

                int rightpx = ClientRectangle.Width - 1;
                int bottompx = ClientRectangle.Height - 1;
                int centrehorzpx = (ClientRectangle.Width - 1) / 2;
                int centrevertpx = (ClientRectangle.Height - 1) / 2;

                int leftmarginpx = msize;
                int rightmarginpx = rightpx - msize;
                int topmarginpx = msize;
                int bottommarginpx = bottompx - msize;

                if (ImageSelected == ImageType.Close)
                {
                    e.Graphics.DrawLine(p2, new Point(leftmarginpx, topmarginpx), new Point(rightmarginpx, bottommarginpx));
                    e.Graphics.DrawLine(p2, new Point(leftmarginpx, bottommarginpx), new Point(rightmarginpx, topmarginpx));
                }
                else if (ImageSelected == ImageType.Minimize)
                {
                    e.Graphics.DrawLine(p2, new Point(leftmarginpx, bottommarginpx), new Point(rightmarginpx, bottommarginpx));
                }
                else if (ImageSelected == ImageType.Gripper)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        e.Graphics.DrawLine(p1, new Point(rightmarginpx - i * msize, bottompx), new Point(rightpx, bottommarginpx - i * msize));
                    }
                }
                else if (ImageSelected == ImageType.EDDB)
                {
                    Brush bbck = new SolidBrush(pc);
                    Rectangle area = new Rectangle(leftmarginpx, topmarginpx, ClientRectangle.Width - 2 * msize, ClientRectangle.Height - 2 * msize);
                    e.Graphics.FillRectangle(bbck, area);
                    bbck.Dispose();

                    Pen pb = new Pen(this.BackColor, 2.0F);
                    Point pt1 = new Point(rightmarginpx, bottommarginpx - msize);
                    Point pt2 = new Point(centrehorzpx - 1, bottommarginpx - msize);
                    Point pt3 = new Point(centrehorzpx - 1, topmarginpx + msize);
                    Point pt4 = new Point(centrehorzpx - 1 - msize, pt3.Y + 2);
                    Point pt5 = new Point(centrehorzpx - 1 + msize, pt3.Y + 2);

                    e.Graphics.DrawLine(pb, pt1, pt2);
                    e.Graphics.DrawLine(pb, pt2, pt3);
                    e.Graphics.DrawLine(pb, pt2, pt3);
                    e.Graphics.DrawLine(pb, pt4, pt5);

                    pb.Dispose();
                }
                else if (ImageSelected == ImageType.Ross)
                {
                    Pen pb = new Pen(pc, 3.0F);
                    Point pt1 = new Point(leftmarginpx + 2, bottommarginpx);
                    Point pt2 = new Point(pt1.X, topmarginpx + 4);
                    Point pt3 = new Point(centrehorzpx + 2, pt2.Y);

                    e.Graphics.DrawLine(pb, pt1, pt2);
                    e.Graphics.DrawLine(pb, pt2, pt3);

                    pb.Dispose();
                }
                else if (ImageSelected == ImageType.InverseText)
                {
                    SizeF size = e.Graphics.MeasureString(this.ImageText, this.Font);
                    double scale = (double)(ClientRectangle.Height - topmarginpx * 2) / (double)size.Height;
                    // given the available height, scale the font up if its bigger than the current font height.

                    using (Font fnt = new Font(this.Font.Name, (float)(this.Font.SizeInPoints * scale), this.Font.Style))
                    {
                        size = e.Graphics.MeasureString(this.ImageText, fnt);
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;     //MUST turn it off to get a sharp rect

                        using (Brush bbck = new SolidBrush(pc))
                            e.Graphics.FillRectangle(bbck, new Rectangle(leftmarginpx, topmarginpx, ClientRectangle.Width - 2 * msize, ClientRectangle.Height - 2 * msize));

                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        using (Brush textb = new SolidBrush(this.BackColor))
                            e.Graphics.DrawString(this.ImageText, fnt, textb, new Point(centrehorzpx - (int)(size.Width / 2), topmarginpx));
                    }
                }
                else if (ImageSelected == ImageType.Text)
                {
                    SizeF size = e.Graphics.MeasureString(this.ImageText, this.Font);
                    double scale = (double)(ClientRectangle.Height - topmarginpx * 2) / (double)size.Height;
                    // given the available height, scale the font up if its bigger than the current font height.

                    using (Font fnt = new Font(this.Font.Name, (float)(this.Font.SizeInPoints * scale), this.Font.Style))
                    {
                        size = e.Graphics.MeasureString(this.ImageText, fnt);
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        using (Brush textb = new SolidBrush(this.ForeColor))
                            e.Graphics.DrawString(this.ImageText, fnt, textb, new Point(centrehorzpx - (int)(size.Width / 2), topmarginpx));
                    }
                }
                else if (ImageSelected == ImageType.Move)
                {
                    centrehorzpx++;
                    centrevertpx++;

                    int o = ClientRectangle.Width / 8;
                    e.Graphics.DrawLine(p2, new Point(centrehorzpx, bottompx), new Point(centrehorzpx, topmarginpx));
                    e.Graphics.DrawLine(p1, new Point(centrehorzpx - o, bottompx - o), new Point(centrehorzpx, bottompx));
                    e.Graphics.DrawLine(p1, new Point(centrehorzpx + o, bottompx - o), new Point(centrehorzpx, bottompx));
                    e.Graphics.DrawLine(p1, new Point(centrehorzpx - o, topmarginpx + o), new Point(centrehorzpx, topmarginpx));
                    e.Graphics.DrawLine(p1, new Point(centrehorzpx + o, topmarginpx + o), new Point(centrehorzpx, topmarginpx));

                    e.Graphics.DrawLine(p2, new Point(leftmarginpx, centrevertpx), new Point(rightmarginpx, centrevertpx));
                    e.Graphics.DrawLine(p1, new Point(leftmarginpx + o, centrevertpx - o), new Point(leftmarginpx, centrevertpx));
                    e.Graphics.DrawLine(p1, new Point(leftmarginpx + o, centrevertpx + o), new Point(leftmarginpx, centrevertpx));
                    e.Graphics.DrawLine(p1, new Point(rightmarginpx - o, centrevertpx - o), new Point(rightmarginpx, centrevertpx));
                    e.Graphics.DrawLine(p1, new Point(rightmarginpx - o, centrevertpx + o), new Point(rightmarginpx, centrevertpx));
                }

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

                p1.Dispose();
                p2.Dispose();
            }
        }

        protected override void OnMouseEnter(EventArgs eventargs)
        {
            base.OnMouseEnter(eventargs);
            mouseover = true;
            mousedown = false;
            mousecapture = false;                   // mouse enter called after capture finished, so clear it
            //Console.WriteLine("DP ME");
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs eventargs)
        {
            base.OnMouseLeave(eventargs);
            mouseover = false;
            mousedown = false;   
            //Console.WriteLine("DP ML");
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            mousedown = true;
            //Console.WriteLine("DP MD");
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            mousedown = false;
            //Console.WriteLine("DP MU");
            Invalidate();
        }

        private byte limit(float a) { if (a > 255F) return 255; else return (byte)a; }
        public Color Multiply(Color from, float m) { return Color.FromArgb(from.A, limit((float)from.R * m), limit((float)from.G * m), limit((float)from.B * m)); }
        public Color Average(Color c1, Color c2, float l) { float r = 1.0F - l; return Color.FromArgb(limit(c1.A * l + c2.A * r), limit(c1.R * l + c2.R * r), limit(c1.G * l + c2.G * r), limit(c1.B * l + c2.B * r)); }

        private bool mouseover = false;
        private bool mousedown = false;
        private bool mousecapture = false;
#endregion
    }

    public class DrawnPanelNoTheme : DrawnPanel     // use if you want the panel to be themed.. sometimes you do, sometimes you don't
    {
        public DrawnPanelNoTheme()
        {

        }
    }


}
