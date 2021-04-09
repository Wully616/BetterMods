using ThunderRoad;
using UnityEngine;
using Wully.Helpers;
using static Wully.Helpers.BetterHelpers;

namespace Wully.Extensions {
	/// <summary>
	/// Class extensions for BAS
	/// </summary>
	public static class BetterExtensions {

		public static bool IsPlayer(this RagdollPart ragdollPart) {
			return BetterHelpers.IsPlayer(ragdollPart);
		}
		/// <summary>
		/// Extended toString
		/// </summary>
		/// <param name="ci">CollisionInstance</param>
		/// <returns></returns>
		public static string ToStringExt( this CollisionInstance ci ) {
			return $"{nameof(ci.active)}: {ci.active}, {nameof(ci.sourceCollider)}: {ci.sourceCollider}, {nameof(ci.targetCollider)}: {ci.targetCollider}, {nameof(ci.impactVelocity)}: {ci.impactVelocity}, {nameof(ci.contactPoint)}: {ci.contactPoint}, {nameof(ci.contactNormal)}: {ci.contactNormal}, {nameof(ci.sourceMaterial)}: {ci.sourceMaterial}, {nameof(ci.targetMaterial)}: {ci.targetMaterial}, {nameof(ci.casterHand)}: {ci.casterHand}, {nameof(ci.damageStruct)}: {ci.damageStruct}, {nameof(ci.sourceColliderGroup)}: {ci.sourceColliderGroup}, {nameof(ci.targetColliderGroup)}: {ci.targetColliderGroup}, {nameof(ci.intensity)}: {ci.intensity}, {nameof(ci.lastRumbleSourcePoint)}: {ci.lastRumbleSourcePoint}, {nameof(ci.lastRumbleTargetPoint)}: {ci.lastRumbleTargetPoint}, {nameof(ci.pressureRelativeVelocity)}: {ci.pressureRelativeVelocity}, {nameof(ci.pressureForce)}: {ci.pressureForce}, {nameof(ci.ignoreDamage)}: {ci.ignoreDamage}, {nameof(ci.lastCheckFrame)}: {ci.lastCheckFrame}, {nameof(ci.lastCheckFrameCount)}: {ci.lastCheckFrameCount}, {nameof(ci.lastStayFrame)}: {ci.lastStayFrame}, {nameof(ci.effectInstance)}: {ci.effectInstance}, {nameof(ci.hasEffect)}: {ci.hasEffect}";
		}
		/// <summary>
		/// Tries to return the ragdoll part if there is one on the sourceColliderGroup
		/// </summary>
		/// <param name="collisionInstance"></param>
		/// <returns>Nullable ragdollPart from targetColliderGroup</returns>
		public static RagdollPart GetRagdollPartFromSource( this CollisionInstance collisionInstance ) {
			BetterHelpers.TryGetObjFromColliderGroup(collisionInstance?.sourceColliderGroup, out RagdollPart ragdollPart);
			return ragdollPart;
		}
		
