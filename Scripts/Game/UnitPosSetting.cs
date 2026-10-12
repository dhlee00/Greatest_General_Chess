using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct SettingResult
{
    public bool Left;
    public bool Right;

    public SettingResult(bool _Left, bool _Right)
    {
        Left = _Left;
        Right = _Right;
    }
}

public class UnitPosSetting : MonoBehaviour
{
    [SerializeField] SettingResult settingResult = new SettingResult();

    [SerializeField] GameObject unitSettingView;
    public GameObject UnitSettingView => unitSettingView;

    [SerializeField] TextMeshProUGUI timeTxt;

    [SerializeField] GameObject waittingView;
    public GameObject WaittingView => waittingView;

    [SerializeField] TextMeshProUGUI waittingTxt;

    [SerializeField] float timer;

    void Update()
    {
        if (unitSettingView.activeSelf)
        {
            timer -= Time.deltaTime;

            timeTxt.text = $"남은 시간 : {string.Format("{0:00}", timer)}초";

            if (timer <= 0.0f)
            {
                DoneBtn();
            }
        }
    }

    // 완료
    public void DoneBtn()
    {
        GameManager.instance.SendUnitPosSettingRpc(true, settingResult.Left, settingResult.Right);

        unitSettingView.SetActive(false);

        waittingView.SetActive(true);

        StartCoroutine(PrintWaittingTxt());
    }

    IEnumerator PrintWaittingTxt()
    {
        while (!GameManager.instance.UnitSettingComplete)
        {
            waittingTxt.text = "대기중.";
            yield return new WaitForSeconds(0.33f);
            waittingTxt.text += ".";
            yield return new WaitForSeconds(0.33f);
            waittingTxt.text += ".";
            yield return new WaitForSeconds(0.33f);
        }

        waittingView.SetActive(false);
    }

    public void ClickChangeBtn(string side)
    {
        int playerNum = 1;
        if (!NetworkManager.Singleton.IsServer) playerNum = 2;

        if (side == "Left")
        {
            settingResult.Left = !settingResult.Left;
        }

        else if (side == "Right")
        {
            settingResult.Right = !settingResult.Right;
        }

        // 유닛 바꾸기 실행
        GameManager.instance.M_Unit.ChangeUnitPos(playerNum, side);
    }
}
