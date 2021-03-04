using UnityEngine;
using UnityEditor;
using System;

public static class EditorUtilites
{
    public static void SafeEditorCall(Action action)
    {
        if(action != null)
        {
#if UNITY_EDITOR
            EditorApplication.delayCall += () => action();
#else
            action();
#endif
        }
    }

    public static bool willChangePlaymode
    {
        get
        {
#if UNITY_EDITOR
            return !EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode;
#else
            return false;
#endif
        }
    }
}