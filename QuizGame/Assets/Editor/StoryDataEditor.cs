using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Newtonsoft.Json;
using StoryDataInterface;

/// <summary>
/// ストーリー制作ツールのエンドポイント
/// </summary>
public class StoryDataEditor : EditorWindow {
    
    [MenuItem("開発ツール/ストーリー制作ツール")]
    public static void MoveStoryEditor() {
        EditorSceneManager.OpenScene("Assets/DevTools/Scenes/StoryEditor.unity");
    }


}
