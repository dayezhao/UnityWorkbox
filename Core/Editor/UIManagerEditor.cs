﻿using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Callbacks;

namespace UnityWorkbox.UI
{
    [CustomEditor(typeof(UIManager), true)]
    public class UIManagerEditor : Editor
    {
        private static UIManagerEditor instance;
        private int tagIndex = 0, sortingLayerIndex = 0, renderMode = 0;

        private void OnEnable()
        {
            var panels = serializedObject.FindProperty("preloadPanels");
            if (panels != null && panels.isArray && panels.arraySize <= 0) panels.arraySize++;
            instance = this;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var overwrite = serializedObject.FindProperty("overwriteCamera");
            if (overwrite.boolValue)
            {
                var tag = serializedObject.FindProperty("cameraTag");
                tagIndex = Array.IndexOf(InternalEditorUtility.tags, tag.stringValue);
                tagIndex = EditorGUILayout.Popup("Camera Tag", tagIndex, InternalEditorUtility.tags);
                tag.stringValue = InternalEditorUtility.tags[tagIndex];
            }
            var ui = serializedObject.FindProperty("startUI");
            if (!ui.objectReferenceValue)
            {
                var prompt = "UIManager initialize fail , you must appoint a StartUI component and show it as first one ,";
                prompt += " You can select it from [Hierarchy] or click below [Create StartUI] button to create it!";
                EditorGUILayout.HelpBox(prompt, MessageType.Error);
                var create = GUILayout.Button("Create StartUI");
                var uiType = ReflectionExtensions
                    .GetAllRuntimeTypes()
                    .Where(u => u.IsSubclassOf(typeof(BaseUI)) && u.IsDefined(typeof(UIStartAttribute), true))
                    .FirstOrDefault();
                if (create && uiType == null) BaseUIEditor.CreateUIPanel(true);
                if (uiType != null && !EditorApplication.isCompiling)
                    CreateChildStartUI(uiType);
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void CreateChildStartUI(Type type)
        {
            var canvasObject = (target as UIManager).gameObject;
            Transform child = canvasObject.transform.Find(type.Name);
            if (!child)
            {
                var obj = new GameObject(type.Name);
                child = obj.transform;
                child.transform.SetParent(canvasObject.transform);
            }
            var comp = child.GetComponent(type);
            if (!comp)
                comp = child.gameObject.AddComponent(type);
            var ui = serializedObject.FindProperty("startUI");
            ui.objectReferenceValue = comp;
        }
    }
}