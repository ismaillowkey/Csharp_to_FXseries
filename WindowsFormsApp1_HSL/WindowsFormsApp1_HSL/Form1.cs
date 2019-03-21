using System;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HslCommunication;
using HslCommunication.Profinet.Melsec;
using System.Threading;

namespace WindowsFormsApp1_HSL
{
	public partial class Form1 : MetroFramework.Forms.MetroForm
	{
		MelsecFxSerial melsecFxSerial;
		private Thread thread = null;
		private int timeSleep = 10;              
		private bool isThreadRun = false;
		private List<PictureBox> LampXInput = new List<PictureBox>();
		private List<PictureBox> LampYOutput = new List<PictureBox>();

		public Form1()
		{
			InitializeComponent();
			melsecFxSerial = new MelsecFxSerial();
			
			//Nilai Baud Rate yang bisa digunakan
			cmbBaudrate.Items.Add(9600);
			cmbBaudrate.Items.Add(19200);
			cmbBaudrate.Items.Add(38400);
			cmbBaudrate.Items.Add(57600);
			cmbBaudrate.Items.Add(115200);

			RefreshPort();

			LampXInput.Add(PBX0); LampYOutput.Add(PBY0);
			LampXInput.Add(PBX1); LampYOutput.Add(PBY1);
			LampXInput.Add(PBX2); LampYOutput.Add(PBY2);
			LampXInput.Add(PBX3); LampYOutput.Add(PBY3);
			LampXInput.Add(PBX4); LampYOutput.Add(PBY4);
			LampXInput.Add(PBX5); LampYOutput.Add(PBY5);
			LampXInput.Add(PBX6); LampYOutput.Add(PBY6);
			LampXInput.Add(PBX7); LampYOutput.Add(PBY7);
		}

		private void RefreshPort()
		{
			cmbPort.Items.Clear();
			String[] myPort;
			myPort = SerialPort.GetPortNames();
			try
			{
				for (short i = 0; i <= myPort.Length - 1; i++)
				{
					cmbPort.Items.Add(myPort[i]);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error", ex.Message);
			}

			if (myPort.Length >= 1)
			{
				cmbBaudrate.SelectedIndex = 0;
				cmbPort.SelectedIndex = 0;
			}
		}
		


		private void MBtnRefresh_Click(object sender, EventArgs e)
		{
			RefreshPort();
		}

		private void MBtnConnect_Click(object sender, EventArgs e)
		{
			melsecFxSerial.SerialPortInni(sp =>
			{
				sp.PortName = cmbPort.Text;
				sp.BaudRate = 9600;
				sp.DataBits = 7;
				sp.StopBits = System.IO.Ports.StopBits.One;
				sp.Parity = System.IO.Ports.Parity.Even;
			});
			melsecFxSerial.ReceiveTimeout = 1000;
			try
			{
				melsecFxSerial.Open();
				HideOrShow(true);
				StartReading();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void HideOrShow(bool status)
		{
			if (status) //hide
			{
				cmbPort.Enabled = false;
				cmbBaudrate.Enabled = false;
				MBtnConnect.Enabled = false;
				MBtnRefresh.Enabled = false;
				MBtnDisconnect.Enabled = true;
			}
			else
			{
				cmbPort.Enabled = true;
				cmbBaudrate.Enabled = true;
				MBtnConnect.Enabled = true;
				MBtnRefresh.Enabled = true;
				MBtnDisconnect.Enabled = false;
			}
		}

		private void MBtnDisconnect_Click(object sender, EventArgs e)
		{
			try
			{
				melsecFxSerial.Close();
				HideOrShow(false);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void StartReading()
		{
			// Start the background thread, periodically read the data in the plc, and then display in the curve control
			if (!isThreadRun)
			{
				//button27.Text = "Stop";
				isThreadRun = true;
				thread = new Thread(ThreadReadServer)
				{
					IsBackground = true
				};
				thread.Start();
			}
			else
			{
				//button27.Text = "Start";
				isThreadRun = false;
			}
		}


		private void ThreadReadServer()
		{
			
			if (melsecFxSerial != null)
			{
				while (isThreadRun)
				{
					Thread.Sleep(timeSleep);
					try
					{
						Task.Run(() => {		
							// Read X0-X7
							var ReadXInput = melsecFxSerial.ReadBool("X0", 8);
							if (ReadXInput.IsSuccess)
							{
								// Tampilkan Data
								if (isThreadRun)
								{
									//Invoke(new Action<short>(AddDataCurve), read.Content);
									this.Invoke((MethodInvoker)delegate
									{
										TSStatusPLC.Text = "Connected to PLC";

										// runs on UI thread
										var value = ReadXInput.Content;
										for (ushort i = 0; i <= LampXInput.Count - 1; i++)
										{
											if (value[i])
											{
												LampXInput[i].Image = Properties.Resources.lamp_green_on;
											}
											else
											{
												LampXInput[i].Image = Properties.Resources.lamp_green_off;
											}

										}

										//metroLabel4.Text = Convert.ToString(value[0]);

									});
								}
							}
							else
							{
								this.Invoke((MethodInvoker)delegate
								{
									// runs on UI thread
									TSStatusPLC.Text = "reconnecting...";
								});
							}
						});

						Task.Run(() => {
							// Read Y0-Y7
							var ReadYOutput = melsecFxSerial.ReadBool("Y0", 8);
							if (ReadYOutput.IsSuccess)
							{
								// Tampilkan Data
								if (isThreadRun)
								{
									//Invoke(new Action<short>(AddDataCurve), read.Content);
									this.Invoke((MethodInvoker)delegate
									{
										// runs on UI thread
										var value = ReadYOutput.Content;
										for (ushort i = 0; i <= LampYOutput.Count - 1; i++)
										{
											if (value[i])
											{
												LampYOutput[i].Image = Properties.Resources.lamp_green_on;
											}
											else
											{
												LampYOutput[i].Image = Properties.Resources.lamp_green_off;
											}
										}
									});
								}
							}
						});
					}
					catch (Exception ex)
					{
						//MessageBox.Show("Read failed：" + ex.Message);
					}
				}
			}
		}

	
	}
}
