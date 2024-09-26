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
#if UNITY_EDITOR
using QuestionDevTool;
#endif

[System.Serializable]
public class Point {
    public Vector2 position;  
    public float width;       
    public float height;      
}

#if UNITY_EDITOR
public class ClickEditor : QuestionEditor {
    [Header("JSON情報の追加")]
    [Tooltip("問題IDを指定してください。他の問題IDと同じものを指定すると上書きされます。")]
    public string questionId;
    [Tooltip("正解の画像を指定してください。")]
    public Sprite correctImage;
    [Tooltip("不正解の画像を指定してください。")]
    public Sprite incorrectImage;
    [Tooltip("不正解画像上のクリックエリアを指定してください。")]
    public List<GameObject> pointObjects = new List<GameObject>(); // 作成されたポイントUIオブジェクトのリスト

    [Tooltip("BGMを指定してください。")]
    public AudioClip BGM;

    [Header("Editor Settings")]
    [SerializeField]
    private Image correctImageObject;   // 正解画像を表示するオブジェクト
    [SerializeField]
    private Image incorrectImageObject; // 不正解画像を表示するオブジェクト
    [SerializeField]
    private GameObject pointPrefab;     // ポイント用のプレハブ
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
        GameObject newPoint = Instantiate(pointPrefab, incorrectImageObject.transform); // 親をincorrectImageObjectに設定
        newPoint.name = "ClickPoint";
        // 位置とサイズの初期値を設定
        newPoint.transform.localPosition = new Vector3(0,0,0);
        newPoint.transform.localScale = new Vector3(100, 100, 1);
        
        // 作成したRectTransformをポイントに保存
        pointObjects.Add(newPoint);
    }

    public override void Clear() {
        correctImage = null;
        incorrectImage = null;
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
        foreach(var pointObj in pointObjects) {
            pointsData.Add(new ClickPoint {
                x = pointObj.transform.localPosition.x,
                y = pointObj.transform.localPosition.y,
                width = pointObj.transform.localScale.x,
                height = pointObj.transform.localScale.y,
            });
        }

        var correctImgRect = correctImageObject.GetComponent<RectTransform>();
        var incorrectImgRect = incorrectImageObject.GetComponent<RectTransform>();

        var quizData = new Question {
            correct = new CorrectImage {
                src = correctImage != null ? base.GetResourcePath(correctImage) : "",
                rect = new ImgRect {
                    x = correctImgRect.anchoredPosition.x,
                    y = correctImgRect.anchoredPosition.y,
                    z = 0.0f,
                    width = correctImgRect.rect.width,
                    height = correctImgRect.rect.height
                }
            },
            incorrect = new IncorrectImage {
                src = incorrectImage != null ? base.GetResourcePath(incorrectImage) : "",
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
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(ClickEditor))]
public class ClickEditorGUI : EditorGUI<ClickEditor> {
    public override void CustomInspectorGUI() {
        ClickEditor clickEditor = (ClickEditor)target;

        if (GUILayout.Button("ポイントを追加")) {
            Undo.RecordObject(clickEditor, "Add Point");
            clickEditor.CreatePointObjects();
        }


        GUILayout.Space(20);

        if (GUILayout.Button("全てクリア")) {
            Undo.RecordObject(clickEditor, "Clear All Points");
            clickEditor.Clear();
        }
    }
}
#endif