		/// <summary>
		/// Tries to return the ragdoll part if there is one on the sourceColliderGroup
		/// </summary>
		/// <param name="collisionInstance"></param>
		/// <param name="ragdollPart"></param>
		/// <returns></returns>
		public static bool TryGetRagdollPartFromSource( this CollisionInstance collisionInstance, out RagdollPart ragdollPart ) {
			return BetterHelpers.TryGetObjFromColliderGroup(collisionInstance?.sourceColliderGroup, out ragdollPart);
		}
		/// <summary>
		/// Tries to return the ragdoll part if there is one on the targetColliderGroup
		/// </summary>
		/// <param name="collisionInstance"></param>
		/// <returns>Nullable ragdollPart from targetColliderGroup</returns>
		public static RagdollPart GetRagdollPartFromTarget( this CollisionInstance collisionInstance ) {
			BetterHelpers.TryGetObjFromColliderGroup(collisionInstance?.targetColliderGroup, out RagdollPart ragdollPart);
			return ragdollPart;
		}
		/// <summary>
		/// Tries to return the ragdoll part if there is one on the targetColliderGroup
		/// </summary>
		/// <param name="collisionInstance"></param>
		/// <param name="ragdollPart"></param>
		/// <returns></returns>
		public static bool TryGetRagdollPartFromTarget( this CollisionInstance collisionInstance, out RagdollPart ragdollPart ) {
			return BetterHelpers.TryGetObjFromColliderGroup(collisionInstance?.targetColliderGroup, out ragdollPart);
		}
		/// <summary>
		/// Tries to return the ragdoll part if there is one on a collider group
		/// </summary>
		/// <param name="colliderGroup"></param>
		/// <returns>Nullable ragdollPart from colliderGroup</returns>
		public static RagdollPart GetRagdollPart( this ColliderGroup colliderGroup ) {
			BetterHelpers.TryGetObjFromColliderGroup(colliderGroup, out RagdollPart ragdollPart);
			return ragdollPart;
		}
		/// <summary>
		/// Tries to return the ragdoll part if there is one on a collider group
		/// </summary>
		/// <param name="colliderGroup"></param>
		/// <param name="ragdollPart"></param>
		/// <returns></returns>
		public static bool TryGetRagdollPart( this ColliderGroup colliderGroup, out RagdollPart ragdollPart ) {
			return BetterHelpers.TryGetObjFromColliderGroup(colliderGroup, out ragdollPart);
		}
		/// <summary>
		/// Tries to return the item if there is one on the sourceColliderGroup
		/// </summary>
		/// <param name="collisionInstance"></param>
		/// <returns>Nullable item from sourceColliderGroup</returns>
		public static Item GetItemFromSource( this CollisionInstance collisionInstance) {
			BetterHelpers.TryGetObjFromColliderGroup(collisionInstance.sourceColliderGroup, out Item item);
			return item;
		}
		/// <summary>
		/// Tries to return the item if there is one on the sourceColliderGroup
		/// </summary>
		/// <param name="collisionInstance"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool TryGetItemFromSource( this CollisionInstance collisionInstance, out Item item ) {
			return BetterHelpers.TryGetObjFromColliderGroup(collisionInstance.sourceColliderGroup, out item);
		}
		/// <summary>
		/// Tries to return the item if there is one on the targetColliderGroup
		/// </summary>
		/// <param name="collisionInstance"></param>
		/// <returns>Nullable item from targetColliderGroup</returns>
		public static Item GetItemFromTarget( this CollisionInstance collisionInstance) {
			 BetterHelpers.TryGetObjFromColliderGroup(collisionInstance.targetColliderGroup, out Item item);
			 return item;
		}
		/// <summary>
		/// Tries to return the item if there is one on the targetColliderGroup
		/// </summary>
		/// <param name="collisionInstance"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool TryGetItemFromTarget( this CollisionInstance collisionInstance, out Item item ) {
			return BetterHelpers.TryGetObjFromColliderGroup(collisionInstance.targetColliderGroup, out item);
		}
		/// <summary>
		/// Returns the item if there is one on a collider group
		/// </summary>
		/// <param name="colliderGroup"></param>
		/// <returns>Nullable item from colliderGroup</returns>
		public static Item GetItem( this ColliderGroup colliderGroup ) {
			BetterHelpers.TryGetObjFromColliderGroup(colliderGroup, out Item item);
			return item;
		}
		/// <summary>
		/// Tries to return the item if there is one on a collider group
		/// </summary>
		/// <param name="colliderGroup"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool TryGetItem( this ColliderGroup colliderGroup, out Item item ) {
			return BetterHelpers.TryGetObjFromColliderGroup(colliderGroup, out item);
		}


		/// <summary>
		/// Returns true if only the player is holding the handle
		/// </summary>
		/// <param name="handle"></param>
		/// <returns></returns>
		public static bool IsOnlyPlayerHolding( this Handle handle ) {
			return BetterHelpers.IsOnlyPlayerHolding(handle);
		}
		/// <summary>
		/// Returns true if only the player is holding the item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool IsOnlyPlayerHolding( this Item item ) {
			return BetterHelpers.IsOnlyPlayerHolding(item);
		}

		/// <summary>
		/// Returns true if a creature other than the player is holding the item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool IsOnlyCreatureExceptPlayerHolding( this Item item ) {
			return BetterHelpers.IsOnlyCreatureExceptPlayerHolding(item);
		}

		/// <summary>
		/// Returns true ifa creature other than the player is holding the handle
		/// </summary>
		/// <param name="handle"></param>
		/// <returns></returns>
		public static bool IsOnlyCreatureExceptPlayerHolding( this Handle handle ) {
			return BetterHelpers.IsOnlyCreatureExceptPlayerHolding(handle);
		}

		/// <summary>
		/// Returns true if the player is TK holding the ragdollPart
		/// </summary>
		/// <param name="ragdollPart"></param>
		/// <returns></returns>
		public static bool IsPlayerTkHolding( this RagdollPart ragdollPart ) {
			return BetterHelpers.IsPlayerTkHolding(ragdollPart);
		}
		/// <summary>
		/// Returns true if the player is TK holding the item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool IsPlayerTkHolding( this Item item ) {
			return BetterHelpers.IsPlayerTkHolding(item);
		}
		/// <summary>
		/// Returns true if the player is TK holding the handle
		/// </summary>
		/// <param name="handle"></param>
		/// <returns></returns>
		public static bool IsPlayerTkHolding( this Handle handle ) {
			return BetterHelpers.IsPlayerTkHolding(handle);
		}
		/// <summary>
		/// Returns true if the player is holding the RagdollPart
		/// </summary>
		/// <param name="ragdollPart"></param>
		/// <returns></returns>
		public static bool IsPlayerHolding( this RagdollPart ragdollPart ) {
			return BetterHelpers.IsPlayerHolding(ragdollPart);
		}
		/// <summary>
		/// Returns true if the player is holding the item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool IsPlayerHolding( this Item item ) {
			return BetterHelpers.IsPlayerHolding(item);
		}
		/// <summary>
		/// Returns true if the player is holding the handle
		/// </summary>
		/// <param name="handle"></param>
		/// <returns></returns>
		public static bool IsPlayerHolding( this Handle handle ) {
			return BetterHelpers.IsPlayerHolding(handle);
		}

		/// <summary>
		/// Tries to return the players hand which is holding the item
		/// </summary>
		/// <param name="item">item being held</param>
		/// <param name="ragdollHand">Players left or right hand</param>
		/// <returns></returns>
		public static bool TryGetPlayerHandHolding( this Item item, out RagdollHand ragdollHand ) {
			return BetterHelpers.TryGetPlayerHandHolding(item, out ragdollHand);
		}
		/// <summary>
		/// Tries to return the players hand which is holding the handle
		/// </summary>
		/// <param name="item">item being held</param>
		/// <param name="ragdollHand">Players left or right hand</param>
		/// <returns></returns>
		public static bool GetPlayerHandHolding( this Item item, out RagdollHand ragdollHand ) {
			BetterHelpers.TryGetPlayerHandHolding(item, out ragdollHand);
			return ragdollHand;
		}

		/// <summary>
		/// Tries to return the players hand which is holding the handle
		/// </summary>
		/// <param name="handle">handle being held</param>
		/// <param name="ragdollHand">Players left or right hand</param>
		/// <returns></returns>
		public static bool TryGetPlayerHandHolding(this Handle handle, out RagdollHand ragdollHand) {
			return BetterHelpers.TryGetPlayerHandHolding(handle, out ragdollHand);
		}
		/// <summary>
		/// Tries to return the players hand which is holding the handle
		/// </summary>
		/// <param name="handle">handle being held</param>
		/// <param name="ragdollHand">Players left or right hand</param>
		/// <returns></returns>
		public static bool GetPlayerHandHolding( this Handle handle, out RagdollHand ragdollHand ) {
			BetterHelpers.TryGetPlayerHandHolding(handle, out ragdollHand);
			return ragdollHand;
		}
		/// <summary>
		/// Disarms creature on both hands
		/// </summary>
		/// <param name="creature"></param>
		public static void Disarm( this Creature creature ) {
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
		public static bool IsGrabbingHandle( this Creature creature ) {
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
		public static void AllowCollisionWith( this Item item, Item otherItem ) {
			MakeItemCollideWith(item, otherItem);
		}

		/// <summary>
		/// Will make an item's colliders collide with a specific collider
		/// </summary>
		/// <param name="item"></param>
		/// <param name="collider"></param>
		public static void AllowCollisionWith( this Item item, Collider collider ) {
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
		public static bool TryGetTkHandle( this SpellCaster spellCaster, out Handle handle ) {
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
