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
    public RectTransform rectTransform; // 各ポイントに対応するUIオブジェクト
}

public class ClickEditor : QuestionEditor {
    [Header("JSON情報の追加")]
    [Tooltip("問題IDを指定してください。他の問題IDと同じものを指定すると上書きされます。")]
    public string questionId;
    [Tooltip("正解の画像を指定してください。")]
    public Sprite correctImage;
    [Tooltip("不正解の画像を指定してください。")]
    public Sprite incorrectImage;
    [Tooltip("不正解の画像上のポイントを指定してください。")]
    public List<Point> points = new List<Point>();
    [Tooltip("BGMを指定してください。")]
    public AudioClip BGM;

    [Header("Editor Settings")]
    [SerializeField]
    private Image correctImageObject;   // 正解画像を表示するオブジェクト
    [SerializeField]
    private Image incorrectImageObject; // 不正解画像を表示するオブジェクト
    [SerializeField]
    private GameObject pointPrefab;     // ポイント用のプレハブ

    private List<GameObject> pointObjects = new List<GameObject>(); // 作成されたポイントUIオブジェクトのリスト

    private void OnValidate() {
        // 背景画像が設定されている場合、背景画像を表示
        if (base.backgroundImage != null && base.backgroundImageObject != null) {
            base.backgroundImageObject.sprite = base.backgroundImage;
        }

        // 正解画像が変更された場合
        if (correctImageObject != null && correctImage != null) {
            correctImageObject.sprite = correctImage;
        }

        // 不正解画像が変更された場合
        if (incorrectImageObject != null && incorrectImage != null) {
            incorrectImageObject.sprite = incorrectImage;
        }
    }

    // シーン上にポイントのUIオブジェクトを配置
    private void Initialize() {
        CreatePointObjects();
    }

    public void CreatePointObjects() {
        // 既存のポイントUIオブジェクトを削除
        foreach (var obj in pointObjects) {
            Destroy(obj);
        }
        pointObjects.Clear();

        // 各ポイントに対応するUIオブジェクトを作成
        foreach (var point in points) {
            GameObject newPoint = Instantiate(pointPrefab, incorrectImageObject.transform); // 親をincorrectImageObjectに設定
            var rectTransform = newPoint.GetComponent<RectTransform>();

            // UIオブジェクトの位置とサイズを設定
            rectTransform.anchoredPosition = point.position;
            rectTransform.sizeDelta = new Vector2(point.width != 0 ? point.width : 100, point.height != 0 ? point.height : 100); // 初期サイズを100x100に設定

            // 作成したRectTransformをポイントに保存
            point.rectTransform = rectTransform;
            pointObjects.Add(newPoint);
        }
    }

    // ポイントデータをUIオブジェクトの状態に基づいて更新
    private void Update() {
        for (int i = 0; i < points.Count; i++) {
            var point = points[i];
            if (point.rectTransform != null) {
                point.position = point.rectTransform.anchoredPosition;
                point.width = point.rectTransform.sizeDelta.x;
                point.height = point.rectTransform.sizeDelta.y;
            }
        }
    }

    // Gizmosを使ってポイントの範囲を可視化
    private void OnDrawGizmos() {
        Gizmos.color = Color.red; // 枠の色を設定

        foreach (var point in points) {
            if (point.rectTransform != null) {
                // RectTransformのサイズと位置を取得
                Vector2 size = point.rectTransform.sizeDelta;
                Vector3 position = point.rectTransform.anchoredPosition;

                // 中心から四角形の位置を決めるためにオフセットを計算
                Vector3 offset = new Vector3(size.x / 2, -size.y / 2, 0);

                // Gizmosで矩形を描画
                Gizmos.DrawWireCube(position - offset, new Vector3(size.x, size.y, 0));
            }
        }
    }

    public override void Clear() {
        correctImage = null;
        incorrectImage = null;
        points.Clear();
        foreach (var obj in pointObjects) {
            DestroyImmediate(obj); // UIオブジェクトもクリア
        }
        pointObjects.Clear();
        // 画像表示オブジェクトのソースをクリア
        correctImageObject.sprite = null;
        incorrectImageObject.sprite = null;
    }

    public override void CreateQuestionData() {
        var uuid = Guid.NewGuid().ToString();
        string folderPath = $"{DevConstants.QuestionDataFolder}/{templateType}/quiz";
        string fileName = $"{questionId}.json"; 

        // JSONデータを構築
        List<ClickPoint> pointsData = new List<ClickPoint>();
        foreach (var point in points) {
            pointsData.Add(new ClickPoint {
                x = point.position.x,
                y = point.position.y,
                width = point.width,
                height = point.height
            });
        }

        var correctImgRect = correctImageObject.GetComponent<RectTransform>();
        var incorrectImgRect = incorrectImageObject.GetComponent<RectTransform>();

        var quizData = new Question {
            correct = new CorrectImage {
                src = correctImage != null ? base.GetSpritePath(correctImage) : "",
                rect = new ImgRect {
                    x = correctImgRect.anchoredPosition.x,
                    y = correctImgRect.anchoredPosition.y,
                    z = 0.0f,
                    width = correctImgRect.rect.width,
                    height = correctImgRect.rect.height
                }
            },
            incorrect = new IncorrectImage {
                src = incorrectImage != null ? base.GetSpritePath(incorrectImage) : "",
                points = pointsData,
                rect = new ImgRect {
                    x = incorrectImgRect.anchoredPosition.x,
                    y = incorrectImgRect.anchoredPosition.y,
                    z = 0.0f,
                    width = incorrectImgRect.rect.width,
                    height = incorrectImgRect.rect.height
                }
            },
            backgroundImage = base.GetResourcePath(base.backgroundImage),
            bgm = base.GetResourcePath(BGM)
        };

        // JSONにシリアライズ
        base.SaveAsJSON(folderPath, fileName, quizData);
    }
}

[CustomEditor(typeof(ClickEditor))]
public class ClickEditorGUI : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        ClickEditor clickEditor = (ClickEditor)target;

        if (GUILayout.Button("ポイントを追加")) {
            Undo.RecordObject(clickEditor, "Add Point");
            clickEditor.points.Add(new Point { position = Vector2.zero, width = 100, height = 100 }); // 初期サイズ100x100で追加
            clickEditor.CreatePointObjects();
        }

        if (GUILayout.Button("ポイントを削除")) {
            if (clickEditor.points.Count > 0) {
                Undo.RecordObject(clickEditor, "Remove Point");
                clickEditor.points.RemoveAt(clickEditor.points.Count - 1);
                clickEditor.CreatePointObjects();
            }
        }

        GUILayout.Space(20);

        if (GUILayout.Button("全てクリア")) {
            Undo.RecordObject(clickEditor, "Clear All Points");
            clickEditor.Clear();
        }
    }
}
