using System;
using System.Diagnostics;
using System.Windows.Forms;
using HTControlHTPCTrayApplication.Properties;

namespace HTControlHTPCTrayApplication
{
    /// <summary>
    /// Class to capsulate NotifyIcon as per http://www.codeproject.com/Articles/290013/Formless-System-Tray-Application
    /// </summary>
    class ProcessIcon : IDisposable
    {
        /// <summary>
        /// The NotifyIcon object.
        /// </summary>
        private NotifyIcon ni;                        
        private bool cancel;
        private bool balloonVisible;
        private int comPortNumber; //container for comport number to be able to show it at double click

        public bool Cancelled 
        {
            get { return this.cancel; }              
        }
        
        public bool BalloonVibible
        {
            get { return this.balloonVisible; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessIcon"/> class.
        /// </summary>
        public ProcessIcon()
        {
            // Instantiate the NotifyIcon object.
            ni = new NotifyIcon();
        }

        /// <summary>
        /// Displays the icon in the system tray.
        /// </summary>
        public void Display(int comPortNumber)
        {
            // Put the icon in the system tray and allow it react to mouse clicks.			
            //ni.MouseClick += new MouseEventHandler(ni_MouseClick);

            ni.BalloonTipClicked += new System.EventHandler(this.ni_BalloonTipClicked);
            ni.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ni_MouseDoubleClick);
            ni.BalloonTipClosed += new System.EventHandler(this.ni_BalloonTipClosed);

            ni.Icon = Resources.AppIcon;                        
            ni.Text = "HT Control HTPC Tray Application";
            ni.Visible = true;

            // Attach a context menu.
            //ni.ContextMenuStrip = new ContextMenus().Create();
            this.comPortNumber = comPortNumber;
        }

        private void ni_BalloonTipClosed(object sender, EventArgs e)
        {
            //Console.WriteLine("Balloon closed!");
            balloonVisible = false;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            // When the application closes, this will remove the icon from the system tray immediately.
            ni.Dispose();
        }

        /// <summary>
        /// Handles the MouseClick event of the ni control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        void ni_MouseClick(object sender, MouseEventArgs e)
        {
            // Handle mouse button clicks.
            if (e.Button == MouseButtons.Left)
            {
                // Start Windows Explorer.
                //Process.Start("explorer", null);
                MessageBox.Show("left click!");
            }
            else if (e.Button == MouseButtons.Right)
            {                
                MessageBox.Show("right click!");            
            }
        }

        private void ni_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DialogResult res = MessageBox.Show(null, "Listening for commands on COM" + this.comPortNumber + Environment.NewLine + Environment.NewLine + "Kill application?", "HT Control HTPC Tray Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {                
                Application.Exit();
            }
        }

        private void ni_BalloonTipClicked(object sender, EventArgs e)
        {
            //Balloon was clicked, cancel shutdown
            cancel = true;
            balloonVisible = false;            
            MessageBox.Show(null, "HTPC Shutdown Cancelled", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }
         
        public void ShowBalloonTip()
        {
            cancel = false;
            balloonVisible = true;
            ni.ShowBalloonTip(10000, "HT Shutdown Initiated", "Click this shit to cancel", ToolTipIcon.Warning);
        }
    }
}
