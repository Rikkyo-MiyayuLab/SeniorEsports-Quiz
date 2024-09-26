using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using PlayType4Interface;
using TMPro;
using QuestionDataInterface;
using EasyTransition;

/// <summary>
/// カードクリック式解答画面のビューアを表すクラス
/// </summary>
public class CardClickViewer : QuestionViewer<Question> {
    public List<GameObject> CardObjs = new List<GameObject>();
    public int rowSize = 0;
    public int columnSize = 0;
    public float margin = 0.2f;
    public int PairSize = 1;
    [System.Serializable]
    public class CardObjectData {
        public Sprite frontImg;
        public Sprite backImg;  // クリックで表示する裏面ソース（オプション）
        public AudioClip audioSrc;  // クリック時に鳴らす音（オプション）
        public bool isCorrect;  // singleモードで正解のカードかどうか（オプション）
        public int displayCount;  // このカードを何枚表示させるか（オプション

    }
    public List<CardObjectData> cards = new List<CardObjectData>();
    public AudioSource SEAudioSource;
    [SerializeField]
    private AudioClip defaultClickSE;
    [SerializeField]
    private AudioClip correctSE;
    [SerializeField]
    private AudioClip incorrectSE;
    [SerializeField]
    private AudioClip clearSE;
    [SerializeField]
    private AudioClip gameoverSE;
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private GameObject CardArea;

    void Start() {
        base.Start();
        SEAudioSource = gameObject.GetComponent<AudioSource>();

        Init();
    }


    public override void GetData() {
        base.CurrentQuestionData = LoadJSON<Question>(QuizData.quiz.questions[CurrentQuestionIndex]);
        rowSize = base.CurrentQuestionData.row;
        columnSize = base.CurrentQuestionData.column;
        BackgroundImageObj.sprite = Resources.Load<Sprite>(base.CurrentQuestionData.backgroundImage);
        base.CurrentBGM = Resources.Load<AudioClip>(base.CurrentQuestionData.bgm);
        margin = base.CurrentQuestionData.margin;
        PairSize = base.CurrentQuestionData.pairSize;
        // カードの情報を設定
        foreach (var card in base.CurrentQuestionData.cards) {
            CardObjectData cardObj = new CardObjectData {
                frontImg = Resources.Load<Sprite>(card.imgSrc),
                backImg = card.backImgSrc == null? null : Resources.Load<Sprite>(card.backImgSrc),
                audioSrc = card.audioSrc == null? defaultClickSE : Resources.Load<AudioClip>(card.audioSrc), //ない場合はデフォルトのSEを設定
                isCorrect = card.isCorrect,
                displayCount = card.displayCount
            };
            cards.Add(cardObj);
        }
    }

    /// <summary>
    /// 指定の行数、列数でカードを生成する
    /// </summary>
    public override void Render() {
        // 背景画像を設定する
        base.CurrentBackground = Resources.Load<Sprite>(base.CurrentQuestionData.backgroundImage);
        BackgroundImageObj.sprite = base.CurrentBackground;

        // BGMの設定（前と同じ場合はそのまま）
        if(base.AudioPlayer.clip != base.CurrentBGM) {
            base.AudioPlayer.clip = base.CurrentBGM;
            base.AudioPlayer.Play();
        }

        // Grid Layoutコンポーネントに設定された行数、列数、マージンを渡す
        var grid = CardArea.GetComponent<GridLayoutGroup>();
        grid.constraintCount = rowSize;
        grid.spacing = new Vector2(margin, margin);

        // 実際に生成するカード情報を生成。この時リストに追加する順番はランダムとし、displayCountの枚数分生成する
        List<CardObjectData> generateCards = new List<CardObjectData>();
        foreach (CardObjectData cardData in cards) {
            for (int count = 0; count < cardData.displayCount; count++) {
                generateCards.Add(cardData);
            }
        }
        // 生成したカード情報をシャッフルしてランダムな位置にカードを配置できるようにする
        List<CardObjectData> shuffledCards = new List<CardObjectData>(generateCards);
        System.Random rand = new System.Random();
        shuffledCards.Sort((a, b) => rand.Next(-1, 2));


        foreach (CardObjectData cardData in shuffledCards) {
            // カードを生成
            GameObject card = Instantiate(prefab, Vector3.zero, Quaternion.identity, CardArea.transform);
            card.GetComponent<SpriteRenderer>().sortingOrder = 1;
            // 生成したカードにfrontImgとbackImgを設定する
            var spriteRenderer = card.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) {
                spriteRenderer.sprite = cardData.frontImg;  // 表面画像を設定
            }
            // TODO;裏面画像設定
            card.AddComponent<CardObject>();
            card.GetComponent<CardObject>().isCorrect = cardData.isCorrect;
            card.GetComponent<CardObject>().audioSrc = cardData.audioSrc;

            // SetCardClickListener(card);
               // イベントトリガーを取得または追加
            EventTrigger trigger = card.GetComponent<EventTrigger>();
            if (trigger == null) {
                trigger = card.AddComponent<EventTrigger>();
            }

            // クリックイベントエントリーを作成
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;  // クリック時のイベント

            // リスナーにクリック処理を追加（ラムダ式で特定のカードを参照）
            entry.callback.AddListener((eventData) => { CardClickListener(card); });

            // EventTriggerにエントリーを追加
            trigger.triggers.Add(entry);
            CardObjs.Add(card);
        }
    }

    /// <summary>
    /// TODO : カードクリック時の効果音、エフェクトの実装
    /// </summary>
    public void CardClickListener(GameObject card) {
        var cardObj = card.GetComponent<CardObject>();
        SEAudioSource.PlayOneShot(cardObj.audioSrc);
        
        if (cardObj.isCorrect) {
            Debug.Log("正解");
            card.GetComponent<SpriteRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            SEAudioSource.PlayOneShot(correctSE);
            correctness.Add(true);
        } else {
            Debug.Log("不正解");
            SEAudioSource.PlayOneShot(incorrectSE);
            correctness.Add(false);
        }
        card.GetComponent<EventTrigger>().enabled = false;

        // correctnessに含まれるtrueの数がcards.isCorrectの数と一致したら結果画面を表示（小問終了）
        if(correctness.Count(b => b == true) == CardObjs.Count(c => c.GetComponent<CardObject>().isCorrect == true)) {
            SEAudioSource.PlayOneShot(clearSE);
            ResultModal.gameObject.SetActive(true);
            ResultModalImage.sprite = Resources.Load<Sprite>("Backgrounds/correctbg");
            NextQuestionButton.gameObject.SetActive(true);
            RetryButton.gameObject.SetActive(false);

            base.timer.PauseTimer();
        } else if(correctness.Count(b => b == false) == CardObjs.Count(c => c.GetComponent<CardObject>().isCorrect == false)) { // すべての不正解カードをクリックした場合は小問不正解として結果画面を表示
            SEAudioSource.PlayOneShot(gameoverSE);
            ResultModal.gameObject.SetActive(true);
            ResultModalImage.sprite = Resources.Load<Sprite>("Backgrounds/incorrectbg");
            NextQuestionButton.gameObject.SetActive(false);
            RetryButton.gameObject.SetActive(true);

            base.TotalIncorrectCount++;
            base.timer.PauseTimer();
        }
    }


    public override void Dispose() {
        ResultModal.gameObject.SetActive(false);
        foreach (var card in CardObjs) {
            Destroy(card);
        }
        CardObjs.Clear();
        cards.Clear();
        correctness.Clear();
        base.CurrentBackground = null;
        BackgroundImageObj.sprite = null;
    }
}
