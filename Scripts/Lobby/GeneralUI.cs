using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct GeneralInfo
{
    public string titleName;
    public string fullName;
    public string biography;

    public SkillInfo[] skillInfo;
}

public struct SkillInfo
{
    public string name;
    public string info;
    public Sprite icon;
}

public class GeneralUI : MonoBehaviour
{
    // 장군 데이터
    GeneralData[] generalData;

    // 장군 이미지 리스트
    Dictionary<string, GeneralImage> generalImageList = new Dictionary<string, GeneralImage>();
    public Dictionary<string, GeneralImage> GeneralImageList => generalImageList;

    // 장군 소개 리스트
    Dictionary<string, GeneralInfo> generalInfoList = new Dictionary<string, GeneralInfo>();

    // 로비 배경
    [SerializeField] Image lobbyBG;

    // 유저 프로필 이미지
    [SerializeField] Image userProfileImg;

    // 계정 정보 창 배경
    [SerializeField] Image userInfoViewBG;

    [Header("Select View")]
    
    // 장군 선택 창
    [SerializeField] GameObject selectView;

    // 장군 선택 버튼 프리팹
    [SerializeField] GameObject generalBtnPrefab;

    // 장군 선택 버튼 리스트
    [SerializeField] GameObject generalBtnList;

    [SerializeField] Sprite originBG;        // 기본 배경
    [SerializeField] Sprite originNameBG;    // 기본 이름 배경
    [SerializeField] Sprite selectBG;        // 선택 시 배경
    [SerializeField] Sprite selectNameBG;    // 선택 시 이름 배경
    [SerializeField] GameObject warningTxt;  // 경고 문구

    // 선택한 장군 버튼
    GeneralBtn selectedBtn;

    [Header("Info View")]

    // 장군 세부 정보 창
    [SerializeField] GameObject InfoView;

    int generalNum;

    [SerializeField] TextMeshProUGUI nameTxt;       // 이름
    [SerializeField] Image image;              // 프로필 이미지
    [SerializeField] TextMeshProUGUI fullNameTxt;   // 전체 이름
    [SerializeField] TextMeshProUGUI biographyTxt;  // 일대기
    
    [Header("Skill 1")]
    [SerializeField] Image skill_1_Icon;            // 스킬 1 아이콘
    [SerializeField] TextMeshProUGUI skill_1_Name;  // 스킬 1 이름
    [SerializeField] TextMeshProUGUI skill_1_Info;  // 스킬 1 설명

    [Header("Skill 2")]
    [SerializeField] Image skill_2_Icon;            // 스킬 2 아이콘
    [SerializeField] TextMeshProUGUI skill_2_Name;  // 스킬 2 이름
    [SerializeField] TextMeshProUGUI skill_2_Info;  // 스킬 2 설명

    void Awake()
    {
        // 장군 데이터 불러오기
        generalData = Resources.LoadAll<GeneralData>("GeneralData");
    }

    void Start()
    {
        int num = 0;

        // 불러온 장군들의 선택 버튼 생성
        foreach (var data in generalData)
        {
            GameObject btn = Instantiate(generalBtnPrefab, generalBtnList.transform);
            GeneralBtn generalBtn = btn.GetComponent<GeneralBtn>();

            generalBtn.GeneralNum = num;
            generalBtn.GeneralCode = data.GeneralCode;
            generalBtn.GeneralImg.sprite = data.GeneralImage.introduceImg;
            generalBtn.GeneralName.text = data.GeneralName;
            generalBtn.Sg = this;

            GeneralInfo generalInfo = new GeneralInfo
            {
                titleName = data.GeneralName,
                fullName = data.GeneralFullName,
                biography = data.GeneralBiography,

                skillInfo = new SkillInfo[]
                {
                    new SkillInfo
                    {
                        name = data.SkillData[0].SkillName,
                        info = data.SkillData[0].SkillInfo,
                        icon = data.SkillData[0].SkillIcon
                    },

                    new SkillInfo
                    {
                        name = data.SkillData[1].SkillName,
                        info = data.SkillData[1].SkillInfo,
                        icon = data.SkillData[1].SkillIcon
                    }
                }
            };
            
            generalImageList.Add(data.GeneralCode, data.GeneralImage);
            generalInfoList.Add(data.GeneralCode, generalInfo);

            num++;
        }
    }

    void Update()
    {
        if (InfoView.activeSelf)
        {
            if (Input.mouseScrollDelta.y > 0)
            {
                InfoLeftBtn();
            }

            else if (Input.mouseScrollDelta.y < 0)
            {
                InfoRightBtn();
            }
        }
    }

    // 장군 선택
    public void Select(GeneralBtn selectBtn, string generalCode)
    {
        if (selectedBtn != null)
        {
            selectedBtn.BtnBG.sprite = originBG;
            selectedBtn.NameBG.sprite = originNameBG;
            selectedBtn.CheckImg.gameObject.SetActive(false);
            selectedBtn = selectBtn;
        }

        else
        {
            selectedBtn = selectBtn;
        }

        selectedBtn.BtnBG.sprite = selectBG;
        selectedBtn.NameBG.sprite = selectNameBG;
        selectedBtn.CheckImg.gameObject.SetActive(true);

        // 플레이어 데이터에 저장
        LobbyManager.instance.MyGeneral = generalCode;

        // 로비 배경 및 유저 프로필 이미지 변경
        userProfileImg.sprite = generalImageList[generalCode].profileImg;
        lobbyBG.sprite = generalImageList[generalCode].illustrationImg;
        userInfoViewBG.sprite = generalImageList[generalCode].illustrationImg;
    }

    public void PrintGeneralInfo(string generalCode)
    {
        image.sprite = generalImageList[generalCode].introduceImg;

        nameTxt.text = generalInfoList[generalCode].titleName;
        fullNameTxt.text = generalInfoList[generalCode].fullName;
        biographyTxt.text = generalInfoList[generalCode].biography;

        skill_1_Icon.sprite = generalInfoList[generalCode].skillInfo[0].icon;
        skill_1_Name.text = generalInfoList[generalCode].skillInfo[0].name;
        skill_1_Info.text = generalInfoList[generalCode].skillInfo[0].info;

        skill_2_Icon.sprite = generalInfoList[generalCode].skillInfo[1].icon;
        skill_2_Name.text = generalInfoList[generalCode].skillInfo[1].name;
        skill_2_Info.text = generalInfoList[generalCode].skillInfo[1].info;
    }

    // 세부 정보 창 열기
    public void OpenInfoView(string generalCode, int num)
    {
        if (!InfoView.activeSelf) InfoView.SetActive(true);

        generalNum = num;

        PrintGeneralInfo(generalCode);
    }

    // 세부 정보 창에서 왼쪽 버튼
    public void InfoLeftBtn()
    {
        if (generalNum > 0)
        {
            generalNum--;
        }

        string code = generalData[generalNum].GeneralCode;

        PrintGeneralInfo(code);
    }

    // 세부 정보 창에서 오른쪽 버튼
    public void InfoRightBtn()
    {
        if (generalNum < generalData.Length - 1)
        {
            generalNum++;
        }

        string code = generalData[generalNum].GeneralCode;

        PrintGeneralInfo(code);
    }

    // 창 활성/비활성
    public void ActiveView(GameObject view)
    {
        view.SetActive(!view.activeSelf);

        warningTxt.SetActive(LobbyManager.instance.IsMatching);
    }
}
