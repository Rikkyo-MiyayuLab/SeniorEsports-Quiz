using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FourChoiceQuizTutorial : TutorialViewer {

    public Canvas OptionButtons;
    public Button AnswerButton;
    public GameObject QuestionArea;
    public Canvas Utility;
    public Button HintButton;
    public GameObject StartPanel;

    protected override IEnumerator ShowTutorial(int index) {
        StartPanel.gameObject.SetActive(false);
        yield return new WaitForSeconds(1.0f); // 1秒待機
        TutorialPanels[index].gameObject.SetActive(true);
    
        //ゲームビューアー用カスタム
        if(index == 0) {
            QuestionArea.GetComponent<SpriteRenderer>().sortingOrder = 2;
        } else if(index == 1) {
            Utility.sortingOrder = 2;
            QuestionArea.GetComponent<SpriteRenderer>().sortingOrder = 1;
            OptionButtons.sortingOrder = 6;
        } else if(index >= 2) {
            Utility.sortingOrder = 6;
            QuestionArea.GetComponent<SpriteRenderer>().sortingOrder = 1;
            OptionButtons.sortingOrder = 1;
        } else {
            // 元に戻す
            QuestionArea.GetComponent<SpriteRenderer>().sortingOrder = 1;
            OptionButtons.sortingOrder = 1;
            Utility.sortingOrder = 2;
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