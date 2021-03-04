using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnitQuadFactory
{
    [System.Serializable]
    public class UnitQuadDesc
    {
        [Header("Unit Quad Properties")]
        public Vector2Int numGrids;
    }

    public static Mesh CreateUnitQuad(UnitQuadDesc desc)
    {
        var numGrids = desc.numGrids;

        // create vertices
        int numVertices = (numGrids.x + 1) * (numGrids.y + 1);
        Vector3[] positions = new Vector3[numVertices];
        Vector2[] uvs = new Vector2[numVertices];
        Vector3[] normals = new Vector3[numVertices];

        Vector2 posStep = new Vector2(
            1.0f / numGrids.x,
            1.0f / numGrids.y
        );
        Vector2 uvStep = posStep;
        Vector2 posOffset = new Vector2(-0.5f, -0.5f);

        for(int vy = 0; vy <= numGrids.y; ++vy)
        {
            for(int vx = 0; vx <= numGrids.x; ++vx)
            {
                int vid = vy * (numGrids.x + 1) + vx;
                Vector2Int gridPos = new Vector2Int(vx, vy);
                Vector2 pos2 = gridPos * posStep + posOffset;
                positions[vid] = new Vector3(pos2.x, pos2.y, 0f);

                Vector2 uv = gridPos * uvStep;
                uvs[vid] = uv;

                normals[vid] = Vector3.forward;
            }
        }

        List<int> indices = new List<int>();

        for(int quadY = 0; quadY < numGrids.y; ++quadY)
        {
            for(int quadX = 0; quadX < numGrids.x; ++quadX)
            {
                int iLowerLeft = quadY * (numGrids.x + 1) + quadX;
                int iLowerRight = quadY * (numGrids.x + 1) + quadX + 1;
                int iUpperLeft = (quadY + 1) * (numGrids.x + 1) + quadX;
                int iUpperRight = (quadY + 1) * (numGrids.x + 1) + quadX + 1;

                indices.Add(iLowerLeft);
                indices.Add(iUpperLeft);
                indices.Add(iUpperRight);
                indices.Add(iLowerRight);
            }
        }

        Debug.Assert(indices.Count == 4 * numGrids.x * numGrids.y);

        Mesh mesh = new Mesh();
        mesh.name = string.Format("unit-quad-x-{0}-y{1}", numGrids.x, numGrids.y);

        mesh.vertices = positions;
        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.SetIndices(indices.ToArray(), MeshTopology.Quads, 0);

        return mesh;
    }
}
