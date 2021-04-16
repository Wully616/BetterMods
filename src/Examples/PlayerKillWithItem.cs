using ThunderRoad;
using UnityEngine;
using Wully.Events;
using Wully.Extensions;
using Wully.Helpers;
using static Wully.Events.BetterEvents;

namespace Wully.Examples {

	/// <summary>
	/// This is an example showing how to track if a player killed a creature with a certain item
	/// </summary>
	public class PlayerKillWithItem : MonoBehaviour {
		private static BetterLogger log = BetterLogger.GetLogger(typeof(BetterEvents));

		//set the item you want to check for either on start, or set it using the setter
		public Item item;

		void Start() {
			OnCreatureKillByPlayer += PlayerKillWithItem_OnCreatureKillByPlayer;
		}

		private void PlayerKillWithItem_OnCreatureKillByPlayer( Events.BetterHit betterHit ) {
			//return if we dont have a tracked item.
			if ( !item ) { return; }

			//We want to check if the player killed a creature with our item.
			//Using a extension from BetterMods to get the item from the collision - if there was one
			if ( betterHit.collisionInstance.TryGetItemFromSource(out Item sourceItem) ) {
				//if our tracked item matches the one the player killed the creature with, do something
				if ( sourceItem.Equals(item) ) {
					//log message with betterlogs
					log.Debug().Message($"Player killed a creature with our cool item: {item.name.Bold().Color(Color.red)}");
				}
			}
		}
	}
}
