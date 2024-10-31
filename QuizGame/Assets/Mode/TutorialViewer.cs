using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialViewer : MonoBehaviour {

    public Canvas Parent;
    public GameObject[] TutorialPanels;

    public int CurrentTutorialIndex = 0;

    public Button[] inactivateButtons;

    void Start() {

        Parent.gameObject.SetActive(false);
        foreach (GameObject child in TutorialPanels) {
            child.gameObject.SetActive(false);
        }

        // 初回ユーザーか否かを判定
        var uuid = PlayerPrefs.GetString("PlayerUUID");
        var playerData = SaveDataManager.LoadPlayerData(uuid);
        // ワールドマップが0 && エリアマップが0の場合は初回ユーザーとみなす
        bool isFirstUser = playerData.CurrentWorld == 0 && playerData.CurrentArea == 0;

        if (isFirstUser) {
            Parent.gameObject.SetActive(true);
            StartCoroutine(ShowTutorial(CurrentTutorialIndex));
            foreach(Button btn in inactivateButtons) {
                btn.interactable = false;
            }
        } else {
            Parent.gameObject.SetActive(false);
        }
    }


    protected virtual IEnumerator ShowTutorial(int index) {
        yield return new WaitForSeconds(1.0f); // 1秒待機
        TutorialPanels[index].gameObject.SetActive(true);
        TutorialPanels[index].transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => {
            TutorialPanels[index].gameObject.SetActive(false);
            if (index < TutorialPanels.Length - 1) {
                StartCoroutine(ShowTutorial(index + 1));
            } else {
                foreach(Button btn in inactivateButtons) {
                    btn.interactable = true;
                }
                Parent.gameObject.SetActive(false);
            }
        });
    }
    
}
