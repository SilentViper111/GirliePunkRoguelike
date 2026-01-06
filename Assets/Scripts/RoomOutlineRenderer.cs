using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates glowing outline lines around each room (hexagon/pentagon).
/// Creates a neon wireframe effect to clearly mark room boundaries.
/// 
/// Reference: User request for visible biome boundaries
/// </summary>
public class RoomOutlineRenderer : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] private float lineWidth = 0.3f;
    [SerializeField] private float heightOffset = 0.5f;
    [SerializeField] private bool useRoomColor = true;
    [SerializeField] private Color defaultColor = Color.magenta;
    
    [Header("Materials")]
    [SerializeField] private Material outlineMaterial;
    
    private List<LineRenderer> _lineRenderers = new List<LineRenderer>();
    private WorldGenerator _worldGenerator;
    
    /// <summary>
    /// Generates outline renderers for all rooms.
    /// </summary>
    public void GenerateOutlines(
        List<Vector3> roomCenters,
        List<int[]> pentagons,
        List<int[]> hexagons,
        List<Vector3> faceCenters,
        float worldRadius,
        List<RoomData> roomDataList = null)
    {
        ClearOutlines();
        
        // Create container
        GameObject container = new GameObject("RoomOutlines");
        container.transform.SetParent(transform);
        
        // Create material if not assigned
        if (outlineMaterial == null)
        {
            outlineMaterial = CreateGlowMaterial();
        }
        
        int roomIndex = 0;
        
        // Pentagon outlines
        foreach (var pentagon in pentagons)
        {
            Color color = defaultColor;
            if (useRoomColor && roomDataList != null && roomIndex < roomDataList.Count)
            {
                color = GetBiomeColor(roomDataList[roomIndex].biome);
            }
            
            CreateRoomOutline(roomCenters[roomIndex], pentagon, faceCenters, worldRadius, color, container.transform, roomIndex);
            roomIndex++;
        }
        
        // Hexagon outlines
        foreach (var hexagon in hexagons)
        {
            Color color = defaultColor;
            if (useRoomColor && roomDataList != null && roomIndex < roomDataList.Count)
            {
                color = GetBiomeColor(roomDataList[roomIndex].biome);
            }
            
            CreateRoomOutline(roomCenters[roomIndex], hexagon, faceCenters, worldRadius, color, container.transform, roomIndex);
            roomIndex++;
        }
        
        Debug.Log($"[RoomOutlineRenderer] Created {_lineRenderers.Count} room outlines");
    }
    
    private void CreateRoomOutline(Vector3 center, int[] faceIndices, List<Vector3> faceCenters, float worldRadius, Color color, Transform parent, int roomIndex)
    {
        // Get corners from face centers
        List<Vector3> corners = new List<Vector3>();
        foreach (int faceIdx in faceIndices)
        {
            if (faceIdx >= 0 && faceIdx < faceCenters.Count)
            {
                corners.Add(faceCenters[faceIdx]);
            }
        }
        
        if (corners.Count < 3) return;
        
        // Sort corners clockwise around room center
        Vector3 normal = center.normalized;
        corners = SortCornersClockwise(corners, center, normal);
        
        // Offset corners slightly above surface
        for (int i = 0; i < corners.Count; i++)
        {
            corners[i] = corners[i] + normal * heightOffset;
        }
        
        // Create line renderer
        GameObject lineObj = new GameObject($"Outline_Room{roomIndex}");
        lineObj.transform.SetParent(parent);
        
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = corners.Count + 1; // +1 to close the loop
        lr.loop = false;
        lr.useWorldSpace = true;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        
        // Set positions
        for (int i = 0; i < corners.Count; i++)
        {
            lr.SetPosition(i, corners[i]);
        }
        lr.SetPosition(corners.Count, corners[0]); // Close loop
        
        // Set material and color
        Material mat = new Material(outlineMaterial);
        mat.SetColor("_BaseColor", color);
        mat.SetColor("_EmissionColor", color * 2f);
        mat.EnableKeyword("_EMISSION");
        lr.material = mat;
        lr.startColor = color;
        lr.endColor = color;
        
        _lineRenderers.Add(lr);
    }
    
    private List<Vector3> SortCornersClockwise(List<Vector3> corners, Vector3 center, Vector3 normal)
    {
        // Project to tangent plane and sort by angle
        Vector3 up = normal;
        Vector3 right = Vector3.Cross(up, Vector3.forward).normalized;
        if (right.sqrMagnitude < 0.01f)
            right = Vector3.Cross(up, Vector3.right).normalized;
        Vector3 forward = Vector3.Cross(right, up).normalized;
        
        corners.Sort((a, b) => {
            Vector3 dirA = (a - center).normalized;
            Vector3 dirB = (b - center).normalized;
            float angleA = Mathf.Atan2(Vector3.Dot(dirA, forward), Vector3.Dot(dirA, right));
            float angleB = Mathf.Atan2(Vector3.Dot(dirB, forward), Vector3.Dot(dirB, right));
            return angleA.CompareTo(angleB);
        });
        
        return corners;
    }
    
    private Material CreateGlowMaterial()
    {
        // Create URP Unlit material with emission
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");
        
        Material mat = new Material(shader);
        mat.EnableKeyword("_EMISSION");
        return mat;
    }
    
    private Color GetBiomeColor(BiomeType biome)
    {
        switch (biome)
        {
            case BiomeType.NeonCity: return new Color(1f, 0f, 1f); // Magenta
            case BiomeType.CrystalLake: return new Color(0f, 0.7f, 1f); // Cyan
            case BiomeType.TechForest: return new Color(0f, 1f, 0.5f); // Green
            case BiomeType.LavaCore: return new Color(1f, 0.3f, 0f); // Orange
            case BiomeType.VoidZone: return new Color(0.5f, 0f, 1f); // Purple
            default: return Color.magenta;
        }
    }
    
    public void ClearOutlines()
    {
        foreach (var lr in _lineRenderers)
        {
            if (lr != null)
            {
                if (Application.isPlaying)
                    Destroy(lr.gameObject);
                else
                    DestroyImmediate(lr.gameObject);
            }
        }
        _lineRenderers.Clear();
        
        Transform container = transform.Find("RoomOutlines");
        if (container != null)
        {
            if (Application.isPlaying)
                Destroy(container.gameObject);
            else
                DestroyImmediate(container.gameObject);
        }
    }
}
