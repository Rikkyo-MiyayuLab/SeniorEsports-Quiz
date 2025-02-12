using System.Collections.Generic;
using System;
using UnityEngine;
public class AreaClickHandler : MonoBehaviour
{
    private Action onClickAction;

    // クリック時に呼ばれるメソッド
    private void OnMouseDown()
    {
        if (onClickAction != null)
        {
            onClickAction.Invoke();
        }
    }

    // コールバックを設定するメソッド
    public void Setup(Action onClick)
    {
        onClickAction = onClick;
    }
}
