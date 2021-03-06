using System;
using ThunderRoad;
using UnityEngine;
using Wully.Extensions;
using Wully.Helpers;
using Wully.Module;
using static Wully.Helpers.BetterHelpers;

namespace Wully.Events {
	/// <summary>
	/// A system for managing Blade and Sorcery events
	/// </summary>
	public class BetterEvents {
		private static BetterLogger log = BetterLogger.GetLogger(typeof(BetterEvents));


		#region item hit events

		#endregion

		#region choking

		/// <summary>
		/// Player choked creature with telekinesis
		/// </summary>
		public static event ChokeEvent OnPlayerTelekinesisChokeCreature;
		/// <summary>
		/// Player choked creature with hand
		/// </summary>
		public static event ChokeEvent OnPlayerHandChokeCreature;
		/// <summary>
		/// Player choked creature with hand or Telekinesis
		/// </summary>
		public static event ChokeEvent OnPlayerChokeCreature;
		/// <summary>
		/// ChokeEvent
		/// </summary>
		/// <param name="side"></param>
		/// <param name="handle"></param>
		/// <param name="creature"></param>
		/// <param name="eventTime"></param>
		public delegate void ChokeEvent( Side side, Handle handle, Creature creature, EventTime eventTime );

		#endregion

		#region playerhand grab

		/// <summary>
		/// Event is called when a player hand grabs something
		/// </summary>
		/// <param name="side"></param> Which hand grabbed it
		/// <param name="handle"></param> Handle of the thing grabbed
		/// <param name="axisPosition"></param>
		/// <param name="orientation"></param>
		public delegate void PlayerHandGrabEvent( Side side, Handle handle, float axisPosition, HandleOrientation orientation );

		/// <summary>
		/// One of the players hands grabs a handle
		/// </summary>
		public static event PlayerHandGrabEvent OnPlayerHandGrabHandle;
		/// <summary>
		/// Players Left hand grabs a handle
		/// </summary>
		public static event PlayerHandGrabEvent OnPlayerLeftHandGrabHandle;
		/// <summary>
		/// Players Right hand grabs a handle
		/// </summary>
		public static event PlayerHandGrabEvent OnPlayerRightHandGrabHandle;
		/// <summary>
		/// One of the players hands grabs a item
		/// </summary>
		public static event PlayerHandGrabEvent OnPlayerHandGrabItemHandle;
		/// <summary>
		/// Players Left hand grabs a item
		/// </summary>
		public static event PlayerHandGrabEvent OnPlayerLeftHandGrabItemHandle;
		/// <summary>
		/// Players Right hand grabs a item
		/// </summary>
		public static event PlayerHandGrabEvent OnPlayerRightHandGrabItemHandle;
		/// <summary>
		/// One of the players hands grabs a ragdollpart
		/// </summary>
		public static event PlayerHandGrabEvent OnPlayerHandGrabRagdollPartHandle;
		/// <summary>
		/// Players Left hand grabs a ragdollpart
		/// </summary>
		public static event PlayerHandGrabEvent OnPlayerLeftHandGrabRagdollPartHandle;
		/// <summary>
		/// Players Right hand grabs a ragdollpart
		/// </summary>
		public static event PlayerHandGrabEvent OnPlayerRightHandGrabRagdollPartHandle;

		public static void InvokePlayerHandGrabEvent( Side side, Handle handle, float axisPosition, HandleOrientation orientation, EventTime eventTime ) {
			log.Debug().Message($"side: {side}, handle: {handle.name}, axisPosition: {axisPosition},isChokeable: {IsHandleChokeable(handle)},  eventTime: {eventTime} ");

			OnPlayerHandGrabHandle?.Invoke(side, handle, axisPosition, orientation);

			if ( side == Side.Right ) {
				//right hand grabbed something
				OnPlayerRightHandGrabHandle?.Invoke(side, handle, axisPosition, orientation);

				if ( handle is HandleRagdoll ) {
					//right hand grabbed ragdoll part
					OnPlayerRightHandGrabRagdollPartHandle?.Invoke(side, handle, axisPosition, orientation);
				} else {
					//right hand grabbed item
					OnPlayerRightHandGrabItemHandle?.Invoke(side, handle, axisPosition, orientation);
				}

			} else {
				OnPlayerLeftHandGrabHandle?.Invoke(side, handle, axisPosition, orientation);

				if ( handle is HandleRagdoll ) {
					//Left hand grabbed ragdoll part
					OnPlayerLeftHandGrabRagdollPartHandle?.Invoke(side, handle, axisPosition, orientation);
				} else {
					//Left hand grabbed item
					OnPlayerLeftHandGrabItemHandle?.Invoke(side, handle, axisPosition, orientation);
				}
			}

			if ( handle is HandleRagdoll handleRagdoll ) {
				OnPlayerHandGrabRagdollPartHandle?.Invoke(side, handle, axisPosition, orientation);
				if ( IsHandleChoked(handle) ) {
					OnPlayerHandChokeCreature?.Invoke(side, handle, handleRagdoll.ragdollPart.ragdoll.creature, EventTime.OnStart);
					OnPlayerChokeCreature?.Invoke(side, handle, handleRagdoll.ragdollPart.ragdoll.creature, EventTime.OnStart);
				}
			} else {
				OnPlayerHandGrabItemHandle?.Invoke(side, handle, axisPosition, orientation);
			}

		}
		#endregion

