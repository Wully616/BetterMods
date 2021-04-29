using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Wully {
	public class RPUtils : MonoBehaviour {
		//Bit of reflection to get the SRD
		public static bool TryGetScriptableRendererData( out ScriptableRendererData scriptableRendererData ) {
			var pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset);
			FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
			scriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0];
			return scriptableRendererData != null;
		}

		public static int GetRendererIndex( string name ) {
			int idx = -1;
			if ( TryGetScriptableRendererData(out ScriptableRendererData scriptableRendererData) ) {
				for ( int i = 0; i < scriptableRendererData?.rendererFeatures.Count; i++ ) {
					if ( scriptableRendererData.rendererFeatures[i].name.Equals(name) ) {
						scriptableRendererData.rendererFeatures[i].SetActive(false);
						idx = i;
						break;
					}
				}
			}
			return idx;
		}
		public static bool TryGetFeature( string name, out ScriptableRendererFeature feature, out ScriptableRendererData scriptableRendererData ) {
			feature = null;

			if ( TryGetScriptableRendererData(out scriptableRendererData) ) {
				feature = scriptableRendererData.rendererFeatures.Where(( f ) => f.name == name).FirstOrDefault();
			}

			return feature != null && scriptableRendererData != null;
		}
	}
}
