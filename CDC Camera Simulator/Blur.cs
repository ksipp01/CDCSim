using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace ASCOM.SimCDC
{
    class Blur
    {
        //private void btnLoad_Click(object sender, EventArgs e)
        //{
        //    using (OpenFileDialog diag = new OpenFileDialog())
        //    {
        //        diag.Filter = "Bitmap|*.bmp;*.jpg;*.gif";
        //        if (diag.ShowDialog() == DialogResult.OK)
        //        {
        //            try
        //            {
        //                picOriginal.Image = Image.FromFile(diag.FileName);
        //            }
        //            catch (Exception)
        //            {
        //                MessageBox.Show("Invalid Image");
        //            }
        //        }
        //    }
        //}
      //  int nAmount = 3;  // amount of blur
        public Bitmap ApplyBlur(Bitmap image, int nAmount)
        {
        //    btnApply.Enabled = false;
         //   this.Cursor = Cursors.WaitCursor;
            image = FastBoxBlur(image, nAmount);
          //  picConvolved.Image = FastBoxBlur(picOriginal.Image, (int)nAmount.Value);

          //  this.Cursor = Cursors.Default;
           // btnApply.Enabled = true;
            return image;
        }

        //private void btnApply_Click(object sender, EventArgs e)
        //{
        //    if (picOriginal.Image != null)
        //    {
        //        btnApply.Enabled = false;
        //        this.Cursor = Cursors.WaitCursor;

        //        picConvolved.Image = FastBoxBlur(picOriginal.Image, (int)nAmount.Value);

        //        this.Cursor = Cursors.Default;
        //        btnApply.Enabled = true;
        //    }
        //}

        private Bitmap Convolve(Bitmap input, float[,] filter)
        {
            //Find center of filter
            int xMiddle = (int)Math.Floor(filter.GetLength(0) / 2.0);
            int yMiddle = (int)Math.Floor(filter.GetLength(1) / 2.0);

            //Create new image
            Bitmap output = new Bitmap(input.Width, input.Height);

            FastBitmap reader = new FastBitmap(input);
            FastBitmap writer = new FastBitmap(output);
            reader.LockImage();
            writer.LockImage();

            for (int x = 0; x < input.Width; x++)
            {
                for (int y = 0; y < input.Height; y++)
                {
                    float r = 0;
                    float g = 0;
                    float b = 0;

                    //Apply filter
                    for (int xFilter = 0; xFilter < filter.GetLength(0); xFilter++)
                    {
                        for (int yFilter = 0; yFilter < filter.GetLength(1); yFilter++)
                        {
                            int x0 = x - xMiddle + xFilter;
                            int y0 = y - yMiddle + yFilter;

                            //Only if in bounds
                            if (x0 >= 0 && x0 < input.Width &&
                                y0 >= 0 && y0 < input.Height)
                            {
                                Color clr = reader.GetPixel(x0, y0);

                                r += clr.R * filter[xFilter, yFilter];
                                g += clr.G * filter[xFilter, yFilter];
                                b += clr.B * filter[xFilter, yFilter];
                            }
                        }
                    }

                    //Normalize (basic)
                    if (r > 255)
                        r = 255;
                    if (g > 255)
                        g = 255;
                    if (b > 255)
                        b = 255;

                    if (r < 0)
                        r = 0;
                    if (g < 0)
                        g = 0;
                    if (b < 0)
                        b = 0;

                    //Set the pixel
                    writer.SetPixel(x, y, Color.FromArgb((int)r, (int)g, (int)b));
                }
            }

            reader.UnlockImage();
            writer.UnlockImage();

            return output;
        }

        /// <summary>
        /// Returns a box filter 1D kernel that is in the format {1,..,n}
        /// </summary>
        private float[,] GetHorizontalFilter(int size)
        {
            float[,] smallFilter = new float[size, 1];
            float constant = size;

            for (int i = 0; i < size; i++)
            {
                smallFilter[i, 0] = 1.0f / constant;
            }

            return smallFilter;
        }

        /// <summary>
        /// Returns a box filter 1D kernel that is in the format {1},...,{n}
        /// </summary>
        private float[,] GetVerticalFilter(int size)
        {
            float[,] smallFilter = new float[1, size];
            float constant = size;

            for (int i = 0; i < size; i++)
            {
                smallFilter[0, i] = 1.0f / constant;
            }

            return smallFilter;
        }

        /// <summary>
        /// Returns a box filter 2D kernel in the format {1,...,n},...,{1,...,n}
        /// </summary>
        private float[,] GetBoxFilter(int size)
        {
            float[,] filter = new float[size, size];
            float constant = size * size;

            for (int i = 0; i < filter.GetLength(0); i++)
            {
                for (int j = 0; j < filter.GetLength(1); j++)
                {
                    filter[i, j] = 1.0f / constant;
                }
            }

            return filter;
        }

        private Bitmap BoxBlur(Image img, int size)
        {
            //Apply a box filter by convolving the image with a 2D kernel
            return Convolve(new Bitmap(img), GetBoxFilter(size));
        }

        private Bitmap FastBoxBlur(Image img, int size)
        {
            //Apply a box filter by convolving the image with two separate 1D kernels (faster)
            return Convolve(Convolve(new Bitmap(img), GetHorizontalFilter(size)), GetVerticalFilter(size));
        }


        unsafe public class FastBitmap
        {
            private struct PixelData
            {
                public byte blue;
                public byte green;
                public byte red;
                public byte alpha;

                public override string ToString()
                {
                    return "(" + alpha.ToString() + ", " + red.ToString() + ", " + green.ToString() + ", " + blue.ToString() + ")";
                }
            }

            private Bitmap workingBitmap = null;
            private int width = 0;
            private BitmapData bitmapData = null;
            private Byte* pBase = null;

            public FastBitmap(Bitmap inputBitmap)
            {
                workingBitmap = inputBitmap;
            }

            public void LockImage()
            {
                Rectangle bounds = new Rectangle(Point.Empty, workingBitmap.Size);

                width = (int)(bounds.Width * sizeof(PixelData));
                if (width % 4 != 0) width = 4 * (width / 4 + 1);

                //Lock Image
                bitmapData = workingBitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                pBase = (Byte*)bitmapData.Scan0.ToPointer();
            }

            private PixelData* pixelData = null;

            public Color GetPixel(int x, int y)
            {
                pixelData = (PixelData*)(pBase + y * width + x * sizeof(PixelData));
                return Color.FromArgb(pixelData->alpha, pixelData->red, pixelData->green, pixelData->blue);
            }

            public Color GetPixelNext()
            {
                pixelData++;
                return Color.FromArgb(pixelData->alpha, pixelData->red, pixelData->green, pixelData->blue);
            }

            public void SetPixel(int x, int y, Color color)
            {
                PixelData* data = (PixelData*)(pBase + y * width + x * sizeof(PixelData));
                data->alpha = color.A;
                data->red = color.R;
                data->green = color.G;
                data->blue = color.B;
            }

            public void UnlockImage()
            {
                workingBitmap.UnlockBits(bitmapData);
                bitmapData = null;
                pBase = null;
            }
        }


    }
}
