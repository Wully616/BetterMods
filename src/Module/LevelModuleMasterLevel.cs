using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using VLB;
using Wully.Extensions;
using Wully.Helpers;
using Wully.Render;
using static Wully.Helpers.BetterLogger;
using static Wully.Helpers.BetterHelpers;
using Action = ThunderRoad.Action;

namespace Wully.Module {
	public class LevelModuleMasterLevel : LevelModule {

		private static BetterLogger log = BetterLogger.GetLogger(typeof(LevelModuleMasterLevel));

		// Configurables
		/// <summary>
		/// Enable/Disable Better Logging for BetterEvents
		/// </summary>
		public bool enableLogging = true;
		/// <summary>
		/// Set the GetLogLevel for BetterEvents
		/// </summary>
		public LogLevel logLevel = LogLevel.Info;
		
		/// <summary>
		/// Local static reference to the currently loaded BetterMods level module
		/// </summary>
		public static LevelModuleMasterLevel local;

		public Level masterLevel;
		public override IEnumerator OnLoadCoroutine( Level level ) {
			try {
				log.SetLoggingLevel(logLevel);
				log.DisableLogging();
				if ( enableLogging ) {
					log.EnableLogging();
				}
				log.Debug().Message("level {0}", level.data.id);


				//master scene is always loaded and this module gets loaded with it
				if ( level.data.id.ToLower() == "master" ) {

					log.Info().Message($"Initialized Wully's BetterMods - Master Level module".Italics());
					if (local == null) {
						local = this;
					}
					
					local.masterLevel = level;
					OnLevelModuleLoaded?.Invoke(local);
				}

			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}

			yield break;
		}

		public static event LoadedEvent OnLevelModuleLoaded;

		public delegate void LoadedEvent( LevelModuleMasterLevel instance );




	}
}
