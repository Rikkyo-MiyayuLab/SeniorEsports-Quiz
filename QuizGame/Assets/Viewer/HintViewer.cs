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


    public void Init(string[] hints) {
        HintDatas = hints;
        HintAvailable = new bool[HintDatas.Length];
        for (int i = 0; i < hints.Length; i++) {
            HintButtons[i].onClick.AddListener(() => ShowHint(i));
            HintAvailable[i] = true;
            // 鍵アイコンを非表示
            HintButtons[i].gameObject.GetComponentInChildren<Image>().enabled = false;
        }
        CloseButton.onClick.AddListener(() => CloseHint());
    }

    private void ShowHint(int index) {
        if(HintAvailable[index]) {
            Hint.text = HintDatas[index];
            Title.text = $"ヒント その{index + 1}";
        }
    }

    private void CloseHint() {
        this.gameObject.SetActive(false);
    }

}