		#region Player hand ungrabbing a handle

		/// <summary>
		/// Event is called when a player hand ungrabs something
		/// </summary>
		/// <param name="side"></param>
		/// <param name="handle"></param>
		/// <param name="throwing"></param>
		public delegate void PlayerHandUnGrabEvent( Side side, Handle handle, bool throwing );

		/// <summary>
		/// One of the players hands ungrabs a handle
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerHandUnGrabHandle;
		/// <summary>
		/// Players Left hand ungrabs a handle
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerLeftHandUnGrabHandle;
		/// <summary>
		/// Players Right hand ungrabs a handle
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerRightHandUnGrabHandle;
		/// <summary>
		/// One of the players hands ungrabs a item
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerHandUnGrabItemHandle;
		/// <summary>
		/// Players Left hand ungrabs a item
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerLeftHandUnGrabItemHandle;
		/// <summary>
		/// Players Right hand ungrabs a item
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerRightHandUnGrabItemHandle;
		/// <summary>
		/// One of the players hands ungrabs a ragdollpart
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerHandUnGrabRagdollPartHandle;
		/// <summary>
		/// Players Left hand ungrabs a ragdollpart
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerLeftHandUnGrabRagdollPartHandle;
		/// <summary>
		/// Players Right hand ungrabs a ragdollpart
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerRightHandUnGrabRagdollPartHandle;
		#region playerhand throw something
		/// <summary>
		/// One of the players hands throws a handle
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerHandThrowHandle;
		/// <summary>
		/// Players Left hand throws a handle
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerLeftHandThrowHandle;
		/// <summary>
		/// Players Right hand throws a handle
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerRightHandThrowHandle;
		/// <summary>
		/// One of the players hands throws a item
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerHandThrowItemHandle;
		/// <summary>
		/// Players Left hand throws a item
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerLeftHandThrowItemHandle;
		/// <summary>
		/// Players Right hand throws a item
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerRightHandThrowItemHandle;
		/// <summary>
		/// One of the players hands throws a ragdollpart
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerHandThrowRagdollPartHandle;
		/// <summary>
		/// Players Left hand throws a ragdollpart
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerLeftHandThrowRagdollPartHandle;
		/// <summary>
		/// Players Right hand throws a ragdollpart
		/// </summary>
		public static event PlayerHandUnGrabEvent OnPlayerRightHandThrowRagdollPartHandle;
		#endregion
		/// <summary>
		/// Triggers PlayerHandUnGrabEvent
		/// </summary>
		/// <param name="side"></param>
		/// <param name="handle"></param>
		/// <param name="throwing"></param>
		/// <param name="eventTime"></param>
		public static void InvokePlayerHandUnGrabEvent( Side side, Handle handle, bool throwing, EventTime eventTime ) {
			log.Debug().Message($"side: {side}, handle: {handle.name}, throwing: {throwing}, isChokeable: {IsHandleChokeable(handle)},  eventTime: {eventTime} ");

			OnPlayerHandUnGrabHandle?.Invoke(side, handle, throwing);

			if ( side == Side.Right ) {
				//right hand grabbed something
				OnPlayerRightHandUnGrabHandle?.Invoke(side, handle, throwing);

				if ( handle is HandleRagdoll ) {
					//right hand grabbed ragdoll part
					OnPlayerRightHandUnGrabRagdollPartHandle?.Invoke(side, handle, throwing);

					if ( throwing ) {
						OnPlayerRightHandThrowRagdollPartHandle?.Invoke(side, handle, throwing);
						OnPlayerRightHandThrowHandle?.Invoke(side, handle, throwing);
					}
				} else {
					//right hand grabbed item
					OnPlayerRightHandUnGrabItemHandle?.Invoke(side, handle, throwing);

					if ( throwing ) {
						OnPlayerRightHandThrowItemHandle?.Invoke(side, handle, throwing);
						OnPlayerRightHandThrowHandle?.Invoke(side, handle, throwing);
					}
				}

			} else {
				OnPlayerLeftHandUnGrabHandle?.Invoke(side, handle, throwing);

				if ( handle is HandleRagdoll ) {
					//Left hand grabbed ragdoll part
					OnPlayerLeftHandUnGrabRagdollPartHandle?.Invoke(side, handle, throwing);
					if ( throwing ) {
						OnPlayerLeftHandThrowRagdollPartHandle?.Invoke(side, handle, throwing);
						OnPlayerLeftHandThrowHandle?.Invoke(side, handle, throwing);
					}
				} else {
					//Left hand grabbed item
					OnPlayerLeftHandUnGrabItemHandle?.Invoke(side, handle, throwing);
					if ( throwing ) {
						OnPlayerLeftHandThrowItemHandle?.Invoke(side, handle, throwing);
						OnPlayerLeftHandThrowHandle?.Invoke(side, handle, throwing);
					}
				}
			}
			if ( throwing ) {
				OnPlayerHandThrowHandle?.Invoke(side, handle, throwing);
			}
			if ( handle is HandleRagdoll handleRagdoll ) {
				OnPlayerHandUnGrabRagdollPartHandle?.Invoke(side, handle, throwing);

				if ( IsHandleChokeable(handleRagdoll) ) {
					//the handle is chokable and has just been ungrabbed so invoke choke event with end time
					OnPlayerHandChokeCreature?.Invoke(side, handle, handleRagdoll.ragdollPart.ragdoll.creature, EventTime.OnEnd);
					OnPlayerChokeCreature?.Invoke(side, handle, handleRagdoll.ragdollPart.ragdoll.creature, EventTime.OnEnd);
				}
				if ( throwing ) {
					OnPlayerHandThrowRagdollPartHandle?.Invoke(side, handle, throwing);
				}
			} else {
				OnPlayerHandUnGrabItemHandle?.Invoke(side, handle, throwing);
				if ( throwing ) {
					OnPlayerHandThrowItemHandle?.Invoke(side, handle, throwing);
				}
			}

		}
		#endregion

