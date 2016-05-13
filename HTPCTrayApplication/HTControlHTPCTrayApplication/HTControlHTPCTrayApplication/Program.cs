using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HTControlHTPCTrayApplication
{
	static class Program
	{
		static SerialPort serialPort;
		static int comPortNumber = 0;
        static bool testMode = false;
        static ProcessIcon pi = new ProcessIcon();
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			            
            try
            {
                //read settings from settings.txt file
                readSettings();

                //open serial port 
                openPort();

                //show tray icon
                pi.Display(comPortNumber);
                
                //make sure the application runs
                Application.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show(null, ex.Message, "Application cannot run", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                closePort();
                if (pi != null) pi.Dispose();
            }
		}

		private static void openPort()
		{
            try
            {            
                serialPort = new SerialPort("COM" + comPortNumber, 9600, Parity.None, 8, StopBits.One);
                serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                serialPort.Open();
                Console.WriteLine("Serial port in " + serialPort.PortName + " opened...");
            }
            catch (Exception)
            {
                throw new Exception("HTControlHTPCTrayApplication: Cannot open serial port in COM" + comPortNumber);                
            }			
		}

		private static void closePort()
		{
            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                    Console.WriteLine("Serial port in " + serialPort.PortName + " closed.");
                }
            }
		}

		private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
		{
			SerialPort sp = (SerialPort)sender;
			string indata = sp.ReadLine();

			Console.WriteLine("Data Received: ");
			Console.Write(indata);

			if (indata == "HTOFF\r")
			{
                ShutDownProcedure();                
            }
		}

        private static void ShutDownProcedure()
        {
            pi.ShowBalloonTip();
            
            while (pi.BalloonVibible) 
            {
                //Wait for user to cancel while ballon is visible
            }

            if (!pi.Cancelled)
            {
                if (testMode)
                {
                    MessageBox.Show("At this point the PC would be forced to shutdown");
                }
                else
                {
                    Console.WriteLine("Shutdown imminent!");
                    var psi = new ProcessStartInfo("shutdown", "/s /f /t 0"); //shutdown forced immediately
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    Process.Start(psi);
                }                
            }
        }

        private static void readSettings()
        {
            string settingFileName = @"settings.txt";
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(dir, settingFileName);
            
            if (File.Exists(path))
            {
                string[] lines = System.IO.File.ReadAllLines(path);

                foreach (var line in lines)
                {
                    if(line.Contains('='))
                    {
                        string[] parts = line.Split('=');

                        switch (parts[0].Trim())
                        {
                            case "comport":
                                comPortNumber = int.Parse(parts[1].Trim());
                                break;
                            case "testmode":
                                testMode = bool.Parse(parts[1].Trim());                                
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else
            {
                throw new Exception ("settings.txt file not found!");
            }            
        }
	}
}
