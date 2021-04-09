using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;
using Wully.Events;
using Wully.Helpers;
using static Wully.Extensions.BetterExtensions;
using static Wully.Helpers.BetterHelpers;
using static Wully.Helpers.BetterLogger;

namespace Wully.Module {
	/// <summary>
	/// LevelModule which monitors, tracks and invokes events with rich data
	/// </summary>
	public class LevelModuleBetterEvents : LevelModule {
		private static BetterLogger log = BetterLogger.GetLogger(typeof(LevelModuleBetterEvents));

		// Configurables
		/// <summary>
		/// Enable/Disable Better Logging for BetterEvents
		/// </summary>
		public bool enableLogging = true;
		/// <summary>
		/// Set the LogLevel for BetterEvents
		/// </summary>
		public Loglevel loglevel = Loglevel.Warn;

		/// <summary>
		/// Local static reference to the currently loaded BetterEvents level module
		/// </summary>
		public static LevelModuleBetterEvents local;

		/// <summary>
		/// Current level the game is on
		/// </summary>
		public Level currentLevel;
		/// <summary>
		/// Currently loaded Wave levelmodule
		/// </summary>
		/// <remarks>nullable</remarks>
		protected LevelModuleWave levelModuleWave;

		private HashSet<CollisionHandler> grabbedCollisionHandlers;
		private Dictionary<Ragdoll, HashSet<RagdollPart>> ragdollHits;

		/// <summary>
		/// Players left spellcaster
		/// </summary>
		public SpellCaster spellCasterLeft;
		private Handle tkLeftHandle;
		/// <summary>
		/// Returns the players left telekinesis grabbed handle
		/// </summary>
		public Handle SpellCasterLeftGrabbedHandle => tkLeftHandle;
		/// <summary>
		/// Players right spellcaster
		/// </summary>
		public SpellCaster spellCasterRight;
		private Handle tkRightHandle;
		/// <summary>
		/// Returns the players right telekinesis grabbed handle
		/// </summary>
		public Handle SpellCasterRightGrabbedHandle => tkRightHandle;

		private static int ALLYFACTION = 2;

		private void InitVars() {
			this.grabbedCollisionHandlers = new HashSet<CollisionHandler>();
			this.ragdollHits = new Dictionary<Ragdoll, HashSet<RagdollPart>>();
		}

		/// <summary>
		/// Called when a level is loaded
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		public override IEnumerator OnLoadCoroutine( Level level ) {

			log.SetLoggingLevel(loglevel);
			log.DisableLogging();
			if ( enableLogging ) {
				log.EnableLogging();
			}
			log.Debug("BetterEvents Module OnLoadCoroutine on level {0}", level.data.id);

			this.currentLevel = level;
			InitVars();
			//master scene is always loaded and this module gets loaded with it
			if ( level.data.id.ToLower() == "master" ) {
				log.Info("Initialized Wully's BetterMods - BetterEvents Module");

				local = this;
				EventManager.onCreatureHit += this.OnCreatureHit;
				EventManager.onCreatureKill += this.OnCreatureKill;
				EventManager.onCreatureParry += this.OnCreatureParry;
				EventManager.onCreatureHeal += this.OnCreatureHeal;
				EventManager.onDeflect += this.OnDeflectEvent;
				EventManager.onPossess += this.OnPossessEvent;
				EventManager.onUnpossess += this.OnUnPossessEvent;

			}
			// if its not the character selection or "master" level, try to get the level modules
			if ( !(level.data.id.Equals("CharacterSelection") || level.data.id.Equals("Master")) ) {
				this.levelModuleWave = level.modeRank.mode.GetModule<LevelModuleWave>();
				if ( this.levelModuleWave != null ) {
					log.Debug("Subscribing to levelModuleWave events on level: {0}", level.data.id);
					this.levelModuleWave.OnWaveBeginEvent += this.OnWaveBegin;
					this.levelModuleWave.OnWaveLoopEvent += this.OnWaveLoop;
				}

			}

			yield break;
		}
		/// <summary>
		/// Called every frame
		/// </summary>
		/// <param name="level"></param>
		public virtual void Update( Level level ) {
			base.Update(level);

			//since TK doesnt have events for grabbing, we need to monitor it and subscribe to the item the player TKs
			if ( spellCasterLeft ) {
				MonitorTk(spellCasterLeft, Side.Left, ref tkLeftHandle);
			}
			//left hand check
			if ( spellCasterRight ) {
				MonitorTk(spellCasterRight, Side.Right, ref tkRightHandle);
			}
		}