		#region playerTelekinesis grab

		#region delegates
		/// <summary>
		/// Event is called when a player Telekinesis grabs something
		/// </summary>
		/// <param name="side"></param>
		/// <param name="handle"></param>
		public delegate void PlayerTelekinesisGrabEvent( Side side, Handle handle );
		/// <summary>
		/// One of the players Telekinesiss grabs a Handle
		/// </summary>
		public static event PlayerTelekinesisGrabEvent OnPlayerTelekinesisGrabHandle;
		/// <summary>
		/// Players Left Telekinesis grabs a Handle
		/// </summary>
		public static event PlayerTelekinesisGrabEvent OnPlayerLeftTelekinesisGrabHandle;
		/// <summary>
		/// Players Right Telekinesis grabs a Handle
		/// </summary>
		public static event PlayerTelekinesisGrabEvent OnPlayerRightTelekinesisGrabHandle;
		/// <summary>
		/// One of the players Telekinesiss grabs a item
		/// </summary>
		public static event PlayerTelekinesisGrabEvent OnPlayerTelekinesisGrabItemHandle;
		/// <summary>
		/// Players Left Telekinesis grabs a item
		/// </summary>
		public static event PlayerTelekinesisGrabEvent OnPlayerLeftTelekinesisGrabItemHandle;
		/// <summary>
		/// Players Right Telekinesis grabs a item
		/// </summary>
		public static event PlayerTelekinesisGrabEvent OnPlayerRightTelekinesisGrabItemHandle;
		/// <summary>
		/// One of the players Telekinesiss grabs a ragdollpart
		/// </summary>
		public static event PlayerTelekinesisGrabEvent OnPlayerTelekinesisGrabRagdollPartHandle;
		/// <summary>
		/// Players Left Telekinesis grabs a ragdollpart
		/// </summary>
		public static event PlayerTelekinesisGrabEvent OnPlayerLeftTelekinesisGrabRagdollPartHandle;
		/// <summary>
		/// Players Right Telekinesis grabs a ragdollpart
		/// </summary>
		public static event PlayerTelekinesisGrabEvent OnPlayerRightTelekinesisGrabRagdollPartHandle;
		#endregion

		/// <summary>
		/// Triggers PlayerTelekinesisGrabEvent
		/// </summary>
		/// <param name="side"></param>
		/// <param name="handle"></param>
		public static void InvokePlayerTelekinesisGrabEvent( Side side, Handle handle ) {
			try {
				log.Debug().Message($"side: {side}, handle: {handle.name}, isChokeable: {IsHandleChokeable(handle)}");
				OnPlayerTelekinesisGrabHandle?.Invoke(side, handle);

				if ( side == Side.Right ) {
					//right Telekinesis grabbed something
					OnPlayerRightTelekinesisGrabHandle?.Invoke(side, handle);

					if ( handle is HandleRagdoll ) {
						//right Telekinesis grabbed ragdoll part
						OnPlayerRightTelekinesisGrabRagdollPartHandle?.Invoke(side, handle);
					} else {
						//right Telekinesis grabbed item
						OnPlayerRightTelekinesisGrabItemHandle?.Invoke(side, handle);
					}

				} else {
					OnPlayerLeftTelekinesisGrabHandle?.Invoke(side, handle);

					if ( handle is HandleRagdoll ) {
						//Left Telekinesis grabbed ragdoll part
						OnPlayerLeftTelekinesisGrabRagdollPartHandle?.Invoke(side, handle);
					} else {
						//Left Telekinesis grabbed item
						OnPlayerLeftTelekinesisGrabItemHandle?.Invoke(side, handle);
					}
				}

				if ( handle is HandleRagdoll handleRagdoll ) {
					OnPlayerTelekinesisGrabRagdollPartHandle?.Invoke(side, handle);
					if ( IsHandleChoked(handle) ) {
						OnPlayerTelekinesisChokeCreature?.Invoke(side, handle,
							handleRagdoll.ragdollPart.ragdoll.creature, EventTime.OnStart);
						OnPlayerChokeCreature?.Invoke(side, handle, handleRagdoll.ragdollPart.ragdoll.creature,
							EventTime.OnStart);
					}
				} else {
					OnPlayerTelekinesisGrabItemHandle?.Invoke(side, handle);
				}
			} catch ( Exception e ) {
				log.Exception().Message(e.StackTrace);
			}
		}
		#endregion

		#region Player Telekinesis ungrabbing a Handle

		#region delegates
		/// <summary>
		/// Event is called when a player Telekinesis ungrabs something
		/// </summary>
		/// <param name="side"></param>
		/// <param name="handle"></param>
		public delegate void PlayerTelekinesisUnGrabEvent( Side side, Handle handle );

