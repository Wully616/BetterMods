using UnityEngine;
using UnityEngine.Rendering.Universal;
using Wully.Helpers;

namespace Wully.Render {


	[ExecuteInEditMode]
	public class CustomPass<T> : MonoBehaviour {
		public static BetterLogger log = BetterLogger.GetLogger(typeof(CustomPass<T>));

		public ScriptableRendererFeature feature;
		public ScriptableRendererData scriptableRendererData;
		public ForwardRendererData forwardRendererData;
		private string featureName;
		private void GetScriptableRenderer() {
			log.Info().Message($"Getting scriptable render data");
			//Grabs the scriptable Renderer data - this is the Assets/SDK/ForwardRenderer file basically
			if (RPUtils.TryGetScriptableRendererData(out scriptableRendererData)) {
				log.Info().Message($"Got scriptable render data");
				//cast it to a ForwardRendererData since we know B&S uses the forward renderer - it has some other settings on it like stencils/default layer mask etc
				forwardRendererData = (ForwardRendererData)this.scriptableRendererData;
			} else {
				log.Error().Message($"Failed to get scriptable render data");
			}
		}


		public virtual void Awake() {
			log.SetLoggingLevel(BetterLogger.LogLevel.Debug);
			GetScriptableRenderer();
		}
		public virtual void OnDisable() {
			DestroyFeature();
		}

		public virtual void UpdateSettings() {
			//Implementor should create a settings object for their feature, cast the SRF to their type and set the settings
			//overlayFeature.settings = settings;

			scriptableRendererData?.SetDirty();
		}

		public virtual void CreateFeature() {
			log.Info().Message($"Creating feature..");
			if ( feature != null ) {
				DestroyFeature();
			}

			//Create instance of our feature
			this.feature = (ScriptableRendererFeature) ScriptableObject.CreateInstance(typeof(T));
			if (feature) {
				log.Info().Message($"Created feature!");
				
				featureName = gameObject.name;
				this.feature.name = featureName;
				//Set it to disabled initially
				this.feature.SetActive(false);

				if (!scriptableRendererData) {
					GetScriptableRenderer();
				}
				UpdateSettings();
				//Add the feature to the render pipeline
				this.scriptableRendererData?.rendererFeatures?.Add(feature);
				//Mark SRD as dirty so it gets updated.
				this.scriptableRendererData?.SetDirty();

			} else {
				log.Error().Message($"Unable to create feature!");
			}



		}

		public virtual void DestroyFeature() {
			//Removes the render feature by searching for its type in the rendererFeatures list.
			//This is why we inherit Blit as our own class instead of using Blit directly
			//Because if we did we could remove the wrong renderer feature
			log.Info().Message("Attempting to destroy feature: " + featureName);
			feature?.SetActive(false);
			int idx = RPUtils.GetRendererIndex(featureName);
			if ( idx >= 0 ) {
				log.Info().Message("Destroying feature: " + featureName);
				scriptableRendererData?.rendererFeatures.RemoveAt(idx);
			} else {
				log.Error().Message("Could not find feature to destroy feature: " + featureName);
			}
		}
		public virtual void DisableFeature() {
			//sets the forward renderers default layermask to render ragdoll layer
			//forwardRendererData.opaqueLayerMask = forwardRendererData.opaqueLayerMask | (1 << 13); //  render ragdoll layer;
			log.Info().Message("Attempting to disable feature: " + featureName);
			feature?.SetActive(false);

			scriptableRendererData?.SetDirty();

		}
		public virtual void EnableFeature() {
			//sets the forward renderers default layermask to NOT render ragdoll layer
			//forwardRendererData.opaqueLayerMask = forwardRendererData.opaqueLayerMask & ~(1 << 13); // dont render ragdoll layer;
			log.Info().Message("Attempting to enable feature: " + featureName);
			if ( feature == null ) {
				CreateFeature();
			} else {
				//check if the  renderer needs to be added
				int idx = RPUtils.GetRendererIndex(featureName);
				if ( idx == -1 ) {
					log.Info().Message("Adding feature: " + featureName);
					scriptableRendererData?.rendererFeatures?.Add(feature);
				}
			}
			UpdateSettings();
			feature?.SetActive(true);
			
			
		}

		public virtual void OnDestroy() {
			DestroyFeature();

		}


		public virtual void OnValidate() {
			//refresh the feature when a value in the inspector changes
			UpdateSettings();
		}
	}

	
	

}