using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using EasyTransition;

public class AreaButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [Tooltip("読み込むストーリーID")]
    public string storyId;
    public string sectionTitle;
    public int NextAreaIdx;
    public string storyDescription;
    [Header("トランジション設定")]
    public TransitionSettings transition;
    public float transitionDuration = 1.0f;
    private TextMeshProUGUI descriptionText;
    private TextMeshProUGUI sectionTitleText;
    private TransitionManager transitionManager;

    public float typingSpeed = 0.05f; // 1文字を表示する間隔時間（秒）
    private Coroutine typingCoroutine;

    
    void Start() {
        Button button = GetComponent<Button>();

        descriptionText = GameObject.Find("StoryInfo").GetComponent<TextMeshProUGUI>();
        sectionTitleText = GameObject.Find("SectionTitle").GetComponent<TextMeshProUGUI>();

        transitionManager = TransitionManager.Instance();


        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { OnPointerEnter((PointerEventData)data); });
        
        var trigger = GetComponent<EventTrigger>();
        trigger.triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerExit;
        entry.callback.AddListener((data) => { OnPointerExit((PointerEventData)data); });
        trigger.triggers.Add(entry);

        
        button.onClick.AddListener(() => {
            // ストーリーIDをシーン遷移前に渡す
            PlayerPrefs.SetString("StoryId", storyId);
            PlayerPrefs.SetString("CurrentArea", SceneManager.GetActiveScene().name);
            transitionManager.Transition("StoryViewer", transition, transitionDuration);
            PlayerPrefs.SetInt("CurrentAreaIdx", NextAreaIdx);
        });
    }

    public void OnPointerEnter(PointerEventData eventData) {
        sectionTitleText.text = sectionTitle;
        if (typingCoroutine != null) {
            StopCoroutine(typingCoroutine);
        }

        // 新しくコルーチンを開始して、AreaDescriptionを1文字ずつ表示
        typingCoroutine = StartCoroutine(TypeText());
    }

    public void OnPointerExit(PointerEventData eventData) {
        descriptionText.text = "";
        sectionTitleText.text = "";
    }

    private IEnumerator TypeText() {
        descriptionText.text = "";  // 表示をクリア
        foreach (char letter in storyDescription) {
            descriptionText.text += letter;  // 1文字追加
            yield return new WaitForSeconds(typingSpeed);  // 指定した時間待つ
        }
    }

}
