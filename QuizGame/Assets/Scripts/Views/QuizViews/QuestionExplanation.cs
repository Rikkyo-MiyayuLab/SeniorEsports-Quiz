using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EasyTransition;

public class QuestionExplanation : MonoBehaviour {
    
    public TextMeshProUGUI ExplanationText;
    public float typingSpeed = 0.2f; // 1文字を表示する間隔時間（秒）
    public Image ExplanationImage;
    public Button NextSceneButton;
    public string NextStoryId;
    public AudioClip BtnSE;
    public TransitionSettings transition;
    public float transitionDuration = 1.0f;
    public bool isCorrectExplanation = true;
    private TransitionManager transitionManager;
    private AudioSource audioSource;

    void Start() {
        transitionManager = TransitionManager.Instance();
        // localStorage経由で、解説データを取得
        var explanation = PlayerPrefs.GetString("Explanation");
        var imagePath = PlayerPrefs.GetString("ExplanationImage");
        var NextStoryId = PlayerPrefs.GetString("NextStoryId");
        var BeforeViewer = PlayerPrefs.GetString("CurrentViewer");
        var RemainQuestionSize = PlayerPrefs.GetInt("RemainQuestionSize");
        var CurrentQuestionIdx = PlayerPrefs.GetInt("CurrentQuestionIdx");
        var NextQuestionIdx = CurrentQuestionIdx + 1; // NOTE : issue-#78 : リスナーが２回実行されるので,次問遷移後のインデックスが狂う問題の対処。
        Debug.Log("RemainQuestionSize: " + RemainQuestionSize);
        PlayerPrefs.Save();
        audioSource = GetComponent<AudioSource>();

        StartCoroutine(TypeText(explanation));
        Sprite img = Resources.Load<Sprite>(imagePath);
        if(img != null) {
            ExplanationImage.sprite = img;
        } else {
            Debug.LogError("Image not found: " + imagePath);
            ExplanationImage.gameObject.SetActive(false); // 画像が見つからなかった場合は表示しない
        }

        if(isCorrectExplanation) {
            if(RemainQuestionSize > 0) {
                NextSceneButton.onClick.AddListener(() => {
                    audioSource.PlayOneShot(BtnSE);
                    //CurrentQuestionIdx++;
                    PlayerPrefs.SetInt("CurrentQuestionIdx", NextQuestionIdx);
                    transitionManager.Transition(BeforeViewer, transition, transitionDuration);
                });
                return;
            }
            NextSceneButton.onClick.AddListener(() => {
                audioSource.PlayOneShot(BtnSE);
                // SceneManager.LoadScene("StoryViewer");
                PlayerPrefs.SetString("StoryId", NextStoryId);
                transitionManager.Transition("StoryViewer", transition, transitionDuration);
            });

        } else {
            NextSceneButton.onClick.AddListener(() => {
                audioSource.PlayOneShot(BtnSE);
                Debug.Log("不正解なので、前の画面に戻ります。");
                transitionManager.Transition(BeforeViewer, transition, transitionDuration);
            });
        }

    }


    // テキストを1文字ずつ表示するコルーチン
    private IEnumerator TypeText(string description) {
        ExplanationText.text = "";  // 表示をクリア
        foreach (char letter in description) {
            ExplanationText.text += letter;  // 1文字追加
            yield return new WaitForSeconds(typingSpeed);  // 指定した時間待つ
        }
    }
}
