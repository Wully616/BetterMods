using IngameDebugConsole;
using JetBrains.Annotations;
using System;
using System.Runtime.CompilerServices;

namespace Wully.Helpers {

	/// <summary>
	/// A simple wrapper around Unity.DebugLog to make it easier to configure logs
	/// </summary>
	public class BetterLogger {

		public static BetterLogger local;

		private bool enabled = true;
		private LogLevel loggingLevel = LogLevel.Info;
		private string className;
		private Type classType;

		
		public string ClassName() {
			return className;
		}
		public bool IsEnabled() {
			return enabled;
		}

		public LogLevel GetLogLevel() {
			return loggingLevel;
		}
		/// <summary>
		/// Sets the logging loggingLevel for this logger instance
		/// </summary>
		/// <param name="logLevel"></param>
		public void SetLoggingLevel( LogLevel logLevel ) {
			this.loggingLevel = logLevel;
		}
		/// <summary>
		/// Toggles Logging on or off
		/// </summary>
		private void ToggleLogging() {
			enabled = !enabled;
		}
		/// <summary>
		/// Enables Logging
		/// </summary>
		public void EnableLogging() {
			enabled = true;
		}
		/// <summary>
		/// Disables Logging
		/// </summary>
		public void DisableLogging() {
			enabled = false;
		}

		BetterLogger( Type classType ) {
			this.classType = classType;
			className = classType.ToString();
			AddCommands(className);

			if ( BetterLogger.local == null ) {
				InitGlobal();
			}
		}

		BetterLogger( string name ) {
			AddCommands(name);
		}
		private void InitGlobal() {
			BetterLogger.local = new BetterLogger("Global");
			BetterLogger.local.DisableLogging();
		}

		private void AddCommands( string name ) {
			DebugLogConsole.AddCommand("Log_" + name + "_EnableLogging", "Enable Logging for: " + name, EnableLogging);
			DebugLogConsole.AddCommand("Log_" + name + "_DisableLogging", "Disable Logging for: " + name, DisableLogging);
			DebugLogConsole.AddCommand("Log_" + name + "_ToggleLogging", "Toggle Logging for: " + name, ToggleLogging);
			DebugLogConsole.AddCommand<LogLevel>("Log_" + name + "_LogLevel", "Set log level for: " + name, SetLoggingLevel);
		}


		/// <summary>
		/// Gets a new instance of BetterLogger
		/// </summary>
		/// <param name="classType"></param>
		/// <returns>Instance of BetterLogger</returns>.
		public static BetterLogger GetLogger( Type classType ) {
			BetterLogger logger = new BetterLogger(classType);
			return logger;
		}


		[Pure]
		public FluentLogger Exception( [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null,
			[CallerFilePath] string path = null ) {
			return new FluentLogger(this, LogLevel.Exception, caller, path, lineNumber);
		}
		[Pure]
		public FluentLogger Error( [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null,
			[CallerFilePath] string path = null ) {
			return new FluentLogger(this, LogLevel.Error, caller, path, lineNumber);
		}
		[Pure]
		public FluentLogger Warn( [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null,
			[CallerFilePath] string path = null ) {
			return new FluentLogger(this, LogLevel.Warn, caller, path, lineNumber);
		}
		[Pure]
		public FluentLogger Info( [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null,
			[CallerFilePath] string path = null ) {
			return new FluentLogger(this, LogLevel.Info, caller, path, lineNumber);
		}
		[Pure]
		public FluentLogger Debug( [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null,
			[CallerFilePath] string path = null ) {
			return new FluentLogger(this, LogLevel.Debug, caller, path, lineNumber);
		}
		/// <summary>
		/// Log Levels
		/// </summary>
		public enum LogLevel {
			Exception = 0,
			Error = 1,
			Warn = 2,
			Info = 3,
			Debug = 4
		}
	}

}
