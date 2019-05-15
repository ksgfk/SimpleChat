using Breakdawn.Protocol;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Breakdawn.Server
{
	internal class ServerSocket : Singleton<ServerSocket>
	{
		private readonly Socket socket;
		private readonly Thread serverThread;
		private readonly ConcurrentDictionary<int, DawnSession> clients = new ConcurrentDictionary<int, DawnSession>();
		private readonly ConcurrentQueue<int> willClearClients = new ConcurrentQueue<int>();
		private int clientCount = 0;
		public static readonly int defaultServiveCount = 5;

		public ConcurrentDictionary<int, DawnSession> Clients { get => clients; }

		public ServerSocket()
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			InitSocket("127.0.0.1", 25565);
			serverThread = new Thread(Update);
			serverThread.Start();
		}

		private void InitSocket(string ip, int port)
		{
			try
			{
				socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
				socket.Listen(10);
				socket.BeginAccept(new AsyncCallback(ClientAccept), socket);
			}
			catch (Exception e)
			{
				JellyWar.Logger.Error($"{e.Message}\n{e.StackTrace}");
			}
		}

		private void ClientAccept(IAsyncResult result)
		{
			try
			{
				Socket client = socket.EndAccept(result);
				clientCount++;
				var session = new ClientSession(clientCount, client);
				if (!clients.TryAdd(clientCount, session))
				{
					JellyWar.Logger.Warn($"无法将客户端添加到列表");
				}
				session.ReceiveHeadMessage();
				JellyWar.Logger.Info($"{(client.RemoteEndPoint as IPEndPoint).Address}:{(client.RemoteEndPoint as IPEndPoint).Port} connected");
			}
			catch (Exception e)
			{
				JellyWar.Logger.Error($"{e.Message}\n{e.StackTrace}");
			}
			socket.BeginAccept(new AsyncCallback(ClientAccept), socket);
		}

		public void Update()
		{
			while (true)
			{
				Thread.Sleep(10000);
				SendHeartBeatPacket();
			}
		}

		private void OnSendFinish(IAsyncResult result)
		{
			try
			{
				var ns = result.AsyncState as NetworkStream;
				ns.EndWrite(result);
				ns.Flush();
				ns.Close();
			}
			catch (Exception e)
			{
				JellyWar.Logger.Error($"{e.Message}\n{e.StackTrace}");
			}
		}

		private void SendHeartBeatPacket()
		{
			foreach (var client in clients)
			{
				try
				{
					var ns = new NetworkStream(client.Value.Socket);
					if (!ns.CanWrite)
					{
						continue;
					}
					var body = DawnUtil.AddCommand(Command.HeartbeatServer);
					var pack = DawnUtil.AddHeadProtocol(body);
					ns.BeginWrite(pack, 0, pack.Length, new AsyncCallback(OnSendFinish), ns);
				}
				catch (Exception e)
				{
					willClearClients.Enqueue(client.Key);
					JellyWar.Logger.Error($"{e.Message}\n{e.StackTrace}");
				}
			}
			foreach (var client in willClearClients)
			{
				if (!clients.TryRemove(client, out _))
				{
					JellyWar.Logger.Warn($"无法将客户端从列表删除");
				}
				JellyWar.Logger.Info($"已删除断开连接的客户端:{client}");
			}
			willClearClients.Clear();
		}
	}
}
