using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ThunderRoad;
using UnityEngine;
using Wully.Events;
using Wully.Helpers;
using Wully.Performance;
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
		/// Set the GetLogLevel for BetterEvents
		/// </summary>
		public LogLevel logLevel = LogLevel.Info;

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
		private bool spellCasterLeftIsCasting = false;
		private bool spellCasterLeftIsSpraying = false;
		private bool spellCasterLeftIsMerging = false;
		/// <summary>
		/// Returns the players left telekinesis grabbed handle
		/// </summary>
		public Handle SpellCasterLeftGrabbedHandle => tkLeftHandle;
		/// <summary>
		/// Players right spellcaster
		/// </summary>
		public SpellCaster spellCasterRight;
		private Handle tkRightHandle;
		private bool spellCasterRightIsCasting = false;
		private bool spellCasterRightIsSpraying = false;
		private bool spellCasterRightIsMerging = false;
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
			try {
				log.SetLoggingLevel(logLevel);
				log.DisableLogging();
				if ( enableLogging ) {
					log.EnableLogging();
				}

				log.Debug().Message("level {0}", level.data.id);

				this.currentLevel = level;
				InitVars();
				//master scene is always loaded and this module gets loaded with it
				if ( level.data.id.ToLower() == "master" ) {


					log.Info().Message("Initialized Wully's BetterMods - BetterEvents Module".Italics());

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
						log.Debug().Message("Subscribing to levelModuleWave events on level: {0}", level.data.id);
						this.levelModuleWave.OnWaveBeginEvent += this.OnWaveBegin;
						this.levelModuleWave.OnWaveLoopEvent += this.OnWaveLoop;
					}

				}
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}

			yield break;
		}
		/// <summary>
		/// Called every frame
		/// </summary>
		/// <param name="level"></param>
		public override void Update( Level level ) {
			try {
				base.Update(level);

				//since TK doesnt have events for grabbing, we need to monitor it and subscribe to the item the player TKs
				if ( spellCasterLeft ) {
					MonitorCast(spellCasterLeft, Side.Left);
					MonitorTk(spellCasterLeft, Side.Left, ref tkLeftHandle);
				}
				//left hand check
				if ( spellCasterRight ) {
					MonitorCast(spellCasterRight, Side.Left);
					MonitorTk(spellCasterRight, Side.Right, ref tkRightHandle);
				}

			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}
		}
		/// <summary>
		/// Called when the player TK grabbed something
		/// </summary>
		/// <param name="handle"></param>
		/// <param name="side"></param>
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
		/// This function monitors when the player casts spells and fires one off events when the player starts/stops casting
		/// </summary>
		/// <param name="spellCaster"></param>
		/// <param name="side"></param>
		public virtual void MonitorCast(SpellCaster spellCaster, Side side) {
			if ( !spellCaster ) { return; }

			// Store the bools for the side we're checking. Means we dont need loads of if statements for left/right
			bool isCasting = spellCasterLeftIsCasting;
			bool isSpraying = spellCasterLeftIsSpraying;
			bool isMerging = spellCasterLeftIsSpraying;

			if (side == Side.Right) {
				isCasting = spellCasterRightIsCasting;
				isSpraying = spellCasterRightIsSpraying;
				isMerging = spellCasterRightIsSpraying;
			}
			
			// Starts casting
			if (spellCaster.isFiring && !isCasting ) {
				log.Debug().Message($"Player started casting {spellCaster.spellInstance.id} with {side} hand");
				BetterEvents.InvokeOnPlayerCastSpellEvent(spellCaster, side, EventTime.OnStart);
			}

			// Stops casting - let go of trigger
			if ( !spellCaster.isFiring && isCasting ) {
				log.Debug().Message($"Player stopped casting {spellCaster.spellInstance.id} with {side} hand");
				BetterEvents.InvokeOnPlayerCastSpellEvent(spellCaster, side, EventTime.OnEnd);
			}

			// Starts spraying 
			if ( spellCaster.isSpraying && !isSpraying ) {
				log.Debug().Message($"Player started spraying {spellCaster.spellInstance.id} with {side} hand");
				BetterEvents.InvokeOnPlayerSpraySpellEvent(spellCaster, side, EventTime.OnStart);
			}
			// Stops spraying 
			if ( !spellCaster.isSpraying && isSpraying ) {
				log.Debug().Message($"Player stopped spraying {spellCaster.spellInstance.id} with {side} hand");
				BetterEvents.InvokeOnPlayerSpraySpellEvent(spellCaster, side, EventTime.OnEnd);
			}

			// Starts merge 
			if ( spellCaster.isMerging && !isMerging ) {
				log.Debug().Message($"Player started merging {spellCaster.spellInstance.id} with {side} hand");
				BetterEvents.InvokeOnPlayerSpraySpellEvent(spellCaster, side, EventTime.OnStart);
			}

			// Stops merge 
			if ( !spellCaster.isMerging && isMerging ) {
				log.Debug().Message($"Player stopped merging {spellCaster.spellInstance.id} with {side} hand");
				BetterEvents.InvokeOnPlayerMergeSpellEvent(spellCaster, side, EventTime.OnEnd);
			}

			if ( side == Side.Left ) {
				spellCasterLeftIsCasting = isCasting;
				spellCasterLeftIsSpraying = isSpraying;
				spellCasterLeftIsSpraying = isMerging;
			}
			if ( side == Side.Right ) {
				spellCasterRightIsCasting = isCasting;
				spellCasterRightIsSpraying = isSpraying;
				spellCasterRightIsSpraying = isMerging;
			}
			

		}
		/// <summary>
		/// Continually checks if the player has grabbed something with Telekinesis and updates a handle reference
		/// </summary>
		/// <param name="spellCaster"></param>
		/// <param name="side"></param>
		/// <param name="handle"></param>
		public virtual void MonitorTk( SpellCaster spellCaster, Side side, ref Handle handle ) {
			if ( !spellCaster ) { return; }

			if ( spellCaster.TryGetTkHandle(out Handle caught) ) {

				//if the handle is currently set, check its not the same one
				if ( caught == handle ) { return; }
				log.Debug().Message("Spellcaster hand {0} grabbed a handle {1}", side, caught.name);
				if ( handle != null ) {
					log.Debug().Message("Spellcaster hand {0} dropped a handle {1}", side, handle.name);
					//something weird happend but the TK is now holding a different item and didnt drop the old one
					//unsubscribe to old handle and call ungrab event
					BetterEvents.InvokePlayerTelekinesisUnGrabEvent(side, handle);
					UnsubscribeToHandleColliders(handle);
				}

				handle = caught;
				TkCaughtHandle(handle, side);

			} else {
				//player dropped what they were Tking
				if ( handle != null ) {
					log.Debug().Message("Spellcaster hand {0} dropped a handle {1}", side, handle.name);
					BetterEvents.InvokePlayerTelekinesisUnGrabEvent(side, handle);
					// unsubscribe to handles colliders
					UnsubscribeToHandleColliders(handle);
					handle = null;
				}

			}

		}

		public virtual void OnUnPossessEvent( Creature creature, EventTime eventTime ) {
			try {
				//do it for onstart
				if ( eventTime == EventTime.OnEnd ) { return; }
				if ( Player.local?.creature == null ) { return; }

				log.Debug().Message("UnSubscribed to player hand/foot events");
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
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}

		}

		public virtual void OnPossessEvent( Creature creature, EventTime eventTime ) {
			try {

				//do it for on end
				if ( eventTime == EventTime.OnStart ) { return; }
				if ( Player.local?.creature == null ) { return; }

				log.Debug().Message("Subscribed to player hand/foot events");
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
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}

		}


		/// <summary>
		/// Called when player locomotion is on ground
		/// </summary>
		/// <param name="grounded"></param>
		/// <param name="velocity"></param>
		public virtual void Locomotion_OnGroundEvent( bool grounded, Vector3 velocity ) {
			try {
				//This gets called continuously.. not what we want
				//log.Debug().Message("grounded: {0} , velocity : {1}", grounded, velocity);
				BetterEvents.InvokePlayerGroundEvent(grounded, velocity);
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}
		}

		/// <summary>
		/// Used to listen to a ragdoll parts slice events
		/// </summary>
		/// <param name="ragdollPart"></param>
		public virtual void SubscribeToRagdollSlice( RagdollPart ragdollPart ) {
			try {

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
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}
		}

		/// <summary>
		/// Called when the players hand stops grabbing or TK grabbing something
		/// </summary>
		/// <param name="side"></param>
		/// <param name="handle"></param>
		/// <param name="throwing"></param>
		/// <param name="eventTime"></param>
		public virtual void Hand_OnUnGrabEvent( Side side, Handle handle, bool throwing, EventTime eventTime ) {
			try {
				if ( eventTime == EventTime.OnEnd ) {
					log.Debug().Message("side: {0}, handle: {1}, throwing: {2} ", side, handle.name, throwing);
					BetterEvents.InvokePlayerHandUnGrabEvent(side, handle, throwing, eventTime);
					UnsubscribeToHandleColliders(handle);
				}
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}
		}

		/// <summary>
		/// Called wh en the players hand grabs or TK grabs something
		/// </summary>
		/// <param name="side"></param>
		/// <param name="handle"></param>
		/// <param name="axisPosition"></param>
		/// <param name="orientation"></param>
		/// <param name="eventTime"></param>
		public virtual void Hand_OnGrabEvent( Side side, Handle handle, float axisPosition, HandleOrientation orientation, EventTime eventTime ) {
			try {
				if ( eventTime == EventTime.OnEnd ) {
					log.Debug().Message("side: {0}, handle: {1} ", side, handle.name);
					BetterEvents.InvokePlayerHandGrabEvent(side, handle, axisPosition, orientation, eventTime);
					SubscribeToHandleColliders(handle);
				}
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}
		}

		/// <summary>
		/// Subscribes to the handles colliders OnCollisionStartEvent
		/// </summary>
		/// <param name="handle"></param>
		public virtual void SubscribeToHandleColliders( Handle handle ) {
			try {
				//when the player grabs something, we want to subscribe to that items collision handlers	
				if ( !handle ) { return; }

				if ( handle is HandleRagdoll handleRagdoll ) {
					CollisionHandler ch = handleRagdoll?.ragdollPart?.collisionHandler;
					if ( ch ) {
						if ( grabbedCollisionHandlers.Contains(ch) ) {
							ch.OnCollisionStartEvent += this.PlayerItemFootHand_OnCollisionStartEvent;
							grabbedCollisionHandlers.Add(ch);
						}
					}
					return;
				} else {

					if ( handle.item ) {
						foreach ( CollisionHandler collisionHandler in handle.item.collisionHandlers ) {
							if ( !grabbedCollisionHandlers.Contains(collisionHandler) ) {
								collisionHandler.OnCollisionStartEvent += this.PlayerItemFootHand_OnCollisionStartEvent;
								grabbedCollisionHandlers.Add(collisionHandler);
							}
						}
					}
				}
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}
		}

		/// <summary>
		/// Unsubscribes to a handles colliders OnCollisionStartEvent
		/// </summary>
		/// <param name="handle"></param>
		public virtual void UnsubscribeToHandleColliders( Handle handle ) {
			try {
				if ( !handle ) { return; }
				// check the players not still holding it though	
				if ( handle.IsPlayerHolding() ) { return; }

				if ( handle is HandleRagdoll handleRagdoll ) {
					CollisionHandler ch = handleRagdoll?.ragdollPart?.collisionHandler;
					if ( ch ) {

						ch.OnCollisionStartEvent -= this.PlayerItemFootHand_OnCollisionStartEvent;
						grabbedCollisionHandlers.Remove(ch);

					}
					return;
				} else {

					if ( handle.item ) {
						foreach ( CollisionHandler collisionHandler in handle.item.collisionHandlers ) {

							collisionHandler.OnCollisionStartEvent -= this.PlayerItemFootHand_OnCollisionStartEvent;
							grabbedCollisionHandlers.Remove(collisionHandler);

						}
					}
				}
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}

		}

		/// <summary>
		/// This is called when the players grabbed, TKed item or fists has begun touching another rigidbody/collider
		/// </summary>
		/// <param name="collisionInstance"></param>
		public virtual void PlayerItemFootHand_OnCollisionStartEvent( CollisionInstance collisionInstance ) {
			try {
				//this is called when this GrabbedItem has begun touching another rigidbody/collider.
				//source is what did the hitting, target is the thing being hit
				log.Debug().Message("{0}", collisionInstance.ToStringExt());

				BetterEvents.InvokeCollisionEvent(this, collisionInstance);

			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}

		}


		/// <summary>
		/// Called when a creature is healed
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="heal"></param>
		/// <param name="healer"></param>
		/// public virtual void OnCreatureHeal( Creature creature, float heal, Creature healer )
		/// <remarks>private for now, since the base game event is fine as it is</remarks>
		private void OnCreatureHeal( Creature creature, float heal, Creature healer ) {
			try {
				if ( !creature && !healer ) { return; }
				log.Debug().Message("Creature {0} healed {1} by {2}", healer.name, creature.name, heal);
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}
		}

		/// <summary>
		/// Called when the current level is unloaded
		/// </summary>
		/// <param name="level"></param>
		public override void OnUnload( Level level ) {
			try {
				if ( this.levelModuleWave != null ) {
					this.levelModuleWave.OnWaveBeginEvent -= this.OnWaveBegin;
					this.levelModuleWave.OnWaveLoopEvent -= this.OnWaveLoop;
				}
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}
		}


		public virtual void OnWaveBegin() {
			try {
				//UnityEngine.Debug.Log("OnWaveBegin");
				InitVars();
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}
		}
		public virtual void OnWaveLoop() {
			try {
				//UnityEngine.Debug.Log("OnWaveLoop");
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}
		}

		/// <summary>
		/// Called when a ragdoll part is sliced
		/// </summary>
		/// <param name="ragdollPart"></param>
		/// <param name="eventTime"></param>
		public virtual void Ragdoll_OnSliceEvent( RagdollPart ragdollPart, EventTime eventTime ) {
			try {
				//use onstart check if creature alive or ded wen starting to know if player is messing with dead bodies
				if ( eventTime == EventTime.OnEnd ) {
					return;
				}

				//ragdoll is being sliced.
				Ragdoll ragdoll = ragdollPart.ragdoll;
				//Last thing th at hit damaged it - most likely causing the slice
				CollisionInstance ch = ragdoll.creature.lastDamage;


				//remove it from the dictionary if we did hit it before
				if ( ragdollHits.ContainsKey(ragdoll) ) {
					if ( ragdollHits[ragdoll].Contains(ragdollPart) ) {

						log.Debug().Message("ragdollPart {0} was being tracked and was sliced",
							ragdollPart.type.ToString());

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
					log.Warn().Message("ragdollPart {0} was not being tracked.. and was sliced",
						ragdollPart.type.ToString());
				}

			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}
		}

		/// <summary>
		/// Called when a spell is deflected
		/// </summary>
		/// <param name="source"></param>
		/// <param name="item"></param>
		/// <param name="target"></param>
		public virtual void OnDeflectEvent( Creature source, Item item, Creature target ) {
			try {
				BetterEvents.InvokeDeflectEvent(source, item, target);
				if ( source.player && target ) {
					log.Debug().Message("Player deflected creatures {0}", item.data.id);
				}

				if ( target.player && source ) {
					log.Debug().Message("Creature deflected players {0}", item.data.id);
				}
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}

		}

		/// <summary>
		/// Called when a creature parrys
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="collisionInstance"></param>
		public virtual void OnCreatureParry( Creature creature, CollisionInstance collisionInstance ) {
			try {
				if ( !creature ) { return; }

				if ( creature.isPlayer ) { return; }

				if ( collisionInstance.IsDoneByPlayer() ) { return; }

				log.Debug().Message("Player parried creature attack");
				BetterEvents.InvokePlayerParryingCreature(creature, collisionInstance);
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}
		}

		/// <summary>
		/// Called when a creature is killed
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="player"></param>
		/// <param name="collisionInstance"></param>
		/// <param name="eventTime"></param>
		public virtual void OnCreatureKill( Creature creature, Player player, CollisionInstance collisionInstance, EventTime eventTime ) {
			try {
				//return if the kill was not done by the player, or if the player was the one killed
				//Event time is the start/end of the kill class on the creature, we want the start event so we know what it was doing
				//when it died
				if ( eventTime == EventTime.OnEnd || player ) { return; }

				BetterHit betterHit = new BetterHit {
					collisionInstance = collisionInstance,
					player = player,
					attackDirection = GetHitDirection(creature, collisionInstance),
					creature = creature,
					creatureStates = GetCreatureState(creature),
					damageArea = GetDamageArea(creature, collisionInstance),
					damageType = GetDamageType(collisionInstance),
					hitStates = GetHitStates(collisionInstance),
					penetrationType = GetPenetrationType(collisionInstance)
				};

				BetterEvents.InvokeCreatureKillEvent(betterHit);
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}
		}

		/// <summary>
		/// Called when a creature is hit
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="collisionInstance"></param>
		/// <remarks>A hit is different from a collision, many collisions can happen in a small time but will probably only be 1 hit</remarks>
		public virtual void OnCreatureHit( Creature creature, CollisionInstance collisionInstance ) {
			try {
				if ( creature == null ) {
					log.Debug().Message("Creature was null");
					return;
				}

				BetterHit betterHit = new BetterHit {
					collisionInstance = collisionInstance,
					attackDirection = GetHitDirection(creature, collisionInstance),
					creature = creature,
					creatureStates = GetCreatureState(creature),
					damageArea = GetDamageArea(creature, collisionInstance),
					damageType = GetDamageType(collisionInstance),
					hitStates = GetHitStates(collisionInstance),
					penetrationType = GetPenetrationType(collisionInstance)
				};
				BetterEvents.InvokeCreatureHitEvent(betterHit);
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
			}
		}


		/// <summary>
		/// The area that was hit, head/neck/body etc
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="collisionInstance"></param>
		/// <returns></returns>
		public virtual BetterEvents.DamageArea GetDamageArea( Creature creature, CollisionInstance collisionInstance ) {
			try {
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
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
				return BetterEvents.DamageArea.None;
			}
		}

		/// <summary>
		/// The type of damage, stabbing, blunt, energy, shock etc
		/// </summary>
		/// <param name="collisionInstance"></param>
		/// <returns></returns>
		public virtual DamageType GetDamageType( CollisionInstance collisionInstance ) {
			try {
				return collisionInstance.damageStruct.damageType;
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
				return DamageType.Unknown;
			}
		}

		/// <summary>
		/// The type of penetration
		/// </summary>
		/// <param name="collisionInstance"></param>
		/// <returns></returns>
		public virtual DamageStruct.Penetration GetPenetrationType( CollisionInstance collisionInstance ) {
			try {
				return collisionInstance.damageStruct.penetration;
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
				return DamageStruct.Penetration.None;
			}
		}
		/// <summary>
		/// Returns a collection of various states a hit could be
		/// </summary>
		/// <param name="collisionInstance"></param>
		/// <returns></returns>
		public virtual HashSet<BetterEvents.HitState> GetHitStates( CollisionInstance collisionInstance ) {
			try {
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

							if ( ch.ragdollPart.ragdoll.tkHandlers.Contains(spellCasterLeft) ||
								ch.ragdollPart.ragdoll.tkHandlers.Contains(spellCasterLeft) ) {
								//	Debug.LogFormat(Time.time + " Action - ragdoll part was telekinesis grabbed ");
								states.Add(BetterEvents.HitState.TelekinesisGrabbedRagdollPart);
							}

							if ( ch.ragdollPart.isSliced ) {
								//	Debug.LogFormat(Time.time + " Action - ragdoll part is dismembered ");
								//TODO: for some reason when the player is hit this is always true?
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

						if ( ch.item.IsPlayerTkHolding() ) {
							//Debug.LogFormat(Time.time + " Action - item is gripped with telekinesis");
							states.Add(BetterEvents.HitState.TelekinesisGrabbedItem);
						}

						if ( collisionInstance?.sourceColliderGroup?.imbue?.spellCastBase != null ) {
							states.Add(BetterEvents.HitState.ImbuedItem);
						}

						foreach ( Imbue imbue in collisionInstance.sourceColliderGroup.collisionHandler.item.imbues ) {
							if ( imbue?.spellCastBase != null ) {
								states.Add(BetterEvents.HitState.ImbuedItem);
								break;
							}

						}
					}
				}

				return states;
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
				return new HashSet<BetterEvents.HitState>();
			}
		}

		public virtual HashSet<BetterEvents.CreatureState> GetCreatureState( Creature creature ) {
			try {
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

				if ( creature.state == Creature.State.Destabilized &&
					(creature.fallState == Creature.FallState.NearGround ||
					 creature.fallState == Creature.FallState.StabilizedOnGround) ) {
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
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
				return new HashSet<BetterEvents.CreatureState>();
			}
		}


		/// <summary>
		/// Returns the direction a creature was hit, ie Direction.back is backstabbed
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="collisionInstance"></param>
		/// <returns></returns>
		public virtual BetterEvents.Direction GetHitDirection( Creature creature, CollisionInstance collisionInstance ) {
			try {
				//get impact direction
				Vector3 lhs = Utils.ClosestDirection(
					creature.transform.InverseTransformDirection(collisionInstance.impactVelocity).normalized,
					Cardinal.XZ);

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
			} catch ( Exception e ) {
				log.Exception().Message($"Exception Caught: {e.StackTrace}");
				return BetterEvents.Direction.None;
			}
		}

	}
}