		/// <summary>
		/// One of the players Telekinesiss ungrabs a Handle
		/// </summary>
		public static event PlayerTelekinesisUnGrabEvent OnPlayerTelekinesisUnGrabHandle;
		/// <summary>
		/// Players Left Telekinesis ungrabs a Handle
		/// </summary>
		public static event PlayerTelekinesisUnGrabEvent OnPlayerLeftTelekinesisUnGrabHandle;
		/// <summary>
		/// Players Right Telekinesis ungrabs a Handle
		/// </summary>
		public static event PlayerTelekinesisUnGrabEvent OnPlayerRightTelekinesisUnGrabHandle;
		/// <summary>
		/// One of the players Telekinesiss ungrabs a item
		/// </summary>
		public static event PlayerTelekinesisUnGrabEvent OnPlayerTelekinesisUnGrabItemHandle;
		/// <summary>
		/// Players Left Telekinesis ungrabs a item
		/// </summary>
		public static event PlayerTelekinesisUnGrabEvent OnPlayerLeftTelekinesisUnGrabItemHandle;
		/// <summary>
		/// Players Right Telekinesis ungrabs a item
		/// </summary>
		public static event PlayerTelekinesisUnGrabEvent OnPlayerRightTelekinesisUnGrabItemHandle;
		/// <summary>
		/// One of the players Telekinesiss ungrabs a ragdollpart
		/// </summary>
		public static event PlayerTelekinesisUnGrabEvent OnPlayerTelekinesisUnGrabRagdollPartHandle;
		/// <summary>
		/// Players Left Telekinesis ungrabs a ragdollpart
		/// </summary>
		public static event PlayerTelekinesisUnGrabEvent OnPlayerLeftTelekinesisUnGrabRagdollPartHandle;
		/// <summary>
		/// Players Right Telekinesis ungrabs a ragdollpart
		/// </summary>
		public static event PlayerTelekinesisUnGrabEvent OnPlayerRightTelekinesisUnGrabRagdollPartHandle;
		#endregion

		/// <summary>
		/// Triggers PlayerTelekinesisUngrabEvent
		/// </summary>
		/// <param name="side"></param>
		/// <param name="handle"></param>
		public static void InvokePlayerTelekinesisUnGrabEvent( Side side, Handle handle ) {
			log.Debug().Message($"side: {side}, handle: {handle.name}, isChokeable: {IsHandleChokeable(handle)}");

			OnPlayerTelekinesisUnGrabHandle?.Invoke(side, handle);

			if ( side == Side.Right ) {
				//right Telekinesis grabbed something
				OnPlayerRightTelekinesisUnGrabHandle?.Invoke(side, handle);

				if ( handle is HandleRagdoll ) {
					//right Telekinesis grabbed ragdoll part
					OnPlayerRightTelekinesisUnGrabRagdollPartHandle?.Invoke(side, handle);
				} else {
					//right Telekinesis grabbed item
					OnPlayerRightTelekinesisUnGrabItemHandle?.Invoke(side, handle);
				}

			} else {
				OnPlayerLeftTelekinesisUnGrabHandle?.Invoke(side, handle);

				if ( handle is HandleRagdoll ) {
					//Left Telekinesis grabbed ragdoll part
					OnPlayerLeftTelekinesisUnGrabRagdollPartHandle?.Invoke(side, handle);
				} else {
					//Left Telekinesis grabbed item
					OnPlayerLeftTelekinesisUnGrabItemHandle?.Invoke(side, handle);
				}
			}

			if ( handle is HandleRagdoll handleRagdoll ) {
				OnPlayerTelekinesisUnGrabRagdollPartHandle?.Invoke(side, handle);
				if ( IsHandleChokeable(handleRagdoll) ) {
					//the handle is chokable and has just been ungrabbed so invoke choke event with end time
					OnPlayerTelekinesisChokeCreature?.Invoke(side, handle, handleRagdoll.ragdollPart.ragdoll.creature, EventTime.OnEnd);
					OnPlayerChokeCreature?.Invoke(side, handle, handleRagdoll.ragdollPart.ragdoll.creature, EventTime.OnEnd);
				}
			} else {
				OnPlayerTelekinesisUnGrabItemHandle?.Invoke(side, handle);
			}

		}
		#endregion

		#region Parry
		/// <summary>
		/// Player parried a creatures attack
		/// </summary>
		/// <remarks>
		/// The creature is the AI creature and the collisionInstance is done by the AI creature
		/// </remarks>
		public delegate void ParryEvent( Creature creature, CollisionInstance collisionInstance );

		/// <summary>
		/// Player parried a creatures attack
		/// </summary>
		public static event ParryEvent OnPlayerParryingCreature;
		/// <summary>
		/// Creature parried the Players attack
		/// </summary>
		public static event ParryEvent OnCreatureParryingPlayer;

		/// <summary>
		/// Invokes PlayerParryingCreature Event
		/// </summary>
		/// <param name="creature">The creature the player parried</param>
		/// <param name="collisionInstance"></param>
		public static void InvokePlayerParryingCreature( Creature creature, CollisionInstance collisionInstance ) {
			log.Debug().Message($"creature: {creature.name}, collisionInstance: {collisionInstance.ToStringExt()}");
			OnPlayerParryingCreature?.Invoke(creature, collisionInstance);
		}

