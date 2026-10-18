using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class UserInfo : MonoBehaviour
{
    [SerializeField] GeneralUI generalUI;

    [SerializeField] Scrollbar scrollbar;

    // 창 활성/비활성
    public void ActiveView(Image view)
    {
        view.gameObject.SetActive(!view.gameObject.activeSelf);

        if (view.gameObject.activeSelf && !LobbyManager.instance.MyGeneral.IsNullOrEmpty())
        {
            view.sprite = generalUI.GeneralImageList[LobbyManager.instance.MyGeneral].illustrationImg;
        }
    }

    public void FixSize()
    {
        scrollbar.size = 0.0f;
    }
}
