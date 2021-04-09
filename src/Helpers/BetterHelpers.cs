using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;
using Wully.Extensions;
using Wully.Module;

namespace Wully.Helpers {
	/// <summary>
	/// A collection of helper methods
	/// </summary>
	public class BetterHelpers {
		private static BetterLogger log = BetterLogger.GetLogger(typeof(BetterHelpers));

		/// <summary>
		/// Returns true if the players held item was hit by a flying item
		/// </summary>
		/// <param name="sourceItem"></param>
		/// <param name="targetItem"></param>
		/// <returns></returns>
		/// <remarks>This function assumes both things hit each other and came from a collisionInstance</remarks>
		public static bool DidFlyingItemHitPlayersItem(Item sourceItem, Item targetItem) {
			//items not null
			if ( !sourceItem || !targetItem ) { return false; }
			//source item not being held at all
			if ( sourceItem.IsHanded() ) { return false; }
			//source item wasn't last touched by the player
			if (sourceItem.lastHandler?.creature?.isPlayer == true) { return false;}

			// player is holding target item
			if ( !targetItem.IsPlayerHolding() ) { return false; }
			// creature isnt holding target
			if ( targetItem.IsCreatureExceptPlayerHolding() ) { return false; }
			return true;
		}

		/// <summary>
		/// Returns true if the players ragdoll part, normally fists, hit the world/ground
		/// </summary>
		/// <param name="sourcePart"></param>
		/// <param name="collisionInstance"></param>
		/// <returns></returns>
		/// <remarks>This function assumes both things hit each other and came from a collisionInstance</remarks>
		public static bool DidPlayersRagdollPartHitGround( RagdollPart sourcePart, CollisionInstance collisionInstance ) {
			//not null
			if ( !sourcePart ) { return false; }
			// collisionInstance has no target, which means it hit the world, not a item/ragdoll
			if ( collisionInstance.targetColliderGroup ) { return false; }
			// source part is the player
			if ( !sourcePart.ragdoll.creature.isPlayer ) { return false; }


			return true;
		}

		/// <summary>
		/// Returns true if the item only held by the player hit the world/ground, not a item or ragdoll
		/// </summary>
		/// <param name="sourceItem"></param>
		/// <param name="collisionInstance"></param>
		/// <returns></returns>
		/// <remarks>This function assumes both things hit each other and came from a collisionInstance</remarks>
		public static bool DidPlayersItemHitGround(Item sourceItem, CollisionInstance collisionInstance) {
			//not null
			if (!sourceItem) { return false; }
			// collisionInstance has no target, which means it hit the world, not a item/ragdoll
			if (collisionInstance.targetColliderGroup) { return false; }
			// source item is held by the player
			if ( !sourceItem.IsPlayerHolding() ) { return false; }
			// and only the player
			if ( sourceItem.IsCreatureExceptPlayerHolding() ) { return false; }

			return true;
		}
		/// <summary>
		/// Returns true if the source Item was held only the player and it hit the player
		/// </summary>
		/// <param name="sourceItem">Item from sourceCollider CollisionInstance</param>
		/// <param name="targetPart">RagdollPart from targetCollider CollisionInstance</param>
		/// <returns></returns>
		/// <remarks>This function assumes both things hit each other and came from a collisionInstance</remarks>
		public static bool DidPlayersItemHitPlayersRagdollPart( Item sourceItem, RagdollPart targetPart ) {
			// not null
			if ( !sourceItem || !targetPart ) { return false; }
			// source item is held by the player
			if ( !sourceItem.IsPlayerHolding() ) { return false; }
			// and only the player
			if ( sourceItem.IsCreatureExceptPlayerHolding() ) { return false; }

			// target ragdoll part is the player
			if ( !targetPart.ragdoll.creature.isPlayer ) { return false; }

			return true;
		}

