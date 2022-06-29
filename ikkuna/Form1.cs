using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ikkuna
{
    public partial class Form1 : Form
    {



        //public static bool Eq(double a, double b)
        //{
        //    // As the pixel values are also double, we need this to compare those.
        //    // If two pixel positions are less than 0.5px from each other, they are considered same
        //    return Math.Abs(a - b) < 2;
        //}

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }


        public const int WM_HOTKEY = 0x0312;



        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();


        //private delegate bool EnumWindowsProc(IntPtr handle, int lParam);

        //[DllImport("USER32.DLL")]
        //private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        //[DllImport("USER32.DLL")]
        //private static extern int GetWindowText(IntPtr handle, StringBuilder lpString, int nMaxCount);

        //[DllImport("USER32.DLL")]
        //private static extern int GetWindowTextLength(IntPtr handle);

        //[DllImport("USER32.DLL")]
        //private static extern bool IsWindowVisible(IntPtr handle);

        //[DllImport("USER32.DLL")]
        //private static extern IntPtr GetShellWindow();



        /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
        /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
        //public static IDictionary<IntPtr, string> GetOpenWindows()
        //{
        //	var shellWindow = GetShellWindow();
        //	var windows = new Dictionary<IntPtr, string>();

        //	EnumWindows(delegate (IntPtr handle, int lParam)
        //	{
        //		if (handle == shellWindow) return true;
        //		if (!IsWindowVisible(handle)) return true;

        //		int length = GetWindowTextLength(handle);
        //		if (length == 0) return true;

        //		StringBuilder builder = new StringBuilder(length);
        //		GetWindowText(handle, builder, length + 1);

        //		windows[handle] = builder.ToString();
        //		return true;

        //	}, 0);

        //	return windows;
        //}












        public Config Config { get; set; }
        public Dictionary<IntPtr, Window> Windows { get; set; }



        public Form1()
        {


            //Config = new Config()
            //{


            //    Slots = new List<Slot>()
            //        {
            //            new Slot()
            //            {
            //                SlotName = "left",
            //                X = 0,
            //                Y = 0,
            //                Width = 50,
            //                Height = 100
            //            },
            //            new Slot()
            //            {
            //                SlotName = "right",
            //                X = 50,
            //                Y = 0,
            //                Width = 50,
            //                Height = 100
            //            }

            //    },
            //    Hotkeys = new List<Hotkey>()
            //    {
            //        new Hotkey()
            //        {
            //            Key = "alt+left",
            //            MoveToSlot = "left"
            //        },
            //        new Hotkey()
            //        {
            //            Key = "alt+right",
            //            MoveToSlot = "right"
            //        },
            //        new Hotkey()
            //        {
            //            Key = "alt+up",
            //            ResizeToFillSlot = true
            //        }
            //    }
            //};


            //var configString = JsonConvert.SerializeObject(Config);
            //Debug.WriteLine(configString);

            Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config/config3.json"));

            var configString = JsonConvert.SerializeObject(Config);
            Debug.WriteLine(configString);

            InitializeComponent();
            Windows = new Dictionary<IntPtr, Window>();

            foreach (var hotkey in Config.Hotkeys)
            {
                hotkey.Register(Handle);
            }


            //Hotkeys.Add(new Hotkey("alt+left", Handle));
            //Hotkeys.Add(new Hotkey("alt+right", Handle));
            //Hotkeys.Add("up", new Hotkey("alt+up", Handle));
            //Hotkeys.Add("down", new Hotkey("alt+down", Handle));

            //Hotkeys.Add("align", new Hotkey("alt+q", Handle));


        }

        protected override void WndProc(ref Message m)
        {

            //var pros = new Dictionary<IntPtr, string>();
            //var processes = Process.GetProcesses();

            //foreach (var process in processes)
            //{
            //	if (!String.IsNullOrEmpty(process.MainWindowTitle))
            //	{
            //		pros.Add(process.MainWindowHandle, process.MainWindowTitle);
            //	}
            //}


            //if any hotkey is pressed
            if (m.Msg == WM_HOTKEY)
            {
                // get hotkey id from message
                int id = m.WParam.ToInt32();


                // create a new Window object if there isn't any
                var activeWindowHandle = GetForegroundWindow();
                if (!Windows.ContainsKey(activeWindowHandle))
                {
                    Windows.Add(activeWindowHandle, new Window(activeWindowHandle));
                }

                var activeWindow = Windows[activeWindowHandle];
                activeWindow.UpdateFirst();

                foreach (var hotkey in Config.Hotkeys)
                {
                    if (hotkey.IsHit(id))
                    {
                        // MoveToSlot
                        if (!string.IsNullOrWhiteSpace(hotkey.MoveToSlot))
                        {
                            var slot = Config.GetSlotByName(hotkey.MoveToSlot);
                            activeWindow.MoveToSlot(slot);
                        }

                        // MoveRight
                        if (hotkey.MoveRight)
                        {
                            var rightSlots = Config.Slots.Where(slot => activeWindow.CenterY < slot.Y + slot.H && activeWindow.CenterY > slot.Y && slot.X > activeWindow.CenterX);
                            var closestSlot = rightSlots.OrderBy(slot => slot.X).FirstOrDefault();
                            if (closestSlot != null) //tähän olis kiva löytää joku slotti kuitenkin
                            {
                                activeWindow.MoveToSlot(closestSlot);
                            }
                        }

                        // MoveLeft
                        if (hotkey.MoveLeft)
                        {
                            var leftSlots = Config.Slots.Where(slot => activeWindow.CenterY < slot.Y + slot.H && activeWindow.CenterY > slot.Y && slot.X + slot.W < activeWindow.CenterX);
                            var closestSlot = leftSlots.OrderByDescending(slot => slot.X).FirstOrDefault();
                            if (closestSlot != null)
                            {
                                activeWindow.MoveToSlot(closestSlot);
                            }
                        }

                        // ResizeToFillSlot
                        if (hotkey.ResizeToFillSlot)
                        {
                            // if slot not found, get closest one
                            if (activeWindow.Slot == null)
                            {
                                activeWindow.Slot = Config.Slots[0];
                                foreach (var slot in Config.Slots)
                                {
                                    //tähän voisi vaihtaa centerit pisteiksi
                                    if (Distance(activeWindow.X, activeWindow.Y, slot.X, slot.Y)
                                        < Distance(activeWindow.X, activeWindow.Y, activeWindow.Slot.X, activeWindow.Slot.Y))
                                    {
                                        activeWindow.Slot = slot;
                                    }
                                }
                            }
                            activeWindow.CycleResizeToFillSlot();
                        }

                        // Resize
                        if (hotkey.Resize != 0)
                        {
                            var screenW = Screen.PrimaryScreen.WorkingArea.Width;
                            var screenH = Screen.PrimaryScreen.WorkingArea.Height;
                            var sizeUnit = hotkey.Resize / 100 * screenH;

                            if (activeWindow.ActuallyFillsSlot)
                            {


                                activeWindow.Slot.Resize(Config.Slots, sizeUnit, 0);



                                //var rightSlots = Config.Slots.Where(slot => activeWindow.CenterY < slot.Y + slot.H && activeWindow.CenterY > slot.Y && slot.X > activeWindow.CenterX);
                                //var leftSlots = Config.Slots.Where(slot => activeWindow.CenterY < slot.Y + slot.H && activeWindow.CenterY > slot.Y && slot.X + slot.W < activeWindow.CenterX);

                                //// slot is in between other slots
                                //if(rightSlots.Any() && leftSlots.Any())
                                //{
                                //    activeWindow.Slot.W += sizeUnit;
                                //    activeWindow.Slot.X -= sizeUnit / 2;

                                //    foreach(var rightSlot in rightSlots)
                                //    {
                                //        rightSlot.W -= sizeUnit / rightSlots.Count();
                                //        rightSlot.X += sizeUnit / 2 / rightSlots.Count();
                                //    }

                                //    foreach (var leftSlot in leftSlots)
                                //    {
                                //        leftSlot.W -= sizeUnit / leftSlots.Count();
                                //        leftSlot.X += sizeUnit / 2 / leftSlots.Count();
                                //    }

                                //}


                                //var widthChange = activeWindow.Slot.W = activeWindow.Slot.W ;
                                //activeWindow.Slot.X = activeWindow.Slot.X - (activeWindow.Slot.W - activeWindow.Slot.W) / 2;


                                foreach (var window in Windows.Values)
                                {
                                    if (window.Slot != null)
                                    {
                                        //if (window.ActuallyFillsSlot)
                                        //{
                                        window.ResizeToFillSlot();
                                        //}
                                        //else
                                        //{
                                        //    window.MoveToSlot();
                                        //}

                                        window.UpdateLast();
                                    }
                                }
                            }
                            else
                            {

                                activeWindow.W += sizeUnit;
                                activeWindow.X -= sizeUnit / 2;
                                activeWindow.H += sizeUnit;
                                activeWindow.Y -= sizeUnit / 2;

                                //var oldWidth = activeWindow.W;
                                //activeWindow.W = activeWindow.W * (100 + hotkey.Resize) / 100;
                                //activeWindow.X = activeWindow.X - (activeWindow.W - oldWidth) / 2;

                                //var oldHeight = activeWindow.H;
                                //activeWindow.H = activeWindow.H * (100 + hotkey.Resize) / 100;
                                //activeWindow.Y = activeWindow.Y - (activeWindow.H - oldHeight) / 2;


                                // Make sure window is not resized beyond its slot size
                                if (activeWindow.Slot != null)
                                {
                                    if (activeWindow.X < activeWindow.Slot.X)
                                        activeWindow.X = activeWindow.Slot.X;
                                    if (activeWindow.W > activeWindow.Slot.W)
                                        activeWindow.W = activeWindow.Slot.W;


                                    if (activeWindow.Y < activeWindow.Slot.Y)
                                        activeWindow.Y = activeWindow.Slot.Y;
                                    if (activeWindow.H > activeWindow.Slot.H)
                                        activeWindow.H = activeWindow.Slot.H;

                                }

                            }
                        }
                    }
                }

                activeWindow.UpdateLast();




                //if (Hotkeys["left"].IsHit(id))
                //{
                //    var slot = Layout.GetSlotByName("left");
                //    Windows[activeWindowHandle].SetPosByPercentage(slot.X, slot.Y, slot.Width, slot.Height);
                //}

                //if (Hotkeys["right"].IsHit(id))
                //{
                //    var slot = Layout.GetSlotByName("right");
                //    Windows[activeWindowHandle].SetPosByPercentage(slot.X, slot.Y, slot.Width, slot.Height);
                //}



                //if (Hotkeys["left"].IsHit(id))
                //{
                //	Overlay.Refresh();
                //}
            }






            base.WndProc(ref m);
        }
    }
}

