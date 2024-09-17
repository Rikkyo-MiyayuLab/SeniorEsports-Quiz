using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using PlayType4Interface;
using TMPro;
using QuestionDataInterface;
using EasyTransition;

/// <summary>
/// カードクリック式解答画面のビューアを表すクラス
/// </summary>
public class CardClickViewer : MonoBehaviour {
    public int currentQuestionIndex = 0;
    public Sprite BackgroundImg;
    public List<GameObject> CardObjs = new List<GameObject>();
    public int rowSize = 0;
    public int columnSize = 0;
    public float margin = 0.2f;
    [System.Serializable]
    public class CardObject {
        public Sprite frontImg;
        public Sprite backImg;  // クリックで表示する裏面ソース（オプション）
        public AudioClip audioSrc;  // クリック時に鳴らす音（オプション）
        public bool isCorrect;  // singleモードで正解のカードかどうか（オプション）
        public int displayCount;  // このカードを何枚表示させるか（オプション

    }
    public List<CardObject> cards = new List<CardObject>();
    public TransitionSettings transition;
    public float transitionDuration = 0.5f;
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private Image backgroundImageObj;
    [SerializeField]
    private Canvas ResultModal;
    [SerializeField]
    private Button RetryButton;
    [SerializeField]
    private Button NextButton;
    [SerializeField]
    private Image ResultModalImage;
    [SerializeField]
    private GameObject CardArea;
    private TransitionManager transitionManager;
    private Question questionData;
    private QuestionData allQuestionData;
    private List<bool> correctness = new List<bool>();

    void Start() {
        transitionManager = TransitionManager.Instance();
        ResultModal.gameObject.SetActive(false);
        GetData();
        GenerateCards();
    }


    public void GetData() {
        //TODO :現在はダミーパス。結合時に遷移前のシーンから、問題データのパスを受け取るように変更する。
        var path = "Assets/StreamingAssets/QuestionData/4/bd73210a-ee5e-4bcf-9512-2f95d9e5eded.json"; // 大問定義情報が来る想定。
        allQuestionData = LoadJSON<QuestionData>(path);
        questionData = LoadJSON<Question>(allQuestionData.quiz.questions[currentQuestionIndex]);
        rowSize = questionData.row;
        columnSize = questionData.column;
        backgroundImageObj.sprite = Resources.Load<Sprite>(questionData.backgroundImage);
        margin = questionData.margin;
        // カードの情報を設定
        foreach (var card in questionData.cards) {
            CardObject cardObj = new CardObject {
                frontImg = Resources.Load<Sprite>(card.imgSrc),
                backImg = card.backImgSrc == null? null : Resources.Load<Sprite>(card.backImgSrc),
                audioSrc = card.audioSrc == null? null : Resources.Load<AudioClip>(card.audioSrc),
                isCorrect = card.isCorrect,
                displayCount = card.displayCount
            };
            cards.Add(cardObj);
        }
    }

    /// <summary>
    /// 指定の行数、列数でカードを生成する
    /// </summary>
    public void GenerateCards() {

        // Grid Layoutコンポーネントに設定された行数、列数、マージンを渡す
        var grid = CardArea.GetComponent<GridLayoutGroup>();
        grid.constraintCount = rowSize;
        grid.spacing = new Vector2(margin, margin);

        // 実際に生成するカード情報を生成。この時リストに追加する順番はランダムとし、displayCountの枚数分生成する
        List<CardObject> generateCards = new List<CardObject>();
        foreach (CardObject cardData in cards) {
            for (int count = 0; count < cardData.displayCount; count++) {
                generateCards.Add(cardData);
            }
        }
        // 生成したカード情報をシャッフルしてランダムな位置にカードを配置できるようにする
        List<CardObject> shuffledCards = new List<CardObject>(generateCards);
        System.Random rand = new System.Random();
        shuffledCards.Sort((a, b) => rand.Next(-1, 2));


        foreach (CardObject cardData in shuffledCards) {
            // カードを生成
            GameObject card = Instantiate(prefab, Vector3.zero, Quaternion.identity, CardArea.transform);
            card.GetComponent<SpriteRenderer>().sortingOrder = 1;
            // 生成したカードにfrontImgとbackImgを設定する
            var spriteRenderer = card.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) {
                spriteRenderer.sprite = cardData.frontImg;  // 表面画像を設定
            }
            CardObjs.Add(card);
        }
    }


    /// <summary>
    /// JSONデータを任意のクラスにデシリアライズして返す
    /// </summary>
    /// <typeparam name="T">デシリアライズしたいクラスの型</typeparam>
    /// <param name="path">jsonまでのパス</param>
    /// <returns>指定された型のオブジェクト</returns>
    private static T LoadJSON<T>(string path) {
        using (StreamReader r = new StreamReader(path)) {
            string json = r.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(json);
        }
    }

}
