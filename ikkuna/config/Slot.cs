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

        public void SetPositionByCenter(double centerX, double centerY)
        {
            X = centerX - W / 2;
            Y = centerY - H / 2;
        }

        public double CenterX()
        {
            return X + W / 2;
        }

        public double CenterY()
        {
            return Y + H / 2;
        }

        public void ResizeForReal(double resizeW, double resizeH, double moveX, double moveY)
        {

        }

        public void Resize(List<Slot> allSlots, double resizeW, double resizeH)
        {
            var screenW = Screen.PrimaryScreen.WorkingArea.Width;
            var screenH = Screen.PrimaryScreen.WorkingArea.Height;


            if (resizeW != 0)
            {
                // for example: N means slot in "nextRightSlots"
                //_____         ___N_
                //_____         ___N_
                //__X__   ->   __XN_
                //_____         ___N_
                //_____         ___N_

                var nextRightSlots = allSlots.Where(s => Eq(X + W, s.X));
                var nextLeftSlots = allSlots.Where(s => Eq(s.X + s.W, X));


                foreach (var n in nextRightSlots)
                {
                    //slot.ResizeRecursive(allSlots, -resizeW, 0, 0, 0, Way.Right);

                    // for example: D means slot in "deeperRightSlots" ( for example first iteration of "nextRightSlots")
                    //_____         ___N_         ___ND
                    //_____         ___N_         ___N_
                    //__X__   ->   __XN_   ->   __XN_
                    //_____         ___N_         ___N_
                    //_____         ___N_         ___N_

                    var deeperRightSlots = allSlots.Where(s => n.CenterY() < s.Y + s.H && n.CenterY() > s.Y && s.X >= n.X).OrderBy(s => s.X).ToList();
                    var i = 0;
                    foreach (var d in deeperRightSlots)
                    {
                        var dividedResizeW = resizeW / 2 / deeperRightSlots.Count;

                        //resize for real:
                        d.W -= dividedResizeW;
                        d.X += resizeW / 2 - dividedResizeW * i;

                        i++;
                    }

                }

                // melkein sama ku oikealle, koida saada yhdistettyä!

                foreach (var n in nextLeftSlots)
                {
                    //slot.ResizeRecursive(allSlots, -resizeW, 0, 0, 0, Way.Left);


                    var deeperLeftSlots = allSlots.Where(s => n.CenterY() < s.Y + s.H && n.CenterY() > s.Y && s.X <= n.X).OrderBy(s => s.X).ToList();
                    var i = 0;
                    foreach (var d in deeperLeftSlots)
                    {
                        var dividedResizeW = resizeW / 2 / deeperLeftSlots.Count;

                        //resize for real:
                        d.W -= dividedResizeW;
                        d.X -= dividedResizeW * i;
                        //var newCenterX = d.CenterX() - dividedResizeW / 2;
                        //d.SetPositionByCenter(newCenterX, d.CenterY());

                        i++;
                    }
                }
            }




            //if (resizeH != 0)
            //{
            //    //näissä ei voi käyttää centeriä!
            //    var nextDownSlots = allSlots.Where(s => Eq(Y + H, s.Y));
            //    var nextUpSlots = allSlots.Where(s => Eq(s.Y + s.H, Y));
            //}



            // leikkaava toteutus: (keskellä oleva ikkuna suurenee enemmän)

            var oldX = X;
            var oldY = Y;
            var oldW = W;

            X -= resizeW / 2;
            Y -= resizeH / 2;
            W += resizeW;
            H += resizeH;

            if (Eq(oldX,  0))
            {
                W += X;
                X = 0;
            }
            if (Eq(oldX + oldW, screenW))
            {
                //en oo varma
                W = screenW - X;
            }

            if (X < 0)
            {
                W += X;
                X = 0;
            }
            if (X + W > screenW)
            {
                W = screenW - X;
            }



            //var newCenterX = CenterX() - resizeW / 4;
            //var newCenterY = CenterY() - resizeH / 4;

            //var oldCenterX = CenterX();
            //var oldCenterY = CenterY();
            //W += resizeW;
            //H += resizeH;

            //SetPositionByCenter(oldCenterX, oldCenterY);


            // make sure slot is not resized beyond screen size


            //if (W > screenW) W = screenW;
            //if (H > screenH) H = screenH;



            // same to y

        }


        public void ResizeRecursive(List<Slot> allSlots, double resizeW, double resizeH, double moveX, double moveY, Way way)
        {
            //var screenW = Screen.PrimaryScreen.WorkingArea.Width;
            //var screenH = Screen.PrimaryScreen.WorkingArea.Height;

            //var newMoveX = 0;
            //var newMoveY = 0;
            //var newResizeW = 0;
            //var newResizeH = 0;

            //var nextSlots = new List<Slot>();

            //if (way == Way.Right)
            //{
            //    nextSlots = allSlots.Where(s => Eq(X + W, s.X)).ToList();
            //    foreach (var slot in nextSlots)
            //    {
            //        slot.ResizeRecursive(allSlots, resizeW / 2, resizeH / 2, resizeW / 2, resizeH / 2, way);
            //    }
            //}
            //else if (way == Way.Left)
            //{
            //    nextSlots = allSlots.Where(s => Eq(s.X + s.W, X)).ToList();
            //    foreach (var slot in nextSlots)
            //    {
            //        slot.ResizeRecursive(allSlots, resizeW / 2, resizeH / 2, resizeW / 2, resizeH / 2, way);
            //    }
            //}


            var nextSlots = new List<Slot>();

            if (way == Way.Right)
            {
                nextSlots = allSlots.Where(slot => CenterY() < slot.Y + slot.H && CenterY() > slot.Y && slot.X > CenterY()).ToList();
                foreach (var slot in nextSlots)
                {
                    slot.ResizeRecursive(allSlots, resizeW / 2, resizeH / 2, resizeW / 2, resizeH / 2, way);
                }
            }
            else if (way == Way.Left)
            {
                nextSlots = allSlots.Where(slot => CenterY() < slot.Y + slot.H && CenterY() > slot.Y && slot.X + slot.W < CenterY()).ToList();
                foreach (var slot in nextSlots)
                {
                    slot.ResizeRecursive(allSlots, resizeW / 2, resizeH / 2, resizeW / 2, resizeH / 2, way);
                }
            }







            // actual moving and resizing
            //X += moveX / 2;
            //Y += moveY / 2;
            //W += resizeW / 2;
            //H += resizeH / 2;

            var newCenterX = CenterX() + resizeW / 4;
            var newCenterY = CenterY() + resizeH / 4;
            W += resizeW / 2;
            H += resizeH / 2;

            SetPositionByCenter(newCenterX, newCenterY);



            // make sure slot is not resized beyond screen size
            //var screenW = Screen.PrimaryScreen.WorkingArea.Width;
            //var screenH = Screen.PrimaryScreen.WorkingArea.Height;

            //if (W > screenW) W = screenW;
            //if (H > screenH) H = screenH;

            //if (X < 0) X = 0;
            //if (X + W > screenW) X = screenW - W;

            //if (Y < 0) Y = 0;
            //if (Y + H > screenH) Y = screenH - H;
        }

    }

}