		/// <summary>
		/// Invokes CreatureParryingPlayer Event
		/// </summary>
		/// <param name="creature">The creature that parried the player</param>
		/// <param name="collisionInstance"></param>
		public static void InvokeCreatureParryingPlayer( Creature creature, CollisionInstance collisionInstance ) {
			log.Debug().Message($"creature: {creature.name}, collisionInstance: {collisionInstance.ToStringExt()}");
			OnCreatureParryingPlayer?.Invoke(creature, collisionInstance);

			//TODO this is only for items, check for fists too
		}
		#endregion

		#region player locomotion
		/// <summary>
		/// Player has just touched the ground
		/// </summary>
		public static event PlayerGroundEvent OnPlayerOnGround;
		/// <summary>
		/// Player has just left the ground
		/// </summary>
		public static event PlayerGroundEvent OnPlayerOffGround;
		/// <summary>
		/// Player touching or leaving the ground event
		/// </summary>
		public delegate void PlayerGroundEvent( bool grounded, Vector3 velocity );
		/// <summary>
		/// Invokes PlayerGroundEvent
		/// </summary>
		/// <param name="grounded"></param>
		/// <param name="velocity"></param>
		public static void InvokePlayerGroundEvent( bool grounded, Vector3 velocity ) {
			if ( grounded ) {
				OnPlayerOnGround?.Invoke(grounded, velocity);
			} else {
				OnPlayerOffGround?.Invoke(grounded, velocity);
			}
		}
		#endregion

		#region Deflects
		/// <summary>
		/// A creature deflects a spell event
		/// </summary>
		/// <param name="source"></param>
		/// <param name="item"></param>
		/// <param name="target"></param>
		public delegate void DeflectEvent( Creature source, Item item, Creature target );

		/// <summary>
		/// Player deflected a spell or projectile from a Creature
		/// </summary>
		public static event DeflectEvent OnPlayerDeflectedCreature;
		/// <summary>
		/// Creature deflected a spell or projectile from the Player
		/// </summary>
		public static event DeflectEvent OnCreatureDeflectPlayer;
		/// <summary>
		/// Triggers a deflect event
		/// </summary>
		/// <param name="source"></param>
		/// <param name="item"></param>
		/// <param name="target"></param>
		public static void InvokeDeflectEvent( Creature source, Item item, Creature target ) {
			log.Debug().Message($"sourceCreature: {source.name}, item: {item.name}, targetCreature: {target.name}");
			if ( source.player && target ) {
				OnPlayerDeflectedCreature?.Invoke(source, item, target);
			}

			if ( target.player && source ) {
				OnCreatureDeflectPlayer?.Invoke(source, item, target);
			}
		}

		#endregion

		#region dismembers

		public static event DismemberEvent OnPlayerDismemberCreatureHead;
		public static event DismemberEvent OnPlayerDismemberCreatureTorso;
		public static event DismemberEvent OnPlayerDismemberCreatureLeftArm;
		public static event DismemberEvent OnPlayerDismemberCreatureRightArm;
		public static event DismemberEvent OnPlayerDismemberCreatureLeftHand;
		public static event DismemberEvent OnPlayerDismemberCreatureRightHand;
		public static event DismemberEvent OnPlayerDismemberCreatureLeftLeg;
		public static event DismemberEvent OnPlayerDismemberCreatureRightLeg;
		public static event DismemberEvent OnPlayerDismemberCreatureLeftFoot;
		public static event DismemberEvent OnPlayerDismemberCreatureRightFoot;
		public static event DismemberEvent OnPlayerDismemberCreature;
		/// <summary>
		/// Event for whenever a part of a creature is dismembered
		/// </summary>
		/// <param name="ragdollPart"></param>
		/// <param name="creatureIsKilled"></param>
		/// <param name="ragdollPartType"></param>
		/// <param name="usingTelekinesis"></param>
		public delegate void DismemberEvent( RagdollPart ragdollPart, bool creatureIsKilled, RagdollPart.Type ragdollPartType, bool usingTelekinesis );
		/// <summary>
		/// Triggers an event for whenever a part of a creature is dismembered
		/// </summary>
		/// <param name="ragdollPart"></param>
		/// <param name="creatureIsKilled"></param>
		/// <param name="ragdollPartType"></param>
		public static void InvokeDismemberEvent( RagdollPart ragdollPart, bool creatureIsKilled, RagdollPart.Type ragdollPartType ) {
			log.Debug().Message($"ragdollPart: {ragdollPart.name}, creatureIsKilled: {creatureIsKilled}, ragdollPartType: {ragdollPartType}");

			Ragdoll ragdoll = ragdollPart.ragdoll;
			bool tkUsed = IsPlayerTkHolding(ragdollPart);
			//player was holding with TK when dismembered

			CollisionInstance ch = ragdoll.creature.lastDamage;
			if ( ch.IsDoneByPlayer() ) {
				OnPlayerDismemberCreature?.Invoke(ragdollPart, creatureIsKilled, ragdollPartType, tkUsed);

				if ( ragdollPart.type == RagdollPart.Type.Head || ragdollPart.type == RagdollPart.Type.Neck ) {
					OnPlayerDismemberCreatureHead?.Invoke(ragdollPart, creatureIsKilled, ragdollPartType, tkUsed);
				}
				if ( ragdollPart.type == RagdollPart.Type.Torso ) {
					OnPlayerDismemberCreatureTorso?.Invoke(ragdollPart, creatureIsKilled, ragdollPartType, tkUsed);
				}
				if ( ragdollPart.type == RagdollPart.Type.LeftArm ) {
					OnPlayerDismemberCreatureLeftArm?.Invoke(ragdollPart, creatureIsKilled, ragdollPartType, tkUsed);
				}
				if ( ragdollPart.type == RagdollPart.Type.RightArm ) {
					OnPlayerDismemberCreatureRightArm?.Invoke(ragdollPart, creatureIsKilled, ragdollPartType, tkUsed);
				}
				if ( ragdollPart.type == RagdollPart.Type.LeftFoot ) {
					OnPlayerDismemberCreatureLeftFoot?.Invoke(ragdollPart, creatureIsKilled, ragdollPartType, tkUsed);
				}
				if ( ragdollPart.type == RagdollPart.Type.RightFoot ) {
					OnPlayerDismemberCreatureRightFoot?.Invoke(ragdollPart, creatureIsKilled, ragdollPartType, tkUsed);
				}
				if ( ragdollPart.type == RagdollPart.Type.LeftHand ) {
					OnPlayerDismemberCreatureLeftHand?.Invoke(ragdollPart, creatureIsKilled, ragdollPartType, tkUsed);
				}
				if ( ragdollPart.type == RagdollPart.Type.RightHand ) {
					OnPlayerDismemberCreatureRightHand?.Invoke(ragdollPart, creatureIsKilled, ragdollPartType, tkUsed);
				}
				if ( ragdollPart.type == RagdollPart.Type.LeftLeg ) {
					OnPlayerDismemberCreatureLeftLeg?.Invoke(ragdollPart, creatureIsKilled, ragdollPartType, tkUsed);
				}
				if ( ragdollPart.type == RagdollPart.Type.RightLeg ) {
					OnPlayerDismemberCreatureRightLeg?.Invoke(ragdollPart, creatureIsKilled, ragdollPartType, tkUsed);
				}
			}

		}

