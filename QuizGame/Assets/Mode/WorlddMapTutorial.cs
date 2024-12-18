using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SaveDataInterface;

/// <summary>
/// このコンポーネントは、ワールドマップのチュートリアルを管理するコンポーネント．
/// チュートリアル用要素にアタッチされる． 
/// </summary>
public class WorlddMapTutorial : MonoBehaviour {
    public GameObject Parent;
    public GameObject WorldMap;
    public GameObject ButtonTutorialPanel;
    public GameObject MapTutorialPanel;

    public Button[] inactivateButtons;

    void Start() {

        // 初回ユーザーか否かを判定
        var uuid = PlayerPrefs.GetString("PlayerUUID");
        var playerData = SaveDataManager.LoadPlayerData(uuid);
        // ワールドマップが0 && エリアマップが0の場合は初回ユーザーとみなす
        bool isFirstUser = playerData.CurrentWorld == 0 && playerData.CurrentArea == 0;
        if (isFirstUser) {
            StartCoroutine(ShowFirstTutorial());
            foreach(Button btn in inactivateButtons) {
                btn.interactable = false;
            }
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
            foreach(Button btn in inactivateButtons) {
                btn.interactable = true;
            }
        });

    }

}
