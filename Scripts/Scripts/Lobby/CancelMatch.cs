using UnityEngine;

public class CancelMatch : MonoBehaviour
{
    public void CancelMatchBtn(GameObject matchingView)
    {
        if (LobbyManager.instance.IsMatching)
        {
            // 매칭 취소
            LobbyManager.instance.CancelMatch();

            matchingView.SetActive(false);
        }
    }
}
