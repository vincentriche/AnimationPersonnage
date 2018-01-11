using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class EditAnimationEditor : EditorWindow
{
    private AnimationClip originalClip;

    private List<AnimLevel> levels;

    private List<float> coefficients;

    private int N = 0;
    private bool multiresolutionEnable = false;

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

            // Multiresolution
            if (GUILayout.Button("Multiresolution Edition"))
                multiresolutionEnable = !multiresolutionEnable;

            if (multiresolutionEnable == true)
            {
                EditorGUI.BeginChangeCheck();
                N = EditorGUILayout.IntField("N", N);

                if (GUILayout.Button("Update Levels"))
                {
                    coefficients = new List<float>();
                    for (int i = 0; i < N; i++)
                        coefficients.Add(1.0f);
                }

                if (coefficients != null)
                {
                    for (int i = 0; i < coefficients.Count; i++)
                        coefficients[i] = EditorGUILayout.FloatField("Level " + i.ToString(), coefficients[i]);
                }

                if (GUILayout.Button("Setup"))
                    SetupMultiresolutionEdition();

                if (GUILayout.Button("Compute"))
                    MultiresolutionFilter();
            }

        }
        EditorGUILayout.EndVertical();
    }

    private void GaussianFilter()
    {
        // comportera la copie de m_animationClip mais filtrer
        AnimationClip clip = new AnimationClip();

        // Copy the m_animationClip in the local variale clip
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
        AssetDatabase.CreateAsset(clip, "Assets/Animations/1. Gaussian.anim");
    }

    private void SetupMultiresolutionEdition()
    {
        if (N == 0)
        {
            EditorGUILayout.HelpBox("Please add levels.", MessageType.Info);
            return;
        }

        levels = new List<AnimLevel>();
        for (int i = 0; i < N; i++)
            levels.Add(new AnimLevel());

        int bCounter = 0;
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(originalClip))
        {
            bCounter++;
            AnimationCurve curve = AnimationUtility.GetEditorCurve(originalClip, binding);
            levels[0].bInfo.Add(new AnimCurve(curve));
        }
        AnimLevel.bCounter = bCounter;
        levels[0].kCounter = levels[0].bInfo[0].curve.length;
        for (int i = 1; i < levels.Count; i++)
        {
            levels[i].bInfo.Clear();
            for (int j = 0; j < bCounter; j++)
                levels[i].bInfo.Add(new AnimCurve());
        }

        for (int i = 1; i < N; i++)
        {
            levels[i].kCounter = levels[i - 1].kCounter / 2;
            for (int j = 0; j < AnimLevel.bCounter; j++)
            {
                for (int k = 0; k + 1 < levels[i - 1].kCounter; k += 2)
                {
                    Keyframe key = new Keyframe();
                    Keyframe key1 = levels[i - 1].bInfo[j].curve[k];
                    Keyframe key2 = levels[i - 1].bInfo[j].curve[k + 1];

                    key.time = (key1.time + key2.time) / 2.0f;
                    key.value = (key1.value + key2.value) / 2.0f;

                    float d = Mathf.Abs(key.value - key1.value);

                    levels[i].bInfo[j].curve.AddKey(key);
                    levels[i].bInfo[j].differences.Add(d);
                }
            }
        }
    }

    private void MultiresolutionFilter()
    {
        for (int i = 1; i < N; i++)
        {
            for (int j = 0; j < AnimLevel.bCounter; j++)
            {
                for (int k = 0; k < levels[i].kCounter; k++)
                    levels[i].bInfo[j].differences[k] *= coefficients[i];
            }
        }

        for (int i = N - 1; i >= 1; i--)
        {
            for (int j = 0; j < AnimLevel.bCounter; j++)
            {

                for (int k = 0, index = 0; k < levels[i].kCounter; k++, index += 2)
                {
                    Keyframe key = levels[i].bInfo[j].curve[k];
                    Keyframe key1 = levels[i - 1].bInfo[j].curve[index];
                    Keyframe key2 = levels[i - 1].bInfo[j].curve[index + 1];

                    float d = levels[i].bInfo[j].differences[k];
                    if (key1.value > key2.value)
                    {
                        key1.value = key.value + d;
                        key2.value = key.value - d;
                    }
                    else
                    {
                        key1.value = key.value - d;
                        key2.value = key.value + d;
                    }

                    levels[i - 1].bInfo[j].curve.MoveKey(index, key1);
                    levels[i - 1].bInfo[j].curve.MoveKey(index + 1, key2);
                }
            }
        }


        AnimationClip clip = new AnimationClip();
        clip.legacy = originalClip.legacy;
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(originalClip);
        for (int i = 0; i < bindings.Length; i++)
            AnimationUtility.SetEditorCurve(clip, bindings[i], levels[0].bInfo[i].curve);
        AssetDatabase.CreateAsset(clip, "Assets/Animations/2. Multiresolution.anim");
    }
}

public class AnimLevel
{
    public static int bCounter;
    public int kCounter;
    public List<AnimCurve> bInfo = new List<AnimCurve>();
}

public class AnimCurve
{
    public AnimationCurve curve;
    public List<float> differences;

    public AnimCurve()
    {
        curve = new AnimationCurve();
        differences = new List<float>();
    }

    public AnimCurve(AnimationCurve c)
    {
        curve = c;
        differences = new List<float>();
    }
}