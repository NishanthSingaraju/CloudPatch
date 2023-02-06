using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;


public class EditorWaitForModal
{
    private static readonly TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>();

    public Task Task => _tcs.Task;

    public static void SetResult()
    {
        _tcs.SetResult(null);
    }
}


