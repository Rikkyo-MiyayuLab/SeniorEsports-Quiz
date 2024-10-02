using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizIntroduction : MonoBehaviour {
    
    public AudioClip soundEffect; // 再生するサウンド
    public float fadeDuration = 1.0f; // 透明度が変化する時間

    private AudioSource audioSource; // サウンドを再生するAudioSource
    private CanvasGroup canvasGroup; // CanvasGroupで透明度を制御


    void Start() {
        // AudioSourceコンポーネントを取得、もしくは追加
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

         // CanvasGroupコンポーネントを取得、もしくは追加
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null) {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 初期状態の透明度を0に設定
        canvasGroup.alpha = 0;
    }

    // Canvasが有効化されたときに呼ばれる
    void OnEnable() {
        // サウンドを再生
        if (soundEffect != null && audioSource != null) {
            audioSource.PlayOneShot(soundEffect);
        }
        StartCoroutine(FadeInCanvasGroup());
    }

    // CanvasGroupの透明度を徐々に上げるコルーチン
    private IEnumerator FadeInCanvasGroup() {
        float time = 0f;

        while (time < fadeDuration) {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / fadeDuration);
            yield return null; // フレームごとに待機
        }

        // 最終的に透明度を1に設定
        canvasGroup.alpha = 1f;
    }
}
