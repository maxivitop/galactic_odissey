using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class StarRenderer : MonoBehaviour
{
    public int seed;
    public int numStars;
    public int numVertsPerStar = 5;
    public Vector2 sizeMinMax;
    [Range(1.01f, 20f)] public float sizeDropOff;
    [Range(-1, 1)] public float minZ;
    public float minBrightness;
    public float maxBrightness = 1;
    public Vector2 dstMinMax;
    [Range(0, 1)] public float farStarsRatio;
    public float farStarsDst;
    public Material mat;
    private Mesh mesh;

    public Gradient colourSpectrum;
    private Texture2D spectrum;
    private bool settingsUpdated;

    private void Start()
    {
        Init(true);
    }

    private void OnValidate()
    {
        settingsUpdated = true;
    }

    private void Update()
    {
        if (Application.isPlaying) return;
        Init(settingsUpdated);
        settingsUpdated = false;
    }

    private void Init(bool regenerateMesh)
    {
        if (regenerateMesh)
        {
            GenerateMesh();
        }

        TextureHelper.TextureFromGradient(colourSpectrum, 64, ref spectrum);
        mat.SetTexture("_Spectrum", spectrum);
    }

    private void GenerateMesh()
    {
        if (mesh)
        {
            mesh.Clear();
        }

        mesh = new Mesh();
        var tris = new List<int>();
        var verts = new List<Vector3>();
        var uvs = new List<Vector2>();

        Random.InitState(seed);
        for (var starIndex = 0; starIndex < numStars; starIndex++)
        {
            var dir = Random.onUnitSphere;
            if (dir.z < minZ) continue; // I don't need stars on the other side
            var (circleVerts, circleTris, circleUvs) = GenerateCircle(dir, verts.Count);
            verts.AddRange(circleVerts);
            tris.AddRange(circleTris);
            uvs.AddRange(circleUvs);
        }

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0, true);
        mesh.SetUVs(0, uvs);
        var meshRenderer = GetComponent<MeshRenderer>();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        meshRenderer.sharedMaterial = mat;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
    }

    private (Vector3[] verts, int[] tris, Vector2[] uvs) GenerateCircle(Vector3 dir,
        int indexOffset)
    {
        var dist = Random.value < farStarsRatio
            ? Random.Range(dstMinMax.x, dstMinMax.y)
            : farStarsDst;
        var size = Random.Range(sizeMinMax.x, sizeMinMax.y) * dist / Mathf.Log(
            sizeDropOff * dist
        );
        var brightness = Random.Range(minBrightness, maxBrightness);
        var spectrumT = Random.value;

        var axisA = Vector3.Cross(dir, Vector3.up).normalized;
        if (axisA == Vector3.zero)
        {
            axisA = Vector3.Cross(dir, Vector3.forward).normalized;
        }

        var axisB = Vector3.Cross(dir, axisA);
        var centre = dir * dist;

        var verts = new Vector3[numVertsPerStar + 1];
        var uvs = new Vector2[numVertsPerStar + 1];
        var tris = new int[numVertsPerStar * 3];

        verts[0] = centre;
        uvs[0] = new Vector2(brightness, spectrumT);

        for (var vertIndex = 0; vertIndex < numVertsPerStar; vertIndex++)
        {
            var currAngle = (vertIndex / (float)(numVertsPerStar)) * Mathf.PI * 2;
            var vert = centre +
                       (axisA * Mathf.Sin(currAngle) + axisB * Mathf.Cos(currAngle)) * size;
            verts[vertIndex + 1] = vert;
            uvs[vertIndex + 1] = new Vector2(0, spectrumT);

            if (vertIndex >= numVertsPerStar) continue;
            tris[vertIndex * 3 + 0] = 0 + indexOffset;
            tris[vertIndex * 3 + 1] = (vertIndex + 1) + indexOffset;
            tris[vertIndex * 3 + 2] = ((vertIndex + 1) % (numVertsPerStar) + 1) + indexOffset;
        }

        return (verts, tris, uvs);
    }
}