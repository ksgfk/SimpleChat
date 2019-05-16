using System;
using System.Net.Sockets;

namespace Breakdawn.Protocol
{
	public abstract class DawnSession
	{
		private readonly int id;
		private readonly Socket socket;
		private byte[] headBuffer;
		private byte[] bodyBuffer;
		private int headIndex;
		private int bodyIndex;
		private int bodyLength;

		public Socket Socket { get => socket; }
		public byte[] BodyBuffer { get => bodyBuffer; }
		public int ID => id;

		public static int headLength = 4;

		public DawnSession(int id, Socket socket)
		{
			this.id = id;
			this.socket = socket;
		}

		protected abstract void OnConnected();

		protected abstract void OnReceiveBody();

		protected abstract void OnCloseConnect();

		public void ReceiveHeadMessage()
		{
			try
			{
				OnConnected();
				headBuffer = new byte[headLength];
				headIndex = 0;
				socket.BeginReceive(headBuffer, 0, headLength, SocketFlags.None, new AsyncCallback(HeadMessageCallBack), this);
			}
			catch (Exception e)
			{
				DawnUtil.Log($"{e.Message}\n{e.StackTrace}", LogLevel.Error);
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
						int allLength = BitConverter.ToInt32(headBuffer, 0);
						bodyLength = allLength - headLength;
						bodyBuffer = new byte[bodyLength];
						bodyIndex = 0;
						socket.BeginReceive(bodyBuffer, 0, bodyLength, SocketFlags.None, new AsyncCallback(ReceiveBodyMessage), result.AsyncState);
					}
				}
				else
				{
					CloseConnect();
				}
			}
			catch (Exception e)
			{
				DawnUtil.Log($"{e.Message}\n{e.StackTrace}", LogLevel.Error);
			}
		}

		private void ReceiveBodyMessage(IAsyncResult result)
		{
			try
			{
				int length = socket.EndReceive(result);
				if (length > 0)
				{
					bodyIndex += length;
					if (bodyIndex < bodyLength)
					{
						socket.BeginReceive(bodyBuffer, bodyIndex, bodyLength - length, SocketFlags.None, new AsyncCallback(ReceiveBodyMessage), result.AsyncState);
					}
					else
					{
						headBuffer = new byte[headLength];
						headIndex = 0;
						socket.BeginReceive(headBuffer, 0, headLength, SocketFlags.None, new AsyncCallback(HeadMessageCallBack), result.AsyncState);
						OnReceiveBody();
					}
				}
				else
				{
					CloseConnect();
				}
			}
			catch (Exception e)
			{
				DawnUtil.Log($"{e.Message}\n{e.StackTrace}", LogLevel.Error);
			}
		}

		private void CloseConnect()
		{
			socket.Close();
			DawnUtil.Log($"会话:{id},已关闭连接", LogLevel.Info);
			OnCloseConnect();
		}
	}
}
