﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtmosphereEffect {

	Transform light;
	protected Material material;

	public void UpdateSettings (CelestialBodyGenerator generator) {

		Shader shader = generator.body.shading.atmosphereSettings.atmosphereShader;

		if (material == null || material.shader != shader) {
			material = new Material (shader);
		}

		if (light == null) {
			light = GameObject.FindWithTag("SunShadowCaster")?.transform;
		}

		//generator.shading.SetAtmosphereProperties (material);
		generator.body.shading.atmosphereSettings.SetProperties (material, generator.BodyScale);

		material.SetVector ("planetCentre", generator.transform.position);
		//material.SetFloat ("atmosphereRadius", (1 + 0.5f) * generator.BodyScale);
		material.SetFloat ("oceanRadius", generator.GetOceanRadius ());

		if (light) {
			Vector3 dirFromPlanetToSun = (light.transform.position - generator.transform.position).normalized;
			//Debug.Log(dirFromPlanetToSun);
			material.SetVector ("dirToSun", dirFromPlanetToSun);
		} else {
			material.SetVector ("dirToSun", Vector3.up);
			Debug.Log ("No SunShadowCaster found");
		}
	}

	public Material GetMaterial () {
		return material;
	}
}