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

        bool isFirstUser = PlayerPrefs.GetInt("isFirstUser", 0) == 1; // ユーザー登録画面から渡される変数値
        string storyID = PlayerPrefs.GetString("StoryId");
        if(storyID == "Tutorial-001") {
            // なにもしない
        } else {
            return;
        }

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
                Debug.Log("Tutorial");
            } else {
                foreach(Button btn in inactivateButtons) {
                    btn.interactable = true;
                }
                Parent.gameObject.SetActive(false);
            }
        });
    }
    
}
