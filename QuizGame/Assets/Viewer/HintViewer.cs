using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using QuestionDataInterface;

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

    void Start() {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Init(string[] hints) {
        HintDatas = hints;
        HintAvailable = new bool[HintDatas.Length];
        for (int i = 0; i < hints.Length; i++) {
            int index = i; // ローカル変数にiの値を保存
            HintButtons[i].onClick.AddListener(() => ShowHint(index));
            HintAvailable[i] = true;
            // 子要素の鍵アイコンを非表示
            HintButtons[i].transform.GetChild(1).gameObject.SetActive(false);
        }
        CloseButton.onClick.AddListener(() => CloseHint());
    }

    private void ShowHint(int index) {
        Debug.Log("Show hint" + index);
        audioSource.PlayOneShot(BtnClickSE);
        if(HintAvailable[index]) {
            Hint.text = HintDatas[index];
            Title.text = $"ヒント その{index + 1}";
        }
    }

    private void CloseHint() {
        this.gameObject.SetActive(false);
        audioSource.PlayOneShot(BtnClickSE);
    }

}
