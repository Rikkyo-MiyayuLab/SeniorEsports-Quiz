using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackSceneButton : MonoBehaviour {
    // Start is called before the first frame update
    public string beforeSceneName;
    public bool useDestroy = true;
    void Start() {
        GetComponent<Button>().onClick.AddListener(() => {

            // BGM等、シーンをまたいで持ち込んでいるオブジェクトを破棄する
            if (useDestroy) {
                GameObject[] objs = GameObject.FindGameObjectsWithTag("DontDestroyOnSceneChange");
                foreach (GameObject obj in objs) {
                    Destroy(obj);
                }
            }
            Debug.Log("シーンを戻ります。");
            UnityEngine.SceneManagement.SceneManager.LoadScene(beforeSceneName);
        });
    }

}
