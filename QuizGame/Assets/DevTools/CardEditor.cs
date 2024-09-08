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
using PlayType4Interface;
using QuestionDataInterface;

/// <summary>
/// 格子状の問題データを生成するエディタ
/// TODO:将来的に、１つの親クラスに統合して、派生させる。
/// </summary>
public class CardEditor : MonoBehaviour {
    [System.Serializable]
    public class CardObject {
        public Sprite frontImg;
        public Sprite backImg;  // クリックで表示する裏面ソース（オプション）
        public AudioClip audioSrc;  // クリック時に鳴らす音（オプション）
        public bool isCorrect;  // singleモードで正解のカードかどうか（オプション）
        public int displayCount;  // このカードを何枚表示させるか（オプション

    }
    [Header("JSON情報の追加")]
    public int templateType = 1;
    /** セルのプレハブ */
    [Tooltip("カードのプレハブ（素材）を指定してください。")]
    public GameObject prefab;
    [Tooltip("カードの情報を設定してください。")]
    public List<CardObject> cards;

    [Tooltip("生成する格子の行数を指定してください。")]
    public int rowSize;
    [Tooltip("生成する格子の列数を指定してください。")]
    public int columnSize;
    [Tooltip("カード間のマージンを指定してください。")]
    public float margin;
    [Tooltip("問題のタイプを指定してください。")]
    public QuizType quizType;
    [Tooltip("問題のタイプがpairの場合、ペアの数を指定してください。")]
    public int pairCount;

    [SerializeField]
    private GameObject CardArea;
    private List<GameObject> cardObjs;
    
    public void Initialize() {
        ClearGird();
        GenerateGrid();
    }

    /// <summary>
    /// カードを格子状にランダム配置する。
    /// </summary>
    public void GenerateGrid() {
        // グリッドのすべての座標をリストとして保持
        List<Vector2> availablePositions = new List<Vector2>();

        for (int i = 0; i < rowSize; i++) {
            for (int j = 0; j < columnSize; j++) {
                // 位置をマージンを考慮して計算
                availablePositions.Add(new Vector2(j * margin, -i * margin));
            }
        }

        // 座標リストをシャッフルしてランダムな位置にカードを配置できるようにする
        System.Random rand = new System.Random();
        for (int i = 0; i < availablePositions.Count; i++) {
            int randomIndex = rand.Next(i, availablePositions.Count);
            Vector2 temp = availablePositions[i];
            availablePositions[i] = availablePositions[randomIndex];
            availablePositions[randomIndex] = temp;
        }

        int positionIndex = 0;

        // カード全体の表示枚数の合計が、グリッドサイズを超えないようにする
        int totalCards = 0;
        foreach (CardObject card in cards) {
            totalCards += card.displayCount;
        }

        if (totalCards > availablePositions.Count) {
            Debug.LogWarning("グリッドのサイズが不足しています。カードが配置できるスペースがありません。");
            return;
        }

        // 各カードの種類ごとに、指定された枚数分だけ配置する
        foreach (CardObject cardData in cards) {
            for (int count = 0; count < cardData.displayCount; count++) {
                if (positionIndex >= availablePositions.Count) {
                    Debug.LogWarning("グリッドのサイズが不足しています。");
                    return;
                }

                // カードを生成
                GameObject card = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
                cardObjs.Add(card);
                card.transform.position = availablePositions[positionIndex];  // ランダムな位置に配置
                card.transform.SetParent(CardArea.transform, false);

                // 生成したカードにfrontImgとbackImgを設定する
                var spriteRenderer = card.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null) {
                    spriteRenderer.sprite = cardData.frontImg;  // 表面画像を設定
                }

                // バックイメージ（裏面）やオーディオクリップがあれば設定
                CardBehavior cardBehavior = card.GetComponent<CardBehavior>();  // カードの動作を管理するスクリプト（例として追加）
                if (cardBehavior != null) {
                    cardBehavior.backImg = cardData.backImg;  // 裏面画像を設定
                    cardBehavior.audioClip = cardData.audioSrc;  // クリック時の音声を設定
                    cardBehavior.isCorrect = cardData.isCorrect;  // 正解カードかどうか
                }

                positionIndex++;  // 次のランダムな位置に移動
            }
        }
    }



    public void ClearGird() {
        foreach (Transform child in CardArea.transform) {
            DestroyImmediate(child.gameObject);
        }
        foreach (var card in cardObjs) {
            DestroyImmediate(card);
        }

        // シーンをリロードすると消えないことがあるので、オブジェクト検索で明示的に削除する
        var gridParent = GameObject.Find("CardArea");
        // 子オブジェクトを全て削除
        foreach (Transform n in gridParent.transform) {
            DestroyImmediate(n.gameObject);
        }
        cardObjs.Clear();
    }

    public void SaveGridAsJSON() {
        var uuid = Guid.NewGuid().ToString();
        string folderPath = $"{DevConstants.QuestionDataFolder}/{templateType}/quiz";
        string filePath = Path.Combine(folderPath, $"{uuid}.json");

        // グリッドの情報をJSONに変換して保存
        Question quizData = new Question {
            cards = new List<Card>(),
            row = rowSize,
            column = columnSize,
            quizType = quizType
        };

        foreach (CardObject cardData in cards) {
            Card card = new Card {
                imgSrc = GetSpritePath(cardData.frontImg),
                backImgSrc = GetSpritePath(cardData.backImg),
                //audioSrc = cardData.audioSrc != null ? : null, TODO: 音声アセットのパスを取得する
                isCorrect = cardData.isCorrect,
                displayCount = cardData.displayCount,
            };
            quizData.cards.Add(card);
        }

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


/// <summary>
/// インスペクタのカスタム
/// </summary>
[CustomEditor(typeof(CardEditor))]
public class CardEditorManager : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        CardEditor editor = (CardEditor)target;

        GUILayout.Space(20);
        if (GUILayout.Button("小問を作成する。")) {
            editor.SaveGridAsJSON();   
        }
        GUILayout.Space(20);
        if(GUILayout.Button("カード再生成")) {
            editor.Initialize();
        }

        // グリッド一括クリアボタン
        if (GUILayout.Button("カードを一括クリア")) {
            editor.ClearGird();
            Debug.Log("グリッドを一括クリアしました");
        }
    }
}
