using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;

namespace Wully.Events {
	/// <summary>
	/// This class provides a way to dynamically create better events for a specific Item
	/// </summary>
	public class BetterItemEvents : BetterEvents {

		private Item item;

		BetterItemEvents( Item item ) {
			this.item = item;

		}

		

	}
}
