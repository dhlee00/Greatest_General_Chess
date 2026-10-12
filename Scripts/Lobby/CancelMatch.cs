using UnityEngine;

public class CancelMatch : MonoBehaviour
{
    public void CancelMatchBtn(GameObject matchingView)
    {
        if (LobbyManager.instance.IsMatching)
        {
            // ¸ÅÄª Ãë¼Ò
            LobbyManager.instance.CancelMatch();

            matchingView.SetActive(false);
        }
    }
}
