using IngameDebugConsole;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;
using Wully.Events;
using Wully.Helpers;
using static Wully.Helpers.BetterHelpers;
using static Wully.Helpers.BetterLogger;

namespace Wully.Module {
	/// <summary>
	/// LevelModule which monitors, tracks and invokes events with rich data
	/// </summary>
	public class LevelModuleBetterEvents : LevelModule {
		BetterLogger log = BetterLogger.GetLogger(typeof(LevelModuleBetterEvents));

		// Configurables
		public bool enableLogging = true;
		public Loglevel loglevel = Loglevel.Debug;


		public static LevelModuleBetterEvents local;


		public Level currentLevel;
		protected LevelModuleWave levelModuleWave;

		private HashSet<CollisionHandler> grabbedCollisionHandlers;
		private Dictionary<Ragdoll, HashSet<RagdollPart>> ragdollHits;


		public SpellCaster spellCasterLeft;
		private Handle tkLeftHandle;
		public SpellCaster spellCasterRight;
		private Handle tkRightHandle;

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
			//UnityEngine.Debug.LogFormat("AlignmentSystem OnLoadCoroutine level on {0}", level.data.id);
			this.currentLevel = level;
			InitVars();
			//master scene is always loaded and this module gets loaded with it
			if ( level.data.id.ToLower() == "master" ) {
				log.Info("Initialized Wully's BetterMods - BetterEvents Module");
				//UnityEngine.Debug.Log("Initialized Wully's BetterMods - BetterEvents Module");

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
					//UnityEngine.Debug.LogFormat("Subscribed to levelModule wave on {0}", level.data.id);
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
		public override void Update( Level level ) {
			base.Update(level);

			//since TK doesnt have events for grabbing, we need to monitor it and subscribe to the item the player TKs
			if ( spellCasterLeft ) {
				MonitorTK(spellCasterLeft, Side.Left, ref tkLeftHandle);
			}
			//left hand check
			if ( spellCasterRight ) {
				MonitorTK(spellCasterRight, Side.Right, ref tkRightHandle);
			}
		}

		private void MonitorTK( SpellCaster spellCaster, Side side, ref Handle handle ) {

			if ( spellCaster?.telekinesis != null ) {
					
				if ( spellCaster.telekinesis.catchedHandle != null ) {
					Handle caught = spellCaster.telekinesis.catchedHandle;

					//if the handle is currently set, check its not the same one
					if ( caught == handle) { return; }
					
					if ( handle == null ) {
						log.Debug("Spellcaster hand " + side.ToString() + " grabbed a handle");
						
						if ( caught is HandleRagdoll handleRagdoll ) {
							//subscribe to ragdoll slice					
							SubscribeToRagdollSlice(handleRagdoll.ragdollPart);
						} else {
							SubscribeToHandleColliders(caught);
						}
						BetterEvents.InvokePlayerTelekinesisGrabEvent(side, caught);
						
					} else {
						//something weird happend but the TK is now holding a different item and didnt drop the old one
						//unsubscribe to old handle and call ungrab event
						BetterEvents.InvokePlayerTelekinesisUnGrabEvent(side, handle);
						UnsubscribeToHandleColliders(handle);

						//add the new handle
						if ( caught is HandleRagdoll handleRagdoll ) {
							//subscribe to ragdoll slice					
							SubscribeToRagdollSlice(handleRagdoll.ragdollPart);
						} else {
							SubscribeToHandleColliders(caught);
						}
						BetterEvents.InvokePlayerTelekinesisGrabEvent(side, caught);

						
					}
					handle = caught;

				} else {
					//player dropped what they were Tking
					if ( handle != null ) {

						BetterEvents.InvokePlayerTelekinesisUnGrabEvent(side, handle);
						// unsubscribe to handles colliders
						UnsubscribeToHandleColliders(handle);
						handle = null;
					}
					
				}
			}
			
		}

		private void OnUnPossessEvent( Creature creature, EventTime eventTime ) {
			if ( eventTime == EventTime.OnStart ) {
				if ( Player.local.creature != null ) {
					//UnityEngine.Debug.LogFormat("UnSubscribed to player hand events");
					//Subscribe to what the player picks up so we can track other things
					Player.local.creature.handLeft.OnGrabEvent -= Hand_OnGrabEvent;
					Player.local.creature.handRight.OnGrabEvent -= Hand_OnGrabEvent;
					Player.local.creature.handLeft.OnUnGrabEvent -= Hand_OnUnGrabEvent;
					Player.local.creature.handRight.OnUnGrabEvent -= Hand_OnUnGrabEvent;
					spellCasterLeft = null;
					spellCasterRight = null;
				}
			}
		}

		private void OnPossessEvent( Creature creature, EventTime eventTime ) {
			if ( eventTime == EventTime.OnEnd ) {
				if ( Player.local.creature != null ) {
					//UnityEngine.Debug.LogFormat("Subscribed to player hand events");
					//Subscribe to what the player picks up so we can track other things
					Player.local.creature.handLeft.OnGrabEvent += Hand_OnGrabEvent;
					Player.local.creature.handRight.OnGrabEvent += Hand_OnGrabEvent;
					Player.local.creature.handLeft.OnUnGrabEvent += Hand_OnUnGrabEvent;
					Player.local.creature.handRight.OnUnGrabEvent += Hand_OnUnGrabEvent;
					Player.local.locomotion.OnGroundEvent += Locomotion_OnGroundEvent;
					spellCasterLeft = Player.local.creature.mana.casterLeft;
					spellCasterRight = Player.local.creature.mana.casterRight;
				}
			}
		}

		private void Locomotion_OnGroundEvent( bool grounded, Vector3 velocity ) {
			BetterEvents.InvokePlayerGroundEvent(grounded, velocity);
		}

		private void SubscribeToRagdollSlice( RagdollPart ragdollPart ) {
			//Debug.LogFormat(Time.time + " DamageArea - Subscribing to ragdolls onSliceEvent");

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

		private void Hand_OnUnGrabEvent( Side side, Handle handle, bool throwing, EventTime eventTime ) {

			//Debug.Log("on ungrabevent - event time: " + eventTime.ToString());
			if ( eventTime == EventTime.OnEnd ) {
				BetterEvents.InvokePlayerHandUnGrabEvent(side, handle, throwing, eventTime);
				UnsubscribeToHandleColliders(handle);
			}
		}

		private void Hand_OnGrabEvent( Side side, Handle handle, float axisPosition, HandleOrientation orientation, EventTime eventTime ) {

			//Debug.Log("on grabevent - event time: " + eventTime.ToString());
			if ( eventTime == EventTime.OnEnd ) {
				BetterEvents.InvokePlayerHandGrabEvent(side, handle, axisPosition, orientation, eventTime);
				SubscribeToHandleColliders(handle);
			}

		}

		private void SubscribeToHandleColliders( Handle handle ) {
			//when the player grabs something, we want to subscribe to that items collision handlers
			if ( handle.item != null && handle.item.collisionHandlers.Count > 0 ) {
				foreach ( CollisionHandler collisionHandler in handle.item.collisionHandlers ) {
					if ( !grabbedCollisionHandlers.Contains(collisionHandler) ) {
						collisionHandler.OnCollisionStartEvent += this.GrabbedItem_OnCollisionStartEvent;
						grabbedCollisionHandlers.Add(collisionHandler);
					}
				}
			}

		}

		private void UnsubscribeToHandleColliders( Handle handle ) {
			if ( handle.item != null && handle.item.collisionHandlers.Count > 0 ) {
				// check the players not still holding it though
				foreach ( RagdollHand hand in handle.item.handlers ) {
					if ( hand.creature.isPlayer ) {
						return;
					}
				}
				foreach ( CollisionHandler collisionHandler in handle.item.collisionHandlers ) {
					collisionHandler.OnCollisionStartEvent -= this.GrabbedItem_OnCollisionStartEvent;
					grabbedCollisionHandlers.Remove(collisionHandler);
				}
			}
		}

		private void GrabbedItem_OnCollisionStartEvent( CollisionInstance collisionInstance ) {
			//this is called when this GrabbedItem has begun touching another rigidbody/collider.
			//our grabbed item is the source, the thing we hit is the target


			//source was the player
			if ( collisionInstance.IsDoneByPlayer() ) {
				//Debug.Log(Time.time + " GrabbedItem - collision done by player");
				if ( collisionInstance.targetColliderGroup != null ) {
					//get the collision handler of the thing we hit					
					CollisionHandler ch = collisionInstance.targetColliderGroup.collisionHandler;
					if ( ch != null && ch.isItem && ch.item.handlers.Count > 0 ) {
						//check if players grabbed item hit a item held by an enemy -- we can assume it means their attack was blocked
						bool isCreatureHoldingItem = true;
						foreach ( RagdollHand hand in ch.item.handlers ) {
							if ( hand.creature.isPlayer ) {
								isCreatureHoldingItem = false; //player was also holding the creatures item
									
							}
						}
						//only creature was holding the thing we hit
						if ( isCreatureHoldingItem ) {
							//Debug.Log(Time.time + " GrabbedItem - Players attack hit a creatures item they were holding " + ch.item.itemId + " type: " + ch.item.data.type.ToString());
							// player attack  with grabbed Item was parried
							BetterEvents.InvokeCreatureParryingPlayer(ch.item.handlers[0].creature, collisionInstance);
						}
						if ( ch.item.tkHandlers.Contains(spellCasterLeft) || ch.item.tkHandlers.Contains(spellCasterLeft) ) {
							//Debug.Log(Time.time + " GrabbedItemTK - Players attack hit a creatures item they were holding " + ch.item.itemId + " type: " + ch.item.data.type.ToString());
							// player attack with TKgrabbed Item was parried 
//TODO this is probably wrong? its checking the collision handler of the thing we hit, rather than if the thing we were holding was held with TK
						}
					}
					if ( ch.isRagdollPart ) {
						if ( ch.ragdollPart.ragdoll != null && ch.ragdollPart.ragdoll.creature != null ) {
							if ( ch.ragdollPart.ragdoll.creature.isKilled ) {
								SubscribeToRagdollSlice(ch.ragdollPart);
								//Debug.Log(Time.time + " GrabbedItem - Player hitting a dead ragdoll! " +" type: " + ch.ragdollPart.type.ToString());
								// player hit a dead ragdoll with a grabbed item

							}
						}
					}

				} else {
					// hit the ground?

				}
			} else {
				//source was the creature
				//nulls
				//Debug.Log(Time.time + " GrabbedItem - collision done by creature");
				if ( collisionInstance.sourceColliderGroup != null ) {
					CollisionHandler ch = collisionInstance.sourceColliderGroup.collisionHandler;
					//Debug.Log(Time.time + " GrabbedItem - checking creature as source");
					CheckFlyingItemHit(ch);
				}

			}
		}

		private void CheckFlyingItemHit( CollisionHandler ch ) {
			//check if its not behing held, but last holder was not the player
			if ( ch != null && ch.isItem && ch.item.handlers != null ) {
				if ( ch.item.handlers.Count == 0 ) {
					//ignore stuff the player had touched
					if ( ch.item.lastHandler != null && ch.item.lastHandler.creature.isPlayer ) { return; }

					
					//if the item has no handlers, and it doesnt have a last handler, then its probably a spell
					if ( ch.item.lastHandler == null ) {
						//spell 4 sure
//TODO: add playerblockspell
						//Debug.Log(Time.time + " GrabbedItem - Player hit a spell item no one ever held " + ch.item.itemId + " type: " + ch.item.data.type.ToString() + " isFlying: " + ch.item.isFlying + " isThrown " + ch.item.isThrowed);
					} else {
						//otherwise it was probably held by the creature, like an arrow, sometimes a spell though		
						//Debug.Log(Time.time + " GrabbedItem - Player hit a creatures item they are no longer holding " + ch.item.itemId + " type: " + ch.item.data.type.ToString() + " isFlying: " + ch.item.isFlying + " isThrown " + ch.item.isThrowed);

					}
				}
			}
		}

		private void OnCreatureHeal( Creature creature, float heal, Creature healer ) {
			//UnityEngine.Debug.Log("OnCreatureHeal");
			if ( healer.isPlayer ) {

			}

		}

		public override void OnUnload( Level level ) {
			//UnityEngine.Debug.LogFormat("AlignmentSystem unload level on {0}", level.data.id);
			if ( this.levelModuleWave != null ) {
				this.levelModuleWave.OnWaveBeginEvent -= this.OnWaveBegin;
				this.levelModuleWave.OnWaveLoopEvent -= this.OnWaveLoop;
			}

		}


		private void OnWaveBegin() {
			//UnityEngine.Debug.Log("OnWaveBegin");
			InitVars();
		}
		private void OnWaveLoop() {
			//UnityEngine.Debug.Log("OnWaveLoop");

		}


		private void Ragdoll_OnSliceEvent( RagdollPart ragdollPart, EventTime eventTime ) {
			//ragdoll is being sliced. This calls at the end of slicing
			Ragdoll ragdoll = ragdollPart.ragdoll;

			CollisionInstance ch = ragdoll.creature.lastDamage;

			//use onstart check if creature alive or ded wen starting to know if player is fuking with ded bodies
			if ( eventTime == EventTime.OnStart ) {
				//remove it from the dictionary if we did hit it before
				if ( ragdollHits.ContainsKey(ragdoll) ) {
					if ( ragdollHits[ragdoll].Contains(ragdollPart) ) {

						BetterEvents.InvokeDismemberEvent(ragdollPart, ragdoll.creature.isKilled, ragdollPart.type);

						if ( ragdoll.tkHandlers.Contains(spellCasterLeft) || ragdoll.tkHandlers.Contains(spellCasterLeft) ) {
							//Debug.LogFormat(Time.time + "OnSlice - Player is currently TKing a tracked ragdoll part - " + ragdollPart.type + " - was creature dead: " + ragdoll.creature.isKilled);

						}
						//last hit was player
						if ( ch.IsDoneByPlayer() ) {
							//Debug.LogFormat(Time.time + "OnSlice - Player sliced tracked ragdoll part - " + ragdollPart.type + " - was creature dead: " + ragdoll.creature.isKilled);

						} else {
							// this happens if the ragdoll was already dead I think, as long as it was tracked though we know we did it 
							//Debug.LogFormat(Time.time + "OnSlice - Tracked ragdoll part sliced, but player wasnt last to hit - " + ragdollPart.type + " - was creature dead: " + ragdoll.creature.isKilled);
						}

						//check if it was a decap?

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
					//this never seems to be called
					if ( ch.IsDoneByPlayer() ) {
						//Debug.LogFormat(Time.time + "OnSlice - Player sliced Untracked ragdoll part - " + ragdollPart.type + " - was creature dead: " + ragdoll.creature.isKilled);
					} else {
						//Debug.LogFormat(Time.time + "OnSlice - Untracked ragdoll part was sliced, but player wasnt last to hit - " + ragdollPart.type + " - was creature dead: " + ragdoll.creature.isKilled);
					}
				}
			}

		}
		private void OnDeflectEvent( Creature source, Item item, Creature target ) {
			//UnityEngine.Debug.Log("OnDeflectEvent");
			BetterEvents.InvokeDeflectEvent(source, item, target);
			if ( source.player && target ) {
				//Debug.Log("Player deflected something!");

			}

			if ( target.player && source ) {
				//Debug.Log("Creature deflected players attack!");

			}

		}

		private void OnCreatureParry( Creature creature, CollisionInstance collisionInstance ) {
			//Debug.Log(Time.time + " OnCreatureParry ");

			if ( creature != null && !creature.isPlayer && !collisionInstance.IsDoneByPlayer() ) {
				//Debug.Log(Time.time + " OnCreatureParry - Player parried creature attack"); //this one
				BetterEvents.InvokePlayerParryingCreature(creature, collisionInstance);
			}

		}

		private void OnCreatureKill( Creature creature, Player player, CollisionInstance collisionInstance, EventTime eventTime ) {

			//return if the kill was not done by the player, or if the player was the one killed
			//Event time is the start/end of the kill class on the creature, we want the start event so we know what it was doing
			//when it died
			if ( eventTime == EventTime.OnEnd || (bool)(UnityEngine.Object)player || !collisionInstance.IsDoneByPlayer() ) {
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

		private void OnCreatureHit( Creature creature, CollisionInstance collisionInstance ) {		
			if ( creature != null ) {
				BetterEvents.InvokeCreatureHitEvent(creature, collisionInstance,
					GetDamageType(collisionInstance),
					GetPenetrationType(collisionInstance),
					GetHitDirection(creature, collisionInstance),
					GetCreatureState(creature), GetHitStates(collisionInstance),GetDamageArea(creature,collisionInstance));
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
		public BetterEvents.DamageArea GetDamageArea( Creature creature, CollisionInstance collisionInstance ) {			
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
		public DamageType GetDamageType( CollisionInstance collisionInstance ) {
			return collisionInstance.damageStruct.damageType;
		}

		/// <summary>
		/// The type of penetration
		/// </summary>
		/// <param name="collisionInstance"></param>
		/// <returns></returns>
		public DamageStruct.Penetration GetPenetrationType( CollisionInstance collisionInstance ) {
			return collisionInstance.damageStruct.penetration;
		}

		public HashSet<BetterEvents.HitState> GetHitStates( CollisionInstance collisionInstance ) {

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

		public HashSet<BetterEvents.CreatureState> GetCreatureState( Creature creature ) {

			HashSet<BetterEvents.CreatureState> states = new HashSet<BetterEvents.CreatureState>();

			if(creature.ragdoll.parts.Any(p => p.isSliced)) {
				//one of the ragdoll parts is sliced
				states.Add(BetterEvents.CreatureState.Dismembered);
			}
			if ( creature.state == Creature.State.Dead ) {
				// hit a dead guy?
				//Debug.LogFormat(Time.time + " Modifier - creature was dead ");
				states.Add(BetterEvents.CreatureState.Dead);
			}
			if( creature.currentHealth > 0 && creature.currentHealth < creature.maxHealth ) {
				states.Add(BetterEvents.CreatureState.Injured);
			}
			if ( creature.currentHealth == creature.maxHealth ) {
				states.Add(BetterEvents.CreatureState.Fullhealth);
			}
			if ( creature.fallState == Creature.FallState.Falling) {
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
			if ( creature.brain.IsAttacking) {
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
			if(creature.brain?.instance?.targetCreature?.isPlayer == true ) {
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
		public BetterEvents.Direction GetHitDirection( Creature creature, CollisionInstance collisionInstance) {
			
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