		public virtual void TkCaughtHandle( Handle handle, Side side ) {
			//add the new handle
			if ( handle is HandleRagdoll handleRagdoll ) {
				//subscribe to ragdoll slice					
				SubscribeToRagdollSlice(handleRagdoll.ragdollPart);
			} else {
				SubscribeToHandleColliders(handle);
			}
			BetterEvents.InvokePlayerTelekinesisGrabEvent(side, handle);
		}
		/// <summary>
		/// Continually checks if the player has grabbed something with Telekinesis and updates a handle reference
		/// </summary>
		/// <param name="spellCaster"></param>
		/// <param name="side"></param>
		/// <param name="handle"></param>
		public virtual void MonitorTk( SpellCaster spellCaster, Side side, ref Handle handle ) {

			if ( spellCaster.TryGetTkHandle(out Handle caught) ) {
				log.Debug("Spellcaster hand {0} grabbed a handle {1}", side, caught.name);

				//if the handle is currently set, check its not the same one
				if ( caught == handle ) { return; }

				if ( handle != null ) {
					log.Debug("Spellcaster hand {0} dropped a handle {1}", side, handle.name);
					//something weird happend but the TK is now holding a different item and didnt drop the old one
					//unsubscribe to old handle and call ungrab event
					BetterEvents.InvokePlayerTelekinesisUnGrabEvent(side, handle);
					UnsubscribeToHandleColliders(handle);
				}
				TkCaughtHandle(handle, side);

				handle = caught;


			} else {
				//player dropped what they were Tking
				if ( handle != null ) {
					log.Debug("Spellcaster hand {0} dropped a handle {1}", side, handle.name);
					BetterEvents.InvokePlayerTelekinesisUnGrabEvent(side, handle);
					// unsubscribe to handles colliders
					UnsubscribeToHandleColliders(handle);
					handle = null;
				}

			}

		}

		public virtual void OnUnPossessEvent( Creature creature, EventTime eventTime ) {
			//do it for onstart
			if(eventTime == EventTime.OnEnd ) { return;}
			if (Player.local?.creature == null) { return; }

			log.Debug("OnUnPossessEvent - UnSubscribed to player hand/foot events");
			//Subscribe to what the player picks up so we can track other things
			Player.local.creature.handLeft.OnGrabEvent -= Hand_OnGrabEvent;
			Player.local.creature.handRight.OnGrabEvent -= Hand_OnGrabEvent;
			Player.local.creature.handLeft.OnUnGrabEvent -= Hand_OnUnGrabEvent;
			Player.local.creature.handRight.OnUnGrabEvent -= Hand_OnUnGrabEvent;
			creature.handRight.colliderGroup.collisionHandler.OnCollisionStartEvent -= PlayerItemFootHand_OnCollisionStartEvent;
			creature.handLeft.colliderGroup.collisionHandler.OnCollisionStartEvent -= PlayerItemFootHand_OnCollisionStartEvent;
			creature.footRight.colliderGroup.collisionHandler.OnCollisionStartEvent -= PlayerItemFootHand_OnCollisionStartEvent;
			creature.footLeft.colliderGroup.collisionHandler.OnCollisionStartEvent -= PlayerItemFootHand_OnCollisionStartEvent;
			spellCasterLeft = null;
			spellCasterRight = null;

		}

