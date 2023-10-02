using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
	Responsible for rendering oceans and atmospheres as post processing effect
*/

[CreateAssetMenu (menuName = "PostProcessing/PlanetEffects")]
public class PlanetEffects: ScriptableObject {

	public Shader oceanShader;
	public Shader atmosphereShader;
	public bool displayOceans = true;
	public bool displayAtmospheres = true;

	List<EffectHolder> effectHolders = new();
	List<float> sortDistances = new();

	List<Material> postProcessingMaterials = new();
	bool active = true;

	public void Init () {
		if (effectHolders.Count == 0 || effectHolders[0].generator == null || !Application.isPlaying) {
			effectHolders.Clear();
			
			var generators = FindObjectsOfType<CelestialBodyGenerator> ();
			effectHolders.AddRange(generators.Select(x => new EffectHolder(x)));
		}
		
		sortDistances.Clear ();
		postProcessingMaterials.Clear ();
	}

	public List<Material> GetMaterials (bool closerThanBlackHole) {

		if (!active) {
			return null;
		}
		Init ();

		if (effectHolders.Count > 0) {
			Camera cam = Camera.current;
			Vector3 camPos = cam.transform.position;

			var activeHolders = effectHolders
				.Where(x => x.DstFromSurface(camPos) < camPos.magnitude ^ closerThanBlackHole)
				.OrderByDescending(x => x.DstFromSurface(camPos))
				.ToList();

			for (int i = 0; i < activeHolders.Count; i++) {
				EffectHolder effectHolder = activeHolders[i];
				Material underwaterMaterial = null;
				// Oceans
				if (displayOceans) {
					if (effectHolder.oceanEffect != null) {

						effectHolder.oceanEffect.UpdateSettings (effectHolder.generator, oceanShader);

						float camDstFromCentre = (camPos - effectHolder.generator.transform.position).magnitude;
						if (camDstFromCentre < effectHolder.generator.GetOceanRadius ()) {
							underwaterMaterial = effectHolder.oceanEffect.GetMaterial ();
						} else {
							postProcessingMaterials.Add (effectHolder.oceanEffect.GetMaterial ());
						}
					}
				}
				// Atmospheres
				if (displayAtmospheres) {
					if (effectHolder.atmosphereEffect != null) {
						effectHolder.atmosphereEffect.UpdateSettings (effectHolder.generator);
						postProcessingMaterials.Add (effectHolder.atmosphereEffect.GetMaterial ());
					}
				}

				if (underwaterMaterial != null) {
					postProcessingMaterials.Add (underwaterMaterial);
				}
			}
		}

		return postProcessingMaterials;
	}

	public class EffectHolder {
		public CelestialBodyGenerator generator;
		public OceanEffect oceanEffect;
		public AtmosphereEffect atmosphereEffect;

		public EffectHolder (CelestialBodyGenerator generator) {
			this.generator = generator;
			if (generator.body.shading.hasOcean && generator.body.shading.oceanSettings) {
				oceanEffect = new OceanEffect ();
			}
			if (generator.body.shading.hasAtmosphere && generator.body.shading.atmosphereSettings) {
				atmosphereEffect = new AtmosphereEffect ();
			}
		}

		public float DstFromSurface (Vector3 viewPos) {
			return Mathf.Max (0, (generator.transform.position - viewPos).magnitude - generator.BodyScale);
		}
	}
}