		#endregion

		#region creature hit
		/// <summary>
		/// A creature was hit, including player
		/// </summary>
		public static event CreatureHitEvent OnCreatureHit;
		/// <summary>
		/// The player was hit
		/// </summary>
		public static event CreatureHitEvent OnPlayerHit;
		/// <summary>
		/// The player was hit by a creature
		/// </summary>
		public static event CreatureHitEvent OnPlayerHitByCreature;
		/// <summary>
		/// The player hit themselves
		/// </summary>
		public static event CreatureHitEvent OnPlayerHitSelf;
		/// <summary>
		/// A creature was hit by the player
		/// </summary>
		public static event CreatureHitEvent OnCreatureHitByPlayer;
		/// <summary>
		/// A creature was hit by another creature, possibly self
		/// </summary>
		public static event CreatureHitEvent OnCreatureHitByCreature;

		public delegate void CreatureHitEvent( BetterHit betterHit );
		/// <summary>
		/// Invokes CreatureHitEvent
		/// </summary>
		/// <param name="betterHit"></param>
		public static void InvokeCreatureHitEvent( BetterHit betterHit ) {
			log.Debug().Message($"betterHit: {betterHit}");
			if ( betterHit.creature?.player && !betterHit.collisionInstance.IsDoneByPlayer() ) {

				//player was hit by creature
				OnPlayerHit?.Invoke(betterHit);
				OnPlayerHitByCreature?.Invoke(betterHit);
				log.Debug().Message("Player was hit by creature");
			}
			if ( !betterHit.creature?.player && betterHit.collisionInstance.IsDoneByPlayer() ) {
				//player hit a creature
				OnCreatureHitByPlayer?.Invoke(betterHit);
				log.Debug().Message("Player hit a creature");

			}
			if ( !betterHit.creature?.player && !betterHit.collisionInstance.IsDoneByPlayer() ) {
				//creature hit a creature
				log.Debug().Message("Creature hit a creature");
				OnCreatureHitByCreature?.Invoke(betterHit);
			}
			if ( betterHit.creature?.player && betterHit.collisionInstance.IsDoneByPlayer() ) {
				//player hit themself
				log.Debug().Message("Player hit themself");
				OnPlayerHitSelf?.Invoke(betterHit);
			}
		}

		#endregion


		#region creature killed
		/// <summary>
		/// A creature was killed, including player
		/// </summary>
		public static event CreatureKillEvent OnCreatureKill;
		/// <summary>
		/// The player was killed
		/// </summary>
		public static event CreatureKillEvent OnPlayerKill;
		/// <summary>
		/// The player was killed by a creature
		/// </summary>
		public static event CreatureKillEvent OnPlayerKillByCreature;
		/// <summary>
		/// The player killed themselves
		/// </summary>
		public static event CreatureKillEvent OnPlayerKillSelf;
		/// <summary>
		/// A creature was killed by the player
		/// </summary>
		public static event CreatureKillEvent OnCreatureKillByPlayer;
		/// <summary>
		/// A creature was killed by another creature, possibly self
		/// </summary>
		public static event CreatureKillEvent OnCreatureKillByCreature;

