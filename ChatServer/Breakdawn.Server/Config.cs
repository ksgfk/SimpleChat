using log4net;
using log4net.Config;
using log4net.Repository;
using System.IO;

namespace Breakdawn.Server
{
	public class LogConfig : Singleton<LogConfig>
	{
		private ILoggerRepository repository;

		private LogConfig()
		{
			repository = LogManager.CreateRepository("Main");
			XmlConfigurator.Configure(repository, new FileInfo("log-config.xml"));
		}

		public static ILog NewLogger(string name)
		{
			return LogManager.GetLogger(Instance.repository.Name, name);
		}
	}
}
