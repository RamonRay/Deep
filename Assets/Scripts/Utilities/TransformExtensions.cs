// copied from: https://forum.unity.com/threads/how-to-assign-matrix4x4-to-transform.121966/
using UnityEngine;

public static class TransformExtensions
{
    public static void FromMatrix(this Transform transform, Matrix4x4 matrix)
    {
        transform.localScale = matrix.ExtractScale();
        transform.rotation = matrix.ExtractRotation();
        transform.position = matrix.ExtractPosition();
    }
}