		public delegate void CreatureKillEvent( BetterHit betterHit );
		/// <summary>
		/// Invokes CreatureKillEvent
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="player"></param>
		/// <param name="collisionInstance"></param>
		/// <param name="damageType"></param>
		/// <param name="penetrationType"></param>
		/// <param name="attackDirection"></param>
		/// <param name="creatureStates"></param>
		/// <param name="hitStates"></param>
		/// <param name="damageArea"></param>
		public static void InvokeCreatureKillEvent( BetterHit betterHit ) {

			log.Debug().Message($"betterHit: {betterHit}");
			if ( betterHit.player && !betterHit.collisionInstance.IsDoneByPlayer() ) {

				//player was killed by creature
				OnPlayerKill?.Invoke(betterHit);
				OnPlayerKillByCreature?.Invoke(betterHit);
				log.Debug().Message("Player was killed by creature");
			}
			if ( !betterHit.player && betterHit.collisionInstance.IsDoneByPlayer() ) {
				//player killed a creature
				OnCreatureKillByPlayer?.Invoke(betterHit);
				log.Debug().Message("Player killed a creature");

			}
			if ( !betterHit.player && !betterHit.collisionInstance.IsDoneByPlayer() ) {
				//creature killed a creature
				log.Debug().Message("Creature killed a creature");
				OnCreatureKillByCreature?.Invoke(betterHit);
			}
			if ( betterHit.creature.player && betterHit.collisionInstance.IsDoneByPlayer() ) {
				//player killed themself
				log.Debug().Message("Player killed themself");
				OnPlayerKillSelf?.Invoke(betterHit);
			}
		}

		#endregion

		#region Collision events

		public static event CollisionEvent OnPlayerHeldItemCollideItem;
		public static event CollisionEvent OnPlayerHeldItemCollideHeldItem;
		public static event CollisionEvent OnPlayerHeldItemCollideUnheldItem;
		public static event CollisionEvent OnCreatureHeldItemCollideItem;
		public static event CollisionEvent OnCreatureHeldItemCollideHeldItem;
		public static event CollisionEvent OnCreatureHeldItemCollideUnheldItem;
		public static event CollisionEvent OnPlayerHeldItemCollideCreatureHeldItem;
		public static event CollisionEvent OnCreatureHeldItemCollidePlayerHeldItem;
		public static event CollisionEvent OnPlayerHeldItemCollideRagdollPart;
		public static event CollisionEvent OnPlayerHeldItemCollideCreatureRagdollPart;
		public static event CollisionEvent OnPlayerHeldItemCollidePlayerRagdollPart;
		public static event CollisionEvent OnPlayerHeldItemCollideDeadCreatureRagdollPart;
		public static event CollisionEvent OnPlayerHeldItemCollideAliveCreatureRagdollPart;
		public static event CollisionEvent OnPlayerHeldItemCollideWorld;
		public static event CollisionEvent OnPlayerRagdollPartCollideWorld;

		public delegate void CollisionEvent( CollisionInstance collisionInstance );

		public static void InvokeCollisionEvent( LevelModuleBetterEvents levelModule, CollisionInstance collisionInstance ) {
			Item targetItem = collisionInstance.GetItemFromTarget();
			Item sourceItem = collisionInstance.GetItemFromSource();
			RagdollPart targetPart = collisionInstance.GetRagdollPartFromTarget();
			RagdollPart sourcePart = collisionInstance.GetRagdollPartFromSource();

			//Player held item hit the creatures held item
			if ( sourceItem && targetItem && IsOnlyPlayerHolding(sourceItem) && IsOnlyCreatureExceptPlayerHolding(targetItem) ) {
				log.Debug().Message("Players {0} hit a creatures {1}", sourceItem.data.id, targetItem.data.id);
				InvokeCreatureParryingPlayer(targetItem.handlers[0].creature, collisionInstance);

				OnPlayerHeldItemCollideCreatureHeldItem?.Invoke(collisionInstance);
				OnPlayerHeldItemCollideItem?.Invoke(collisionInstance);
				OnPlayerHeldItemCollideHeldItem?.Invoke(collisionInstance);

			}

			//Creatures held item hit the players held item
			if ( sourceItem && targetItem && IsOnlyCreatureExceptPlayerHolding(sourceItem) && IsOnlyPlayerHolding(targetItem) ) {
				log.Debug().Message("Creatures {0} hit players {1}", sourceItem.data.id, targetItem.data.id);
				OnCreatureHeldItemCollidePlayerHeldItem?.Invoke(collisionInstance);
				OnCreatureHeldItemCollideItem?.Invoke(collisionInstance);
				OnCreatureHeldItemCollideHeldItem?.Invoke(collisionInstance);
			}

			//Players held item hit a creature ragdollpart
			if ( sourceItem && targetPart && IsOnlyPlayerHolding(sourceItem) && !IsPlayer(targetPart) ) {

				OnPlayerHeldItemCollideRagdollPart?.Invoke(collisionInstance);
				OnPlayerHeldItemCollideCreatureRagdollPart?.Invoke(collisionInstance);
				if ( targetPart.ragdoll.creature.isKilled ) {
					log.Debug().Message("Players {0} hit a dead creatures {1}", sourceItem.data.id, targetPart.name);
					levelModule.SubscribeToRagdollSlice(targetPart);
					OnPlayerHeldItemCollideDeadCreatureRagdollPart?.Invoke(collisionInstance);
				} else {
					log.Debug().Message("Players {0} hit a creatures {1}", sourceItem.data.id, targetPart.name);
					OnPlayerHeldItemCollideAliveCreatureRagdollPart?.Invoke(collisionInstance);
				}
			}

			//Players held item hit a player ragdollpart
			if ( sourceItem && targetPart && IsOnlyPlayerHolding(sourceItem) && IsPlayer(targetPart) ) {
				OnPlayerHeldItemCollideRagdollPart?.Invoke(collisionInstance);
				OnPlayerHeldItemCollidePlayerRagdollPart?.Invoke(collisionInstance);
				log.Debug().Message("Players {0} hit players {1}",
					sourceItem.data.id, targetPart.name);
			}

			//Players held item hit the ground/world
			if ( sourceItem && IsOnlyPlayerHolding(sourceItem) && !collisionInstance.targetColliderGroup ) {
				OnPlayerHeldItemCollideWorld?.Invoke(collisionInstance);
				log.Debug().Message("Players {0} hit ground",
					sourceItem.data.id);
			}

			//Players ragdollpart hit the ground/world
			if ( sourcePart && IsPlayer(sourcePart) && !collisionInstance.targetColliderGroup ) {
				OnPlayerRagdollPartCollideWorld?.Invoke(collisionInstance);
				log.Debug().Message("Players {0} hit ground",
					sourcePart.name);
			}

			//An unheld item(flying/dropped/falling) that wasnt held last by player hit the players held item
			if ( sourceItem && targetItem && IsNotHeld(sourceItem) && !IsLastHeldByPlayer(sourceItem) && IsOnlyPlayerHolding(targetItem) ) {
				OnPlayerHeldItemCollideUnheldItem?.Invoke(collisionInstance);
				log.Debug().Message("Unheld item {0} hit players {1}",
					sourceItem.data.id, targetItem.data.id);
			}
			//An unheld item(flying/dropped/falling) that wasnt held last by creature hit the creature held item
			if ( sourceItem && targetItem && IsNotHeld(sourceItem) && !IsLastHeldByCreatureExceptPlayer(sourceItem) && IsOnlyCreatureExceptPlayerHolding(targetItem) ) {
				OnCreatureHeldItemCollideUnheldItem?.Invoke(collisionInstance);
				log.Debug().Message("Unheld item {0} hit creatures {1}",
					sourceItem.data.id, targetItem.data.id);
			}
		}


