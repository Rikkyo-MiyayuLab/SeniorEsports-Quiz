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
    public string storyDescription;
    public Sprite Thumbnail;
    [Header("トランジション設定")]
    public TransitionSettings transition;
    public float transitionDuration = 1.0f;

    private Image thumbnailImage;
    private TextMeshProUGUI descriptionText;
    private TransitionManager transitionManager;

    
    void Start() {
        Button button = GetComponent<Button>();

        thumbnailImage = GameObject.Find("StoryThumbnail").GetComponent<Image>();
        descriptionText = GameObject.Find("StoryInfo").GetComponent<TextMeshProUGUI>();

        transitionManager = TransitionManager.Instance();

        
        button.onClick.AddListener(() => {
            // ストーリーIDをシーン遷移前に渡す
            PlayerPrefs.SetString("StoryId", storyId);
            transitionManager.Transition("StoryViewer", transition, transitionDuration);
        });
    }

    public void OnPointerEnter(PointerEventData eventData) {
            thumbnailImage.sprite = Thumbnail;
            descriptionText.text = storyDescription;
        }

    public void OnPointerExit(PointerEventData eventData) {
        thumbnailImage.sprite = null;
        descriptionText.text = "";
    }

}
