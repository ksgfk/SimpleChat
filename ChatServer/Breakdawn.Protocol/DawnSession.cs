using System;
using System.Net.Sockets;

namespace Breakdawn.Protocol
{
	public class DawnSession
	{
		private readonly int id;
		private readonly Socket socket;
		private int survivalCount = 5;
		private byte[] headBuffer;
		private byte[] cmdBuffer;
		private byte[] bodyBuffer;
		private int headIndex;
		private int cmdIndex;
		private Action<DawnSession> afterReceive;

		public Socket Socket { get => socket; }
		public byte[] HeadBuffer { get => headBuffer; }
		public byte[] CmdBuffer { get => cmdBuffer; }
		public int SurvivalCount { get => survivalCount; set => survivalCount = value; }

		public static int headLength = 4;
		public static int cmdLength = 4;

		public DawnSession(int id, Socket socket, Action<DawnSession> afterReceive = null)
		{
			this.id = id;
			this.socket = socket;
			this.afterReceive = afterReceive;
		}

		public void ReceiveHeadMessage()
		{
			try
			{
				headBuffer = new byte[headLength];
				headIndex = 0;
				socket.BeginReceive(headBuffer, 0, headLength, SocketFlags.None, new AsyncCallback(HeadMessageCallBack), this);
				DawnUtil.Log($"会话:{id},开始接收头协议", LogLevel.Info);
			}
			catch (Exception e)
			{
				DawnUtil.Log(e.Message, LogLevel.Error);
				DawnUtil.Log(e.StackTrace, LogLevel.Error);
			}
		}

		private void HeadMessageCallBack(IAsyncResult result)
		{
			try
			{
				if (socket.Available == 0)
				{
					CloseConnect();
					return;
				}
				var length = socket.EndReceive(result);
				if (length > 0)
				{
					headIndex += length;//将接收的比特数和头部相加
					if (headIndex < headLength)//如果接收到的比特数比头部标准小,继续接收
					{
						socket.BeginReceive(headBuffer, headIndex, headLength - length, SocketFlags.None, new AsyncCallback(HeadMessageCallBack), result.AsyncState);
					}
					else
					{
						cmdBuffer = new byte[BitConverter.ToInt32(headBuffer)];
						cmdIndex = 0;
						socket.BeginReceive(cmdBuffer, 0, cmdLength, SocketFlags.None, new AsyncCallback(ReceiveCmdMessage), result.AsyncState);
						DawnUtil.Log($"会话:{id},开始接收命令协议", LogLevel.Info);
					}
				}
				else
				{
					CloseConnect();
				}
			}
			catch (Exception e)
			{
				DawnUtil.Log(e.Message, LogLevel.Error);
				DawnUtil.Log(e.StackTrace, LogLevel.Error);
			}
		}

		private void ReceiveCmdMessage(IAsyncResult result)
		{
			try
			{
				int length = socket.EndReceive(result);
				if (length > 0)
				{
					cmdIndex += length;
					if (cmdIndex < cmdLength)
					{
						socket.BeginReceive(cmdBuffer, cmdIndex, cmdLength - length, SocketFlags.None, new AsyncCallback(ReceiveCmdMessage), result.AsyncState);
					}
					else
					{
						DawnUtil.Log($"会话:{id},命令{(Command)BitConverter.ToInt32(cmdBuffer)}", LogLevel.Info);
						DawnUtil.Log($"会话:{id},命令协议接收完成,暂未实现主体,开始下一轮", LogLevel.Info);
						headBuffer = new byte[headLength];
						headIndex = 0;
						socket.BeginReceive(headBuffer, 0, headLength, SocketFlags.None, new AsyncCallback(HeadMessageCallBack), result.AsyncState);
						afterReceive(this);
					}
				}
				else
				{
					CloseConnect();
				}
			}
			catch (Exception e)
			{
				DawnUtil.Log(e.Message, LogLevel.Error);
				DawnUtil.Log(e.StackTrace, LogLevel.Error);
			}
		}

		private void CloseConnect()
		{
			socket.Close();
			DawnUtil.Log($"会话:{id},已关闭连接", LogLevel.Info);
			//TODO:删除表中的客户端
		}
	}
}
