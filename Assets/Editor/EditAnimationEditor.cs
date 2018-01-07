using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class EditAnimationEditor : EditorWindow
{
    private AnimationClip originalClip;

    // Store all the N curves for each bone of the clip
    private List<List<AnimationCurve>> curves;

    // Store the user defined coefficients for each level N
    private List<float> levelCoefficients;


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
        // comportera la copie de m_animationClip mais filtrer
        AnimationClip clip = new AnimationClip();

        // Copy the m_animationClip in the local variale clip
        clip.legacy = originalClip.legacy;
        float kernel = 0.3f;
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(originalClip))
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(originalClip, binding);
            for (int i = 1; i < curve.length - 1; i++)
            {
                float valueBefore = curve.keys[i - 1].value;
                float value = curve.keys[i].value;
                float valueAfter = curve.keys[i].value;

                float time = curve.keys[i].time;
                curve.MoveKey(i, new Keyframe(time, (valueBefore + value + valueAfter) * kernel));
            }
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }
        AssetDatabase.CreateAsset(clip, "Assets/Animations/Gaussian.anim");
    }
}