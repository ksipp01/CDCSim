﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;



namespace ASCOM.SimCDC
{
    class ScreenCapture
    {

        Camera camera;
    //    SetupDialogForm sdf = new SetupDialogForm();
        Blur blr = new Blur();
        //   SetupDialogForm setup = new SetupDialogForm();
        //  string fullPath = @"C:\Users\Public\CDC_CamSimulator\CDC Camera Simulator\CDC Camera Simulator";
        //  string fullPath = @"C:\Users\Public\CDC_CamSimulator\CDC Camera Simulator\CDC Camera Simulator\bin\Debug";
        //   string fullPath = Path.GetDirectoryName(SetupDialogForm.CapturePath);
        string fullPath = SetupDialogForm.CapturePath;
       // string fullPath = SetupDialogForm.CapturePath;

        //    string fullPath = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly((this.GetType())).Location);
        /// <summary>
        /// Creates an Image object containing a screen shot of the entire desktop
        /// </summary>
        /// <returns></returns>
        public Image CaptureScreen()
        {
            return CaptureWindow(User32.GetDesktopWindow());
        }
        /// <summary>
        /// Creates an Image object containing a screen shot of a specific window
        /// </summary>
        /// <param name="handle">The handle to the window. 
        /// (In windows forms, this is obtained by the Handle property)</param>
        /// <returns></returns>
        public Image CaptureWindow(IntPtr handle)
        {
            // get te hDC of the target window
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            // get the size
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);

         

            //   int width = windowRect.right - windowRect.left;
            //   int height = windowRect.bottom - windowRect.top;

            //int width = 800;
            //int height = 600;
            int width = SetupDialogForm.Width;
            int height = SetupDialogForm.Height;
            int xPoint = SetupDialogForm.xPoint;
            int yPoint = SetupDialogForm.yPoint;


            // create a device context we can copy to
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            // bitblt over
           
            //   GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 500, 300, GDI32.SRCCOPY);
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, xPoint, yPoint, GDI32.SRCCOPY);
            // restore selection
            GDI32.SelectObject(hdcDest, hOld);
            // clean up
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            GDI32.DeleteObject(hBitmap);
            return img;
        }
        /// <summary>
        /// Captures a screen shot of a specific window, and saves it to a file
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="filename"></param>
        /// <param name="format"></param>

       private int focusPos = 25000;

        public void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format)
        {
            if (System.IO.File.Exists(filename))
            {
                System.IO.File.Delete(filename);
            }

            Image img = CaptureWindow(handle);
            Bitmap bmp = (Bitmap)img;
      //      int pos = setup.focuser.Position;

          //  int pos = SetupDialogForm.focuser.Position;
         //   int amount = Math.Abs(pos / 100 - focusPos  / 100);
            if (SetupDialogForm.FocusStepSize != 0)  // first run capture can't use this.  
            {
                int amount = Math.Abs(SetupDialogForm.focuser.Position / SetupDialogForm.FocusStepSize - SetupDialogForm.FocusPoint / SetupDialogForm.FocusStepSize);
                if (amount > 10)
                    amount = 10;
                //   if (((focusPos - pos) > 100) || ((pos-focusPos > 100)))
                if (amount > 0)
                    img = blr.ApplyBlur(bmp, amount +1);
            }
            img.Save(filename, format);
        }
        /// <summary>
        /// Captures a screen shot of the entire desktop, and saves it to a file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public void CaptureScreenToFile(string filename, ImageFormat format)
        {
            Image img = CaptureScreen();
            img.Save(filename, format);
        }

        /// <summary>
        /// Helper class containing Gdi32 API functions
        /// </summary>
        private class GDI32
        {

            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        /// <summary>
        /// Helper class containing User32 API functions
        /// </summary>
        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
        }



        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lp1, string lp2);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        // add
       // [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
       // public static extern int GetSystemMetrics(int which);
       // [DllImport("user32.dll")]
       // public static extern void
       //SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter,
       //             int X, int Y, int width, int height, uint flags);
       // private static IntPtr HWND_TOP = IntPtr.Zero;
       // private const int SWP_SHOWWINDOW = 64; // 0x0040
       // private const int SM_CXSCREEN = 0;
       // private const int SM_CYSCREEN = 1;
       // public static int ScreenX
       // {
       //     get { return GetSystemMetrics(SM_CXSCREEN); }
       // }

       // public static int ScreenY
       // {
       //     get { return GetSystemMetrics(SM_CYSCREEN); }
       // }
       // public static void SetWinFullScreen(IntPtr hwnd)
       // {
       //     SetWindowPos(hwnd, HWND_TOP, 0, 0, ScreenX, ScreenY, SWP_SHOWWINDOW);
       // }

//end add



        public void GetCapture()
        {

            IntPtr handle = FindWindow("Window", "Cartes du Ciel - Chart_1");
            SetForegroundWindow(handle);
    //        SetWinFullScreen(handle);
     //       System.Threading.Thread.Sleep(500);
            ScreenCapture sc = new ScreenCapture();
            Image img = sc.CaptureScreen();
        //    sc.CaptureWindowToFile(handle, "C:\\atest3\\TestCapture.jpg", ImageFormat.Jpeg);
        //    sc.CaptureWindowToFile(handle, @"C:\Users\Public\CDC_CamSimulator\CDC Camera Simulator\CDC Camera Simulator\bin\Debug\TestCapture.jpg", ImageFormat.Jpeg);
       //     sc.CaptureWindowToFile(handle,  Path.Combine(fullPath, @"TestCapture.bmp"), ImageFormat.Bmp);
       //     sc.CaptureWindowToFile(handle, Path.Combine(fullPath, @"SimCapture.jpg"), ImageFormat.Jpeg);
            sc.CaptureWindowToFile(handle, Path.Combine(fullPath, @"SimCapture.jpg"), ImageFormat.Jpeg);
        }
    }
}