		public virtual void OnPossessEvent( Creature creature, EventTime eventTime ) {
			//do it for on end
			if ( eventTime == EventTime.OnStart ) { return; }
			if ( Player.local?.creature == null ) { return; }
			
			log.Debug("OnPossessEvent - Subscribed to player hand/foot events");
			//Subscribe to what the player picks up so we can track other things
			creature.handLeft.OnGrabEvent += Hand_OnGrabEvent;
			creature.handRight.OnGrabEvent += Hand_OnGrabEvent;
			creature.handLeft.OnUnGrabEvent += Hand_OnUnGrabEvent;
			creature.handRight.OnUnGrabEvent += Hand_OnUnGrabEvent;
			//subscribe to player hands collisions
			creature.handRight.colliderGroup.collisionHandler.OnCollisionStartEvent += PlayerItemFootHand_OnCollisionStartEvent;
			creature.handLeft.colliderGroup.collisionHandler.OnCollisionStartEvent += PlayerItemFootHand_OnCollisionStartEvent;
			creature.footRight.colliderGroup.collisionHandler.OnCollisionStartEvent += PlayerItemFootHand_OnCollisionStartEvent;
			creature.footLeft.colliderGroup.collisionHandler.OnCollisionStartEvent += PlayerItemFootHand_OnCollisionStartEvent;
			Player.local.locomotion.OnGroundEvent += Locomotion_OnGroundEvent;
			
			spellCasterLeft = Player.local?.creature?.mana?.casterLeft;
			spellCasterRight = Player.local?.creature?.mana?.casterRight;
				
			
		}



		public virtual void Locomotion_OnGroundEvent( bool grounded, Vector3 velocity ) {
			log.Debug("Locomotion_OnGroundEvent - grounded: {0} , velocity : {1}", grounded, velocity);
			BetterEvents.InvokePlayerGroundEvent(grounded, velocity);
		}

		public virtual void SubscribeToRagdollSlice( RagdollPart ragdollPart ) {
			Ragdoll ragdoll = ragdollPart.ragdoll;
			//ragdoll already exists
			if ( ragdollHits.TryGetValue(ragdoll, out HashSet<RagdollPart> existingParts) ) {
				//does the part already exist
				if ( !existingParts.Contains(ragdollPart) ) {
					existingParts.Add(ragdollPart);
					ragdollPart.ragdoll.OnSliceEvent += Ragdoll_OnSliceEvent;
				}

			} else {
				//new ragdoll, new part
				existingParts = new HashSet<RagdollPart>();
				existingParts.Add(ragdollPart);
				ragdollHits.Add(ragdoll, existingParts);
				ragdollPart.ragdoll.OnSliceEvent += Ragdoll_OnSliceEvent;
			}
		}

		public virtual void Hand_OnUnGrabEvent( Side side, Handle handle, bool throwing, EventTime eventTime ) {
			if ( eventTime == EventTime.OnEnd ) {
				log.Debug("Hand_OnUnGrabEvent side: {0}, handle: {1}, throwing: {2} ", side, handle.name, throwing);
				BetterEvents.InvokePlayerHandUnGrabEvent(side, handle, throwing, eventTime);
				UnsubscribeToHandleColliders(handle);
			}
		}

		public virtual void Hand_OnGrabEvent( Side side, Handle handle, float axisPosition, HandleOrientation orientation, EventTime eventTime ) {

			if ( eventTime == EventTime.OnEnd ) {
				log.Debug("Hand_OnGrabEvent side: {0}, handle: {1} ", side, handle.name);
				BetterEvents.InvokePlayerHandGrabEvent(side, handle, axisPosition, orientation, eventTime);
				SubscribeToHandleColliders(handle);
			}

		}

		public virtual void SubscribeToHandleColliders( Handle handle ) {
			//when the player grabs something, we want to subscribe to that items collision handlers	
			if ( handle?.item == null ) { return; }
			foreach ( CollisionHandler collisionHandler in handle.item.collisionHandlers ) {
				if ( !grabbedCollisionHandlers.Contains(collisionHandler) ) {
					collisionHandler.OnCollisionStartEvent += this.PlayerItemFootHand_OnCollisionStartEvent;
					grabbedCollisionHandlers.Add(collisionHandler);
				}
			}
		}

		public virtual void UnsubscribeToHandleColliders( Handle handle ) {
			if ( handle != null ) {
				if ( handle.IsPlayerHolding() ) {
					return;
				}

				// check the players not still holding it though				
				foreach ( CollisionHandler collisionHandler in handle?.item?.collisionHandlers ) {
					collisionHandler.OnCollisionStartEvent -= this.PlayerItemFootHand_OnCollisionStartEvent;
					grabbedCollisionHandlers.Remove(collisionHandler);
				}
			}
		}

