using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using Wully.Module;
using UnityEngine;

namespace Wully.Helpers {
	/// <summary>
	/// A collection of helper methods
	/// </summary>
	public class BetterHelpers {

		/// <summary>
		/// Make a creature drop whatever they are holding
		/// </summary>
		/// <param name="creature"></param>
		public static void DisarmCreature( Creature creature ) {
			if ( creature == null ) {
				Debug.LogWarning("DisarmCreature: creature is null");
				return;
			}
			DisarmCreature(creature, Side.Left);
			DisarmCreature(creature, Side.Right);
		}

		/// <summary>
		/// Make a creature drop whatever they are holding in a particular side
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="side"></param>
		public static void DisarmCreature(Creature creature, Side side ) {
			if(creature == null ) {
				Debug.LogWarning("DisarmCreature: creature is null");
				return;
			}
			if(side == Side.Left ) {
				if(IsCreatureGrabbingHandle(creature, side) ) {
					creature.handLeft.UnGrab(false);
				}
			} else {
				if ( IsCreatureGrabbingHandle(creature, side) ) {
					creature.handRight.UnGrab(false);
				}
			}
		}

		/// <summary>
		/// Returns true if the creature is holding something
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public static bool IsCreatureArmed( Creature creature ) {
			return IsCreatureGrabbingHandle(creature);
		}
		/// <summary>
		/// Returns true if the creature is holding something in a particular side
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="side"></param>
		/// <returns></returns>
		public static bool IsCreatureArmed( Creature creature, Side side ) {
			return IsCreatureGrabbingHandle(creature, side);
		}
		/// <summary>
		/// Returns true if the creature is holding something
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public static bool IsCreatureGrabbingHandle( Creature creature ) {			
			return IsCreatureGrabbingHandle(creature,Side.Left) || IsCreatureGrabbingHandle(creature, Side.Right);						
		}
		/// <summary>
		/// Returns true if the creature is holding something in a particular side
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="side"></param>
		/// <returns></returns>
		public static bool IsCreatureGrabbingHandle(Creature creature, Side side ) {
			if(side == Side.Left ) {
				return creature?.handLeft.grabbedHandle != null;
			} else {
				return creature?.handRight.grabbedHandle != null;
			}
		}

		/// <summary>
		/// Will make an item's colliders collide with another item's colliders
		/// </summary>
		/// <param name="item"></param>
		/// <param name="otherItem"></param>
		public static void MakeItemCollideWithOtherItem( Item item, Item otherItem ) {
			if(item == null ) {
				Debug.LogWarning("MakeItemCollideWithOtherItem: item is null");
				return;
			}
			if ( otherItem == null ) {
				Debug.LogWarning("MakeItemCollideWithOtherItem: otherItem is null");
				return;
			}
	
			foreach ( ColliderGroup colliderGroup in item.colliderGroups ) {
				foreach ( Collider collider in colliderGroup.colliders ) {
					MakeItemCollideWithCollider(otherItem, collider);
				}
			}

		}

		/// <summary>
		/// Will make an item's colliders collide with a specific collider
		/// </summary>
		/// <param name="item"></param>
		/// <param name="otherCollider"></param>
		public static void MakeItemCollideWithCollider( Item item, Collider otherCollider ) {
			if ( item == null ) {
				Debug.LogWarning("MakeItemCollideWithCollider: item is null");
				return;
			}
			if ( otherCollider == null ) {
				Debug.LogWarning("MakeItemCollideWithCollider: otherCollider is null");
				return;
			}
			foreach ( ColliderGroup colliderGroup in item.colliderGroups ) {
				foreach ( Collider collider in colliderGroup.colliders ) {
					Physics.IgnoreCollision(collider, otherCollider, false);
				}
			}
		}

		/// <summary>
		/// Will make an item's colliders ignore collisions with another items colliders
		/// </summary>
		/// <param name="item"></param>
		/// <param name="otherItem"></param>
		public static void MakeItemNotCollideWithOtherItem( Item item, Item otherItem ) {
			if ( item == null ) {
				Debug.LogWarning("MakeItemNotCollideWithOtherItem: item is null");
				return;
			}
			if ( otherItem == null ) {
				Debug.LogWarning("MakeItemNotCollideWithOtherItem: otherItem is null");
				return;
			}
			foreach ( ColliderGroup colliderGroup in item.colliderGroups ) {
				foreach ( Collider collider in colliderGroup.colliders ) {
					MakeItemNotCollideWithCollider(otherItem, collider);					
				}
			}
		}

