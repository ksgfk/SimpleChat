using Breakdawn.Protocol;
using System.Collections.Concurrent;

namespace Breakdawn.Server
{
	internal class ProcessCommand : Singleton<ProcessCommand>
	{
		private ConcurrentQueue<DawnMessage> chatQueue = new ConcurrentQueue<DawnMessage>();

		public ConcurrentQueue<DawnMessage> ChatQueue { get => chatQueue; }

		private ProcessCommand()
		{

		}

		public void SendChatMessage()
		{
			if (chatQueue.Count <= 0 || !chatQueue.TryDequeue(out var msg))
			{
				return;
			}
			msg.cmd = Command.ReceiveChat;
			byte[] pack = DawnUtil.PackageMessage(msg);
			foreach (var client in ServerSocket.Instance.Clients)
			{
				DawnUtil.SendMessage(client.Value.Socket, pack);
			}
		}
	}
}
