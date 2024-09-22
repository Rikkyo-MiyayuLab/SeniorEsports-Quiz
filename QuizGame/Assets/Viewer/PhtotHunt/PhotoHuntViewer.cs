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
using PlayType5Interface;
using TMPro;
using QuestionDataInterface;
using EasyTransition;
public class PhotoHuntViewer : QuestionViewer<Question>{
    
    public Sprite correctImg; //比較用画像
    public Sprite inCorrectImg; //実際の解答用画像[SerializeField]
    public Canvas correctImgArea;
    public Canvas inCorrectImgArea;
    public Image correctImageObj;
    public Image incorrectImageObj;
    public List<GameObject> ClickPoints;

    [SerializeField]
    private AudioClip defaultClickSE;
    [SerializeField]
    private CorrectImage correctImgData;
    [SerializeField]
    private IncorrectImage inCorrectImgData;
   


    void Start() {
        base.Start();
        base.RetryButton.onClick.AddListener(() => {
            base.ResultModal.gameObject.SetActive(false);
            //TODO : 現在の小問をリトライする処理
        });

        base.NextQuestionButton.onClick.AddListener(() => {
            base.ResultModal.gameObject.SetActive(false);
            if(base.CurrentQuestionIndex < base.QuizData.quiz.questions.Count - 1) { // 次問遷移
                NextQuestion();
            } else { // 大問終了
                base.TransitionManager.Transition(base.Transition, base.TransitionDuration);
                base.TransitionManager.onTransitionEnd = () => {
                    //TODO : 全ての小問を終えた後、解説用ストーリー画面へ遷移する処理
                    // ここで、ストーリーIDを指定して、ストーリー用シーンへ遷移する
                    // Ex). SceneManager.LoadScene("StoryScene");
                    Debug.Log("大問終了遷移");
                };
            }
        });

        base.Init();
    }

    public override void GetData() {
        //TODO :現在はダミーパス。結合時に遷移前のシーンから、問題データのパスを受け取るように変更する。
        var path = "Assets/StreamingAssets/QuestionData/5/34842a64-9b24-40e9-a27c-81ee6372f397.json"; // 大問定義情報が来る想定。
        base.QuizData = Viewer.LoadJSON<QuestionData>(path);
        base.CurrentQuestionData = Viewer.LoadJSON<Question>(base.QuizData.quiz.questions[base.CurrentQuestionIndex]);

        correctImgData = base.CurrentQuestionData.correct;
        inCorrectImgData = base.CurrentQuestionData.incorrect;
        base.CurrentBGM = Resources.Load<AudioClip>(base.CurrentQuestionData.bgm);
    }


    public override void Dispose() {
        // 画像の削除
        correctImageObj.sprite = null;
        incorrectImageObj.sprite = null;
        base.BackgroundImageObj.sprite = null;

        // ポイントの削除
        foreach(GameObject ClickPoint in ClickPoints) {
            ClickPoint.gameObject.SetActive(false);
            Destroy(ClickPoint.gameObject);
        }
        ClickPoints.Clear();
        //inCorrectImgArea.ForceUpdateCanvases();

        base.correctness.Clear();

        base.ResultModal.gameObject.SetActive(false);
    }


    public override void Render() {
        // 背景画像の設定
        // 背景画像を設定する
        base.CurrentBackground = Resources.Load<Sprite>(base.CurrentQuestionData.backgroundImage);
        base.BackgroundImageObj.sprite = base.CurrentBackground;
        
        // BGMの設定（前と同じ場合はそのまま）
        if(base.CurrentBGM != null && base.AudioPlayer.clip != base.CurrentBGM) {
            base.AudioPlayer.clip = base.CurrentBGM;
            base.AudioPlayer.Play();
        }

        // 問題画像の設定
        correctImg = Resources.Load<Sprite>(correctImgData.src);
        correctImageObj.GetComponent<Image>().sprite = correctImg;
        correctImageObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(correctImgData.rect.x, correctImgData.rect.y);
        correctImageObj.GetComponent<RectTransform>().sizeDelta = new Vector2(correctImgData.rect.width, correctImgData.rect.height);

        // 解答用画像の設定
        inCorrectImg = Resources.Load<Sprite>(inCorrectImgData.src);
        incorrectImageObj.GetComponent<Image>().sprite = inCorrectImg;
        incorrectImageObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(inCorrectImgData.rect.x, inCorrectImgData.rect.y);
        incorrectImageObj.GetComponent<RectTransform>().sizeDelta = new Vector2(inCorrectImgData.rect.width, inCorrectImgData.rect.height);
        
        // 解答用画像上のポイントの設定
        foreach(var point in inCorrectImgData.points) {
            GameObject pointObj = new GameObject("ClickPoint");
            RectTransform rect = pointObj.AddComponent<RectTransform>();
            rect.SetParent(inCorrectImgArea.transform, false);
            rect.anchoredPosition = new Vector2(point.x, point.y);
            rect.sizeDelta = new Vector2(point.width, point.height);

            Image pointImage = pointObj.AddComponent<Image>();
            pointImage.color = Color.white;
            pointImage.color = new Color(pointImage.color.r, pointImage.color.g, pointImage.color.b, 0.5f);

            Button button = pointObj.AddComponent<Button>();
            button.onClick.AddListener(() => {
                base.AudioPlayer.PlayOneShot(defaultClickSE);
            
                Outline outline = pointObj.AddComponent<Outline>();
                outline.effectColor = Color.red;
                outline.effectDistance = new Vector2(1, 1);

                base.correctness.Add(true);

                // クリックイベントを無効化
                pointObj.GetComponent<Button>().interactable = false;

                if(base.correctness.Count == inCorrectImgData.points.Count) { //正解ポイントをすべて発見
                    base.ResultModal.gameObject.SetActive(true);
                    base.RetryButton.gameObject.SetActive(false);
                    base.ResultModalImage.sprite = Resources.Load<Sprite>("Backgrounds/correctbg");
                }
            });

            ClickPoints.Add(pointObj);
        }
    }
}