		/// <summary>
		/// Will make an item's colliders ignore collisions with a specific collider
		/// </summary>
		/// <param name="item"></param>
		/// <param name="otherCollider"></param>
		public static void MakeItemNotCollideWithCollider(Item item, Collider otherCollider) {
			if ( item == null ) {
				Debug.LogWarning("MakeItemNotCollideWithCollider: item is null");
				return;
			}
			if ( otherCollider == null ) {
				Debug.LogWarning("MakeItemNotCollideWithCollider: otherCollider is null");
				return;
			}
			foreach ( ColliderGroup colliderGroup in item.colliderGroups ) {
				foreach ( Collider collider in colliderGroup.colliders ) {					
					Physics.IgnoreCollision(collider, otherCollider, true);					
				}
			}
		}
		/// <summary>
		/// returns true if grip button or cast button is being pressed on any controller side
		/// </summary>
		/// <returns></returns>
		public static bool IsGripOrCastPressed() {
			return IsGripAndCastPressed(Side.Left) || IsGripAndCastPressed(Side.Right);
		}
		/// <summary>
		/// returns true if grip button or cast button is being pressed on a controller side
		/// </summary>
		/// <param name="side"></param>
		/// <returns></returns>
		public static bool IsGripOrCastPressed( Side side ) {
			return IsGripPressed(side) || IsCastPressed(side);
		}
		/// <summary>
		/// returns true if grip button and cast button is being pressed on any controller side
		/// </summary>
		/// <returns></returns>
		public static bool IsGripAndCastPressed() {
			return IsGripAndCastPressed(Side.Left) || IsGripAndCastPressed(Side.Right);
		}
		/// <summary>
		/// returns true if grip button and cast button is being pressed on a controller side
		/// </summary>
		/// <param name="side"></param>
		/// <returns></returns>
		public static bool IsGripAndCastPressed( Side side ) {
			return IsGripPressed(side) && IsCastPressed(side);
		}
		/// <summary>
		/// returns true if grip button is being pressed on any controller
		/// </summary>
		/// <returns></returns>
		public static bool IsGripPressed() {
			return IsGripPressed(Side.Left) || IsGripPressed(Side.Right);
		}
		/// <summary>
		/// returns true if grip button is being pressed on a controller side
		/// </summary>
		/// <param name="side"></param>
		/// <returns></returns>
		public static bool IsGripPressed( Side side ) {
			return PlayerControl.GetHand(side).gripPressed;
		}
		/// <summary>
		/// returns true if cast button is being pressed on any controller
		/// </summary>
		/// <returns></returns>
		public static bool IsCastPressed() {
			return IsCastPressed(Side.Left) || IsCastPressed(Side.Right);
		}
		/// <summary>
		/// returns true if cast button is being pressed on controller side
		/// </summary>
		/// <param name="side"></param>
		/// <returns></returns>
		public static bool IsCastPressed( Side side ) {
			return PlayerControl.GetHand(side).castPressed;
		}

		/// <summary>
		/// Tries to return the Ragdoll hand for a creatures Side
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="side"></param>
		/// <param name="ragdollHand"></param>
		/// <returns></returns>
		public static bool TryGetRagdollHand(Creature creature, Side side, out RagdollHand ragdollHand ) {
			ragdollHand = null;
			if ( side == Side.Left ) {
				ragdollHand = (RagdollHand)(creature?.ragdoll?.GetPart(RagdollPart.Type.LeftHand));				
			} else {
				ragdollHand = (RagdollHand)(creature?.ragdoll?.GetPart(RagdollPart.Type.LeftHand));
			}
			if ( ragdollHand ) { return true;  }
			return false;
		}
		/// <summary>
		/// Tries to get the item a creature is holding on a particular side
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="side"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool TryGetHeldItem( Creature creature, Side side, out Item item ) {
			item = null;
			if(side == Side.Left ) {
				if ( TryGetRagdollHand(creature, side, out RagdollHand ragdollHand) ) {
					TryGetHeldItem(ragdollHand, out item);
				}
			} else {
				if ( TryGetRagdollHand(creature, side, out RagdollHand ragdollHand) ) {
					TryGetHeldItem(ragdollHand, out item);
				}
			}
			if ( item ) { return true; }
			return false;
		}
		/// <summary>
		/// Tries to get the item a ragdollhand is holding
		/// </summary>
		/// <param name="ragdollHand"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool TryGetHeldItem( RagdollHand ragdollHand, out Item item ) {
			item = ragdollHand?.grabbedHandle?.item;
			if ( item ) {
				return true;
			}
			return false;
		}