		/// <summary>
		/// Returns true if the source Item was held only the player and it hit a creature that wasnt the player
		/// </summary>
		/// <param name="sourceItem">Item from sourceCollider CollisionInstance</param>
		/// <param name="targetPart">RagdollPart from targetCollider CollisionInstance</param>
		/// <returns></returns>
		/// <remarks>This function assumes both things hit each other and came from a collisionInstance</remarks>
		public static bool DidPlayersItemHitCreaturesRagdollPart(Item sourceItem, RagdollPart targetPart) {
			// not null
			if ( !sourceItem || !targetPart ) { return false; }
			// source item is held by the player
			if ( !sourceItem.IsPlayerHolding() ) { return false; }
			// and only the player
			if ( sourceItem.IsCreatureExceptPlayerHolding() ) { return false; }

			// target ragdoll part is not the player
			if (targetPart.ragdoll.creature.isPlayer) { return false; }

			return true;
		}
		/// <summary>
		/// Returns true if the source item was held by the player and hit the target Item held only by a creature that wasn't the player
		/// </summary>
		/// <param name="sourceItem">Item from sourceCollider CollisionInstance</param>
		/// <param name="targetItem">Item from targetCollider CollisionInstance</param>
		/// <returns></returns>
		/// <remarks>This function assumes both items hit each other and came from a collisionInstance</remarks>
		public static bool DidCreaturesItemHitPlayersItem( Item sourceItem, Item targetItem ) {
			//items not null
			if ( !sourceItem || !targetItem) { return false; }
			//source item is held by a creature
			if ( !sourceItem.IsCreatureExceptPlayerHolding() ) { return false; }
			// but not by the player as well
			if ( sourceItem.IsPlayerHolding() ) { return false; } 
			// player is holding target item
			if ( !targetItem.IsPlayerHolding() ) { return false; } 
			// creature isnt holding target
			if ( targetItem.IsCreatureExceptPlayerHolding() ) { return false; }

			return true;
		}

		/// <summary>
		/// Returns true if the source item was held by the player and hit the target Item held only by a creature that wasn't the player
		/// </summary>
		/// <param name="sourceItem">Item from sourceCollider CollisionInstance</param>
		/// <param name="targetItem">Item from targetCollider CollisionInstance</param>
		/// <returns></returns>
		/// <remarks>This function assumes both items hit each other and came from a collisionInstance</remarks>
		public static bool DidPlayersItemHitCreaturesItem( Item sourceItem, Item targetItem ) {
			//items not null
			if ( !sourceItem || !targetItem ) { return false; }
			// source item is held by the player
			if ( !sourceItem.IsPlayerHolding() ) { return false; }
			// and only the player
			if ( sourceItem.IsCreatureExceptPlayerHolding() ) { return false; }
			// target is held by the a creature
			if ( !targetItem.IsCreatureExceptPlayerHolding() ) { return false; }
			// by not the player
			if ( targetItem.IsPlayerHolding() ) { return false; }
			
			return true;
		}
		/// <summary>
		/// Tries to return the item if there is one on a collider group
		/// </summary>
		/// <param name="colliderGroup"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool TryGetObjFromColliderGroup( ColliderGroup colliderGroup, out Item item ) {
			item = colliderGroup?.collisionHandler?.item;
			return item != null;
		}
		/// <summary>
		/// Tries to return the ragdoll part if there is one on a collider group
		/// </summary>
		/// <param name="colliderGroup"></param>
		/// <param name="ragdollPart"></param>
		/// <returns></returns>
		public static bool TryGetObjFromColliderGroup( ColliderGroup colliderGroup, out RagdollPart ragdollPart ) {
			ragdollPart = colliderGroup?.collisionHandler?.ragdollPart;
			return ragdollPart != null;
		}

