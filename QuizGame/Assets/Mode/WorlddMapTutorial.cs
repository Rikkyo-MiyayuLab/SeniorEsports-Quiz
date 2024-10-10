using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// このコンポーネントは、ワールドマップのチュートリアルを管理するコンポーネント．
/// チュートリアル用要素にアタッチされる． 
/// </summary>
public class WorlddMapTutorial : MonoBehaviour {
    public GameObject Parent;
    public GameObject WorldMap;
    public GameObject ButtonTutorialPanel;
    public GameObject MapTutorialPanel;

    void Start() {

        // 初回ユーザーか否かを判定
        bool isFirstUser = PlayerPrefs.GetInt("isFirstUser", 0) == 1; // ユーザー登録画面から渡される変数値
        if (isFirstUser) {
            StartCoroutine(ShowFirstTutorial());
        } else {
            Parent.gameObject.SetActive(false);
        }

    }

    private IEnumerator ShowFirstTutorial() {
        Parent.gameObject.SetActive(false);
        ButtonTutorialPanel.SetActive(false);
        // 2秒まってからチュートリアル最初の要素を表示
        yield return new WaitForSeconds(1.0f);
        Parent.gameObject.SetActive(true);
        MapTutorialPanel.gameObject.SetActive(true);
        WorldMap.GetComponent<SpriteRenderer>().sortingOrder = 3;
        MapTutorialPanel.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => {
            StartCoroutine(ShowSecondTutorial());
        });
    }


    private IEnumerator ShowSecondTutorial() {
         yield return new WaitForSeconds(1.0f);

        MapTutorialPanel.gameObject.SetActive(false);
        WorldMap.GetComponent<SpriteRenderer>().sortingOrder = 1;
        ButtonTutorialPanel.gameObject.SetActive(true);

        ButtonTutorialPanel.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => {

            ButtonTutorialPanel.gameObject.SetActive(false);
            Parent.gameObject.SetActive(false);

        });

    }

}
