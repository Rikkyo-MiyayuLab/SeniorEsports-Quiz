using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class QuestionDescriptDisplay : MonoBehaviour {
    
    public Button OpneCloseButton;
    public Sprite OpenBtnSprite;
    public Sprite CloseBtnSprite;

    public bool isOpen = true;
    public float slideDuration = 0.5f;  // スライドするのにかかる時間（秒）
    public Vector3 closedPosition; // 閉じた時の位置（画面外）
    public Vector3 openPosition;   // 開いた時の位置（元の位置）
    private RectTransform rectTransform;

    void Start() {
        // RectTransformを取得
        rectTransform = GetComponent<RectTransform>();
        
        // ボタンにリスナーを追加
        OpneCloseButton.onClick.AddListener(PanelMove);

        // 初期位置を設定（元の位置を開いた位置とする）
        openPosition = rectTransform.anchoredPosition;
        closedPosition = openPosition + new Vector3(-rectTransform.rect.width, 0, 0); // 左へ画面外分だけ移動させる
    }

    private void PanelMove() {
        if (isOpen) {
            ClosePanel();
            OpneCloseButton.image.sprite = OpenBtnSprite;
        } else {
            OpenPanel();
            OpneCloseButton.image.sprite = CloseBtnSprite;
        }
    }

    // パネルを左にスライドして閉じる
    private void ClosePanel() {
        if (isOpen) {
            StartCoroutine(SlideToPosition(closedPosition));
            isOpen = false;
        }
    }

    // パネルを元の位置にスライドして開く
    private void OpenPanel() {
        if (!isOpen) {
            StartCoroutine(SlideToPosition(openPosition));
            isOpen = true;
        }
    }

    // コルーチンでスライドアニメーションを行う
    private IEnumerator SlideToPosition(Vector3 targetPosition) {
        float elapsedTime = 0f;
        Vector3 startingPosition = rectTransform.anchoredPosition;

        while (elapsedTime < slideDuration) {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / slideDuration);
            rectTransform.anchoredPosition = Vector3.Lerp(startingPosition, targetPosition, t);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
    }
}
