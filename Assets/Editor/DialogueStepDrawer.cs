using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DialogueStep))]
public class DialogueStepDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty stepName = property.FindPropertyRelative("stepName");
        SerializedProperty npcText = property.FindPropertyRelative("npcText");

        string title = !string.IsNullOrWhiteSpace(stepName.stringValue)
            ? stepName.stringValue
            : npcText.stringValue;

        EditorGUI.PropertyField(position, property, new GUIContent(title), true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