		/// <summary>
		/// Tries to get the item a spellcaster is holding
		/// </summary>
		/// <param name="spellCaster"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool TryGetHeldItem(SpellCaster spellCaster, out Item item ) {
			item = spellCaster?.ragdollHand?.grabbedHandle?.item;
			if(item) {
				return true;
			}			
			return false;
		}
		/// <summary>
		/// Helper method to Try and get the current instance of the BetterEvents levelmodule
		/// </summary>
		/// <param name="levelModuleBetterEvents"></param>
		/// <returns></returns>
		public static bool TryGetLevelModuleBetterEvents( out LevelModuleBetterEvents levelModuleBetterEvents ) {
			levelModuleBetterEvents = null;
			if ( LevelModuleBetterEvents.local != null ) {
				levelModuleBetterEvents = LevelModuleBetterEvents.local;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Returns true if player using TK in any hand
		/// </summary>
		/// <returns></returns>
		public static bool IsPlayerUsingTelekinesis() {
			return IsPlayerUsingTelekinesis(Side.Left) || IsPlayerUsingTelekinesis(Side.Right);
		}
		/// <summary>
		/// Returns true if player using TK for given hand side
		/// </summary>
		/// <param name="side"></param>
		/// <returns></returns>
		public static bool IsPlayerUsingTelekinesis( Side side ) {

			if ( TryGetLevelModuleBetterEvents(out LevelModuleBetterEvents module)){
				SpellCaster spellCaster = module.spellCasterRight;				
				if ( side == Side.Left ) {
					spellCaster = module.spellCasterLeft;
				}
				if ( spellCaster?.telekinesis?.catchedHandle != null ) {
					return true;
				}				
			}
			return false;
		}

		/// <summary>
		/// Checks to see if the ragdoll is being choked
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public static bool IsBeingChoked( Creature creature ) {
			foreach ( RagdollPart ragdollPart in creature.ragdoll.parts ) {
				foreach ( HandleRagdoll handleRagdoll in ragdollPart.handles ) {
					if ( handleRagdoll.handleRagdollData.choke && handleRagdoll.ragdollPart.ragdoll.creature.state != Creature.State.Dead && (handleRagdoll.telekinesisHandler || handleRagdoll.IsHanded()) ) {
						return true;
					}
				}
			}
			return false;
		}
		/// <summary>
		/// Checks to see if the ragdoll was being choked and is now dead
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public static bool WasBeingChoked( Creature creature ) {
			foreach ( RagdollPart ragdollPart in creature.ragdoll.parts ) {
				foreach ( HandleRagdoll handleRagdoll in ragdollPart.handles ) {
					if ( handleRagdoll.handleRagdollData.choke && (handleRagdoll.telekinesisHandler || handleRagdoll.IsHanded()) ) {
						return true;
					}
				}
			}
			return false;
		}


		/// <summary>
		/// Check if handle is being choked
		/// </summary>
		/// <param name="handle"></param>
		/// <returns></returns>
		public static bool IsHandleChoked( Handle handle ) {
			if ( handle is HandleRagdoll handleRagdoll ) {
				if ( IsHandleChokeable(handleRagdoll) && handleRagdoll.ragdollPart.ragdoll.creature.state != Creature.State.Dead && (handleRagdoll.telekinesisHandler || handleRagdoll.IsHanded()) ) {
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Check if a specific handle is chokable
		/// </summary>
		/// <param name="handle"></param>
		/// <returns></returns>
		public static bool IsHandleChokeable( Handle handle ) {
			if ( handle is HandleRagdoll handleRagdoll ) {
				return IsHandleChokeable(handleRagdoll);
			}
			return false;
		}
		/// <summary>
		/// Check if a specific HandleRagdoll is chokable
		/// </summary>
		/// <param name="handle"></param>
		/// <returns></returns>
		public static bool IsHandleChokeable( HandleRagdoll handle ) {
			if ( handle.handleRagdollData.choke ) {
				return true;
			}
			return false;
		}
	}
}
