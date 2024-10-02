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

        //Areas = GameObject.FindGameObjectsWithTag("AreaButton");

        for (int i = 0; i < Areas.Length; i++) {
            GameObject area = Areas[i];
            var areaData = area.GetComponent<WorldMapButton>();
            Button button = area.GetComponent<Button>();
            Image Icon = area.GetComponent<Image>();
            Image StatusIcon = area.transform.GetChild(1).GetComponent<Image>();

            if( i == playerData.CurrentWorld) {
                Icon.sprite = null;
                StatusIcon.sprite = statusIconCurrent;
                // ボタンの点滅を起動させる
                area.GetComponent<ButtonBlink>().StartBlinking();
            } else if(i > playerData.CurrentWorld) {
                Icon.sprite = statusIconLocked;
                StatusIcon.gameObject.SetActive(false);
                button.interactable = false;
                //アルファ値を1
                var color = Icon.color;
                color.a = 1.0f;
            } else {
                Icon.sprite = null;
                button.interactable = true;
                //OutLineコンポーネントを追加
                var outline = area.AddComponent<Outline>();
                // 茶色のアウトラインを設定
                outline.effectColor = new Color(0.5f, 0.3f, 0.1f);
                // アウトラインの太さを設定
                outline.effectDistance = new Vector2(10, 10);
            }

            button.onClick.AddListener(() => {
                SceneManager.LoadScene("Scenes/Areas/" + areaData.SceneName);
            });
        }
   }
}
