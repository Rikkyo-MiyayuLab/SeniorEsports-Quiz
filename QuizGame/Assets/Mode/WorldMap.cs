using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SaveDataInterface;


public class WorldMap : MonoBehaviour {
   
   public GameObject[] Areas;
   public PlayerData playerData;

   private Sprite statusIconCurrent;
    private Sprite statusIconLocked;

   void Start() {
        statusIconCurrent = Resources.Load<Sprite>("System/nazo_icon");
        statusIconLocked = Resources.Load<Sprite>("System/lock_icon");

        // ユーザーデータをロード
        var uuid = PlayerPrefs.GetString("PlayerUUID");
        bool isFirstTime = PlayerPrefs.GetInt("FirstTime") != 0;
        playerData = SaveDataManager.LoadPlayerData(uuid);

        Areas = GameObject.FindGameObjectsWithTag("AreaButton");

        for (int i = 0; i <Areas.Length; i++) {
            GameObject area = Areas[i];
            var areaData = area.GetComponent<WorldMapButton>();
            Button button = area.GetComponent<Button>();
            Image StatusIcon = areaData.GetComponent<Image>();

            if( i == playerData.CurrentWorld) {
                StatusIcon.sprite = statusIconCurrent;
            } else if(i > playerData.CurrentWorld) {
                StatusIcon.sprite = statusIconLocked;
                button.interactable = false;
            } else {
                StatusIcon.sprite = null;
                button.interactable = true;
            }

            button.onClick.AddListener(() => {
                SceneManager.LoadScene("Scenes/Areas/" + areaData.SceneName);
            });
        }
   }
}
