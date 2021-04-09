using ThunderRoad;
using Wully.Helpers;

namespace Wully.Events {
	/// <summary>
	/// This class provides a way to dynamically create better events for a specific Item
	/// </summary>
	public class BetterItemEvents : BetterEvents {
		private static BetterLogger log = BetterLogger.GetLogger(typeof(BetterItemEvents));

		private Item item;

		BetterItemEvents( Item item ) {
			this.item = item;

		}



	}
}
