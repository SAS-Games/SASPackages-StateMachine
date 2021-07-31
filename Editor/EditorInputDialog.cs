using System;
using UnityEditor;
using UnityEngine;

public class EditorInputDialog : EditorWindow
{
    private string _description, _inputText;
    private string _okButton, _cancelButton;
    private bool _initializedPosition = false;
    Action onOKButton;

    bool shouldClose = false;
    Vector2 maxScreenPos;

    void OnGUI()
    {
        var e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            switch (e.keyCode)
            {
                // Escape pressed
                case KeyCode.Escape:
                    shouldClose = true;
                    e.Use();
                    break;

                // Enter pressed
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    onOKButton?.Invoke();
                    shouldClose = true;
                    e.Use();
                    break;
            }
        }

        if (shouldClose)
        {  // Close this dialog
            Close();
            //return;
        }

        // Draw our control
        var rect = EditorGUILayout.BeginVertical();

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField(_description);

        EditorGUILayout.Space(8);
        GUI.SetNextControlName("inText");
        _inputText = EditorGUILayout.TextField("", _inputText);
        GUI.FocusControl("inText");   // Focus text field
        EditorGUILayout.Space(12);

        // Draw OK / Cancel buttons
        var r = EditorGUILayout.GetControlRect();
        r.width /= 2;
        if (GUI.Button(r, _okButton))
        {
            onOKButton?.Invoke();
            shouldClose = true;
        }

        r.x += r.width;
        if (GUI.Button(r, _cancelButton))
        {
            _inputText = null;   // Cancel - delete inputText
            shouldClose = true;
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.EndVertical();

        // Force change size of the window
        if (rect.width != 0 && minSize != rect.size)
        {
            minSize = maxSize = rect.size;
        }

        // Set dialog position next to mouse position
        if (!_initializedPosition && e.type == EventType.Layout)
        {
            _initializedPosition = true;

            // Move window to a new position. Make sure we're inside visible window
            var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            mousePos.x += 32;
            if (mousePos.x + position.width > maxScreenPos.x) mousePos.x -= position.width + 64; // Display on left side of mouse
            if (mousePos.y + position.height > maxScreenPos.y) mousePos.y = maxScreenPos.y - position.height;

            position = new Rect(mousePos.x, mousePos.y, position.width, position.height);

            // Focus current window
            Focus();
        }
    }

    public static string Show(string title, string description, string inputText, string okButton = "OK", string cancelButton = "Cancel")
    {
        var maxPos = GUIUtility.GUIToScreenPoint(new Vector2(Screen.width, Screen.height));

        string ret = null;
        var window = CreateInstance<EditorInputDialog>();
        window.maxScreenPos = maxPos;
        window.titleContent = new GUIContent(title);
        window._description = description;
        window._inputText = inputText;
        window._okButton = okButton;
        window._cancelButton = cancelButton;
        window.onOKButton += () => ret = window._inputText;
        window.ShowModal();

        return ret;
    }
}