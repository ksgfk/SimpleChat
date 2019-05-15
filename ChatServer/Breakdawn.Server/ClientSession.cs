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

		}

		protected override void OnReceiveCommand()
		{
			var cmd = (Command)BitConverter.ToInt32(CmdBuffer, 0);
			switch (cmd)
			{
				case Command.HeartbeatServer:
					JellyWar.Logger.Error($"客户端心跳包命令出错!");
					break;
				case Command.HeartbeatClient:
					SurvivalCount = 5;
					JellyWar.Logger.Info($"收到{ID}的心跳包");
					break;
				default:
					JellyWar.Logger.Error($"客户端返回命令出错!");
					break;
			}
		}
	}
}
