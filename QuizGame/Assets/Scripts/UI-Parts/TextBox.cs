using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// 説明文の表示や、セリフの表示などテキスト表示を行うUIにアタッチする。
/// </summary>
public class TextBox : MonoBehaviour {
    public Action OnRendered;
    public TMP_Text TextField;
    public bool isTextRendering = false;
    public AudioClip RendringSound;
    public AudioSource RenderingSoundSource;

    // TextFieldがTextMeshProUGUIかTextMeshProかを判定してキャスト
    private void Awake() {
        if (TextField is TextMeshProUGUI) {
            TextField = GetComponent<TextMeshProUGUI>();
        } else if (TextField is TextMeshPro) {
            TextField = GetComponent<TextMeshPro>();
        }
    }

    
    /// <summary>
    /// テキストを1文字ずつ表示するコルーチン
    /// </summary>
    /// <param name="text">表示するテキスト</param>
    /// <returns>コルーチン</returns>
    private IEnumerator ProgressTextCoroutine(string text) {
        TextField.text = "";
        isTextRendering = true; // テキストレンダリング中フラグをON
        foreach (var c in text) {
            characterTextField.text += c;
            if(RenderingSoundSource) {
                RendringSound.PlayOneShot(RenderingSoundSource);
            }
            yield return new WaitForSeconds(textSpeed);
        }
        isTextRendering = false; // テキストレンダリング中フラグをOFF
        
        OnRendered?.Invoke();
    }
}