		/// <summary>
		/// This is called when the players grabbed, TKed item or fists has begun touching another rigidbody/collider
		/// </summary>
		/// <param name="collisionInstance"></param>
		public virtual void PlayerItemFootHand_OnCollisionStartEvent( CollisionInstance collisionInstance ) {
			//this is called when this GrabbedItem has begun touching another rigidbody/collider.
			//source is what did the hitting, target is the thing being hit
			log.Debug("PlayerItemFootHand_OnCollisionStartEvent {0}", collisionInstance.ToStringExt());

			Item targetItem = collisionInstance.GetItemFromTarget();
			Item sourceItem = collisionInstance.GetItemFromSource();
			RagdollPart targetPart = collisionInstance.GetRagdollPartFromTarget();
			RagdollPart sourcePart = collisionInstance.GetRagdollPartFromSource();

			//Player held item hit the creatures held item
			if(IsOnlyPlayerHolding(sourceItem) && IsOnlyCreatureExceptPlayerHolding(targetItem) ) { 
				log.Debug("PlayerItemFootHand_OnCollisionStartEvent - Players {0} hit a creatures {1}",
					sourceItem.data.id, targetItem.data.id);
				BetterEvents.InvokeCreatureParryingPlayer(targetItem.handlers[0].creature,
					collisionInstance);
			}
			//Creatures held item hit the players held item
			if (IsOnlyCreatureExceptPlayerHolding(sourceItem) && IsOnlyPlayerHolding(targetItem)) {
				log.Debug("PlayerItemFootHand_OnCollisionStartEvent - Creatures {0} hit players {1}",
					sourceItem.data.id, targetItem.data.id);
			}

			//Players held item hit a creature ragdollpart
			if ( IsOnlyPlayerHolding(sourceItem) && !IsPlayer(targetPart)) {
				
				if (targetPart.ragdoll.creature.isKilled) {
					log.Debug("PlayerItemFootHand_OnCollisionStartEvent - Players {0} hit a dead creatures {1}",
						sourceItem.data.id, targetPart.name);
					SubscribeToRagdollSlice(targetPart);
				} else {
					log.Debug("PlayerItemFootHand_OnCollisionStartEvent - Players {0} hit a creatures {1}",
						sourceItem.data.id, targetPart.name);
				}
			}
			//Players held item hit a player ragdollpart
			if ( IsOnlyPlayerHolding(sourceItem) && IsPlayer(targetPart) ) {
				log.Debug("PlayerItemFootHand_OnCollisionStartEvent - Players {0} hit players {1}",
					sourceItem.data.id, targetPart.name);
			}
			//Players held item hit the ground/world
			if ( IsOnlyPlayerHolding(sourceItem) && !collisionInstance.targetColliderGroup) {
				log.Debug("PlayerItemFootHand_OnCollisionStartEvent - Players {0} hit ground",
					sourceItem.data.id);
			}
			//Players ragdollpart hit the ground/world
			if ( IsPlayer(sourcePart) && !collisionInstance.targetColliderGroup ) {
				log.Debug("PlayerItemFootHand_OnCollisionStartEvent - Players {0} hit ground",
					sourceItem.data.id);
			}
			//An unheld item(flying/dropped/falling) that wasnt held last by player hit the players held item
			if ( IsNotHeld(sourceItem) && !IsLastHeldByPlayer(sourceItem) && IsOnlyPlayerHolding(targetItem)  ) {
				log.Debug("PlayerItemFootHand_OnCollisionStartEvent - Unheld item {0} hit players {1}",
					sourceItem.data.id, targetItem.data.id);
			}

		}



		public virtual void OnCreatureHeal( Creature creature, float heal, Creature healer ) {
			log.Debug("OnCreatureHeal - Creature {0} healed {1} by {2}", healer.name, creature.name, heal);
		}

