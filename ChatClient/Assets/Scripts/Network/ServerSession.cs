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
		
	}

	protected override void OnReceiveCommand()
	{
		var res = BitConverter.ToInt32(CmdBuffer, 0);
		Debug.Log(res);
		Command cmd = (Command)res;
		switch (cmd)
		{
			case Command.HeartbeatServer:
				var pack = DawnUtil.AddHeadProtocol(DawnUtil.AddCommand(Command.HeartbeatClient));
				SendMessage(pack);
				break;
			case Command.HeartbeatClient:
				throw new Exception("服务器出错");
			default:
				throw new Exception("服务器出错");
		}
	}

	public void SendMessage(byte[] message)
	{
		try
		{
			var ns = new NetworkStream(Socket);
			ns.BeginWrite(message, 0, message.Length, new AsyncCallback(OnSendFinish), ns);
		}
		catch (Exception e)
		{
			Debug.LogError($"{e.Message}\n{e.StackTrace}");
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
			Debug.LogError($"{e.Message}\n{e.StackTrace}");
		}
	}
}
