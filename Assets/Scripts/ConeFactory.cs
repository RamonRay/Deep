using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConeFactory
{
    [System.Serializable]
    public class ConeDesc
    {
        [Header("Cone Properties")]
        public int sideSections = 10;
	    public float radiusTop = 0f;
	    public float radiusBottom = 1f;
	    public float height = 1f;
        public int spans = 2;

        public bool useConeAngle = false;
        [Range(0f, 180f)]
        public float coneAngle = 45f;

        public bool includeBase = true;
    }

    public enum ConeType
    {
        Truncated,
        TopApex,
        BottomApex
    }


    public static Mesh CreateCone(ConeDesc desc)
    {
        var sideSections = desc.sideSections;
        var radiusTop = desc.radiusTop;
        var radiusBottom = desc.radiusBottom;
        var height = desc.height;
        var spans = desc.spans;
        var useConeAngle = desc.useConeAngle;
        var coneAngle = desc.coneAngle;

        var coneMesh = new Mesh();

        if(useConeAngle)
        {
            var angleRad = coneAngle * Mathf.Deg2Rad;
            radiusBottom = radiusTop + height * Mathf.Tan(angleRad);
        }


        string meshName = string.Format(
            "Cone {0}v{1}t{2}b{3}h",
            sideSections,
            radiusTop,
            radiusBottom,
            height
        );

        coneMesh.name = meshName;

        // modified from: http://wiki.unity3d.com/index.php/CreateCone
        var mesh = coneMesh;
        ConeType coneType = ConeType.Truncated;
        if(radiusTop == 0)
            coneType = ConeType.TopApex;
        else if(radiusBottom == 0)
            coneType = ConeType.BottomApex;
        else
            coneType = ConeType.Truncated;

        // create side vertices
        var numSlices = spans + 1;
        var numVerticesPerSlice = sideSections + 1;
        Vector3[] sidePositions = new Vector3[numSlices * numVerticesPerSlice];
        Vector3[] sideNormals = new Vector3[numSlices * numVerticesPerSlice];
        Vector2[] sideUvs = new Vector2[numSlices * numVerticesPerSlice];

        float slope = Mathf.Atan((radiusBottom - radiusTop) / height);
        float slopeSin = Mathf.Sin(slope);
        float slopeCos = Mathf.Cos(slope);

        var baseAnglePerSection = 2.0f * Mathf.PI / sideSections;
        for(int i = 0; i < numVerticesPerSlice; ++i)
        {
            float baseAngle = baseAnglePerSection * i;
            float baseAngleSin = Mathf.Sin(baseAngle);
            float baseAngleCos = Mathf.Cos(baseAngle);

            float baseHalfAngle = baseAnglePerSection * (i + 0.5f);
            float baseHalfAngleSin = Mathf.Sin(baseHalfAngle);
            float baseHalfAngleCos = Mathf.Cos(baseHalfAngle);

            var vNormal = new Vector3(
                slopeCos * baseAngleCos, slopeCos * baseAngleSin, -slopeSin
            );
            var vHalfNormal = new Vector3(
                slopeCos * baseHalfAngleCos, slopeCos * baseHalfAngleSin, -slopeSin
            );

            vNormal.Normalize();
            vHalfNormal.Normalize();

            for(int sliceId = 0; sliceId < numSlices; ++sliceId)
            {
                float t = (float)sliceId / (float)(numSlices - 1);
                var sliceRadius = Mathf.Lerp(radiusTop, radiusBottom, t);
                var sliceHeight = t * height;

                var vid = numVerticesPerSlice * sliceId + i;

                sidePositions[vid] =
                    new Vector3(sliceRadius * baseAngleCos, sliceRadius * baseAngleSin, sliceHeight);

                if(sliceId == 0 && coneType == ConeType.TopApex)
                {
                    sideNormals[vid] = vHalfNormal;
                }
                else if(sliceId == numSlices - 1 && coneType == ConeType.BottomApex)
                {
                    sideNormals[vid] = vHalfNormal;
                }
                else
                {
                    sideNormals[vid] = vNormal;
                }

                sideUvs[vid] = new Vector2(1.0f*i/(sideSections), 1.0f*sliceId/(numSlices - 1));
            }
        }

        List<int> sideTrianglesList = new List<int>();
        // create side triangles
        for(int sliceId = 0; sliceId < numSlices - 1; ++sliceId)
        {
            if(sliceId == 0 && coneType == ConeType.TopApex)
            {
                for(int triId = 0; triId < sideSections; ++triId)
                {
                    sideTrianglesList.Add(triId); // apex
                    sideTrianglesList.Add(
                        numVerticesPerSlice + triId + 1
                    );
                    sideTrianglesList.Add(numVerticesPerSlice + triId);
                }
            }
            else if(sliceId == numSlices - 2 && coneType == ConeType.BottomApex)
            {
                for(int triId = 0; triId < sideSections; ++triId)
                {
                    sideTrianglesList.Add((sliceId + 1) * numVerticesPerSlice + triId); // apex
                    sideTrianglesList.Add(
                        sliceId * numVerticesPerSlice + triId + 1
                    );
                    sideTrianglesList.Add(sliceId * numVerticesPerSlice + triId);
                }
            }
            else
            {
                for(int quadId = 0; quadId < sideSections; ++quadId)
                {
                    var topOffset = sliceId * numVerticesPerSlice;
                    var bottomOffset = (sliceId + 1) * numVerticesPerSlice;

                    var topVertexId0 = topOffset + quadId;
                    var topVertexId1 = topOffset + quadId + 1;

                    var bottomVertexId0 = bottomOffset + quadId;
                    var bottomVertexId1 = bottomOffset + quadId + 1;

                    sideTrianglesList.Add(topVertexId0);
                    sideTrianglesList.Add(topVertexId1);
                    sideTrianglesList.Add(bottomVertexId0);
                    sideTrianglesList.Add(topVertexId1);
                    sideTrianglesList.Add(bottomVertexId1);
                    sideTrianglesList.Add(bottomVertexId0);
                }
            }
        }


        var sideMesh = new Mesh();
        sideMesh.vertices = sidePositions;
        sideMesh.normals = sideNormals;
        sideMesh.uv = sideUvs;
        sideMesh.SetTriangles(sideTrianglesList, 0);
        sideMesh.RecalculateBounds();
        sideMesh.name = meshName + "#nobase";

        if(!desc.includeBase)
        {
            return sideMesh;
        }

        // create base vertices
        int numBaseVertices = coneType == ConeType.Truncated ? 2 * sideSections : sideSections;

        Vector3[] basePositions = new Vector3[numBaseVertices];
        Vector3[] baseNormals = new Vector3[numBaseVertices];
        Vector2[] baseUvs = new Vector2[numBaseVertices];

        var normal = coneType == ConeType.TopApex ? Vector3.forward : -Vector3.forward;
        var offset = coneType == ConeType.TopApex ? (numSlices - 1) * numVerticesPerSlice : 0;
        var radius = coneType == ConeType.TopApex ? radiusBottom : radiusTop;
        for(int i = 0; i < sideSections; ++i)
        {
            basePositions[i] = sidePositions[i + offset];
            baseNormals[i] = normal;
            // NOTE: hack uv
            baseUvs[i] = sidePositions[i + offset] / radius;
        }

        if(coneType == ConeType.Truncated)
        {
            for(int i = 0; i < sideSections; ++i)
            {
                var baseOffset = sideSections;
                var sideOffset = (numSlices - 1) * sideSections;

                basePositions[baseOffset + i] = sidePositions[sideOffset + i];
                baseNormals[baseOffset + i] = Vector3.forward;
                // NOTE: hack uv
                baseUvs[baseOffset + i] = sidePositions[sideOffset + i] / radiusBottom;
            }
        }

        int numTrianglesPerBase = sideSections - 2;
        int numBaseTriangles = coneType == ConeType.Truncated ? numTrianglesPerBase * 2 : numTrianglesPerBase;
        int[] baseTriangles = new int[numBaseTriangles * 3];

        for(int triId = 0; triId < numTrianglesPerBase; ++triId)
        {
            baseTriangles[3 * triId    ] = 0;
            baseTriangles[3 * triId + 1] = coneType == ConeType.TopApex ? triId + 1 : triId + 2;
            baseTriangles[3 * triId + 2] = coneType == ConeType.TopApex ? triId + 2 : triId + 1;
        }
        if(coneType == ConeType.Truncated)
        {
            for(int triId = numTrianglesPerBase; triId < numTrianglesPerBase * 2; ++triId)
            {
                baseTriangles[3 * triId    ] = sideSections;
                baseTriangles[3 * triId + 1] = sideSections + (triId - numTrianglesPerBase) + 1;
                baseTriangles[3 * triId + 2] = sideSections + (triId - numTrianglesPerBase) + 2;
            }
        }

        var baseMesh = new Mesh();
        baseMesh.vertices  = basePositions;
        baseMesh.normals   = baseNormals;
        baseMesh.uv        = baseUvs;
        baseMesh.triangles = baseTriangles;
        baseMesh.RecalculateBounds();

        CombineInstance sideCombine = new CombineInstance();
        sideCombine.mesh = sideMesh;
        sideCombine.transform = Matrix4x4.identity;

        CombineInstance baseCombine = new CombineInstance();
        baseCombine.mesh = baseMesh;
        baseCombine.transform = Matrix4x4.identity;

        mesh.CombineMeshes(new CombineInstance[] {sideCombine, baseCombine});
        mesh.RecalculateBounds();

        return mesh;
    }

}
