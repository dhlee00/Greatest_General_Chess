using UnityEngine;
using WebSocketSharp;

public class StartMatch : MonoBehaviour
{
    public void StartMatchBtn(GameObject matchingView)
    {
        if (!LobbyManager.instance.MyGeneral.IsNullOrEmpty())
        {
            // 매칭 시작
            LobbyManager.instance.StartMatch();

            matchingView.SetActive(true);
        }
    }
}
