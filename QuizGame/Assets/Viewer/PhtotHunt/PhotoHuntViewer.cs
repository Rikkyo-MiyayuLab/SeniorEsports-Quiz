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
    public GameObject pointPrefab;
    public int RemainCount;
    public TextMeshProUGUI RemainCountText;

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

        base.CurrentQuestionData = Viewer.LoadJSON<Question>($"{Application.streamingAssetsPath}/{QuizData.quiz.questions[CurrentQuestionIndex]}");

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

        //base.ResultModal.gameObject.SetActive(false);
        RemainCount = 0;
        RemainCountText.text = RemainCount.ToString();
    }


    public override void Render() {
        RemainCount = inCorrectImgData.points.Count;
        RemainCountText.text = RemainCount.ToString();
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
            GameObject pointObj = Instantiate(pointPrefab, incorrectImageObj.transform);
            // 透明にする。
            SpriteRenderer renderer = pointObj.GetComponent<SpriteRenderer>();
            renderer.color = new Color(0, 0, 0, 0); // 透明にする
            pointObj.transform.localPosition = new Vector2(point.x, point.y);
            pointObj.transform.localScale = new Vector3(point.width, point.height, 1);

            pointObj.AddComponent<AreaClickHandler>().Setup(() => {
                base.AudioPlayer.PlayOneShot(correctSE);

                //透明度を戻す（薄いピンク色）
                SpriteRenderer renderer = pointObj.GetComponent<SpriteRenderer>();
                renderer.color = new Color(1, 0, 0, 0.5f);

                Outline outline = pointObj.AddComponent<Outline>();
                outline.effectColor = Color.red;
                outline.effectDistance = new Vector2(1, 1);

                base.correctness.Add(true);
                RemainCount--;
                RemainCountText.text = RemainCount.ToString();

                // コライダーを無効化
                pointObj.GetComponent<Collider2D>().enabled = false;


                if(RemainCount == 0) {
                    base.timer.PauseTimer();
                    base.QuestionAnswered(true);
                }
            });

            ClickPoints.Add(pointObj);
        }
    }


    void Update() {
        // マウスクリックが発生した場合
        if (Input.GetMouseButtonDown(0) && RemainCount > 0) {
            // マウスのスクリーン座標を取得
            Vector2 mousePos = Input.mousePosition;

            // incorrectImageObjの範囲内にマウスがあるか確認
            RectTransform incorrectRectTransform = incorrectImageObj.GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(incorrectRectTransform, mousePos, Camera.main)) {
                bool clickedOnPoint = false;

                // ワールド座標でのマウス位置を取得
                Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

                // クリックされたポイントが範囲内か確認
                foreach (var point in ClickPoints) {
                    // ポイントのTransformを取得
                    Transform pointTransform = point.transform;

                    // ポイントの中心座標
                    Vector3 pointPosition = pointTransform.position;

                    // ポイントのスケールで幅と高さを取得
                    float pointWidth = pointTransform.localScale.x;
                    float pointHeight = pointTransform.localScale.y;

                    // ポイントの範囲を計算 (中心からのオフセット)
                    Vector3 minBounds = pointPosition - new Vector3(pointWidth / 2, pointHeight / 2, 0);
                    Vector3 maxBounds = pointPosition + new Vector3(pointWidth / 2, pointHeight / 2, 0);

                    // マウス位置がポイントの範囲内にあるか判定
                    if (worldMousePos.x >= minBounds.x && worldMousePos.x <= maxBounds.x &&
                        worldMousePos.y >= minBounds.y && worldMousePos.y <= maxBounds.y) {
                        clickedOnPoint = true;
                        break;
                    }
                }

                // クリックされたポイントが範囲外の場合
                if (!clickedOnPoint) {
                    OnClickOutsidePoints();
                }
            }
        }
    }


    private void OnClickOutsidePoints() {
        Debug.Log("指定のポイント以外がクリックされました。");
        base.TotalIncorrectCount++;
        base.AudioPlayer.PlayOneShot(incorrectSE);
    }
}
