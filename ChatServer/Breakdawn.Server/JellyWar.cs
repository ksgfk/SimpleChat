using Breakdawn.Protocol;
using log4net;
using System.Diagnostics;
using System.Threading;

namespace Breakdawn.Server
{
	public class JellyWar
	{
		public static ILog Logger = LogConfig.NewLogger("JellyWar");

		private static ServerSocket ss;

		public static void Main(string[] args)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			Logger.Info("Server start up");
			DawnUtil.LogAction = (message, level) =>
			{
				switch (level)
				{
					case LogLevel.Info:
						Logger.Info(message);
						break;
					case LogLevel.Warn:
						Logger.Warn(message);
						break;
					case LogLevel.Error:
						Logger.Error(message);
						break;
					default:
						Logger.Error(message);
						break;
				}
			};
			ss = new ServerSocket();
			sw.Stop();
			Logger.Info($"Done({sw.ElapsedMilliseconds}ms)!");

			while (true)
			{
				Thread.Sleep(10000);
			}
		}
	}
}
