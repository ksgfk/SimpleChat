using System;

namespace Breakdawn.Server
{
	public class Singleton<T> where T : class
	{
		private static readonly Lazy<T> instance = new Lazy<T>(() =>
		{
			T inst = null;
			try
			{
				inst = Activator.CreateInstance(typeof(T), true) as T;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
			return inst;
		});

		public static T Instance { get => instance.Value; }
	}
}
