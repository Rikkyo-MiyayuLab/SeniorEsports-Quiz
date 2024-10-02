using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using SaveDataInterface;

public class WorldMapButton : MonoBehaviour {
    public string SceneName;
    public string AreaName;
    public string AreaDescription;
    public TextMeshProUGUI AreaNameDisplayer;
    public TextMeshProUGUI AreaDescriptionDisplayer;
    [SerializeField]
    private PlayerData playerData;

    public float typingSpeed = 0.05f; // 1文字を表示する間隔時間（秒）
    private Coroutine typingCoroutine;

    void Start() {
    
        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { OnPointerEnter((PointerEventData)data); });
        
        var trigger = GetComponent<EventTrigger>();
        trigger.triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerExit;
        entry.callback.AddListener((data) => { OnPointerExit((PointerEventData)data); });
        trigger.triggers.Add(entry);

    }
    public void OnPointerEnter(PointerEventData eventData) {
        AreaNameDisplayer.text = AreaName;
        // 既存のコルーチンが動いている場合は停止
        if (typingCoroutine != null) {
            StopCoroutine(typingCoroutine);
        }

        // 新しくコルーチンを開始して、AreaDescriptionを1文字ずつ表示
        typingCoroutine = StartCoroutine(TypeText());
    }

    // テキストを1文字ずつ表示するコルーチン
    private IEnumerator TypeText() {
        AreaDescriptionDisplayer.text = "";  // 表示をクリア
        foreach (char letter in AreaDescription) {
            AreaDescriptionDisplayer.text += letter;  // 1文字追加
            yield return new WaitForSeconds(typingSpeed);  // 指定した時間待つ
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        AreaNameDisplayer.text = "せかいちず";
        AreaDescriptionDisplayer.text = "";
    }
}