		public virtual void OnUnload( Level level ) {

			if ( this.levelModuleWave != null ) {
				this.levelModuleWave.OnWaveBeginEvent -= this.OnWaveBegin;
				this.levelModuleWave.OnWaveLoopEvent -= this.OnWaveLoop;
			}

		}


		public virtual void OnWaveBegin() {
			//UnityEngine.Debug.Log("OnWaveBegin");
			InitVars();
		}
		public virtual void OnWaveLoop() {
			//UnityEngine.Debug.Log("OnWaveLoop");

		}


		public virtual void Ragdoll_OnSliceEvent( RagdollPart ragdollPart, EventTime eventTime ) {
			//use onstart check if creature alive or ded wen starting to know if player is messing with dead bodies
			if ( eventTime == EventTime.OnEnd ) { return; }

			//ragdoll is being sliced.
			Ragdoll ragdoll = ragdollPart.ragdoll;
			//Last thing th at hit damaged it - most likely causing the slice
			CollisionInstance ch = ragdoll.creature.lastDamage;
			
			
			//remove it from the dictionary if we did hit it before
			if ( ragdollHits.ContainsKey(ragdoll) ) {
				if ( ragdollHits[ragdoll].Contains(ragdollPart) ) {

					log.Debug("Ragdoll_OnSliceEvent - ragdollPart {0} was being tracked and was sliced", ragdollPart.type.ToString());

					BetterEvents.InvokeDismemberEvent(ragdollPart, ragdoll.creature.isKilled, ragdollPart.type);

					//remove ragdoll part from list
					ragdollHits[ragdoll].Remove(ragdollPart);
					if ( ragdollHits[ragdoll].Count == 0 ) {
						//unsubscribed
						ragdoll.OnSliceEvent -= Ragdoll_OnSliceEvent;
						//remove the ragdol from the dict too
						ragdollHits.Remove(ragdoll);
					}

				}
			} else {
				log.Warn("Ragdoll_OnSliceEvent - ragdollPart {0} was not being tracked.. and was sliced", ragdollPart.type.ToString());
			}
			

		}
		public virtual void OnDeflectEvent( Creature source, Item item, Creature target ) {
			
			BetterEvents.InvokeDeflectEvent(source, item, target);
			if ( source.player && target ) {
				log.Debug("OnDeflectEvent - Player deflected creatures {0}", item.data.id);
			}

			if ( target.player && source ) {
				log.Debug("OnDeflectEvent - Creature deflected players {0}", item.data.id);
			}

		}

		public virtual void OnCreatureParry( Creature creature, CollisionInstance collisionInstance ) {
			
			if ( !creature ) { return; }
			if (creature.isPlayer ) { return;}
			if ( collisionInstance.IsDoneByPlayer() ) { return; }

			log.Debug("OnCreatureParry - Player parried creature attack");
			BetterEvents.InvokePlayerParryingCreature(creature, collisionInstance);

		}

		public virtual void OnCreatureKill( Creature creature, Player player, CollisionInstance collisionInstance, EventTime eventTime ) {

			//return if the kill was not done by the player, or if the player was the one killed
			//Event time is the start/end of the kill class on the creature, we want the start event so we know what it was doing
			//when it died
			if ( eventTime == EventTime.OnEnd || player || !collisionInstance.IsDoneByPlayer() ) {
				return;
			}

			BetterEvents.InvokeCreatureKillEvent(creature, player, collisionInstance,
					GetDamageType(collisionInstance),
					GetPenetrationType(collisionInstance),
					GetHitDirection(creature, collisionInstance),
					GetCreatureState(creature), GetHitStates(collisionInstance), GetDamageArea(creature, collisionInstance));

			if ( creature.factionId == ALLYFACTION ) {
				//Debug.LogFormat(Time.time + " Player killed ally. Creature faction:{0}", creature.factionId);			
			} else {
				//Debug.LogFormat(Time.time + " Player killed enemy. Creature faction:{0}", creature.faction.id);
			}

		}

		public virtual void OnCreatureHit( Creature creature, CollisionInstance collisionInstance ) {
			if ( creature != null ) {
				BetterEvents.InvokeCreatureHitEvent(creature, collisionInstance,
					GetDamageType(collisionInstance),
					GetPenetrationType(collisionInstance),
					GetHitDirection(creature, collisionInstance),
					GetCreatureState(creature), GetHitStates(collisionInstance), GetDamageArea(creature, collisionInstance));
			} else {
				Debug.LogFormat(Time.time + " OnCreatureHit - Creature was null");
			}
		}


