using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class EditAnimationEditor : EditorWindow
{
    private AnimationClip originalClip;
    
    // The new item in the menu
    [MenuItem("Window/Edit Animation", false, 2000)]
    public static void DoWindow()
    {
        EditAnimationEditor window = GetWindow<EditAnimationEditor>();
        window.Show();
    }

    public void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        originalClip = EditorGUILayout.ObjectField("Current Animation Clip", originalClip, typeof(AnimationClip), false) as AnimationClip;

        if (originalClip != null)
        {
            // Gaussian
            if (GUILayout.Button("Gaussian Filter"))
                GaussianFilter();
        }
        EditorGUILayout.EndVertical();
    }

    private void GaussianFilter()
    {
        AnimationClip clip = new AnimationClip();
        
        clip.legacy = originalClip.legacy;
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(originalClip))
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(originalClip, binding);
            for (int i = 1; i < curve.length - 1; i++)
            {
                float valueBefore = curve.keys[i - 1].value;
                float value = curve.keys[i].value;
                float valueAfter = curve.keys[i].value;

                float time = curve.keys[i].time;
                curve.MoveKey(i, new Keyframe(time, (valueBefore + value + valueAfter) * 0.33f));
            }
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }
        AssetDatabase.CreateAsset(clip, "Assets/Animations/Gaussian.anim");
    }    
}