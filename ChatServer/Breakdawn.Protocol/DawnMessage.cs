using System;

namespace Breakdawn.Protocol
{
	public enum Command
	{
		HeartbeatServer = 100,
		HeartbeatClient = 101,
		ChatMessage = 201
	}

	[Serializable]
	public class DawnMessage
	{
		public Command cmd;

		public string charMessage;
	}
}
