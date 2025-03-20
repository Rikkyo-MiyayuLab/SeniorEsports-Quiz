using UnityEngine;

/// <summary>
/// TODO : Modalクラスを継承し、現TutorialViewerを移管する
/// </summary>
public class TutorialModal : Modal {
    public void Close() {
        Destroy(gameObject);
    }
    
}