		/// <summary>
		/// The area that was hit, head/neck/body etc
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="collisionInstance"></param>
		/// <returns></returns>
		public virtual BetterEvents.DamageArea GetDamageArea( Creature creature, CollisionInstance collisionInstance ) {
			DamageStruct ds = collisionInstance.damageStruct;
			if ( ds.hitRagdollPart ) {
				if ( !creature.isPlayer ) {
					SubscribeToRagdollSlice(ds.hitRagdollPart);
				}
				//try parse ragdoll part to our damage area
				string ragdollpart = ds.hitRagdollPart.type.ToString();

				if ( Enum.TryParse(ragdollpart, true, out BetterEvents.DamageArea damageAreaRagdollPart) ) {
					if ( Enum.IsDefined(typeof(BetterEvents.DamageArea), damageAreaRagdollPart) ) {
						return damageAreaRagdollPart;
					}
				}
			}
			return BetterEvents.DamageArea.None;

		}

		/// <summary>
		/// The type of damage, stabbing, blunt, energy, shock etc
		/// </summary>
		/// <param name="collisionInstance"></param>
		/// <returns></returns>
		public virtual DamageType GetDamageType( CollisionInstance collisionInstance ) {
			return collisionInstance.damageStruct.damageType;
		}

		/// <summary>
		/// The type of penetration
		/// </summary>
		/// <param name="collisionInstance"></param>
		/// <returns></returns>
		public virtual DamageStruct.Penetration GetPenetrationType( CollisionInstance collisionInstance ) {
			return collisionInstance.damageStruct.penetration;
		}

		public virtual HashSet<BetterEvents.HitState> GetHitStates( CollisionInstance collisionInstance ) {

			//source collider group is the thing is the thing doing the hitting
			//no source collider group means it was something magical, htting it I think
			HashSet<BetterEvents.HitState> states = new HashSet<BetterEvents.HitState>();

			if ( collisionInstance.sourceColliderGroup ) {
				// hit with a ragdoll
				CollisionHandler ch = collisionInstance.sourceColliderGroup.collisionHandler;
				if ( ch.isRagdollPart ) {
					//Debug.LogFormat(Time.time + " Action - hit with ragdoll part ");

					//could be a force push, or the player grabbing a ragdoll part and hitting them with it?
					//maybe need to check if its the creature hitting itself
					//check if in air?

					if ( ch.ragdollPart != null ) {
						if ( ch.ragdollPart.ragdoll.creature.isPlayer ) {
							//	Debug.LogFormat(Time.time + " Action - player punch or kick ");
							states.Add(BetterEvents.HitState.PlayerRagdollPart);
						} else {
							//	Debug.LogFormat(Time.time + " Action - creature punch or kick ");

							states.Add(BetterEvents.HitState.CreatureRagdollPart);

						}
						if ( ch.ragdollPart.isGrabbed ) {
							//	Debug.LogFormat(Time.time + " Action - ragdoll part was grabbed ");
							states.Add(BetterEvents.HitState.GrabbedRagdollPart);
						}
						if ( ch.ragdollPart.ragdoll.tkHandlers.Contains(spellCasterLeft) || ch.ragdollPart.ragdoll.tkHandlers.Contains(spellCasterLeft) ) {
							//	Debug.LogFormat(Time.time + " Action - ragdoll part was telekinesis grabbed ");
							states.Add(BetterEvents.HitState.TelekinesisGrabbedRagdollPart);
						}
						if ( ch.ragdollPart.isSliced ) {
							//	Debug.LogFormat(Time.time + " Action - ragdoll part is dismembered ");
							states.Add(BetterEvents.HitState.DismemberedRagdollPart);
						}
					}
				}

				// hit with a item
				if ( ch.isItem ) {
					//Debug.LogFormat(Time.time + " Action - hit with item " + ch.item.itemId + " type: " + ch.item.data.type.ToString());
					//maybe count favourite weapon based on what weapon they hit with?

					if ( ch.item.data.type == ItemData.Type.Body ) {
						//Debug.LogFormat(Time.time + " Action - punch or kick");
						states.Add(BetterEvents.HitState.PunchOrKick);
					}
					if ( ch.item.isThrowed || ch.item.isFlying ) {
						//Debug.LogFormat(Time.time + " Action - item was thrown/flying");
						states.Add(BetterEvents.HitState.DismemberedRagdollPart);
					}

					if ( ch.item.isGripped ) {
						//Debug.LogFormat(Time.time + " Action - item is gripped");
						states.Add(BetterEvents.HitState.GrabbedItem);
					}
					if ( ch.item.isTelekinesisGrabbed ) {
						//Debug.LogFormat(Time.time + " Action - item is gripped with telekinesis");
						states.Add(BetterEvents.HitState.TelekinesisGrabbedItem);
					}
					foreach ( Imbue imbue in collisionInstance.sourceColliderGroup.collisionHandler.item.imbues ) {
						if ( imbue.energy > 0 ) {
							//Debug.LogFormat(Time.time + " Action - item is imbued with " + imbue.spellCastBase.id);
							states.Add(BetterEvents.HitState.TelekinesisGrabbedItem);
							break;
						}
					}
				}
			}
			return states;

		}

