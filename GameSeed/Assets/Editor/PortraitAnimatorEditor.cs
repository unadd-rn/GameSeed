// ═══════════════════════════════════════════════════════════════════════════
//  PortraitAnimatorEditor.cs
//  Place this file inside any folder named  Editor/  in your project.
//  It will NOT be included in builds — Unity strips Editor/ folders.
//
//  What it does:
//    Each PortraitEventAnimation has six animation type blocks (Slide, Wipe…).
//    This editor hides every block EXCEPT the one matching the chosen
//    AnimationType dropdown, keeping the Inspector readable even with many
//    portraits and events.
// ═══════════════════════════════════════════════════════════════════════════
// ═══════════════════════════════════════════════════════════════════════════
//  PortraitAnimatorEditor.cs
//  Place this file inside any folder named Editor/ in your project.
// ═══════════════════════════════════════════════════════════════════════════

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PortraitAnimator))]
public class PortraitAnimatorEditor : Editor
{
    private readonly Dictionary<string, bool> _open = new Dictionary<string, bool>();

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Header("Toggle Button (optional)");
        Prop("toggleButton");
        Prop("toggleEventName");

        EditorGUILayout.Space(6);

        Header("Portraits (Sequence Order)");
        var listProp = serializedObject.FindProperty("portraits");

        for (int pi = 0; pi < listProp.arraySize; pi++)
        {
            var pProp = listProp.GetArrayElementAtIndex(pi);
            
            // Track if an element was moved to skip drawing inconsistencies in the same frame
            if (DrawPortrait(listProp, pProp, pi))
            {
                break; 
            }
        }

        if (GUILayout.Button("＋  Add New Portrait to Queue"))
        {
            listProp.InsertArrayElementAtIndex(listProp.arraySize);
            var newP = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
            newP.FindPropertyRelative("label").stringValue = "Portrait";
        }