		/// <summary>
		/// Returns true if a creature other than the player is holding the item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool IsCreatureExceptPlayerHolding( Item item ) {
			if ( item != null ) {
				foreach ( RagdollHand hand in item.handlers ) {
					if ( !hand.creature.isPlayer ) {
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Returns true ifa creature other than the player is holding the handle
		/// </summary>
		/// <param name="handle"></param>
		/// <returns></returns>
		public static bool IsCreatureExceptPlayerHolding( Handle handle ) {
			if ( handle != null ) {
				foreach ( RagdollHand hand in handle?.handlers ) {
					if ( !hand.creature.isPlayer ) {
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Returns true if the player is holding the item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool IsPlayerHolding( Item item ) {
			if ( item != null ) {
				foreach ( RagdollHand hand in item.handlers ) {
					if ( hand.creature.isPlayer ) {
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Returns true if the player is holding the handle
		/// </summary>
		/// <param name="handle"></param>
		/// <returns></returns>
		public static bool IsPlayerHolding( Handle handle ) {
			if (handle == null) { return false;}

			return Player.currentCreature?.handLeft?.grabbedHandle == handle ||
			       Player.currentCreature?.handRight?.grabbedHandle == handle;
		}

		/// <summary>
		/// Tries to return the players hand which is holding a item
		/// </summary>
		/// <param name="item">Item being held</param>
		/// <param name="ragdollHand">Players left or right hand</param>
		/// <returns></returns>
		public static bool TryGetPlayerHandHolding( Item item, out RagdollHand ragdollHand) {
			ragdollHand = null;
			if (item == null) { return false;}

			//need to loop so we can check all in handlers
			foreach ( RagdollHand hand in item.handlers ) {
				if ( hand.creature.isPlayer ) {
					ragdollHand = hand;
					return true;
				}
			}
			
			return false;
		}

		/// <summary>
		/// Tries to return the players hand which is holding the handle
		/// </summary>
		/// <param name="handle">handle being held</param>
		/// <param name="ragdollHand">Players left or right hand</param>
		/// <returns></returns>
		public static bool TryGetPlayerHandHolding( Handle handle, out RagdollHand ragdollHand ) {
			ragdollHand = null;
			if ( handle == null ) { return false; }

			if ( Player.currentCreature?.handLeft?.grabbedHandle == handle) {
				ragdollHand = Player.currentCreature.handLeft;
				return true;
			}
			if ( Player.currentCreature?.handRight?.grabbedHandle == handle ) {
				ragdollHand = Player.currentCreature.handRight;
				return true;
			}

			return false;
		}
		/// <summary>
		/// Get a list of item Ids's for a particular ItemData type
		/// </summary>
		/// <param name="type">ItemData type, such as Weapon, Spell, Shield</param>
		/// <returns></returns>
		public static List<string> GetItemDataIdList( ItemData.Type type ) {
			return (
				from item in Catalog.GetDataList(Catalog.Category.Item)
				where ((ItemData)item).type == type
				select item.id).ToList<string>();
		}

		/// <summary>
		/// Get a list of ItemData's for a particular ItemData type
		/// </summary>
		/// <param name="type">ItemData type, such as Weapon, Spell, Shield</param>
		/// <returns></returns>
		public static List<ItemData> GetItemDataList( ItemData.Type type ) {
			return (
				from item in Catalog.GetDataList(Catalog.Category.Item)
				where ((ItemData)item).type == type
				select ((ItemData)item)).ToList<ItemData>();
		}

		/// <summary>
		/// Returns true if the player is currently pointing at a book menu
		/// </summary>
		/// <returns></returns>
		public static bool IsPlayerPointingAtBook() {
			return Pointer.GetActive().isPointingUI;
		}
		/// <summary>
		/// Make a creature drop whatever they are holding
		/// </summary>
		/// <param name="creature"></param>
		public static void DisarmCreature( Creature creature ) {
			if ( creature == null ) {
				log.Debug("DisarmCreature: creature is null");
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
		public static void DisarmCreature( Creature creature, Side side ) {
			if ( creature == null ) {
				log.Debug("DisarmCreature: creature is null");
				return;
			}
			if ( side == Side.Left ) {
				if ( IsCreatureGrabbingHandle(creature, side) ) {
					log.Debug("DisarmCreature: Disarming creature left");
					creature.handLeft.UnGrab(false);
				}
			} else {
				if ( IsCreatureGrabbingHandle(creature, side) ) {
					log.Debug("DisarmCreature: Disarming creature right");
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
			return IsCreatureGrabbingHandle(creature, Side.Left) || IsCreatureGrabbingHandle(creature, Side.Right);
		}
		/// <summary>
		/// Returns true if the creature is holding something in a particular side
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="side"></param>
		/// <returns></returns>
		public static bool IsCreatureGrabbingHandle( Creature creature, Side side ) {
			if ( side == Side.Left ) {
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
		public static void MakeItemCollideWith( Item item, Item otherItem ) {
			if ( item == null ) {
				log.Debug("MakeItemCollideWithOtherItem: item is null");
				return;
			}
			if ( otherItem == null ) {
				log.Debug("MakeItemCollideWithOtherItem: otherItem is null");
				return;
			}

			foreach ( ColliderGroup colliderGroup in item.colliderGroups ) {
				foreach ( Collider collider in colliderGroup.colliders ) {
					MakeItemCollideWith(otherItem, collider);
				}
			}

		}

		/// <summary>
		/// Will make an item's colliders collide with a specific collider
		/// </summary>
		/// <param name="item"></param>
		/// <param name="otherCollider"></param>
		public static void MakeItemCollideWith( Item item, Collider otherCollider ) {
			if ( item == null ) {
				log.Debug("MakeItemCollideWithCollider: item is null");
				return;
			}
			if ( otherCollider == null ) {
				log.Debug("MakeItemCollideWithCollider: otherCollider is null");
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
		public static void MakeItemNotCollideWith( Item item, Item otherItem ) {
			if ( item == null ) {
				log.Debug("MakeItemNotCollideWithOtherItem: item is null");
				return;
			}
			if ( otherItem == null ) {
				log.Debug("MakeItemNotCollideWithOtherItem: otherItem is null");
				return;
			}
			foreach ( ColliderGroup colliderGroup in item.colliderGroups ) {
				foreach ( Collider collider in colliderGroup.colliders ) {
					MakeItemNotCollideWith(otherItem, collider);
				}
			}
		}

		/// <summary>
		/// Will make an item's colliders ignore collisions with a specific collider
		/// </summary>
		/// <param name="item"></param>
		/// <param name="otherCollider"></param>
		public static void MakeItemNotCollideWith( Item item, Collider otherCollider ) {
			if ( item == null ) {
				log.Debug("MakeItemNotCollideWithCollider: item is null");
				return;
			}
			if ( otherCollider == null ) {
				log.Debug("MakeItemNotCollideWithCollider: otherCollider is null");
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
		/// returns true if alternate use button is being pressed on any controller
		/// </summary>
		/// <returns></returns>
		public static bool IsAlternateUsePressed() {
			return IsAlternateUsePressed(Side.Left) || IsAlternateUsePressed(Side.Right);
		}

		/// <summary>
		/// returns true if alternate use button is being pressed on controller side
		/// </summary>
		/// <param name="side"></param>
		/// <returns></returns>
		public static bool IsAlternateUsePressed( Side side ) {
			return PlayerControl.GetHand(side).alternateUsePressed;
		}

		/// <summary>
		/// Tries to return the handle the spellcaster is currently holding with telekinesis
		/// </summary>
		/// <param name="spellCaster">side specific spellcaster</param>
		/// <param name="handle">The handle held by the spellcaster with telekinesis</param>
		/// <returns></returns>
		public static bool TryGetTelekinesisCaughtHandle( SpellCaster spellCaster, out Handle handle ) {
			handle = spellCaster?.telekinesis?.catchedHandle;
			if ( handle ) {
				log.Debug("TryGetTelekinesisCaughtHandle: spellcaster is holding a handle with telekinesis");
				return true;
			}
			log.Debug("TryGetTelekinesisCaughtHandle: spellcaster not holding handle with telekinesis");
			return false;
		}

		/// <summary>
		/// Tries to return the Ragdoll hand for a creatures Side
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="side"></param>
		/// <param name="ragdollHand"></param>
		/// <returns></returns>
		public static bool TryGetRagdollHand( Creature creature, Side side, out RagdollHand ragdollHand ) {
			ragdollHand = null;
			if ( side == Side.Left ) {
				ragdollHand = (RagdollHand)(creature?.ragdoll?.GetPart(RagdollPart.Type.LeftHand));
			} else {
				ragdollHand = (RagdollHand)(creature?.ragdoll?.GetPart(RagdollPart.Type.LeftHand));
			}
			if ( ragdollHand ) { return true; }
			log.Debug("TryGetRagdollHand: Could not get ragdollHand");
			return false;
		}

		/// <summary>
		/// Tries to return both ragdoll hands for a creature
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="leftRagdollHand"></param>
		/// <param name="rightRagdollHand"></param>
		/// <returns></returns>
		public static bool TryGetRagdollHand( Creature creature, out RagdollHand leftRagdollHand, out RagdollHand rightRagdollHand ) {

			leftRagdollHand = (RagdollHand)(creature?.ragdoll?.GetPart(RagdollPart.Type.LeftHand));
			rightRagdollHand = (RagdollHand)(creature?.ragdoll?.GetPart(RagdollPart.Type.LeftHand));

			if ( leftRagdollHand && rightRagdollHand ) { return true; }
			log.Debug("TryGetRagdollHand: Could not get both ragdollHands");
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
			if ( side == Side.Left ) {
				if ( TryGetRagdollHand(creature, side, out RagdollHand ragdollHand) ) {
					TryGetHeldItem(ragdollHand, out item);
				}
			} else {
				if ( TryGetRagdollHand(creature, side, out RagdollHand ragdollHand) ) {
					TryGetHeldItem(ragdollHand, out item);
				}
			}
			if ( item ) { return true; }
			log.Debug("TryGetHeldItem: Could not get heldItem");
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
			log.Debug("TryGetHeldItem: Could not get heldItem");
			return false;
		}

		/// <summary>
		/// Tries to get the item a spellcaster is holding
		/// </summary>
		/// <param name="spellCaster"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool TryGetHeldItem( SpellCaster spellCaster, out Item item ) {
			item = spellCaster?.ragdollHand?.grabbedHandle?.item;
			if ( item ) {
				return true;
			}
			log.Debug("TryGetHeldItem: Could not get heldItem");
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
			log.Debug("TryGetLevelModuleBetterEvents: Could not get LevelModuleBetterEvents");
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
		/// <remarks>This actually checks if the player is holding something with TK</remarks>
		public static bool IsPlayerUsingTelekinesis( Side side ) {

			//Tries to get the betterevents module first since that will be monitoring the players TK
			if ( TryGetLevelModuleBetterEvents(out LevelModuleBetterEvents module) ) {

				if ( side == Side.Left ) {
					if ( module.SpellCasterLeftGrabbedHandle != null ) {
						log.Debug("IsPlayerUsingTelekinesis: true for side: {0}", side.ToString());
						return true;
					}
				} else {
					if ( module.SpellCasterRightGrabbedHandle != null ) {
						log.Debug("IsPlayerUsingTelekinesis: true for side: {0}", side.ToString());
						return true;
					}
				}
			}

			// if betterevents module is disabled or unavailable, check directly
			if ( side == Side.Left ) {
				if ( TryGetTelekinesisCaughtHandle(Player.local?.creature?.mana?.casterLeft, out Handle handle) ) {
					log.Debug("IsPlayerUsingTelekinesis: true for side: {0}", side.ToString());
					return true;
				}
			} else {
				if ( TryGetTelekinesisCaughtHandle(Player.local?.creature?.mana?.casterRight, out Handle handle) ) {
					log.Debug("IsPlayerUsingTelekinesis: true for side: {0}", side.ToString());
					return true;
				}
			}

			log.Debug("IsPlayerUsingTelekinesis: false");
			return false;
		}

		/// <summary>
		/// Checks to see if the ragdoll is being choked
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public static bool TryGetChokedHandle( Creature creature, out HandleRagdoll chokedHandleRagdoll ) {
			chokedHandleRagdoll = null;
			foreach ( RagdollPart ragdollPart in creature.ragdoll.parts ) {
				foreach ( HandleRagdoll handleRagdoll in ragdollPart.handles ) {
					if ( handleRagdoll.handleRagdollData.choke && handleRagdoll.ragdollPart.ragdoll.creature.state != Creature.State.Dead && (handleRagdoll.telekinesisHandler || handleRagdoll.IsHanded()) ) {
						log.Debug("IsBeingChoked: true");
						chokedHandleRagdoll = handleRagdoll;
						return true;
					}
				}
			}
			log.Debug("IsBeingChoked: false");
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
						log.Debug("IsBeingChoked: true");
						return true;
					}
				}
			}
			log.Debug("IsBeingChoked: false");
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
						log.Debug("WasBeingChoked: true");
						return true;
					}
				}
			}
			log.Debug("WasBeingChoked: false");
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
					log.Debug("IsHandleChoked: true");
					return true;
				}
			}
			log.Debug("IsHandleChoked: false");
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
			log.Debug("IsHandleChokeable: false");
			return false;
		}
		/// <summary>
		/// Check if a specific HandleRagdoll is chokable
		/// </summary>
		/// <param name="handle"></param>
		/// <returns></returns>
		public static bool IsHandleChokeable( HandleRagdoll handle ) {
			if ( handle.handleRagdollData.choke ) {
				log.Debug("IsHandleChokeable: true");
				return true;
			}
			log.Debug("IsHandleChokeable: false");
			return false;
		}
	}
}
