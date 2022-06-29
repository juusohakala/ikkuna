using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ikkuna
{



    static class Program
    {

        private static NotifyIcon notifyIcon;

        static void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            notifyIcon.Visible = false;

            Application.Exit();
        }

        static void About(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/juusohakala/ikkuna") { UseShellExecute = true });
        }



        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());


            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new System.Drawing.Icon("logo.ico");
            notifyIcon.Visible = true;
            notifyIcon.Text = "ikkuna";


            notifyIcon.ContextMenuStrip = new ContextMenuStrip()
            {
                Items = {
                    new ToolStripMenuItem("About", null, About),
                    new ToolStripMenuItem("Exit", null, Exit) 
                }
            };




            //            private NotifyIcon notifyIcon;

            //private void Form1_Load(object sender, EventArgs e)
            //{
            //    notifyIcon = new NotifyIcon();
            //    notifyIcon.Icon = new System.Drawing.Icon("logo.ico");


            //    notifyIcon.ContextMenuStrip = new ContextMenuStrip()
            //    {
            //        Items = { new ToolStripMenuItem("Exit", null, Exit) }
            //    };

            //}

            //void Exit(object sender, EventArgs e)
            //{
            //    // Hide tray icon, otherwise it will remain shown until user mouses over it
            //    notifyIcon.Visible = false;

            //    Application.Exit();
            //}




            Application.Run(new CustomApplicationContext(new Form1()));
        }
    }



    /// <summary>
    /// An implementation of <see cref="ApplicationContext"/>.
    /// </summary>
    public class CustomApplicationContext : ApplicationContext
    {
        /// <summary>
        /// The main application form.
        /// </summary>
        private Form _mainForm;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomApplicationContext"/> class.
        /// </summary>
        /// <param name="mainForm">The main form of the application.</param>
        public CustomApplicationContext(Form mainForm)
        {
            _mainForm = mainForm;

            if (_mainForm != null)
            {
                // Wire up the destroy events similar to how the base ApplicationContext
                // does things when a form is provided.
                _mainForm.HandleDestroyed += OnFormDestroy;

                // We still want to call Show() here, but we can at least hide it from the user
                // by setting Opacity to 0 while the form is being shown for the first time.
                _mainForm.Opacity = 0;
                _mainForm.Show();
                _mainForm.Hide();
                _mainForm.Opacity = 1;
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.HandleDestroyed"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void OnFormDestroy(object sender, EventArgs e)
        {
            if (sender is Form form && !form.RecreatingHandle)
            {
                form.HandleDestroyed -= OnFormDestroy;
                OnMainFormClosed(sender, e);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// true if invoked from the <see cref="IDisposable.Dispose"/> method;
        /// false if invoked from the finalizer.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_mainForm != null)
                {
                    if (!_mainForm.IsDisposed)
                    {
                        _mainForm.Dispose();
                    }

                    _mainForm = null;
                }
            }

            base.Dispose(disposing);
        }
    }




}
