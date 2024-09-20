using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WorldMap : MonoBehaviour {
   
   public GameObject[] Areas;

   void Start() {
        Areas = GameObject.FindGameObjectsWithTag("AreaButton");

        foreach (GameObject area in Areas) {
            var areaData = area.GetComponent<AreaData>();
            Button button = area.GetComponent<Button>();
            button.onClick.AddListener(() => {
                SceneManager.LoadScene("Scenes/Areas/" + areaData.SceneName);
            });
        }
   }
}
