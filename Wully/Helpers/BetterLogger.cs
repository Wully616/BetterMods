using IngameDebugConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Wully.Helpers {

    /// <summary>
    /// A simple wrapper around Unity.DebugLog to make it easier to configure logs
    /// </summary>
	public class BetterLogger {

        private bool enabled = true;
        private Loglevel logLevel = Loglevel.Debug;
        private string className;
        private Type classType;

        public enum Loglevel {
            Error = 0,
            Warn = 1,
            Info = 2,
            Debug = 3
		}

        /// <summary>
        /// Sets the logging loglevel for this logger instance
        /// </summary>
        /// <param name="loglevel"></param>
        public void SetLoggingLevel(Loglevel loglevel ) {
            logLevel = loglevel;
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
            className = classType.Name;
            DebugLogConsole.AddCommand("BetterLogger_" + className + "_EnableLogging", "Enable Logging for: "+className, EnableLogging);
            DebugLogConsole.AddCommand("BetterLogger_" + className + "_DisableLogging", "Disable Logging for: " + className, DisableLogging);
            DebugLogConsole.AddCommand("BetterLogger_" + className + "_ToggleLogging", "Toggle Logging for: " + className, ToggleLogging);
            DebugLogConsole.AddCommand<Loglevel>("BetterLogger_" + className + "_LogLevel", "Set Log loglevel for: " + className, SetLoggingLevel);
        }

        /// <summary>
        /// Gets a new instance of BetterLogger
        /// </summary>
        /// <param name="classType"></param>
        /// <returns>Instance of BetterLogger</returns>.
        public static BetterLogger GetLogger(Type classType ) {            
            BetterLogger logger = new BetterLogger(classType);          
            return logger;
		}

        private string Log( Loglevel loglevel, string format, params object[] args ) {
            string message = string.Format("{0}{1}{2}{3}{4}",
                "[" + Time.time + "]",
                "[" + loglevel.ToString() + "]",
                "[" + Thread.CurrentThread.ManagedThreadId.ToString() + "]",
                "[" + className + "] : ",
                string.Format(format, args));

            return message;
        }
        /// <summary>
        /// Logs Errors
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Error( string format, params object[] args ) {
            if ( enabled && logLevel >= Loglevel.Error ) {
                UnityEngine.Debug.LogError(Log(Loglevel.Error, format, args));
            }
        }
        /// <summary>
        /// Logs Warnings
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Warn( string format, params object[] args ) {
            if ( enabled && logLevel >= Loglevel.Warn ) {
                UnityEngine.Debug.LogWarning(Log(Loglevel.Warn, format, args));
            }
        }
        /// <summary>
        /// Logs Information messages
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Info( string format, params object[] args ) {
            if ( enabled && logLevel >= Loglevel.Info ) {
                UnityEngine.Debug.Log(Log(Loglevel.Info, format, args));
            }
        }
        /// <summary>
        /// Logs Debug Messages
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Debug( string format, params object[] args ) {
            if( enabled && logLevel >= Loglevel.Debug ) {
                UnityEngine.Debug.Log(Log(Loglevel.Debug, format, args));
            }
		}
       
    }
}
