using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generates the vertices and faces of a Truncated Icosahedron (soccer ball shape).
/// This forms the basis of the "Cyber-Sphere" world map with 32 rooms:
/// - 12 Pentagons (VIP/Special Rooms)
/// - 20 Hexagons (Combat Rooms)
/// Reference: Knowledge Base Section II.A
/// </summary>
public static class TruncatedIcosahedronGenerator
{
    // Cached data for face generation
    private static List<Vector3> _icosaVerts;
    private static int[][] _icosaFaces;
    private static Dictionary<(int, int), (Vector3, Vector3)> _edgePoints;
    private static Dictionary<Vector3, int> _vertIndex;

    /// <summary>
    /// Generates the 60 vertices of a Truncated Icosahedron.
    /// </summary>
    /// <param name="radius">Scale factor for the polyhedron</param>
    /// <returns>List of 60 Vector3 vertices</returns>
    public static List<Vector3> GenerateVertices(float radius = 1f)
    {
        float phi = (1f + Mathf.Sqrt(5f)) / 2f; // Golden Ratio ≈ 1.618

        // Base icosahedron vertices (12 vertices, normalized to unit sphere)
        _icosaVerts = new List<Vector3>
        {
            new Vector3(-1, phi, 0).normalized, new Vector3(1, phi, 0).normalized,
            new Vector3(-1, -phi, 0).normalized, new Vector3(1, -phi, 0).normalized,
            new Vector3(0, -1, phi).normalized, new Vector3(0, 1, phi).normalized,
            new Vector3(0, -1, -phi).normalized, new Vector3(0, 1, -phi).normalized,
            new Vector3(phi, 0, -1).normalized, new Vector3(phi, 0, 1).normalized,
            new Vector3(-phi, 0, -1).normalized, new Vector3(-phi, 0, 1).normalized
        };

        // Icosahedron faces (20 triangles)
        _icosaFaces = new int[][]
        {
            new int[] {0, 11, 5}, new int[] {0, 5, 1}, new int[] {0, 1, 7}, new int[] {0, 7, 10}, new int[] {0, 10, 11},
            new int[] {1, 5, 9}, new int[] {5, 11, 4}, new int[] {11, 10, 2}, new int[] {10, 7, 6}, new int[] {7, 1, 8},
            new int[] {3, 9, 4}, new int[] {3, 4, 2}, new int[] {3, 2, 6}, new int[] {3, 6, 8}, new int[] {3, 8, 9},
            new int[] {4, 9, 5}, new int[] {2, 4, 11}, new int[] {6, 2, 10}, new int[] {8, 6, 7}, new int[] {9, 8, 1}
        };

        // Generate edge points (truncation at 1/3 marks)
        _edgePoints = new Dictionary<(int, int), (Vector3, Vector3)>();
        for (int i = 0; i < _icosaFaces.Length; i++)
        {
            int a = _icosaFaces[i][0];
            int b = _icosaFaces[i][1];
            int c = _icosaFaces[i][2];
            AddEdgePoints(a, b);
            AddEdgePoints(b, c);
            AddEdgePoints(c, a);
        }

        // Collect unique vertices (60 total)
        List<Vector3> truncatedVerts = new List<Vector3>();
        _vertIndex = new Dictionary<Vector3, int>(new Vector3Comparer());
        int index = 0;
        foreach (var pts in _edgePoints.Values)
        {
            AddUniqueVert(pts.Item1, truncatedVerts, ref index);
            AddUniqueVert(pts.Item2, truncatedVerts, ref index);
        }

        // Scale to desired radius
        for (int i = 0; i < truncatedVerts.Count; i++)
        {
            truncatedVerts[i] *= radius;
        }

        Debug.Log($"[TruncatedIcosahedron] Generated {truncatedVerts.Count} vertices (expected: 60)");
        return truncatedVerts;
    }

