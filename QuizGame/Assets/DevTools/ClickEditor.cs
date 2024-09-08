using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Newtonsoft.Json;
using PlayType5Interface;
using QuestionDataInterface;

[System.Serializable]
public class Point {
    public Vector2 position;  // 四角形の左上座標
    public float width;       // 四角形の幅
    public float height;      // 四角形の高さ
}

public class ClickEditor : MonoBehaviour {
    [Header("JSON情報の追加")]
    public int templateType = 5;
    [Tooltip("正解の画像を指定してください。")]
    public Sprite correctImage;
    [Tooltip("不正解の画像を指定してください。")]
    public Sprite incorrectImage;
    [Tooltip("不正解の画像上のポイントを指定してください。")]
    public List<Point> points = new List<Point>();

    [Header("Editor Settings")]
    [SerializeField]
    private SpriteRenderer correctImageObject;   // 正解画像を表示するオブジェクト
    [SerializeField]
    private SpriteRenderer incorrectImageObject; // 不正解画像を表示するオブジェクト

    // インスペクタで画像が変更されたときに自動的にシーンビューに反映
    private void OnValidate() {
        // 正解画像が変更された場合
        if (correctImageObject != null && correctImage != null) {
            correctImageObject.sprite = correctImage;
        }

        // 不正解画像が変更された場合
        if (incorrectImageObject != null && incorrectImage != null) {
            incorrectImageObject.sprite = incorrectImage;
        }
    }

    // シーンビューでポイントを描画するためのメソッド
    private void OnDrawGizmos(){
        if (points == null || points.Count == 0)
            return;

        Gizmos.color = Color.red;

        // 各ポイントの範囲をシーンビューに描画
        foreach (var point in points) {
            Gizmos.DrawWireCube(point.position + new Vector2(point.width / 2, -point.height / 2), new Vector3(point.width, point.height, 0));
        }
    }


    public void ClearAll() {
        correctImage = null;
        incorrectImage = null;
        points.Clear();
    }

    public void SaveGridAsJSON() {
        var uuid = Guid.NewGuid().ToString();
        string folderPath = $"{DevConstants.QuestionDataFolder}/{templateType}/quiz";
        string filePath = Path.Combine(folderPath, $"{uuid}.json"); 

        // エディタ上で設定した情報を元に、JSONデータを構築
        // 間違いポイントのシリアライズ
        List<ClickPoint> pointsData = new List<ClickPoint>();
        foreach (var point in points) {
            pointsData.Add(new ClickPoint {
                x = (float)point.position.x,
                y = (float)point.position.y,
                width = point.width,
                height = point.height
            });
        }
        Question quizData = new Question {
            correct = new CorrectImage {
                src = correctImage != null ? GetSpritePath(correctImage) : ""
            },
            incorrect = new IncorrectImage {
                src = incorrectImage != null ? GetSpritePath(incorrectImage) : "",
                points = pointsData
            }
        };


        string json = JsonConvert.SerializeObject(quizData, Formatting.Indented);
        // フォルダが存在しない場合は作成
        if (!Directory.Exists(folderPath)) {
            Directory.CreateDirectory(folderPath);
            Debug.Log($"フォルダ作成: {folderPath}");
        }

        // ファイルにJSONデータを書き込む
        StreamWriter streamWriter = new StreamWriter(filePath);
        streamWriter.Write(json);
        streamWriter.Flush();
        streamWriter.Close();
        Debug.Log($"JSONデータが保存されました: {filePath}");
        Debug.Log("格子問題データを保存しました。");
        // 大問データに小問を追加
        var ParentDataPath = PlayerPrefs.GetString(DevConstants.QuestionDataFileKey);
        string parentJson = File.ReadAllText(ParentDataPath);
        QuestionData parentData = JsonConvert.DeserializeObject<QuestionData>(parentJson);
        parentData.quiz.questions.Add(filePath);
        File.WriteAllText(ParentDataPath, JsonConvert.SerializeObject(parentData));
        Debug.Log($"大問データに小問追加: {filePath}");
        streamWriter = new StreamWriter(ParentDataPath);
        streamWriter.Write(JsonConvert.SerializeObject(parentData));
        streamWriter.Flush();
        streamWriter.Close();
        // Unityにアセットの変更を認識させる
        AssetDatabase.Refresh();
    }