		public virtual HashSet<BetterEvents.CreatureState> GetCreatureState( Creature creature ) {

			HashSet<BetterEvents.CreatureState> states = new HashSet<BetterEvents.CreatureState>();

			if ( creature.ragdoll.parts.Any(p => p.isSliced) ) {
				//one of the ragdoll parts is sliced
				states.Add(BetterEvents.CreatureState.Dismembered);
			}
			if ( creature.state == Creature.State.Dead ) {
				// hit a dead guy?
				//Debug.LogFormat(Time.time + " Modifier - creature was dead ");
				states.Add(BetterEvents.CreatureState.Dead);
			}
			if ( creature.currentHealth > 0 && creature.currentHealth < creature.maxHealth ) {
				states.Add(BetterEvents.CreatureState.Injured);
			}
			if ( creature.currentHealth == creature.maxHealth ) {
				states.Add(BetterEvents.CreatureState.Fullhealth);
			}
			if ( creature.fallState == Creature.FallState.Falling ) {
				// falling
				states.Add(BetterEvents.CreatureState.Falling);
			}
			if ( creature.state == Creature.State.Destabilized && (creature.fallState == Creature.FallState.NearGround || creature.fallState == Creature.FallState.StabilizedOnGround) ) {
				//Debug.LogFormat(Time.time + " Modifier - creature was destablized near ground ");
				states.Add(BetterEvents.CreatureState.LayingOnGround);

			}
			if ( creature.state == Creature.State.Destabilized ) {
				//Debug.LogFormat(Time.time + " Modifier - creature was destablized ");
				states.Add(BetterEvents.CreatureState.Destabilized);
			}

			if ( !creature.locomotion.isGrounded ) {
				//Debug.LogFormat(Time.time + " Modifier - creature was flying ");
				states.Add(BetterEvents.CreatureState.NotGrounded);
			}
			if ( creature.locomotion.isGrounded ) {
				//Debug.LogFormat(Time.time + " Modifier - creature was on the ground ");
				states.Add(BetterEvents.CreatureState.Grounded);
			}
			if ( !creature.equipment.GetHeldWeapon(Side.Left) && !creature.equipment.GetHeldWeapon(Side.Right) ) {
				//Debug.LogFormat(Time.time + " Modifier - creature was unarmed ");
				states.Add(BetterEvents.CreatureState.IsUnarmed);
			}
			if ( creature.equipment.GetHeldWeapon(Side.Left) || creature.equipment.GetHeldWeapon(Side.Right) ) {
				//Debug.LogFormat(Time.time + " Modifier - creature was armed ");
				states.Add(BetterEvents.CreatureState.IsArmed);
			}
			if ( creature.ragdoll.isGrabbed ) {
				states.Add(BetterEvents.CreatureState.IsGrabbed);
			}
			if ( creature.brain.IsRunningAction<ActionShock>() ) {
				//Debug.LogFormat(Time.time + " Modifier - creature is shocked ");
				states.Add(BetterEvents.CreatureState.IsBeingElectrocuted);
			}
			if ( creature.brain.IsRunningAction<ActionAttackMelee>() ) {
				//Debug.LogFormat(Time.time + " Modifier - creature was attacking with melee ");
				states.Add(BetterEvents.CreatureState.IsAttackingWithMelee);
			}
			if ( creature.brain.IsRunningAction<ActionAttackBow>() ) {
				//Debug.LogFormat(Time.time + " Modifier - creature was attacking with bow");
				states.Add(BetterEvents.CreatureState.IsAttackingWithBow);
			}
			if ( creature.brain.IsRunningAction<ActionAttackCast>() ) {
				//Debug.LogFormat(Time.time + " Modifier - creature was attacking with cast");
				states.Add(BetterEvents.CreatureState.IsAttackingWithCast);
			}
			if ( creature.brain.IsAttacking ) {
				//Debug.LogFormat(Time.time + " Modifier - creature was attacking");
				states.Add(BetterEvents.CreatureState.IsAttacking);
			}
			if ( creature.brain.IsRunningAction<ActionStagger>() ) {
				//Debug.LogFormat(Time.time + " Modifier - creature was attacking");
				states.Add(BetterEvents.CreatureState.IsAttacking);
			}
			if ( creature.brain.IsRunningAction<ActionDraw>() ) {
				//Debug.LogFormat(Time.time + " Modifier - creature was attacking");
				states.Add(BetterEvents.CreatureState.IsDrawingWeapon);
			}
			if ( creature.brain.IsRunningAction<ActionDying>() ) {
				//Debug.LogFormat(Time.time + " Modifier - creature was attacking");
				states.Add(BetterEvents.CreatureState.IsDying);
			}
			if ( IsBeingChoked(creature) || WasBeingChoked(creature) ) {
				//Debug.LogFormat(Time.time + " Modifier - creature was being choked");
				states.Add(BetterEvents.CreatureState.IsBeingChoked);
			}
			if ( creature.brain?.instance?.targetCreature?.isPlayer == true ) {
				states.Add(BetterEvents.CreatureState.IsTargetingPlayer);
			}
			if ( creature.brain?.instance?.targetCreature?.isPlayer == false ) {
				states.Add(BetterEvents.CreatureState.IsTargetingNPC);
			}

			//try to see if the creature is trying to grab a weapon
			if ( creature.brain.IsRunningAction<ActionGrab>() ) {
				states.Add(BetterEvents.CreatureState.IsTargetingWeapon);
			}

			//if ( creature.brain.instance.GetType() == typeof(BrainHuman) ) {
			//	states.Add(BetterEvents.CreatureState.IsTargetingWeapon);
			//}

			//TODO check if the creature is fleeing. by maybe checking the action move position == one of the flee points

			return states;

		}


		/// <summary>
		/// Returns the direction a creature was hit, ie Direction.back is backstabbed
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="collisionInstance"></param>
		/// <returns></returns>
		public virtual BetterEvents.Direction GetHitDirection( Creature creature, CollisionInstance collisionInstance ) {

			//get impact direction
			Vector3 lhs = Utils.ClosestDirection(creature.transform.InverseTransformDirection(collisionInstance.impactVelocity).normalized, Cardinal.XZ);

			if ( lhs == Vector3.back ) {
				//hit front
				return BetterEvents.Direction.Front;
			} else if ( lhs == Vector3.forward ) {
				//hit back
				return BetterEvents.Direction.Back;
			} else if ( lhs == Vector3.left ) {
				return BetterEvents.Direction.Left;
			} else if ( lhs == Vector3.right ) {
				//hit right
				return BetterEvents.Direction.Right;
			} else if ( lhs == Vector3.up ) {
				//hit from underneith
				return BetterEvents.Direction.Under;
			} else if ( lhs == Vector3.down ) {
				//hit from underneith
				return BetterEvents.Direction.Above;
			}
			return BetterEvents.Direction.None;

		}

	}
}