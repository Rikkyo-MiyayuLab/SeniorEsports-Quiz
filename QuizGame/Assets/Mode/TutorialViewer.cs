using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialViewer : MonoBehaviour {
    
    public GameObject[] Wrappers; // チュートリアルの各要素を格納するGameObject
    public bool isDisplay = false; // チュートリアルを表示中かどうかのフラグ

    private int currentWrapperIndex = 0; // 現在表示中の要素のインデックス

    void Start() {
        // 最初の要素を表示
        ShowWrapper(currentWrapperIndex);
    }

    //マウスクリックで次のチュートリアルを表示させる。

    // ��ュートリアル要素を表示する
    public void ShowWrapper(int index) {
        // 現在表示中の要素を非表示にする
        Wrappers[currentWrapperIndex].SetActive(false);
        // 新たに表示する要素を表示
        Wrappers[index].SetActive(true);
        currentWrapperIndex = index;
    }
}
