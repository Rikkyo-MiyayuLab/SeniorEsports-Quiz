using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UtilFuncs;
using QuizDataInterface;
using EasyTransition;


public class WorldMapView : MonoBehaviour {
   
    public GameObject[] Areas;
    private Sprite statusIconCurrent;
    private Sprite statusIconLocked;
    [SerializeField]
    private GameObject SaveQuitePanel; // 途中保存の確認モーダル
    [SerializeField]
    private Button SaveQuiteButton; // 途中保存ボタン
    [SerializeField]
    private Button CloseSaveQuitePanelButton; // 途中保存モーダルを閉じるボタン
    [SerializeField]
    private float TransitionDuration = 1.0f;
    [SerializeField]
    private TransitionSettings Transition;
    private TransitionManager TransitionManager;

   void Start() {
        TransitionManager = TransitionManager.Instance();
        // 画像の読み込み
        statusIconCurrent = Resources.Load<Sprite>("System/nazo_icon");
        statusIconLocked = Resources.Load<Sprite>("System/lock_icon");
        SaveQuitePanel.SetActive(false);

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

        // #93 : 途中保存した問題がある場合、途中から再開するか確認するモーダルを表示
        var hasSaveQuestion = GameStateManager.Instance.Player.SaveQuizPath != null;
        if (hasSaveQuestion) {
            SaveQuitePanel.SetActive(true);
        }
        CloseSaveQuitePanelButton.onClick.AddListener(() => {
            SaveQuitePanel.SetActive(false);
        });
        SaveQuiteButton.onClick.AddListener(() => {
            SaveQuitePanel.SetActive(false);
            MoveSavedQuestion(GameStateManager.Instance.Player.SaveQuizPath, GameStateManager.Instance.Player.SaveQuestionIdx);
        });
   }

   private void MoveSavedQuestion(string quizPath, int questionIdx) {
        PlayerPrefs.SetString("QuizPath", quizPath);
        PlayerPrefs.SetInt("CurrentQuestionIdx", questionIdx);
        var quizData = DataLoaders.LoadJSON<QuizData>($"{Application.streamingAssetsPath}/{quizPath}");
        var viewerType = quizData.type;
        switch (viewerType) {
            case 1:
            case 2:
            case 3:
                TransitionManager.Transition("FourChoiceQuiz", Transition, TransitionDuration);
                //SceneManager.LoadScene("FourChoiceQuiz");
                break;
            case 4:
                TransitionManager.Transition("CardClickViewer", Transition, TransitionDuration);
                //SceneManager.LoadScene("CardClickViewer");
                break;
            case 5:
                TransitionManager.Transition("PhotoHuntViewer", Transition, TransitionDuration);
                //SceneManager.LoadScene("PhotoHuntViewer");
                break;
            default:
                break;
        }
   }
}
