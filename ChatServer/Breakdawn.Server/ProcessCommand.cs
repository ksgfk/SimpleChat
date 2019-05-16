using Breakdawn.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Breakdawn.Server
{
	internal class ProcessCommand : Singleton<ProcessCommand>
	{
		private ConcurrentQueue<string> chatQueue = new ConcurrentQueue<string>();

		public ConcurrentQueue<string> ChatQueue { get => chatQueue; }

		private ProcessCommand()
		{

		}

		public void SendChatMessage()
		{
			if (chatQueue.Count <= 0 || !chatQueue.TryDequeue(out var msg))
			{
				return;
			}
			var m = new DawnMessage
			{
				cmd = Command.ReceiveChat,
				charMessage = msg,
			};
			byte[] pack = DawnUtil.PackageMessage(m);
			foreach (var client in ServerSocket.Instance.Clients)
			{
				DawnUtil.SendMessage(client.Value.Socket, pack);
			}
		}
	}
}
