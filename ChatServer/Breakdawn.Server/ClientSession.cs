using Breakdawn.Protocol;
using System;
using System.Net.Sockets;

namespace Breakdawn.Server
{
	public class ClientSession : DawnSession
	{
		public ClientSession(int id, Socket socket) : base(id, socket)
		{

		}

		protected override void OnCloseConnect()
		{
			ServerSocket.Instance.Clients.TryRemove(ID, out _);
		}

		protected override void OnConnected()
		{
			JellyWar.Logger.Info($"客户端:{ID} 已连接");
		}

		protected override void OnReceiveBody()
		{
			var message = DawnUtil.DeSerialize<DawnMessage>(BodyBuffer);
			switch (message.cmd)
			{
				case Command.HeartbeatServer:
					JellyWar.Logger.Error($"心跳命令错误");
					break;
				case Command.HeartbeatClient:

					break;
				case Command.ChatMessage:
					ProcessCommand.Instance.ChatQueue.Enqueue(message.charMessage);
					ProcessCommand.Instance.SendChatMessage();
					break;
				default:
					JellyWar.Logger.Error($"命令错误");
					break;
			}
		}
	}
}
