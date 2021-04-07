using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using Wully.Helpers;
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

		/// <summary>
		/// Returns true if the creature is holding something
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public static bool IsArmed( this Creature creature ) {
			return IsCreatureArmed(creature);
		}

		/// <summary>
		/// Returns true if the creature is holding something in a particular side
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="side"></param>
		/// <returns></returns>
		public static bool IsArmed( this Creature creature, Side side ) {
			return IsCreatureArmed(creature, side);
		}

		/// <summary>
		/// Returns true if the creature is holding something
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public static bool IsGrabbingHandle(this Creature creature ) {
			return IsCreatureGrabbingHandle(creature);
		}
		/// <summary>
		/// Returns true if the creature is holding something in a particular side
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="side"></param>
		/// <returns></returns>
		public static bool IsGrabbingHandle( this Creature creature, Side side ) {
			return IsCreatureGrabbingHandle(creature, side);
		}

		/// <summary>
		/// Will make an item's colliders collide with another item's colliders
		/// </summary>
		/// <param name="item"></param>
		/// <param name="otherItem"></param>
		public static void AllowCollisionWith(this Item item, Item otherItem ) {
			MakeItemCollideWith(item, otherItem);
		}

		/// <summary>
		/// Will make an item's colliders collide with a specific collider
		/// </summary>
		/// <param name="item"></param>
		/// <param name="collider"></param>
		public static void AllowCollisionWith( this Item item, Collider collider) {
			MakeItemCollideWith(item, collider);
		}

		/// <summary>
		/// Will make an item's colliders not collide with another item's colliders
		/// </summary>
		/// <param name="item"></param>
		/// <param name="otherItem"></param>
		public static void IgnoreCollisionWith( this Item item, Item otherItem ) {
			MakeItemCollideWith(item, otherItem);
		}

		/// <summary>
		/// Will make an item's colliders not collide with a specific collider
		/// </summary>
		/// <param name="item"></param>
		/// <param name="collider"></param>
		public static void IgnoreCollisionWith( this Item item, Collider collider ) {
			MakeItemCollideWith(item, collider);
		}

		/// <summary>
		/// Tries to return the handle the spellcaster is currently holding with telekinesis
		/// </summary>
		/// <param name="spellCaster">side specific spellcaster</param>
		/// <param name="handle">The handle held by the spellcaster with telekinesis</param>
		/// <returns></returns>
		public static bool TryGetTelekinesisCaughtHandle(this SpellCaster spellCaster, out Handle handle ) {			
			return BetterHelpers.TryGetTelekinesisCaughtHandle(spellCaster, out handle);
		}

		/// <summary>
		/// Tries to return the Ragdoll hand for a creatures Side
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="side"></param>
		/// <param name="ragdollHand"></param>
		/// <returns></returns>
		public static bool TryGetRagdollHand( this Creature creature, Side side, out RagdollHand ragdollHand ) {
			return BetterHelpers.TryGetRagdollHand(creature, side, out ragdollHand);
		}

		/// <summary>
		/// Tries to return both ragdoll hands for a creature
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="leftRagdollHand"></param>
		/// <param name="rightRagdollHand"></param>
		/// <returns></returns>
		public static bool TryGetRagdollHand( this Creature creature, out RagdollHand leftRagdollHand, out RagdollHand rightRagdollHand ) {
			return BetterHelpers.TryGetRagdollHand(creature, out leftRagdollHand, out rightRagdollHand);
		}

		/// <summary>
		/// Tries to get the item a creature is holding on a particular side
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="side"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool TryGetHeldItem( this Creature creature, Side side, out Item item ) {
			return BetterHelpers.TryGetHeldItem(creature, side, out item);
		}

		/// <summary>
		/// Tries to get the item a ragdollhand is holding
		/// </summary>
		/// <param name="ragdollHand"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool TryGetHeldItem( this RagdollHand ragdollHand, out Item item ) {
			return BetterHelpers.TryGetHeldItem(ragdollHand, out item);
		}


		/// <summary>
		/// Tries to get the item a spellcaster is holding
		/// </summary>
		/// <param name="spellCaster"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool TryGetHeldItem( this SpellCaster spellCaster, out Item item ) {
			return BetterHelpers.TryGetHeldItem(spellCaster, out item);
		}


		/// <summary>
		/// Checks to see if the ragdoll is being choked
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public static bool IsBeingChoked( this Creature creature ) {
			return BetterHelpers.IsBeingChoked(creature);
		}
		/// <summary>
		/// Checks to see if the ragdoll was being choked and is now dead
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public static bool WasBeingChoked( this Creature creature ) {
			return BetterHelpers.WasBeingChoked(creature);
		}

		/// <summary>
		/// Check if handle is being choked
		/// </summary>
		/// <param name="handle"></param>
		/// <returns></returns>
		public static bool IsHandleChoked( this Handle handle ) {			
			return BetterHelpers.IsHandleChoked(handle);
		}
	}
}
