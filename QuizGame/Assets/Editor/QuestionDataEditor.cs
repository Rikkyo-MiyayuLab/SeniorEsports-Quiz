using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement; // シーン遷移用
using QuestionDataInterface;

/// <summary>
/// 問題制作ツールのエンドポイント
/// </summary>
public class QuestionDataEditor : EditorWindow{
    
    /** 問題タイトル*/
    private string title = "問題タイトル";
    /** 問題文*/
    private string description = "問題文";
    /** 1小問当たりの制限時間 */
    private int limits = 60;
    /** 難易度*/
    private int difficulty = 5;
    /** 画面タイプ*/
    private int type = 1;
    private string endStory = "";
    
    private Texture2D[] TemplateImages;
    private int templateTypeCount = 5;

    
    [MenuItem("開発ツール/問題制作ツール")]
    public static void ShowWindow() {
        GetWindow<QuestionDataEditor>("問題制作ツール");
    }

    private
    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable() {
        TemplateImages = new Texture2D[templateTypeCount];
        for (int i = 0; i < templateTypeCount; i++) {
            TemplateImages[i] = (Texture2D)AssetDatabase.LoadAssetAtPath($"Assets/DevTools/TemplateImgs/T-{i + 1}.png", typeof(Texture2D));
        }
    }

    private void OnGUI() {
        GUILayout.Label("大問を作成します。問題情報を入力してください。", EditorStyles.boldLabel);

        GUILayout.Space(20);

        title = EditorGUILayout.TextField("問題タイトル", title);
        description = EditorGUILayout.TextField("問題文", description);
        limits = EditorGUILayout.IntField("制限時間（秒）", limits);
        difficulty = EditorGUILayout.IntSlider("難易度", difficulty, 1, 5);
        type = EditorGUILayout.IntField("テンプレートタイプ", type);
        endStory = EditorGUILayout.TextField("大問終了後のストーリー", endStory);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("この条件で問題を作成する。\n ※小問作成へ遷移します。",  GUILayout.Width(200), GUILayout.Height(50))) {
            var savePath = SaveJSON();
            EditorSceneManager.OpenScene($"Assets/DevTools/Scenes/T-{type}Editor.unity");
            // 生成した大問JSONの情報を次のシーンに引き継ぐ
            PlayerPrefs.SetString(DevConstants.QuestionDataFileKey, savePath); 
            PlayerPrefs.Save();
        }
        GUILayout.FlexibleSpace();           
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        //テンプレートのイメージ画像を横並びで表示
        GUILayout.BeginHorizontal();
        for (int i = 0; i < templateTypeCount; i++) {
            GUILayout.BeginVertical();
            GUILayout.Label(TemplateImages[i], GUILayout.Width(100), GUILayout.Height(100));
            GUILayout.Label($"テンプレート{i + 1}");
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();

    }


    private string SaveJSON() {
        // 大問データを構築
        QuestionData data = new QuestionData {
            title = title,
            description = description,
            limits = limits,
            difficulty = difficulty,
            type = type,
            endStory = endStory
        };

        // JSON形式に変換
        string jsonData = JsonUtility.ToJson(data, true);
        // 大問毎固有のUUIDを生成
        string uuid = Guid.NewGuid().ToString();
        string folderPath = $"{DevConstants.QuestionDataFolder}/{type}";
        string filePath = Path.Combine(folderPath, $"{uuid}.json");

        // フォルダが存在しない場合は作成
        if (!Directory.Exists(folderPath)) {
            Directory.CreateDirectory(folderPath);
            Debug.Log($"フォルダ作成: {folderPath}");
        }

        // ファイルにJSONデータを書き込む
        File.WriteAllText(filePath, jsonData);
        Debug.Log($"JSONデータが保存されました: {filePath}");
        // Unityにアセットの変更を認識させる
        AssetDatabase.Refresh();

        return filePath;

    }
}

