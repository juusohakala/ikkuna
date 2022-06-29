using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ikkuna
{
    /// <summary>
    /// Represents a single window. First time some action happens to active window, this object is created for it.
    /// </summary>
    public class Window
    {
        // Actual current dimensions, updated first at every call. In pixels
        public double X { get; set; }
        public double Y { get; set; }
        public double W { get; set; }
        public double H { get; set; }

        public double CenterX { get; set; }
        public double CenterY { get; set; }

        // Original floating window size, used when cycling fill slot on off
        public double OriginalX { get; set; }
        public double OriginalY { get; set; }
        public double OriginalW { get; set; }
        public double OriginalH { get; set; }

        public bool ActuallyFillsSlot { get; set; }
        public bool WantsToFillSlot { get; set; }
        public Slot Slot { get; set; }


        private const short SWP_NOMOVE = 0X2;
        private const short SWP_NOSIZE = 1;
        private const short SWP_NOZORDER = 0X4;
        private const int SWP_SHOWWINDOW = 0x0040;



        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr handle, int handleInsertAfter, int x, int y, int w, int h, int flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr handle, ref Rect rect);
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;        // x position of left side
            public int Top;         // y position of top side
            public int Right;       // x position of right side
            public int Bottom;      // y position of bottom side
        }

        //public string ProcessName { get; set; }
        public IntPtr WindowHandle { get; set; }



        public Window(IntPtr handle)
        {
            WindowHandle = handle;
        }


        //public double PixelToPercentageX(double x)
        //{
        //    var screenW = Screen.PrimaryScreen.Bounds.Width;
        //    return (x / screenW * 100);
        //}

        //public double PixelToPercentageY(double y)
        //{
        //    var screenH = Screen.PrimaryScreen.Bounds.Height;
        //    return (y / screenH * 100);
        //}

        //public double PercentageToPixelX(double x)
        //{
        //    var screenW = Screen.PrimaryScreen.Bounds.Width;
        //    return (screenW * x / 100);
        //}

        //public double PercentageToPixelY(double y)
        //{
        //    var screenH = Screen.PrimaryScreen.Bounds.Height;
        //    return (screenH * y / 100);
        //}




        public void MoveToSlot(Slot slot = null)
        {
            if (slot != null)
            {
                Slot = slot;
            }


            if (W > Slot.W)
            {
                W = Slot.W;
                X = Slot.X;
                OriginalX = Slot.X;
            }
            else
            {
                X = Slot.X + Slot.W / 2 - W / 2;
                OriginalX = Slot.X + Slot.W / 2 - OriginalW / 2;
            }


            if (H > Slot.H)
            {
                H = Slot.H;
                Y = Slot.Y;
                OriginalY = Slot.Y;
            }
            else
            {
                Y = Slot.Y + Slot.H / 2 - H / 2;
                OriginalY = Slot.Y + Slot.H / 2 - OriginalH / 2;
            }









        }



        public void UpdateFirst()
        {
            var rect = new Rect();
            if (!GetWindowRect(WindowHandle, ref rect))
            {
                throw new Exception("Can't read window bounds");
            }

            X = rect.Left;
            Y = rect.Top;
            W = rect.Right - rect.Left;
            H = rect.Bottom - rect.Top;
            CenterX = X + W / 2;
            CenterY = Y + H / 2;

            if (Slot != null)
            {
                if (Eq(W, Slot.W) && Eq(H, Slot.H) && Eq(X, Slot.X) && Eq(Y, Slot.Y))
                //if (X == Slot.X && Y == Slot.Y && W == Slot.W && H == Slot.H)
                {
                    ActuallyFillsSlot = true;
                }
                else
                {
                    ActuallyFillsSlot = false;
                }
            }


        }

        public void CycleResizeToFillSlot()
        {
            WantsToFillSlot = true;
            //if (FillsSlot)
            //{
            //    X = OriginalX;
            //    Y = OriginalY;
            //    W = OriginalW;
            //    H = OriginalH;

            //}
            //else
            {
                OriginalX = X;
                OriginalY = Y;
                OriginalW = W;
                OriginalH = H;

                ResizeToFillSlot();
            }

        }

        public void ResizeToFillSlot()
        {
            X = Slot.X;
            Y = Slot.Y;
            W = Slot.W;
            H = Slot.H;
            ActuallyFillsSlot = true;
        }

        public void UpdateLast()
        {

            SetPosByPixels(X, Y, W, H);

        }




        public void SetPosByPercentage(double x, double y, double w = 0, double h = 0)
        {
            var screenW = Screen.PrimaryScreen.Bounds.Width;
            var screenH = Screen.PrimaryScreen.Bounds.Height;

            SetWindowPos(
                WindowHandle,                                   //window handle
                0,                                              //??
                Convert.ToInt32(screenW * x / 100),             //x
                Convert.ToInt32(screenH * y / 100),             //y
                Convert.ToInt32(screenW * w / 100),             //w
                Convert.ToInt32(screenH * h / 100),             //h
                w != 0 || h != 0 ? 0 : SWP_NOSIZE               //flags
                                                                //SWP_NOZORDER | SWP_NOSIZE | SWP_SHOWWINDOW
            );
        }

        public void SetPosByPixels(double x, double y, double w = 0, double h = 0)
        {
            var screenW = Screen.PrimaryScreen.Bounds.Width;
            var screenH = Screen.PrimaryScreen.Bounds.Height;

            SetWindowPos(
                WindowHandle,                                   //window handle
                0,                                              //??
                Convert.ToInt32(x),             //x
                Convert.ToInt32(y),             //y
                Convert.ToInt32(w),             //w
                Convert.ToInt32(h),             //h
                w != 0 || h != 0 ? 0 : SWP_NOSIZE               //flags
                                                                //SWP_NOZORDER | SWP_NOSIZE | SWP_SHOWWINDOW
            );
        }



        public static bool Eq(double a, double b)
        {
            // As the pixel values are also double, we need this to compare those.
            // If two pixel positions are less than 0.5px from each other, they are considered same
            return Math.Abs(a - b) < 0.5;
        }

    }
}