    /// <summary>
    /// Generates the faces of the Truncated Icosahedron.
    /// Must be called after GenerateVertices().
    /// </summary>
    /// <returns>Tuple of (12 pentagon faces, 20 hexagon faces)</returns>
    public static (List<int[]> pentagons, List<int[]> hexagons) GenerateFaces()
    {
        if (_icosaVerts == null || _edgePoints == null || _vertIndex == null)
        {
            Debug.LogError("[TruncatedIcosahedron] Must call GenerateVertices() before GenerateFaces()!");
            return (new List<int[]>(), new List<int[]>());
        }

        // Build adjacency for base icosahedron
        List<int>[] adjacency = new List<int>[12];
        for (int i = 0; i < adjacency.Length; i++) adjacency[i] = new List<int>();
        
        for (int i = 0; i < _icosaFaces.Length; i++)
        {
            int a = _icosaFaces[i][0], b = _icosaFaces[i][1], c = _icosaFaces[i][2];
            if (!adjacency[a].Contains(b)) adjacency[a].Add(b);
            if (!adjacency[a].Contains(c)) adjacency[a].Add(c);
            if (!adjacency[b].Contains(a)) adjacency[b].Add(a);
            if (!adjacency[b].Contains(c)) adjacency[b].Add(c);
            if (!adjacency[c].Contains(a)) adjacency[c].Add(a);
            if (!adjacency[c].Contains(b)) adjacency[c].Add(b);
        }

        // Generate 12 Pentagon faces (from original icosahedron vertices)
        List<int[]> pentagons = new List<int[]>();
        for (int v = 0; v < 12; v++)
        {
            List<Vector3> pentPoints = new List<Vector3>();
            foreach (int w in adjacency[v])
            {
                int min = Mathf.Min(v, w), max = Mathf.Max(v, w);
                var points = _edgePoints[(min, max)];
                Vector3 nearV = (v == min) ? points.Item1 : points.Item2;
                pentPoints.Add(nearV);
            }

            // Order points clockwise around the face center
            Vector3 center = pentPoints.Aggregate(Vector3.zero, (sum, p) => sum + p) / 5f;
            Vector3 normal = center.normalized;
            Vector3 refDir = (pentPoints[0] - center).normalized;
            Vector3 tan1 = Vector3.Cross(normal, refDir).normalized;
            Vector3 tan2 = Vector3.Cross(normal, tan1);
            
            List<(float angle, int idx)> sorted = new List<(float, int)>();
            for (int i = 0; i < pentPoints.Count; i++)
            {
                Vector3 dir = (pentPoints[i] - center).normalized;
                float x = Vector3.Dot(dir, tan1);
                float y = Vector3.Dot(dir, tan2);
                float angle = Mathf.Atan2(y, x);
                sorted.Add((angle, i));
            }
            sorted.Sort((s1, s2) => s1.angle.CompareTo(s2.angle));
            
            int[] face = new int[5];
            for (int i = 0; i < 5; i++)
            {
                face[i] = _vertIndex[pentPoints[sorted[i].idx]];
            }
            pentagons.Add(face);
        }

        // Generate 20 Hexagon faces (from original icosahedron faces)
        List<int[]> hexagons = new List<int[]>();
        for (int f = 0; f < _icosaFaces.Length; f++)
        {
            int a = _icosaFaces[f][0], b = _icosaFaces[f][1], c = _icosaFaces[f][2];
            
            var abPoints = _edgePoints[(Mathf.Min(a, b), Mathf.Max(a, b))];
            Vector3 p_ab_nearA = (a < b) ? abPoints.Item1 : abPoints.Item2;
            Vector3 p_ab_nearB = (b < a) ? abPoints.Item1 : abPoints.Item2;
            
            var bcPoints = _edgePoints[(Mathf.Min(b, c), Mathf.Max(b, c))];
            Vector3 p_bc_nearB = (b < c) ? bcPoints.Item1 : bcPoints.Item2;
            Vector3 p_bc_nearC = (c < b) ? bcPoints.Item1 : bcPoints.Item2;
            
            var caPoints = _edgePoints[(Mathf.Min(c, a), Mathf.Max(c, a))];
            Vector3 p_ca_nearC = (c < a) ? caPoints.Item1 : caPoints.Item2;
            Vector3 p_ca_nearA = (a < c) ? caPoints.Item1 : caPoints.Item2;
            
            // Order: nearA_ab, nearA_ca, nearC_ca, nearC_bc, nearB_bc, nearB_ab
            Vector3[] hexPoints = { p_ab_nearA, p_ca_nearA, p_ca_nearC, p_bc_nearC, p_bc_nearB, p_ab_nearB };
            int[] face = new int[6];
            for (int i = 0; i < 6; i++)
            {
                face[i] = _vertIndex[hexPoints[i]];
            }
            hexagons.Add(face);
        }

        Debug.Log($"[TruncatedIcosahedron] Generated {pentagons.Count} pentagons + {hexagons.Count} hexagons (expected: 12 + 20)");
        return (pentagons, hexagons);
    }

    /// <summary>
    /// Builds adjacency graph for room navigation (32 rooms total).
    /// </summary>
    public static List<int>[] BuildRoomAdjacency(List<int[]> pentagons, List<int[]> hexagons)
    {
        int totalRooms = pentagons.Count + hexagons.Count; // 32
        List<int>[] roomAdjacency = new List<int>[totalRooms];
        for (int i = 0; i < totalRooms; i++) roomAdjacency[i] = new List<int>();

        // Combine all faces: 0-11 = pentagons, 12-31 = hexagons
        List<int[]> allFaces = new List<int[]>();
        allFaces.AddRange(pentagons);
        allFaces.AddRange(hexagons);

        // Two rooms are adjacent if they share an edge (2 vertices)
        for (int i = 0; i < allFaces.Count; i++)
        {
            for (int j = i + 1; j < allFaces.Count; j++)
            {
                int sharedVerts = allFaces[i].Intersect(allFaces[j]).Count();
                if (sharedVerts >= 2)
                {
                    roomAdjacency[i].Add(j);
                    roomAdjacency[j].Add(i);
                }
            }
        }

        return roomAdjacency;
    }

    private static void AddEdgePoints(int a, int b)
    {
        int min = Mathf.Min(a, b), max = Mathf.Max(a, b);
        var key = (min, max);
        if (!_edgePoints.ContainsKey(key))
        {
            Vector3 vMin = _icosaVerts[min];
            Vector3 vMax = _icosaVerts[max];
            Vector3 p1 = Vector3.Lerp(vMin, vMax, 1f / 3f); // near min
            Vector3 p2 = Vector3.Lerp(vMin, vMax, 2f / 3f); // near max
            _edgePoints[key] = (p1, p2);
        }
    }

    private static void AddUniqueVert(Vector3 v, List<Vector3> list, ref int index)
    {
        if (!_vertIndex.ContainsKey(v))
        {
            _vertIndex[v] = index++;
            list.Add(v);
        }
    }
}

/// <summary>
/// Vector3 comparer for floating-point precision handling.
/// </summary>
public class Vector3Comparer : IEqualityComparer<Vector3>
{
    private const float Epsilon = 1e-5f;
    
    public bool Equals(Vector3 a, Vector3 b) => Vector3.Distance(a, b) < Epsilon;
    public int GetHashCode(Vector3 v)
    {
        // Round to avoid floating-point hash collisions
        int x = Mathf.RoundToInt(v.x * 1000);
        int y = Mathf.RoundToInt(v.y * 1000);
        int z = Mathf.RoundToInt(v.z * 1000);
        return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
    }
}