		#endregion


		#region Player casting
		/// <summary>
		/// Called when player starts and stops casting a spell. eventType onStart/onStop is for casting start/stop
		/// </summary>
		/// <remarks>Use spellcaster.spellInstance.id to find out what spell it is</remarks>
		public static event SpellEvent OnPlayerCast;
		/// <summary>
		///  Called when player starts and stops spraying a spell. eventType onStart/onStop is for casting start/stop
		/// </summary>
		/// <remarks>Use spellcaster.spellInstance.id to find out what spell it is</remarks>
		public static event SpellEvent OnPlayerSpray;
		/// <summary>
		/// Called when player starts and stops merging a spell. eventType onStart/onStop is for casting start/stop
		/// </summary>
		/// <remarks>Use spellcaster.spellInstance.id to find out what spell it is</remarks>
		public static event SpellEvent OnPlayerMerge;
		public delegate void SpellEvent( SpellCaster spellCaster, Side side, EventTime eventTime );

		public static void InvokeOnPlayerCastSpellEvent( SpellCaster spellCaster, Side side, EventTime eventTime) {
			OnPlayerCast?.Invoke(spellCaster,side,eventTime);
		}
		public static void InvokeOnPlayerSpraySpellEvent( SpellCaster spellCaster, Side side, EventTime eventTime ) {
			OnPlayerSpray?.Invoke(spellCaster, side, eventTime);
		}
		public static void InvokeOnPlayerMergeSpellEvent( SpellCaster spellCaster, Side side, EventTime eventTime ) {
			OnPlayerMerge?.Invoke(spellCaster, side, eventTime);
		}
		#endregion

		/// <summary>
		/// The area where a collision hit
		/// </summary>
		public enum DamageArea {
			None,
			Head,
			Neck,
			Torso,
			LeftArm,
			RightArm,
			LeftHand,
			RightHand,
			LeftLeg,
			RightLeg,
			LeftFoot,
			RightFoot
		}
		/// <summary>
		/// The direction a collision hit from
		/// </summary>
		public enum Direction {
			Front,
			Back,
			Left,
			Right,
			Above,
			Under,
			None
		}
		/// <summary>
		/// The multiple states a hit could be
		/// </summary>
		public enum HitState {
			PlayerRagdollPart,
			CreatureRagdollPart,
			DismemberedRagdollPart,
			GrabbedRagdollPart,
			TelekinesisGrabbedRagdollPart,
			PunchOrKick,
			ThrownItem,
			GrabbedItem,
			TelekinesisGrabbedItem,
			ImbuedItem

		}
		/// <summary>
		/// The multiple states a creature could be in
		/// </summary>
		public enum CreatureState {
			Dismembered,
			Injured,
			Fullhealth,
			Dead,
			Alive,
			Falling,
			LayingOnGround,
			Stabilized,
			Destabilized,
			Grounded,
			NotGrounded,
			IsArmed,
			IsUnarmed,
			IsGrabbed,
			IsGrabbedWithTelekinesis,
			IsBeingElectrocuted,
			IsBeingChoked,
			IsStaggered,
			IsDying,
			IsAttacking,
			IsDrawingWeapon,
			IsAttackingWithMelee,
			IsAttackingWithBow,
			IsAttackingWithCast,
			IsTargetingPlayer,
			IsTargetingNPC,
			IsTargetingWeapon
		}
	}

}
