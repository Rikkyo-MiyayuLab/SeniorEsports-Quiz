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
    private AudioClip correctSE;
    [SerializeField]
    private AudioClip incorrectSE;
    [SerializeField]
    private CorrectImage correctImgData;
    [SerializeField]
    private IncorrectImage inCorrectImgData;
   


    void Start() {
        base.Start();
        base.Init();
    }

    public override void GetData() {

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
        base.CurrentBackground = Resources.Load<Sprite>(base.CurrentQuestionData.backgroundImage);
        base.BackgroundImageObj.sprite = base.CurrentBackground;
        
        // BGMの設定（前と同じ場合はそのまま）
        if (base.CurrentBGM != null && base.AudioPlayer.clip != base.CurrentBGM) {
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
        RectTransform incorrectImgRect = incorrectImageObj.GetComponent<RectTransform>();
        Vector2 incorrectImgSize = incorrectImgRect.sizeDelta;

        foreach(var point in inCorrectImgData.points) {
            GameObject pointObj = new GameObject("ClickPoint");
            RectTransform rect = pointObj.AddComponent<RectTransform>();
            rect.SetParent(inCorrectImgArea.transform, false);

            // 画像のスケールに合わせてポイントの位置とサイズを補正
            float relativeX = point.x / inCorrectImgData.rect.width * incorrectImgSize.x;
            float relativeY = point.y / inCorrectImgData.rect.height * incorrectImgSize.y;
            float relativeWidth = point.width / inCorrectImgData.rect.width * incorrectImgSize.x;
            float relativeHeight = point.height / inCorrectImgData.rect.height * incorrectImgSize.y;

            rect.anchoredPosition = new Vector2(relativeX, relativeY);
            rect.sizeDelta = new Vector2(relativeWidth, relativeHeight);

            Image pointImage = pointObj.AddComponent<Image>();
            pointImage.color = Color.white;
            pointImage.color = new Color(pointImage.color.r, pointImage.color.g, pointImage.color.b, 0.5f);

            Button button = pointObj.AddComponent<Button>();
            button.onClick.AddListener(() => {
                base.AudioPlayer.PlayOneShot(correctSE);

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
                    base.timer.PauseTimer();
                }
            });

            ClickPoints.Add(pointObj);
        }
    }


    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector2 mousePos = Input.mousePosition;
            bool clickedOnPoint = false;

            foreach (var point in ClickPoints) {
                RectTransform rectTransform = point.GetComponent<RectTransform>();
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos, Camera.main)) {
                    clickedOnPoint = true;
                    break;
                }
            }

            if (!clickedOnPoint) {
                OnClickOutsidePoints();
            }
        }
    }

    private void OnClickOutsidePoints() {
        Debug.Log("指定のポイント以外がクリックされました。");
        base.TotalIncorrectCount++;
        base.AudioPlayer.PlayOneShot(incorrectSE);
    }
}
