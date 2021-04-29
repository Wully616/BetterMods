using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using Wully.Extensions;
using Wully.Helpers;
using static Wully.Helpers.BetterLogger;
using static Wully.Helpers.BetterHelpers;

namespace Wully.Module {
	public class LevelModuleGlobalItemModule : LevelModule {

		public static BetterLogger log = BetterLogger.GetLogger(typeof(LevelModuleGlobalItemModule));

		// Configurables
		/// <summary>
		/// Enable/Disable Better Logging for BetterEvents
		/// </summary>
		public bool enableLogging = true;
		/// <summary>
		/// Set the GetLogLevel for BetterEvents
		/// </summary>
		public LogLevel logLevel = LogLevel.Info;

		public List<ItemModule> modules;

		/// <summary>
		/// Local static reference to the currently loaded BetterMods level module
		/// </summary>
		public static LevelModuleGlobalItemModule local;

		public HashSet<ItemData.Type> itemTypes;
		

		public override IEnumerator OnLoadCoroutine( Level level ) {
			try {
				log.SetLoggingLevel(logLevel);
				log.DisableLogging();
				if ( enableLogging ) {
					log.EnableLogging();
				}


				//master scene is always loaded and this module gets loaded with it
				if ( level.data.id.ToLower() == "master" ) {

					log.Info().Message($"Initialized Wully's BetterMods - Global ItemModule".Italics());

					local = this;

					ApplyItemModules();
				}

			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}

			yield break;
		}


		public virtual void ApplyItemModules() {

			if ( modules != null && modules.Count > 0 ) {
				//add all of these modules to the Items in the catalog
				try {
					foreach ( ItemModule itemModule in modules ) {
						foreach ( ItemData itemData in GetItemDataList() ) {
							//skip if there is a itemtype filter specified
							if (itemTypes != null && itemTypes.Count > 0 && !itemTypes.Contains(itemData.type)) {
								continue;
							}
							log.Info().Message($"Adding module {itemModule.type} to ItemData: {itemData.id}");
							if ( itemData.modules != null ) {
								itemData.modules.Add(itemModule);
							} else {
								itemData.modules = new List<ItemModule>();
								itemData.modules.Add(itemModule);
							}
							//refresh the catalog
							itemData.OnCatalogRefresh();

						}
					}


				} catch ( Exception e ) {
					log.Exception().Message($"Exception adding item module to itemDatas. {e.Message}");
				}
			} else {
				log.Warn().Message($"There are no global item modules in json");
			}
		}


	}
}
