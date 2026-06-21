using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DialogueReply))]
public class DialogueReplyDrawer : PropertyDrawer
{
    private const float VerticalSpacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty choiceName = property.FindPropertyRelative("choiceName");
        SerializedProperty replyText = property.FindPropertyRelative("replyText");
        SerializedProperty nextStepNumber = property.FindPropertyRelative("nextStepNumber");
        SerializedProperty endsDialogue = property.FindPropertyRelative("endsDialogue");

        string title = !string.IsNullOrWhiteSpace(choiceName.stringValue)
            ? choiceName.stringValue
            : replyText.stringValue;

        EditorGUI.BeginProperty(position, label, property);

        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, new GUIContent(title), true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            float y = foldoutRect.yMax + VerticalSpacing;
            DrawProperty(ref y, position, choiceName, new GUIContent("Choice Name"));
            DrawProperty(ref y, position, replyText, new GUIContent("Reply Text"));
            DrawNextStepPopup(ref y, position, property, nextStepNumber);
            DrawProperty(ref y, position, endsDialogue, new GUIContent("Ends Dialogue"));

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        SerializedProperty choiceName = property.FindPropertyRelative("choiceName");
        SerializedProperty replyText = property.FindPropertyRelative("replyText");
        SerializedProperty endsDialogue = property.FindPropertyRelative("endsDialogue");

        return EditorGUIUtility.singleLineHeight
            + VerticalSpacing + EditorGUI.GetPropertyHeight(choiceName, true)
            + VerticalSpacing + EditorGUI.GetPropertyHeight(replyText, true)
            + VerticalSpacing + EditorGUIUtility.singleLineHeight
            + VerticalSpacing + EditorGUI.GetPropertyHeight(endsDialogue, true);
    }

    private static void DrawProperty(ref float y, Rect position, SerializedProperty property, GUIContent label)
    {
        float height = EditorGUI.GetPropertyHeight(property, true);
        Rect rect = new Rect(position.x, y, position.width, height);
        EditorGUI.PropertyField(rect, property, label, true);
        y += height + VerticalSpacing;
    }

    private static void DrawNextStepPopup(
        ref float y,
        Rect position,
        SerializedProperty replyProperty,
        SerializedProperty nextStepNumber)
    {
        SerializedProperty steps = replyProperty.serializedObject.FindProperty("dialogueSteps");
        if (steps == null || !steps.isArray)
        {
            DrawProperty(ref y, position, nextStepNumber, new GUIContent("Next Step Number"));
            return;
        }

        int optionCount = steps.arraySize + 1;
        string[] labels = new string[optionCount];
        int[] values = new int[optionCount];

        labels[0] = "0 - 下一段";
        values[0] = 0;

        for (int i = 0; i < steps.arraySize; i++)
        {
            SerializedProperty step = steps.GetArrayElementAtIndex(i);
            string stepTitle = GetStepTitle(step);
            int stepNumber = i + 1;

            labels[stepNumber] = stepNumber + " - " + stepTitle;
            values[stepNumber] = stepNumber;
        }

        int selectedIndex = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == nextStepNumber.intValue)
            {
                selectedIndex = i;
                break;
            }
        }

        Rect rect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
        int newIndex = EditorGUI.Popup(rect, "Next Step", selectedIndex, labels);
        nextStepNumber.intValue = values[newIndex];
        y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
    }

    private static string GetStepTitle(SerializedProperty step)
    {
        SerializedProperty stepName = step.FindPropertyRelative("stepName");
        if (stepName != null && !string.IsNullOrWhiteSpace(stepName.stringValue))
            return stepName.stringValue;

        SerializedProperty npcText = step.FindPropertyRelative("npcText");
        if (npcText != null && !string.IsNullOrWhiteSpace(npcText.stringValue))
            return npcText.stringValue;

        return "未命名文本";
    }
}
