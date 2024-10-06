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
        audioSource = GetComponent<AudioSource>();

        StartCoroutine(TypeText(explanation));
        ExplanationImage.sprite = Resources.Load<Sprite>(imagePath);
        // TODO : まだ小問がある場合は、次の小問を表示するように
        if(isCorrectExplanation) {
            NextSceneButton.onClick.AddListener(() => {
                audioSource.PlayOneShot(BtnSE);
                // SceneManager.LoadScene("StoryViewer");
                PlayerPrefs.SetString("StoryId", NextStoryId);
                transitionManager.Transition("StoryViewer", transition, transitionDuration);

            });
        } else {
            NextSceneButton.GetComponent<BackSceneButton>().beforeSceneName = BeforeViewer;
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
