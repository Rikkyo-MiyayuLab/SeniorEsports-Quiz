using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhiotoHuntTutorial : TutorialViewer {

    public Canvas AnswerArea;
    public Canvas CompareArea;
    public Image RemainCounter;
    public GameObject StartPanel;

    protected override IEnumerator ShowTutorial(int index) {
        StartPanel.gameObject.SetActive(false);
        yield return new WaitForSeconds(1.0f); // 1秒待機
        TutorialPanels[index].gameObject.SetActive(true);

        if(index == 0) {
            AnswerArea.sortingOrder = 2;
            CompareArea.sortingOrder =2;
        } else if(index == 1) {
            AnswerArea.sortingOrder = 2;
            CompareArea.sortingOrder = 1;
        } else if(index == 2) {
            CompareArea.sortingOrder = 2;
            AnswerArea.sortingOrder = 1;
        } else if(index == 3) {
            AnswerArea.sortingOrder = 1;
            CompareArea.sortingOrder = 1;
            //RemainCounter.sortingOrder = 2;
        } 

        TutorialPanels[index].transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => {
            TutorialPanels[index].gameObject.SetActive(false);
            if (index < TutorialPanels.Length - 1) {
                StartCoroutine(ShowTutorial(index + 1));
            } else {
                Parent.gameObject.SetActive(false);
                StartPanel.gameObject.SetActive(true);
            }
        });
    }

}