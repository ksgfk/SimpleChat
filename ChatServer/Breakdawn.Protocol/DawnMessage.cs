using System;

namespace Breakdawn.Protocol
{
	public enum Command : uint
	{
		HeartbeatServer = 100,
		HeartbeatClient = 101
	}

	[Serializable]
	public class DawnMessage
	{
		public int cmd;
		public int err;

		public string charMessage;
	}
}
