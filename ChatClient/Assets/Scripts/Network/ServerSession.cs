using System;
using System.Net.Sockets;
using Breakdawn.Protocol;
using UnityEngine;

public class ServerSession : DawnSession
{
	public ServerSession(int id, Socket socket) : base(id, socket)
	{

	}

	protected override void OnCloseConnect()
	{
		Debug.Log("已断开连接");
	}

	protected override void OnConnected()
	{
		Debug.Log("已连接服务器");
	}

	protected override void OnReceiveBody()
	{
		var message = Utils.DeSerialize<DawnMessage>(BodyBuffer);
		switch (message.cmd)
		{
			case Command.HeartbeatServer:
				var heart = new DawnMessage { cmd = Command.HeartbeatClient };
				var body = Utils.Serialize(heart);
				var pack = Utils.AddHeadProtocol(body);
				Debug.Log("回复心跳");
				Utils.SendMessage(Socket, pack);
				break;
			case Command.HeartbeatClient:
				Debug.LogError("心跳命令错误");
				break;
			case Command.ChatMessage:
				break;
			default:
				Debug.LogError("心跳命令错误");
				break;
		}
	}
}
