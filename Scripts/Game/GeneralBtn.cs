using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GeneralBtn : MonoBehaviour
{
    // 장군 선택 스크립트
    GeneralUI generalUI;
    public GeneralUI Sg { get => generalUI; set => generalUI = value; }

    // 장군 번호
    int generalNum;
    public int GeneralNum { get => generalNum; set => generalNum = value; }

    // 장군 코드
    string generalCode;
    public string GeneralCode { get => generalCode; set => generalCode = value; }

    // 버튼 배경
    [SerializeField] Image btnBG;
    public Image BtnBG => btnBG;

    // 버튼 배경
    [SerializeField] Image nameBG;
    public Image NameBG => nameBG;

    // 장군 이름
    [SerializeField] TextMeshProUGUI generalName;
    public TextMeshProUGUI GeneralName => generalName;

    // 장군 이미지
    [SerializeField] Image generalImg;
    public Image GeneralImg => generalImg;

    // 장군 이미지
    [SerializeField] Image checkImg;
    public Image CheckImg => checkImg;

    public void Select()
    {
        if (LobbyManager.instance.IsMatching)
        {
            return;
        }

        generalUI.Select(this, generalCode);
    }

    public void OpenInfo(BaseEventData eventData)
    {
        PointerEventData pointerData = eventData as PointerEventData;

        if (pointerData != null && pointerData.button == PointerEventData.InputButton.Right)
        {
            generalUI.OpenInfoView(generalCode, generalNum);
        }
    }
}
