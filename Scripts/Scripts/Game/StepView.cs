using UnityEngine;
using UnityEngine.UI;

public enum GameReadyStep
{
    UnutPosSetting,
    CoinToss,
    GameStart
}

public class StepView : MonoBehaviour
{
    [SerializeField] UIManager m_UI;

    [SerializeField] Sprite[] step_Img;

    Image image;

    [SerializeField] GameReadyStep step;
    public GameReadyStep Step { get => step; set => step = value; }

    void Start()
    {
        image = GetComponent<Image>();
    }

    void EndAnim()
    {
        switch (step)
        {
            case GameReadyStep.UnutPosSetting:
                m_UI.UnitPosSetting.UnitSettingView.SetActive(true);
                image.sprite = step_Img[1];
                break;

            case GameReadyStep.CoinToss:
                GameManager.instance.StartCoinToss();
                image.sprite = step_Img[2];
                break;

            case GameReadyStep.GameStart:
                // 준비 완료
                GameManager.instance.SendReadyCompleteRpc(true);
                break;
        }

        gameObject.SetActive(false);
    }
}
