using System;
using System.Reflection;
using API.Controllers;

namespace API
{
	public class AssemblyVersion
	{
		private static string _Current;

		public static string Current
		{
			get
			{
				if (_Current == null)
					_Current = typeof(VisionController).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

				return _Current;
			}
		}

		public static Version AsVersion
		{
			get
			{
				return Version.Parse(Current.Split("-")[0]);
			}
		}
	}
}
