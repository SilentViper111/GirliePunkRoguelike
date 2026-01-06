using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates a smooth, continuous mesh from geodesic room data.
/// Each room (hexagon/pentagon) is triangulated from center → corners.
/// Room centers are kept flat, corners follow sphere surface for smooth transitions.
/// 
/// Reference: KB Section II.A (extended for mesh generation)
/// </summary>
public static class GeodesicMeshBuilder
{
    /// <summary>
    /// Builds a Unity Mesh from geodesic room data.
    /// </summary>
    /// <param name="roomCenters">Center positions of each room (vertices from geodesic sphere)</param>
    /// <param name="roomCornerIndices">For each room, indices into faceCenters that form its corners</param>
    /// <param name="faceCenters">Corner positions (centers of triangular faces)</param>
    /// <param name="flattenAmount">0 = full sphere curvature, 1 = completely flat rooms</param>
    /// <returns>Complete Unity Mesh with vertices, triangles, normals, and UVs</returns>
    public static Mesh BuildMesh(
        List<Vector3> roomCenters,
        List<int[]> pentagons,
        List<int[]> hexagons,
        List<Vector3> faceCenters,
        float flattenAmount = 0.7f)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Color> colors = new List<Color>();

        // Process all rooms (pentagons first, then hexagons)
        List<int[]> allRooms = new List<int[]>();
        allRooms.AddRange(pentagons);
        allRooms.AddRange(hexagons);

        for (int roomIdx = 0; roomIdx < allRooms.Count; roomIdx++)
        {
            int[] cornerIndices = allRooms[roomIdx];
            Vector3 roomCenter = roomCenters[roomIdx];
            Vector3 roomNormal = roomCenter.normalized;
            bool isPentagon = roomIdx < pentagons.Count;
            float roomRadius = roomCenter.magnitude;

            // Room center stays at sphere surface (no inward push)
            Vector3 flatCenter = roomCenter;

            // Get original corner positions
            List<Vector3> sphereCorners = new List<Vector3>();
            foreach (int faceIdx in cornerIndices)
            {
                if (faceIdx < faceCenters.Count)
                {
                    sphereCorners.Add(faceCenters[faceIdx]);
                }
            }

            if (sphereCorners.Count < 3) continue;

            // Project ALL corners onto the tangent plane at roomCenter
            // This creates a truly flat polygon
            List<Vector3> flatCorners = new List<Vector3>();
            foreach (Vector3 sphereCorner in sphereCorners)
            {
                Vector3 projected = ProjectToPlane(sphereCorner, roomCenter, roomNormal);
                flatCorners.Add(projected);
            }

            // Final corners are interpolated between sphere and flat based on flattenAmount
            List<Vector3> corners = new List<Vector3>();
            for (int i = 0; i < sphereCorners.Count; i++)
            {
                // flattenAmount = 0 → sphere surface
                // flattenAmount = 1 → completely flat
                Vector3 blended = Vector3.Lerp(sphereCorners[i], flatCorners[i], flattenAmount);
                corners.Add(blended);
            }

            // Create fan triangulation: center → corner[i] → corner[i+1]
            int centerVertexIdx = vertices.Count;
            vertices.Add(flatCenter);
            uvs.Add(new Vector2(0.5f, 0.5f)); // Center UV
            colors.Add(isPentagon ? new Color(1f, 0f, 1f, 1f) : new Color(0f, 1f, 1f, 1f));

            // Add corner vertices
            int firstCornerIdx = vertices.Count;
            for (int i = 0; i < corners.Count; i++)
            {
                vertices.Add(corners[i]);
                
                // UV: radial mapping
                float angle = (float)i / corners.Count * Mathf.PI * 2f;
                uvs.Add(new Vector2(0.5f + Mathf.Cos(angle) * 0.5f, 0.5f + Mathf.Sin(angle) * 0.5f));
                colors.Add(isPentagon ? new Color(1f, 0f, 1f, 1f) : new Color(0f, 1f, 1f, 1f));
            }

            // Create triangles (fan from center)
            for (int i = 0; i < corners.Count; i++)
            {
                int corner1 = firstCornerIdx + i;
                int corner2 = firstCornerIdx + ((i + 1) % corners.Count);
                
                // Winding order for outward-facing normals (from sphere center)
                triangles.Add(centerVertexIdx);
                triangles.Add(corner1);
                triangles.Add(corner2);
            }
        }

        // Create the mesh
        Mesh mesh = new Mesh();
        mesh.name = "GeodesicWorldMesh";
        
        // Handle large meshes (>65k vertices)
        if (vertices.Count > 65000)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(colors);
        
        // Calculate normals for smooth shading
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        Debug.Log($"[GeodesicMeshBuilder] Built mesh: {vertices.Count} vertices, {triangles.Count / 3} triangles");
        return mesh;
    }

    /// <summary>
    /// Projects a point onto a plane defined by a point and normal.
    /// </summary>
    private static Vector3 ProjectToPlane(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
    {
        float distance = Vector3.Dot(point - planePoint, planeNormal);
        return point - distance * planeNormal;
    }

    /// <summary>
    /// Creates a simple neon floor material for URP.
    /// </summary>
    public static Material CreateNeonFloorMaterial()
    {
        // Try to find URP Lit shader
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            urpLit = Shader.Find("Standard");
        }

        Material mat = new Material(urpLit);
        mat.name = "NeonFloor";
        
        // Dark purple base
        mat.SetColor("_BaseColor", new Color(0.1f, 0.04f, 0.18f, 1f));
        
        // Enable emission
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(1f, 0f, 1f, 1f) * 1.5f); // Magenta emission
        
        // High smoothness for reflections
        mat.SetFloat("_Smoothness", 0.85f);

        return mat;
    }
}
