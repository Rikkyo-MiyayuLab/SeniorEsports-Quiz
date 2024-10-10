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
    public Sprite DefaultCardSprite;
    public TextMeshProUGUI WarningText;
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
    [SerializeField]
    private List<GameObject> SelectedCards;

    void Start() {
        base.Start();
        SEAudioSource = gameObject.GetComponent<AudioSource>();
        SelectedCards = new List<GameObject>();
        Init();


        base.AnswerButton.interactable = false;
        WarningText.gameObject.SetActive(false);
        base.AnswerButton.onClick.AddListener(() => {
            // 問題のペアサイズと選択されたカードの数が一致しているか
            if(SelectedCards.Count == PairSize) {
                Judgement();
            } else {
                WarningText.text = $"カードを{PairSize}枚選んでいません。カードを選んでから押してください。";
                WarningText.gameObject.SetActive(true);
            }
        });
    }


    public override void GetData() {
        base.CurrentQuestionData = LoadJSON<Question>($"{Application.streamingAssetsPath}/{QuizData.quiz.questions[CurrentQuestionIndex]}");
        rowSize = base.CurrentQuestionData.row;
        columnSize = base.CurrentQuestionData.column;
        BackgroundImageObj.sprite = Resources.Load<Sprite>(base.CurrentQuestionData.backgroundImage);
        base.CurrentBGM = Resources.Load<AudioClip>(base.CurrentQuestionData.bgm);
        margin = base.CurrentQuestionData.margin;
        PairSize = base.CurrentQuestionData.pairSize;
        // カードの情報を設定
        foreach (var cardObj in base.CurrentQuestionData.cards) {
            CardObjectData cardData = new CardObjectData {
                frontImg = Resources.Load<Sprite>(cardObj.imgSrc),
                backImg = cardObj.backImgSrc == null? DefaultCardSprite : Resources.Load<Sprite>(cardObj.backImgSrc),
                audioSrc = cardObj.audioSrc == null? defaultClickSE : Resources.Load<AudioClip>(cardObj.audioSrc), //ない場合はデフォルトのSEを設定
                isCorrect = cardObj.isCorrect,
                displayCount = cardObj.displayCount
            };
            cards.Add(cardData);
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
            GameObject cardObj = Instantiate(prefab, Vector3.zero, Quaternion.identity, CardArea.transform);
            cardObj.GetComponent<SpriteRenderer>().sortingOrder = 1;

            var card = cardObj.AddComponent<CardObject>();
            card.isCorrect = cardData.isCorrect;
            card.audioSrc = cardData.audioSrc;
            card.frontImg = cardData.frontImg;
            card.backImg = cardData.backImg;

            // SetCardClickListener(cardObj);
               // イベントトリガーを取得または追加
            EventTrigger trigger = cardObj.GetComponent<EventTrigger>();
            if (trigger == null) {
                trigger = cardObj.AddComponent<EventTrigger>();
            }

            // クリックイベントエントリーを作成
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;  // クリック時のイベント

            // リスナーにクリック処理を追加（ラムダ式で特定のカードを参照）
            entry.callback.AddListener((eventData) => { CardClickListener(cardObj); });

            // EventTriggerにエントリーを追加
            trigger.triggers.Add(entry);
            CardObjs.Add(cardObj);
        }
    }

    /// <summary>
    /// カード１枚分のクリック処理
    /// </summary>
    public void CardClickListener(GameObject cardObj) {
        var card = cardObj.GetComponent<CardObject>();
        // 既に裏返されているカードの場合は、表にもどして、回数を１増やす。
        if(card.isFlipped) {
            // 選択済みカードのリストに含まれている場合は、リストから削除
            if (SelectedCards.Contains(cardObj)) {
                SelectedCards.Remove(cardObj);
                correctness.Remove(card.isCorrect);
            }
            card.FlipCard();

            if(SelectedCards.Count == PairSize) {
                base.AnswerButton.interactable = true;
            } else {
                base.AnswerButton.interactable = false;
            }
            return;
        }
        SEAudioSource.PlayOneShot(card.audioSrc);  // 効果音再生
        card.FlipCard();  // カードを裏返す
        base.ClickCount++;
        SelectedCards.Add(cardObj);
        if(SelectedCards.Count == PairSize) {
            base.AnswerButton.interactable = true;
        } else {
            base.AnswerButton.interactable = false;
        }

        if (card.isCorrect) {
            Debug.Log("正解");
            cardObj.GetComponent<SpriteRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            
            correctness.Add(true);
        } else {
            Debug.Log("不正解");
            correctness.Add(false);
        }
    }


    public void Judgement() {
        Debug.Log("Judgement");
        if (correctness.Count(b => b == true) == CardObjs.Count(c => c.GetComponent<CardObject>().isCorrect == true)) {
            PlayerPrefs.SetInt("UseThinkingScene", 1);
            base.QuestionAnswered(true);
            base.timer.PauseTimer();
        } else {
            base.TotalIncorrectCount++;
            base.timer.PauseTimer();
            PlayerPrefs.SetInt("UseThinkingScene", 1);
            base.QuestionAnswered(false);
        }
    }


    public override void Dispose() {
        //ResultModal.gameObject.SetActive(false);
        foreach (var cardObj in CardObjs) {
            Destroy(cardObj);
        }
        CardObjs.Clear();
        cards.Clear();
        correctness.Clear();
        base.CurrentBackground = null;
        BackgroundImageObj.sprite = null;
    }
}
