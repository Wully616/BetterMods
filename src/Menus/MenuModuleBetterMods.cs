using System;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;
using UnityEngine.UI;
using Wully.Extensions;
using Wully.Helpers;
using Wully.Module;

namespace Wully.Menus {
	public class MenuModuleBetterMods : MenuModule {

		private static BetterLogger log = BetterLogger.GetLogger(typeof(MenuModuleBetterMods));

		private GameObject modNamesSection;
		private GameObject optionsSection;
		private GameObject modNamebutton;

		public override void Init( MenuData menuData, Menu menu ) {
			base.Init(menuData, menu);
			log.Info().Message($"Initialized Wully's BetterMods -Mod Options Menu".Italics());

			modNamesSection = menu.GetCustomReference("ModNames").gameObject;
			optionsSection = menu.GetCustomReference("Options").gameObject;
			modNamebutton = menu.GetCustomReference("ModButton").gameObject;
			//for testing create a few test mod names
			modNamebutton?.SetActive(false);
			for (int i = 0; i < 20; i++) {
				if (modNamebutton && modNamesSection && optionsSection) {
					GameObject modButton = GameObject.Instantiate(modNamebutton);
					modButton.transform.SetParent(modNamesSection.transform);
				}
			}

			MenuModuleBetterMods.current = this;
		}

		public override void OnShow( bool show ) {
			if ( show ) {
				this.RefreshPage();
			}
		}


		// Token: 0x06001C66 RID: 7270 RVA: 0x000D6FB8 File Offset: 0x000D51B8
		public void RefreshPage() {
	

		}

		// Token: 0x040020AE RID: 8366
		public static MenuModuleBetterMods current;

	}
}
