#if UNITY_EDITOR
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System.Threading.Tasks;   

    public class GCPSpecificArgWindow : BaseSpecificArg{
    public static string _bucket;
    public static string _target;

    public override void CustomParameters(){
        _bucket = EditorGUILayout.TextField("Bucket:", _bucket);
        _target = EditorGUILayout.TextField("Target:", _target);
    }
 }
#endif