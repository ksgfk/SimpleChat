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
		private Socket socket;
		private Thread serverThread;
		private ConcurrentDictionary<int, DawnSession> clients = new ConcurrentDictionary<int, DawnSession>();
		private ConcurrentQueue<int> willClearClients = new ConcurrentQueue<int>();
		private int clientCount = 0;
		public static readonly int defaultServiveCount = 5;

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
				JellyWar.Logger.Error(e.Message);
				JellyWar.Logger.Error(e.StackTrace);
			}
		}

		private void ClientAccept(IAsyncResult result)
		{
			try
			{
				Socket client = socket.EndAccept(result);
				clientCount++;
				int tryCount = 1;
				var session = new DawnSession(clientCount, client, (s) =>
				 {
					 var cmd = (Command)BitConverter.ToInt32(s.CmdBuffer, 0);
					 switch (cmd)
					 {
						 case Command.HeartbeatServer:
							 JellyWar.Logger.Error($"客户端心跳包命令出错!");
							 break;
						 case Command.HeartbeatClient:
							 s.SurvivalCount = defaultServiveCount;
							 break;
						 default:
							 JellyWar.Logger.Error($"客户端返回命令出错!");
							 break;
					 }
				 });
				while (!clients.TryAdd(clientCount, session))
				{
					JellyWar.Logger.Warn($"无法将客户端添加到列表,重新尝试{tryCount}");
					if (tryCount > 5)
					{
						JellyWar.Logger.Error($"尝试次数超过{tryCount},跳过");
						break;
					}
					tryCount++;
				}
				session.ReceiveHeadMessage();
				JellyWar.Logger.Info($"{(client.RemoteEndPoint as IPEndPoint).Address}:{(client.RemoteEndPoint as IPEndPoint).Port} connected");
			}
			catch (Exception e)
			{
				JellyWar.Logger.Error(e.Message);
				JellyWar.Logger.Error(e.StackTrace);
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
				JellyWar.Logger.Error(e.Message);
				JellyWar.Logger.Error(e.StackTrace);
			}
		}

		private void SendHeartBeatPacket()
		{
			foreach (var client in clients)
			{
				try
				{
					var ns = new NetworkStream(client.Value.Socket);
					var pack = DawnUtil.PacketMessage<DawnMessage>(Command.HeartbeatServer);
					ns.BeginWrite(pack, 0, pack.Length, new AsyncCallback(OnSendFinish), ns);
				}
				catch (Exception e)
				{
					willClearClients.Enqueue(client.Key);
					JellyWar.Logger.Error(e.Message);
					JellyWar.Logger.Error(e.StackTrace);
				}
			}
			foreach (var client in willClearClients)
			{
				int tryCount = 0;
				while (!clients.TryRemove(client, out _))
				{
					JellyWar.Logger.Warn($"无法将客户端添加到列表,重新尝试{tryCount}");
					if (tryCount > 5)
					{
						JellyWar.Logger.Error($"尝试次数超过{tryCount},跳过");
						break;
					}
					tryCount++;
				}
				JellyWar.Logger.Info($"已删除断开连接的客户端:{client}");
			}
			willClearClients.Clear();
		}
	}
}
