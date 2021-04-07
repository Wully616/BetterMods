using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using static Wully.Helpers.BetterHelpers;
namespace Wully.Extensions {
	/// <summary>
	/// Class extensions for BAS
	/// </summary>
	public static class BetterExtensions {

		/// <summary>
		/// Disarms creature on both hands
		/// </summary>
		/// <param name="creature"></param>
		public static void Disarm(this Creature creature ) {
			DisarmCreature(creature);
		}

		/// <summary>
		/// Disarms creature on a particular side
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="side"></param>
		public static void Disarm( this Creature creature, Side side ) {
			DisarmCreature(creature, side);
		}
	}
}
