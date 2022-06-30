using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;

namespace ikkuna
{


    /// <summary>
    /// One slot of the layout. 
    /// </summary>
    public class Slot
    {
        private int screenW = Screen.PrimaryScreen.WorkingArea.Width;
        private int screenH = Screen.PrimaryScreen.WorkingArea.Height;




        public static bool Eq(double a, double b)
        {
            // As the pixel values are also double, we need this to compare those.
            // If two pixel positions are less than 0.5px from each other, they are considered same
            return Math.Abs(a - b) < 0.5;
        }



        public string SlotName { get; set; }
        public int Layer { get; set; }
        public int Display { get; set; } // 0 is for all displays

        [OnDeserialized]
        internal void ConvertPercentageValuesToPixel(StreamingContext context)
        {
            var screenW = Screen.PrimaryScreen.WorkingArea.Width;
            var screenH = Screen.PrimaryScreen.WorkingArea.Height;

            X = screenW * Xp / 100;
            Y = screenH * Yp / 100;
            W = screenW * Wp / 100;
            H = screenH * Hp / 100;
        }

        // original size X
        // original Y .....


        // Pixel sizes, mainly used in the code
        [JsonIgnore]
        public double X { get; set; }
        [JsonIgnore]
        public double Y { get; set; }
        [JsonIgnore]
        public double W { get; set; }
        [JsonIgnore]
        public double H { get; set; }

        // Percentage sizes for configuration script
        [JsonProperty("X")]
        public double Xp { get; set; }
        [JsonProperty("Y")]
        public double Yp { get; set; }
        [JsonProperty("Width")]
        public double Wp { get; set; }
        [JsonProperty("Height")]
        public double Hp { get; set; }


        public enum Way
        {
            Right,
            Down,
            Left,
            Up
        }


        public double CenterX
        {
            get
            {
                return X + W / 2;
            }
        }

        public double CenterY
        {
            get
            {
                return Y + H / 2;
            }
        }

        public void ResizeForReal(double resizeW, double resizeH, double moveX, double moveY)
        {

        }

        public void ResizeLeft(List<Slot> allSlots, double resize)
        {
            if (resize == 0) return;

            var nextLeftSlots = allSlots.Where(s => Eq(s.X + s.W, X));

            foreach (var n in nextLeftSlots)
            {
                //slot.ResizeRecursive(allSlots, -resizeW, 0, 0, 0, Way.Left);


                var deeperLeftSlots = allSlots.Where(s => n.CenterY < s.Y + s.H && n.CenterY > s.Y && s.X <= n.X).OrderBy(s => s.X).ToList();
                var i = 0;
                foreach (var d in deeperLeftSlots)
                {
                    var dividedResizeW = resize / deeperLeftSlots.Count;

                    //resize for real:
                    d.W -= dividedResizeW;
                    d.X -= dividedResizeW * i;
                    //var newCenterX = d.CenterX() - dividedResizeW / 2;
                    //d.SetPositionByCenter(newCenterX, d.CenterY());

                    i++;
                }
            }

            // actual resize:


            W += Math.Min(resize, X);
            X -= Math.Min(resize, X);


        }

        public void ResizeRight(List<Slot> allSlots, double resize)
        {
            if (resize == 0) return;

            // for example: N means slot in "nextRightSlots"
            //_____         ___N_
            //_____         ___N_
            //__X__   ->   __XN_
            //_____         ___N_
            //_____         ___N_

            var nextRightSlots = allSlots.Where(s => Eq(X + W, s.X));



            foreach (var n in nextRightSlots)
            {
                //slot.ResizeRecursive(allSlots, -resizeW, 0, 0, 0, Way.Right);

                // for example: D means slot in "deeperRightSlots" ( for example first iteration of "nextRightSlots")
                //_____         ___N_         ___ND
                //_____         ___N_         ___N_
                //__X__   ->   __XN_   ->   __XN_
                //_____         ___N_         ___N_
                //_____         ___N_         ___N_

                var deeperRightSlots = allSlots.Where(s => n.CenterY < s.Y + s.H && n.CenterY > s.Y && s.X >= n.X).OrderBy(s => s.X).ToList();
                var i = 0;
                foreach (var d in deeperRightSlots)
                {
                    var dividedResizeW = resize / deeperRightSlots.Count;

                    //resize for real:
                    d.W -= dividedResizeW;
                    d.X += resize - dividedResizeW * i;

                    i++;
                }

            }


            // actual resize:


            W += Math.Min(resize, screenW - X + W);


        }

        public void Resize(List<Slot> allSlots, double resizeW, double resizeH)
        {



            if (resizeW != 0)
            {
                if (Eq(X, 0))
                {
                    ResizeRight(allSlots, resizeW); // if slot is leftest, resize everythiung to right
                }

                else if (Eq(X + W, screenW))
                {
                    ResizeLeft(allSlots, resizeW);
                }

                else
                {
                    ResizeRight(allSlots, resizeW / 2);
                    ResizeLeft(allSlots, resizeW / 2);
                }
            }



        }



    }

}
