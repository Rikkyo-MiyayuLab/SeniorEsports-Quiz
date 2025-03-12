using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;

/// <summary>
/// 説明文の表示や、セリフの表示などテキスト表示を行うUIにアタッチする。
/// </summary>
public class TextBox : MonoBehaviour {
    /// <summary>
    /// テキストのレンダリングが完了した際に呼び出されるイベント
    /// </summary>
    public Action OnRendered;

    /// <summary>
    /// テキストのレンダリングが開始された際に呼び出されるイベント
    /// </summary>
    public Action OnStartRendering;

    /// <summary>
    /// テキストボックス
    /// </summary>
    public TMP_Text TextField;
    public bool isTextRendering = false;
    public AudioClip RenderingSoundSource;
    [HideInInspector]
    public AudioSource RenderSoundPlayer;
    private Coroutine renderingCoroutine;


    private void Awake() {
        TextField = transform.GetComponent<TextMeshProUGUI>() as TMP_Text ?? transform.GetComponent<TextMeshPro>() as TMP_Text;
        RenderSoundPlayer = gameObject.AddComponent<AudioSource>();

    }

    

    /// <summary>
    /// テキストを1文字ずつ表示する
    /// </summary>
    /// <param name="text">表示したいテキスト全文</param>
    /// <param name="textSpeed">テキストの表示速度</param>
    public void Render(string text, float textSpeed) {
        renderingCoroutine = StartCoroutine(ProgressTextCoroutine(text, textSpeed));
    }

    public void ForceRender(string text) {
        StopCoroutine(renderingCoroutine);
        Delete();
        TextField.text = text;
    }

    public void Delete() {
        TextField.text = "";
    }

    
    /// <summary>
    /// テキストを1文字ずつ表示するコルーチン
    /// </summary>
    /// <param name="text">表示するテキスト</param>
    /// <param name="textSpeed">テキストの表示速度</param>
    private IEnumerator ProgressTextCoroutine(string text, float textSpeed) {
        try {
            OnStartRendering?.Invoke();
            TextField.text = "";
            isTextRendering = true; // テキストレンダリング中フラグをON
            foreach (var c in text) {
            TextField.text += c;
                if(RenderingSoundSource) {
                    RenderSoundPlayer.PlayOneShot(RenderingSoundSource);
                }
                yield return new WaitForSeconds(textSpeed);
            }
        } finally {
            // FIXME : StopCoroutineで下記が実行されない
            Debug.Log("Rendered");
            isTextRendering = false; // テキストレンダリング中フラグをOFF
            OnRendered?.Invoke();
        }
    }
}