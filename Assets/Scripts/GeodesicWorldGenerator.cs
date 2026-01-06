using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generates a subdivided geodesic polyhedron based on icosahedron.
/// Allows scaling complexity while maintaining exactly 12 pentagon (VIP) rooms.
/// 
/// Mathematical basis:
/// - Base icosahedron: 12 vertices, 20 faces, 30 edges
/// - Each subdivision level quadruples the face count
/// - Pentagons always remain at 12 (topological constraint from Euler's formula)
/// 
/// Subdivision Level | Vertices | Faces | Hexagons | Pentagons
/// 0 (base)          | 12       | 20    | 0        | 12 (triangles become pentagons at vertices)
/// 1                 | 42       | 80    | ~68      | 12
/// 2                 | 162      | 320   | ~308     | 12
/// 
/// Reference: KB Section II.A (extended for larger worlds)
/// </summary>
public static class GeodesicWorldGenerator
{
    private static List<Vector3> _vertices;
    private static List<int[]> _triangles;
    private static Dictionary<long, int> _middlePointCache;

    /// <summary>
    /// Generates a geodesic sphere with specified subdivision level.
    /// </summary>
    /// <param name="subdivisions">0 = 20 faces, 1 = 80 faces, 2 = 320 faces, etc.</param>
    /// <param name="radius">World radius</param>
    /// <returns>Tuple of (vertices, triangular faces)</returns>
    public static (List<Vector3> vertices, List<int[]> faces) GenerateGeodesicSphere(int subdivisions, float radius = 1f)
    {
        _vertices = new List<Vector3>();
        _triangles = new List<int[]>();
        _middlePointCache = new Dictionary<long, int>();

        // Golden ratio for icosahedron construction
        float t = (1f + Mathf.Sqrt(5f)) / 2f;

        // Create 12 vertices of icosahedron
        AddVertex(new Vector3(-1, t, 0));
        AddVertex(new Vector3(1, t, 0));
        AddVertex(new Vector3(-1, -t, 0));
        AddVertex(new Vector3(1, -t, 0));

        AddVertex(new Vector3(0, -1, t));
        AddVertex(new Vector3(0, 1, t));
        AddVertex(new Vector3(0, -1, -t));
        AddVertex(new Vector3(0, 1, -t));

        AddVertex(new Vector3(t, 0, -1));
        AddVertex(new Vector3(t, 0, 1));
        AddVertex(new Vector3(-t, 0, -1));
        AddVertex(new Vector3(-t, 0, 1));

        // Create 20 triangular faces of icosahedron
        _triangles.Add(new int[] { 0, 11, 5 });
        _triangles.Add(new int[] { 0, 5, 1 });
        _triangles.Add(new int[] { 0, 1, 7 });
        _triangles.Add(new int[] { 0, 7, 10 });
        _triangles.Add(new int[] { 0, 10, 11 });

        _triangles.Add(new int[] { 1, 5, 9 });
        _triangles.Add(new int[] { 5, 11, 4 });
        _triangles.Add(new int[] { 11, 10, 2 });
        _triangles.Add(new int[] { 10, 7, 6 });
        _triangles.Add(new int[] { 7, 1, 8 });

        _triangles.Add(new int[] { 3, 9, 4 });
        _triangles.Add(new int[] { 3, 4, 2 });
        _triangles.Add(new int[] { 3, 2, 6 });
        _triangles.Add(new int[] { 3, 6, 8 });
        _triangles.Add(new int[] { 3, 8, 9 });

        _triangles.Add(new int[] { 4, 9, 5 });
        _triangles.Add(new int[] { 2, 4, 11 });
        _triangles.Add(new int[] { 6, 2, 10 });
        _triangles.Add(new int[] { 8, 6, 7 });
        _triangles.Add(new int[] { 9, 8, 1 });

        // Subdivide
        for (int i = 0; i < subdivisions; i++)
        {
            var newTriangles = new List<int[]>();
            foreach (var tri in _triangles)
            {
                // Get midpoints of each edge
                int a = GetMiddlePoint(tri[0], tri[1]);
                int b = GetMiddlePoint(tri[1], tri[2]);
                int c = GetMiddlePoint(tri[2], tri[0]);

                // Create 4 new triangles from 1
                newTriangles.Add(new int[] { tri[0], a, c });
                newTriangles.Add(new int[] { tri[1], b, a });
                newTriangles.Add(new int[] { tri[2], c, b });
                newTriangles.Add(new int[] { a, b, c });
            }
            _triangles = newTriangles;
        }

        // Scale to radius
        for (int i = 0; i < _vertices.Count; i++)
        {
            _vertices[i] *= radius;
        }

        Debug.Log($"[GeodesicWorld] Subdivisions: {subdivisions}, Vertices: {_vertices.Count}, Faces: {_triangles.Count}");
        return (_vertices, _triangles);
    }

