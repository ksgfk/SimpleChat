using Breakdawn.Protocol;
using log4net;
using System;
using System.Diagnostics;

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
			ss = ServerSocket.Instance;
			_ = ProcessCommand.Instance;
			sw.Stop();
			Logger.Info($"Done({sw.ElapsedMilliseconds}ms)!");

			while (true)//emm,mc控制台命令是怎么做到的...
			{
				if (Console.ReadLine() == "stop")//应该不会阻塞线程,毕竟操作都是在其他线程里
				{
					Environment.Exit(0);
				}
			}
		}
	}
}
