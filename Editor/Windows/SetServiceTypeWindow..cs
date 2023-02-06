#if UNITY_EDITOR
    using UnityEngine;
    using UnityEditor;

    public class SetServiceTypeWindow : EditorWindow
    {
        private static ServiceType _selectedServiceType;

        private void OnGUI()
        {
            _selectedServiceType = (ServiceType)EditorGUILayout.EnumPopup("Select Service Type:", _selectedServiceType);

            if (GUILayout.Button("Save"))
            {
                BackendService.SetServiceType(_selectedServiceType);
                Close();
            }
        }
    }
 #endif