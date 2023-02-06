#if UNITY_EDITOR
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System.Threading.Tasks;   

    public class UnstatiqSpecificArgWindow : BaseSpecificArg{
    public static string _game;
    public static string _target;

    public override void CustomParameters(){
        _game = EditorGUILayout.TextField("Game:", _game);
        _target = EditorGUILayout.TextField("Target:", _target);
    }
 }
#endif