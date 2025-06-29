using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ヒント表示用ビューアのスクリプト
/// ヒント用モーダルにアタッチされる想定。
/// </summary>
public class HintViewer : MonoBehaviour {
    
    public Button[] HintButtons;
    public bool[] HintAvailable;
    public Button CloseButton;
    public TextMeshProUGUI Hint;
    public TextMeshProUGUI Title;
    public string[] HintDatas;
    public AudioClip BtnClickSE;
    private AudioSource audioSource;

    public int UsedHintCount { get; private set; } = 0;
    private bool[] hintShown;

    void Start() {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Init(string[] hints) {
        HintDatas = hints;
        HintAvailable = new bool[HintDatas.Length];
        hintShown = new bool[HintDatas.Length];
        for (int i = 0; i < hints.Length; i++) {
            int index = i; // ローカル変数にiの値を保存
            HintButtons[i].onClick.AddListener(() => ShowHint(index));
            HintAvailable[i] = true;
            hintShown[i] = false;
            // 子要素の鍵アイコンを非表示
            HintButtons[i].transform.GetChild(1).gameObject.SetActive(false);
        }
        CloseButton.onClick.AddListener(() => CloseHint());
        UsedHintCount = 0;
    }

    private void ShowHint(int index) {
        Debug.Log("Show hint" + index);
        audioSource.PlayOneShot(BtnClickSE);
        if(HintAvailable[index]) {
            Hint.text = HintDatas[index];
            Title.text = $"ヒント その{index + 1}";
            if (!hintShown[index]) {
                UsedHintCount++;
                hintShown[index] = true;
            }
        }
    }

    private void CloseHint() {
        this.gameObject.SetActive(false);
        audioSource.PlayOneShot(BtnClickSE);
    }

}
