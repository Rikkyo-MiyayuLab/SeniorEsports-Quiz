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
using QuestionDevTool;

[System.Serializable]
public class Point {
    public Vector2 position;  // 四角形の左上座標
    public float width;       // 四角形の幅
    public float height;      // 四角形の高さ
}

public class ClickEditor : QuestionEditor {
    [Header("JSON情報の追加")]
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
    private void OnDrawGizmos() {
        if (points == null || points.Count == 0)
            return;

        Gizmos.color = Color.red;

        // 各ポイントの範囲をシーンビューに描画
        foreach (var point in points) {
            Gizmos.DrawWireCube(point.position + new Vector2(point.width / 2, -point.height / 2), new Vector3(point.width, point.height, 0));
        }
    }


    public override void Clear() {
        correctImage = null;
        incorrectImage = null;
        points.Clear();
        // 画像表示オブジェクトのソースをクリア
        correctImageObject.sprite = null;
        incorrectImageObject.sprite = null;
    }

    public override void CreateQuestionData() {
        var uuid = Guid.NewGuid().ToString();
        string folderPath = $"{DevConstants.QuestionDataFolder}/{templateType}/quiz";
        string fileName = $"{uuid}.json"; 

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

        // 問題画像のRect情報を記録
        var correctImgRect = correctImageObject.bounds;
        var incorrectImgRect = incorrectImageObject.bounds;
        Question quizData = new Question {
            correct = new CorrectImage {
                src = correctImage != null ? base.GetSpritePath(correctImage) : "",
                rect = new ImgRect {
                    x = correctImgRect.center.x,
                    y = correctImgRect.center.y,
                    z = correctImgRect.center.z,
                    width = correctImgRect.size.x,
                    height = correctImgRect.size.y,
                }
                
            },
            incorrect = new IncorrectImage {
                src = incorrectImage != null ? base.GetSpritePath(incorrectImage) : "",
                points = pointsData,
                rect = new ImgRect {
                    x = incorrectImgRect.center.x,
                    y = incorrectImgRect.center.y,
                    z = incorrectImgRect.center.z,
                    width = incorrectImgRect.size.x,
                    height = incorrectImgRect.size.y,
                }
            }
        };

        // JSONにシリアライズ
        base.SaveAsJSON(folderPath, fileName, quizData);
        
    }
}


[CustomEditor(typeof(ClickEditor))]
public class ClickEditorGUI : EditorGUI<ClickEditor> { 

    private void OnSceneGUI() {
        // シーンビューで各ポイントを視覚的に調整
        for (int i = 0; i < base.editor.points.Count; i++) {
            var point = base.editor.points[i];

            // 四角形の位置をドラッグで動かせるようにする
            Vector2 newPosition = Handles.PositionHandle(point.position, Quaternion.identity);
            if (newPosition != point.position) {
                Undo.RecordObject(base.editor, "Move Point Position");
                point.position = newPosition;
            }

            // 四角形のサイズをドラッグで変更できるようにする
            Vector2 sizeHandle = new Vector2(point.position.x + point.width, point.position.y - point.height);
            var fmh_100_72_638614213584702255 = Quaternion.identity; Vector2 newSizeHandle = Handles.FreeMoveHandle(sizeHandle, 0.1f, Vector3.zero, Handles.RectangleHandleCap);

            if (newSizeHandle != sizeHandle) {
                Undo.RecordObject(base.editor, "Resize Point");
                point.width = Mathf.Abs(newSizeHandle.x - point.position.x);
                point.height = Mathf.Abs(newSizeHandle.y - point.position.y);
            }

            // 四角形の枠をシーン上に描画
            Handles.color = Color.red;
            Handles.DrawWireCube(point.position + new Vector2(point.width / 2, -point.height / 2), new Vector3(point.width, point.height, 0));
        }
    }

    public override void CustomInspectorGUI() {

        if (GUILayout.Button("ポイントを追加")) {
            Undo.RecordObject(base.editor, "Add Point");
            base.editor.points.Add(new Point { position = Vector2.zero, width = 10, height = 10 });
        }

        if (GUILayout.Button("ポイントを削除")) {
            if (base.editor.points.Count > 0){
                Undo.RecordObject(base.editor, "Remove Point");
                base.editor.points.RemoveAt(base.editor.points.Count - 1);
            }
        }
        GUILayout.Space(20);
        if (GUILayout.Button("全てクリア")) {
            base.editor.Clear();
        }
    }
}

