using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SaveDataInterface;


public class WorldMapView : MonoBehaviour {
   
    public GameObject[] Areas;
    private Sprite statusIconCurrent;
    private Sprite statusIconLocked;

   void Start() {
        statusIconCurrent = Resources.Load<Sprite>("System/nazo_icon");
        statusIconLocked = Resources.Load<Sprite>("System/lock_icon");

        for (int i = 0; i < Areas.Length; i++) {
            GameObject area = Areas[i];
            var areaData = area.GetComponent<WorldMapButton>();
            Button button = area.GetComponent<Button>();
            Image Icon = area.GetComponent<Image>();
            Image StatusIcon = area.transform.GetChild(1).GetComponent<Image>();

            if( i == GameStateManager.Instance.Player.CurrentWorld) {
                Icon.sprite = null;
                StatusIcon.sprite = statusIconCurrent;
                // ボタンの点滅を起動させる
                area.GetComponent<ButtonBlink>().StartBlinking();
            } else if(i > GameStateManager.Instance.Player.CurrentWorld) {
                Icon.sprite = statusIconLocked;
                StatusIcon.gameObject.SetActive(false);
                button.interactable = false;
                //アルファ値を1
                var color = Icon.color;
                color.a = 1.0f;
            } else {
                Icon.sprite = null;
                button.interactable = true;
                StatusIcon.gameObject.SetActive(false);
            }

            button.onClick.AddListener(() => {
                SceneManager.LoadScene("Scenes/Areas/" + areaData.SceneName);
            });
        }
   }
}