    /// <summary>
    /// Converts triangular faces to hexagonal/pentagonal rooms using dual graph.
    /// Each original vertex becomes a room (pentagon for original 12, hexagon for others).
    /// </summary>
    public static (List<int[]> pentagons, List<int[]> hexagons, List<Vector3> roomCenters) 
        ConvertToHexagonalRooms(List<Vector3> vertices, List<int[]> triangles)
    {
        // Build vertex-to-faces adjacency
        Dictionary<int, List<int>> vertexFaces = new Dictionary<int, List<int>>();
        for (int i = 0; i < vertices.Count; i++)
            vertexFaces[i] = new List<int>();

        for (int f = 0; f < triangles.Count; f++)
        {
            foreach (int v in triangles[f])
            {
                vertexFaces[v].Add(f);
            }
        }

        // Calculate face centers
        List<Vector3> faceCenters = new List<Vector3>();
        foreach (var tri in triangles)
        {
            Vector3 center = (vertices[tri[0]] + vertices[tri[1]] + vertices[tri[2]]) / 3f;
            faceCenters.Add(center.normalized * vertices[0].magnitude); // Project to sphere
        }

        // Each vertex becomes a room (dual graph)
        List<int[]> pentagons = new List<int[]>();
        List<int[]> hexagons = new List<int[]>();
        List<Vector3> roomCenters = new List<Vector3>();

        for (int v = 0; v < vertices.Count; v++)
        {
            List<int> adjacentFaces = vertexFaces[v];
            int[] room = adjacentFaces.ToArray();

            // Order faces around the vertex (clockwise)
            room = OrderFacesAroundVertex(vertices[v], adjacentFaces, faceCenters);

            roomCenters.Add(vertices[v]);

            // Original 12 icosahedron vertices have 5 adjacent faces (pentagons)
            // All others have 6 (hexagons)
            if (adjacentFaces.Count == 5)
            {
                pentagons.Add(room);
            }
            else
            {
                hexagons.Add(room);
            }
        }

        Debug.Log($"[GeodesicWorld] Rooms: {pentagons.Count} pentagons (VIP) + {hexagons.Count} hexagons (Combat)");
        return (pentagons, hexagons, roomCenters);
    }

    /// <summary>
    /// Builds room adjacency graph (two rooms are adjacent if they share a face).
    /// </summary>
    public static List<int>[] BuildRoomAdjacency(int roomCount, List<int[]> triangles, Dictionary<int, List<int>> vertexFaces)
    {
        List<int>[] adjacency = new List<int>[roomCount];
        for (int i = 0; i < roomCount; i++)
            adjacency[i] = new List<int>();

        // Two vertices (rooms) are adjacent if they share an edge (appear in same triangle)
        foreach (var tri in triangles)
        {
            // Each pair of vertices in a triangle are adjacent rooms
            AddAdjacency(adjacency, tri[0], tri[1]);
            AddAdjacency(adjacency, tri[1], tri[2]);
            AddAdjacency(adjacency, tri[2], tri[0]);
        }

        return adjacency;
    }

    private static void AddAdjacency(List<int>[] adj, int a, int b)
    {
        if (!adj[a].Contains(b)) adj[a].Add(b);
        if (!adj[b].Contains(a)) adj[b].Add(a);
    }

    private static int AddVertex(Vector3 p)
    {
        _vertices.Add(p.normalized);
        return _vertices.Count - 1;
    }

    private static int GetMiddlePoint(int p1, int p2)
    {
        // Check cache
        long smallerIndex = Mathf.Min(p1, p2);
        long greaterIndex = Mathf.Max(p1, p2);
        long key = (smallerIndex << 32) + greaterIndex;

        if (_middlePointCache.TryGetValue(key, out int ret))
            return ret;

        // Create new vertex at midpoint, projected to sphere
        Vector3 middle = (_vertices[p1] + _vertices[p2]) / 2f;
        int i = AddVertex(middle);

        _middlePointCache[key] = i;
        return i;
    }

    private static int[] OrderFacesAroundVertex(Vector3 vertex, List<int> faces, List<Vector3> faceCenters)
    {
        if (faces.Count == 0) return new int[0];

        Vector3 normal = vertex.normalized;
        Vector3 refDir = (faceCenters[faces[0]] - vertex).normalized;
        Vector3 tan1 = Vector3.Cross(normal, refDir).normalized;
        Vector3 tan2 = Vector3.Cross(normal, tan1);

        var ordered = faces.Select(f =>
        {
            Vector3 dir = (faceCenters[f] - vertex).normalized;
            float angle = Mathf.Atan2(Vector3.Dot(dir, tan2), Vector3.Dot(dir, tan1));
            return (face: f, angle: angle);
        }).OrderBy(x => x.angle).Select(x => x.face).ToArray();

        return ordered;
    }
}
