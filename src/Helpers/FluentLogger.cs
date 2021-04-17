using System;
using System.Threading;
using UnityEngine;
using Wully.Extensions;
using static Wully.Helpers.BetterLogger;

namespace Wully.Helpers {
	/// <summary>
	/// A fluent class for wrapping callerMemberName and lineNumbers, giving more detailed information in logs
	/// </summary>
	public class FluentLogger {

		private string _callerMemberName;
		private int _callerLineNumber;
		private string _callerFilePath;
		private LogLevel logLevel;
		private BetterLogger log;
		private string className;

		public FluentLogger( BetterLogger betterLogger, LogLevel level, string callerMemberName, string callerFilePath, int callerLineNumber ) {
			log = betterLogger;
			className = log.ClassName();
			logLevel = level;
			_callerMemberName = callerMemberName;
			_callerFilePath = callerFilePath;
			_callerLineNumber = callerLineNumber;
		}

		/// <summary>
		/// Log message text
		/// </summary>
		/// <param name="message"></param>
		/// <param name="messageArgs"></param>
		public void Message( string message, params object[] messageArgs ) {
			try {
				//If the global instance is available and enabled, use its log level  for ALL loggers.
				if ( BetterLogger.local == null || !BetterLogger.local.IsEnabled() ) {
					// dont print if logging disabled
					if ( !log.IsEnabled() ) {
						return;
					}

					// dont print if current log level is below this messages log level
					if ( log.GetLogLevel() < logLevel ) {
						return;
					}
				}

				switch ( logLevel ) {
					case LogLevel.Exception:
						UnityEngine.Debug.LogError(Format(message, messageArgs));
						break;
					case LogLevel.Error:
						UnityEngine.Debug.LogError(Format(message, messageArgs));
						break;
					case LogLevel.Warn:
						UnityEngine.Debug.LogWarning(Format(message, messageArgs));
						break;
					default: //Info/Debug/Default all use Log()
						UnityEngine.Debug.Log(Format(message, messageArgs));
						break;
				}
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}
		}

		private string Format( string format, params object[] args ) {
			try {
				DateTime dateTime = DateTime.Now;
				string dt = String.Format("{0:u}", dateTime);
				string callingMethod =
					String.Join(".", className, _callerMemberName, _callerLineNumber).Color(Color.cyan);
				int thread = Thread.CurrentThread.ManagedThreadId;

				return
					$"{dt}\t{Time.time}\t{LogLevelColor(logLevel).Bold()}\t{thread}\t{callingMethod}\t: {string.Format(format, args)}";
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
				return "Error, could not log";
			}
		}

		private string LogLevelColor( LogLevel level ) {
			switch ( level ) {
				case LogLevel.Error:
					return logLevel.ToString().Color(Color.red).Bold();
				case LogLevel.Exception:
					return logLevel.ToString().Color(Color.red);
				case LogLevel.Warn:
					return logLevel.ToString().Color(Color.yellow);
				default:
					return logLevel.ToString().Color(Color.green);
			}
		}
		/// <summary>
		/// Convert colour to html string
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static string ConvertToHtml( Color color, string text ) {
			return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
		}

		public static string ConvertToHtml( Style style, int value, string text ) {
			switch ( style ) {
				case Style.bold:
					return $"<b>{text}</b>";
				case Style.italic:
					return $"<i>{text}</i>";
				case Style.size:
					return $"<size={value}>{text}</size>";
				case Style.normal:
					return text;
			}
			return text;
		}

		public enum Style {
			normal,
			italic,
			bold,
			size
		}

	}
}