        EditorGUILayout.Space(8);
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset All"))
                ((PortraitAnimator)target).ResetAll();
            if (GUILayout.Button("Warm Up Shaders"))
                ((PortraitAnimator)target).WarmUpShaders();
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Draws the portrait block. Returns true if the list hierarchy shifted (moved/deleted).
    /// </summary>
    private bool DrawPortrait(SerializedProperty listProp, SerializedProperty pProp, int pi)
    {
        string pLabel = pProp.FindPropertyRelative("label").stringValue;
        if (string.IsNullOrEmpty(pLabel)) pLabel = $"Portrait {pi}";

        string pKey  = $"p{pi}";
        
        // Horizontal group to put reorder buttons right next to the foldout header
        EditorGUILayout.BeginHorizontal();
        bool pOpen = Fold(pKey, pLabel, defaultOpen: true);
        
        GUILayout.FlexibleSpace();
        
        // Reordering Queue Controls
        EditorGUI.BeginDisabledGroup(pi == 0);
        if (GUILayout.Button("▲ Up", EditorStyles.miniButtonLeft, GUILayout.Width(45)))
        {
            listProp.MoveArrayElement(pi, pi - 1);
            EditorGUILayout.EndHorizontal();
            return true;
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(pi == listProp.arraySize - 1);
        if (GUILayout.Button("▼ Down", EditorStyles.miniButtonRight, GUILayout.Width(45)))
        {
            listProp.MoveArrayElement(pi, pi + 1);
            EditorGUILayout.EndHorizontal();
            return true;
        }
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.EndHorizontal();

        if (!pOpen) 
        { 
            EditorGUILayout.Space(2); 
            return false; 
        }

        EditorGUI.indentLevel++;

        PropOf(pProp, "label");
        PropOf(pProp, "uiPortrait");

        EditorGUILayout.Space(4);
        MiniLabel("Events");

        var eventsProp = pProp.FindPropertyRelative("events");
        for (int ei = 0; ei < eventsProp.arraySize; ei++)
        {
            DrawEvent(eventsProp.GetArrayElementAtIndex(ei), pKey, ei);

            EditorGUI.indentLevel++;
            if (SmallButton($"Remove event {ei}"))
            {
                eventsProp.DeleteArrayElementAtIndex(ei);
                EditorGUI.indentLevel--;
                break;
            }
            EditorGUI.indentLevel--;
        }

        if (GUILayout.Button("＋  Add Event"))
        {
            eventsProp.InsertArrayElementAtIndex(eventsProp.arraySize);
            eventsProp
                .GetArrayElementAtIndex(eventsProp.arraySize - 1)
                .FindPropertyRelative("eventName").stringValue = "NewEvent";
        }

        EditorGUI.indentLevel--;

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (SmallButton($"✕  Remove {pLabel}"))
        {
            listProp.DeleteArrayElementAtIndex(pi);
            EditorGUILayout.EndHorizontal();
            return true;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);
        return false;
    }

    private void DrawEvent(SerializedProperty evProp, string pKey, int ei)
    {
        string evName = evProp.FindPropertyRelative("eventName").stringValue;
        if (string.IsNullOrEmpty(evName)) evName = $"Event {ei}";

        string evKey  = $"{pKey}_e{ei}";
        bool   evOpen = Fold(evKey, $"  ▸  {evName}", defaultOpen: true, box: true);
        if (!evOpen) return;

        EditorGUI.indentLevel++;
        PropOf(evProp, "eventName");

        EditorGUILayout.Space(3);
        MiniLabel("▶  In Animation");
        DrawAnimBlock(evProp.FindPropertyRelative("inAnimation"), $"{evKey}_in");

        EditorGUILayout.Space(3);
        MiniLabel("◀  Out Animation");
        DrawAnimBlock(evProp.FindPropertyRelative("outAnimation"), $"{evKey}_out");
        EditorGUI.indentLevel--;
    }

    private void DrawAnimBlock(SerializedProperty a, string key)
    {
        PropOf(a, "animationType");
        PropOf(a, "followDelay", new GUIContent("Follow Delay", "Seconds before the NEXT portrait starts after this animation triggers."));
        DrawTrail(a, key);

        var type = (AnimationType)a.FindPropertyRelative("animationType").enumValueIndex;

        EditorGUILayout.Space(2);
        EditorGUI.indentLevel++;

        switch (type)
        {
            case AnimationType.Slide:
                MiniLabel("Slide");
                PropOf(a, "offScreenOffset", new GUIContent("Off-Screen Offset", "Offset ADDED to the rest position relative to its current layout home."));
                PropOf(a, "slideDuration");
                break;

            case AnimationType.SpinZoom:
                MiniLabel("Spin & Zoom");
                PropOf(a, "spinDegrees");
                PropOf(a, "zoomInScale");
                PropOf(a, "spinZoomDuration");
                break;

            case AnimationType.Wipe:
                MiniLabel("Wipe");
                PropOf(a, "wipeMask", new GUIContent("Wipe Mask", "RectTransform on a RectMask2D that is an ancestor of the portrait."));
                PropOf(a, "wipeDirection");
                PropOf(a, "wipeDuration");
                break;

            case AnimationType.Bounce:
                MiniLabel("Bounce");
                PropOf(a, "bounceStartScale");
                PropOf(a, "bounceOvershoot");
                PropOf(a, "bounceDuration");
                break;

            case AnimationType.Shake:
                MiniLabel("Shake");
                PropOf(a, "shakeStrength");
                PropOf(a, "shakeVibrato");
                PropOf(a, "shakeRandomness");
                PropOf(a, "shakeDuration");
                break;

            case AnimationType.Fade:
                MiniLabel("Fade");
                PropOf(a, "canvasGroup", new GUIContent("Canvas Group", "CanvasGroup on or above the portrait. Can be in any parent."));
                PropOf(a, "fadeDuration");
                break;
        }

        EditorGUI.indentLevel--;
    }

    private void DrawTrail(SerializedProperty a, string key)
    {
        string tKey   = $"trail_{key}";
        bool   tOpen  = Fold(tKey, "Trail (optional — leave at defaults to disable)", defaultOpen: false);
        if (!tOpen) return;

        EditorGUI.indentLevel++;
        PropOf(a, "trailColor");
        PropOf(a, "maximumTrailLag");
        PropOf(a, "maximumTrailLag2");
        PropOf(a, "finalTrailOffset");
        EditorGUI.indentLevel--;
    }

    private void Prop(string name) =>
        EditorGUILayout.PropertyField(serializedObject.FindProperty(name));

    private static void PropOf(SerializedProperty parent, string name, GUIContent label = null)
    {
        var child = parent.FindPropertyRelative(name);
        if (child == null) return;
        if (label != null) EditorGUILayout.PropertyField(child, label);
        else               EditorGUILayout.PropertyField(child);
    }

    private static void Header(string text) =>
        EditorGUILayout.LabelField(text, EditorStyles.boldLabel);

    private static void MiniLabel(string text) =>
        EditorGUILayout.LabelField(text, EditorStyles.miniBoldLabel);

    private static bool SmallButton(string label) =>
        GUILayout.Button(label, GUILayout.Height(18));

    private bool Fold(string key, string label, bool defaultOpen, bool box = false)
    {
        if (!_open.TryGetValue(key, out bool current))
        {
            current    = defaultOpen;
            _open[key] = current;
        }

        if (box) EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        bool next = EditorGUILayout.BeginFoldoutHeaderGroup(current, label);
        EditorGUILayout.EndFoldoutHeaderGroup();
        if (box) EditorGUILayout.EndVertical();

        if (next != current) _open[key] = next;
        return next;
    }
}
#endif