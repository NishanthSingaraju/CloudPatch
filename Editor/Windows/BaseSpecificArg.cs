#if UNITY_EDITOR
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System.Threading.Tasks;   

    public abstract class BaseSpecificArg : EditorWindow{
        public static async Task ShowWindowAsync()
        {
            Debug.Log("Function: ShowWindowAsync");
            var window = (GCPSpecificArgWindow)EditorWindow.GetWindow(typeof(GCPSpecificArgWindow));
            window.Show();
            var editorWaitForModal = new EditorWaitForModal();
            await editorWaitForModal.Task;
            Debug.Log("Function: ShowWindowAsync (Done)");
        }

        private void OnGUI()
        {
            CustomParameters();

            if (GUILayout.Button("Submit")){
                EditorWaitForModal.SetResult();
                Close();
            }
        }

        public abstract void CustomParameters();
 }
#endif