using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PostProcessingEffect : ScriptableObject {

	public abstract void Render (RenderTexture source, RenderTexture destination);
}