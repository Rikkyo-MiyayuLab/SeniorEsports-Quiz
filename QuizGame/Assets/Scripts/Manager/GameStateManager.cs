using UnityEngine;
using SaveDataInterface;


/// <summary>
/// アプリ全体の状態管理を行うクラス
/// </summary>
public class GameStateManager : MonoBehaviour {
    
    private static GameStateManager instance;
    public static GameStateManager Instance {
        get {
            if (instance == null) {
                GameObject obj = new GameObject("GameStateManager");
                instance = obj.AddComponent<GameStateManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject); // すでに存在する場合は破棄
        }
    }

    /// <summary>
    /// チュートリアルユーザーかどうか
    /// </summary>
    public bool IsTutorialUser = false;
    public PlayerData Player;
}