     // エディタ限定の関数でPrefabのGUIDを取得
    public string GetPrefabGUID(GameObject prefab) {
    #if UNITY_EDITOR
        // Prefabのパスを取得
        string prefabPath = AssetDatabase.GetAssetPath(prefab);

        if (!string.IsNullOrEmpty(prefabPath)) {
            // パスからGUIDを取得
            string prefabGUID = AssetDatabase.AssetPathToGUID(prefabPath);
            Debug.Log("Prefab GUID: " + prefabGUID);
            return prefabGUID;
        } else {
            Debug.LogError("Prefab is not assigned or the path is invalid.");
        }
    #else
        Debug.LogError("This function can only be used in the Unity Editor.");
    #endif
        return null;
    }

    public string GetSpritePath(Sprite sprite) {
    #if UNITY_EDITOR
            if (sprite != null) {
                // Spriteのテクスチャアセットのパスを取得
                string assetPath = AssetDatabase.GetAssetPath(sprite.texture);
                return assetPath;
            } else {
                Debug.LogError("Spriteが設定されていません。");
            }
    #else
            Debug.LogError("この機能はエディタのみで使用可能です。");
    #endif
        return null;
    }
}


[CustomEditor(typeof(ClickEditor))]
public class ClickEditorManager : Editor {
    private ClickEditor clickEditor;

    private void OnEnable() {
        clickEditor = (ClickEditor)target;
    }

    private void OnSceneGUI() {
        // シーンビューで各ポイントを視覚的に調整
        for (int i = 0; i < clickEditor.points.Count; i++) {
            var point = clickEditor.points[i];

            // 四角形の位置をドラッグで動かせるようにする
            Vector2 newPosition = Handles.PositionHandle(point.position, Quaternion.identity);
            if (newPosition != point.position) {
                Undo.RecordObject(clickEditor, "Move Point Position");
                point.position = newPosition;
            }

            // 四角形のサイズをドラッグで変更できるようにする
            Vector2 sizeHandle = new Vector2(point.position.x + point.width, point.position.y - point.height);
            var fmh_100_72_638614213584702255 = Quaternion.identity; Vector2 newSizeHandle = Handles.FreeMoveHandle(sizeHandle, 0.1f, Vector3.zero, Handles.RectangleHandleCap);

            if (newSizeHandle != sizeHandle) {
                Undo.RecordObject(clickEditor, "Resize Point");
                point.width = Mathf.Abs(newSizeHandle.x - point.position.x);
                point.height = Mathf.Abs(newSizeHandle.y - point.position.y);
            }

            // 四角形の枠をシーン上に描画
            Handles.color = Color.red;
            Handles.DrawWireCube(point.position + new Vector2(point.width / 2, -point.height / 2), new Vector3(point.width, point.height, 0));
        }
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("ポイントを追加")) {
            Undo.RecordObject(clickEditor, "Add Point");
            clickEditor.points.Add(new Point { position = Vector2.zero, width = 10, height = 10 });
        }

        if (GUILayout.Button("ポイントを削除")) {
            if (clickEditor.points.Count > 0){
                Undo.RecordObject(clickEditor, "Remove Point");
                clickEditor.points.RemoveAt(clickEditor.points.Count - 1);
            }
        }
        GUILayout.Space(20);
        if (GUILayout.Button("小問を作成する")) {
            clickEditor.SaveGridAsJSON();
        }
        GUILayout.Space(20);
        if (GUILayout.Button("全てクリア")) {
            clickEditor.ClearAll();
        }
    }
}

