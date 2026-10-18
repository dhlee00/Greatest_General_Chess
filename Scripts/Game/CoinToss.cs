using UnityEngine;

public enum CoinTossResult
{
    firstTurn,
    secondTurn
}

public class CoinToss : MonoBehaviour
{
    [SerializeField] UIManager m_UI;

    [SerializeField] SpriteRenderer frontImg;

    [SerializeField] Sprite firstTurn_Img;
    [SerializeField] Sprite secondTurn_Img;

    CoinTossResult coinTossResult;
    public CoinTossResult CoinTossResult { get => coinTossResult; set => coinTossResult = value; }

    void SetCoin()
    {
        frontImg.sprite = coinTossResult == CoinTossResult.firstTurn ? firstTurn_Img : secondTurn_Img;
    }

    void EndToss()
    {
        transform.parent.gameObject.SetActive(false);

        // 게임 시작 출력
        m_UI.PrintStep(GameReadyStep.GameStart);
    }
}
