using System;
using System.Diagnostics;

namespace PassManager
{
	class Logger
	{
		public enum SeverityLevel
		{
			Info,
			Warning,
			Exception,
			Error,
			FatalError
		}
		
		public static void Log(object message, SeverityLevel severityLevel = SeverityLevel.Info)
		{
			Debug.WriteLine($"{message} \t#time: {DateTime.Now.TimeOfDay}", severityLevel.ToString());
		}
		public static void Log(Exception ex)
		{
			Debug.WriteLine($"{ex.ToString()}\nErrMsg: {ex.Message}\t#time: {DateTime.Now.TimeOfDay}", SeverityLevel.Exception.ToString());
		}
	}
}
