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
        static bool writeLog = true;
        static int startupDelay = 0;
        static ProcessIcon pi = new ProcessIcon();
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
            appendLineToLog("--- Application started at " + DateTime.Now.ToString());
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			            
            try
            {
                
                //read settings from settings.txt file
                appendLineToLog("Before Reading settings");
                readSettings();
                appendLineToLog("After reading settings");

                //wait for specified amount of seconds
                System.Threading.Thread.Sleep(startupDelay * 1000);

                //open serial port 
                appendLineToLog("Before opening port");
                openPort();
                appendLineToLog("After opening port ");

                //show tray icon
                appendLineToLog("Before pi.Display()");
                pi.Display(comPortNumber);
                appendLineToLog("After pi.Display()");

                appendLineToLog("Before Application.Run()");
                //make sure the application runs
                Application.Run();
                appendLineToLog("After Application.Run()");
            }
            catch (Exception ex)
            {
                appendLineToLog("Error catched: "+ex.Message);
                MessageBox.Show(null, ex.Message, "Application cannot run", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                appendLineToLog("Finalizing");
                closePort();
                if (pi != null) pi.Dispose();
            }
            appendLineToLog("--- Application exit at " + DateTime.Now.ToString());
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
                //Wait for user to cancel while balloon is visible
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
            //string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); //could make shortcut work from startup folder, so must put .exe there and therefore cannot have settings file in the same folder, so using hardcoded path for setting files instead
            string dir = @"C:\HTControl\";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
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
                            case "startupdelay":
                                startupDelay = int.Parse(parts[1].Trim());
                                break;
                            case "writelog":
                                writeLog = bool.Parse(parts[1].Trim());
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else
            {                
                using (StreamWriter sw = File.CreateText(path))
                {                                                                                
                    sw.WriteLine("comport=3");
                    sw.WriteLine("testmode=true");
                    sw.WriteLine("startupdelay=0");
                    sw.WriteLine("writelog=true");        
            
                    comPortNumber = 3;
                    testMode = true;
                    startupDelay = 0;
                    writeLog = true;
                }                
                MessageBox.Show(null, path+ " not found, default file created", "Info",  MessageBoxButtons.OK, MessageBoxIcon.Information);
            }            
        }

        private static void appendLineToLog(string text)
        {
            if (writeLog)
            {
                string logFileName = @"log.txt";
                //string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string dir = @"C:\HTControl\";
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, logFileName);
                

                File.AppendAllText(path, text + Environment.NewLine);
            }
        }
	}
}
