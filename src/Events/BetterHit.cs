using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;
using Wully.Extensions;

namespace Wully.Events {
	/// <summary>
	/// This class is a wrapper around various objects which provide extra information about hits in B&S
	/// </summary>
	public class BetterHit {

		public Creature creature;
		public Player player;
		public CollisionInstance collisionInstance;
		public DamageType damageType;
		public DamageStruct.Penetration penetrationType;
		public BetterEvents.Direction attackDirection;
		public HashSet<BetterEvents.CreatureState> creatureStates;
		public HashSet<BetterEvents.HitState> hitStates;
		public BetterEvents.DamageArea damageArea;


		public override string ToString() {
			Color field = Color.cyan;
			//generate with resharper, use search and replace in notepad++
			//find: (nameof\(\w+\))
			//replacewith: $1.Bold\(\).Color\(field\)
			return $"{nameof(creature).Bold().Color(field)}: {creature},\n {nameof(collisionInstance).Bold().Color(field)}: {collisionInstance.ToStringExt()},\n {nameof(damageType).Bold().Color(field)}: {damageType},\n {nameof(penetrationType).Bold().Color(field)}: {penetrationType},\n {nameof(attackDirection).Bold().Color(field)}: {attackDirection},\n {nameof(creatureStates).Bold().Color(field)}: [{string.Join(", ", creatureStates)}], {nameof(hitStates).Bold().Color(field)}: [{string.Join(", ", hitStates)}], {nameof(damageArea).Bold().Color(field)}: {damageArea}";
		}
	}
}
