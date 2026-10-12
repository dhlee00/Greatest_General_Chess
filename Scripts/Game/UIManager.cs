using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ButtonState
{
    Default,
    Active,
    Deactive
}

public class UIManager : MonoBehaviour
{
    // 플레이어 매니저
    [SerializeField] PlayerManager m_Player;
    
    // 선수 소개 UI
    [SerializeField] PlayerIntroduce playerIntroduce;
    public PlayerIntroduce PlayerIntroduce => playerIntroduce;
    
    // 유닛 위치 세팅 UI
    [SerializeField] UnitPosSetting unitPosSetting;
    public UnitPosSetting UnitPosSetting => unitPosSetting;

    [Header("User UI")]
    [SerializeField] Image allyUserGeneralImg;                // 내 유저 장군 이미지
    public Image AllyUserGeneralImg => allyUserGeneralImg;    // 내 유저 장군 이미지

    [SerializeField] TextMeshProUGUI allyUserNameTxt;         // 내 유저 이름 텍스트
    [SerializeField] TextMeshProUGUI allyUserTitleTxt;        // 내 유저 칭호 텍스트

    [SerializeField] Image enemyUserGeneralImg;               // 상대 유저 장군 이미지
    public Image EnemyUserGeneralImg => enemyUserGeneralImg;  // 내 유저 장군 이미지

    [SerializeField] TextMeshProUGUI enemyUserNameTxt;        // 상대 유저 이름 텍스트
    [SerializeField] TextMeshProUGUI enemyUserTitleTxt;       // 상대 유저 칭호 텍스트

    [Header("Turn UI")]
    // 코인토스 이미지
    [SerializeField] CoinToss coinToss;

    // 턴 수 텍스트
    [SerializeField] TextMeshProUGUI turnCountTxt;

    // 턴 시간 텍스트
    [SerializeField] TextMeshProUGUI turnTimerTxt;
    public TextMeshProUGUI TurnTimerTxt => turnTimerTxt;

    [SerializeField] RectTransform blueTurnImg;  // 블루 턴 이미지
    [SerializeField] RectTransform redTurnImg;   // 레드 턴 이미지

    [Header("Control Unit")]
    [SerializeField] GameObject controlUnitInfo;          // 조작 중인 유닛 정보창
    [SerializeField] Image controlUnitImg;                // 조작 중인 유닛 이미지
    [SerializeField] TextMeshProUGUI controlUnitNameTxt;  // 조작 중인 유닛 이름 텍스트
    [SerializeField] Slider controlUnitHpBar;             // 조작 중인 유닛 체력바 이미지
    [SerializeField] TextMeshProUGUI controlUnitHpTxt;    // 조작 중인 유닛 체력 텍스트
    [SerializeField] TextMeshProUGUI controlUnitStatTxt;  // 조작 중인 유닛 스탯

    [Header("Check Unit")]
    [SerializeField] GameObject checkUnitInfo;          // 정보 확인 유닛 정보창
    [SerializeField] Image checkUnitImg;                // 정보 확인 유닛 이미지
    [SerializeField] Image checkUnitInfoImg;            // 정보 확인 유닛 정보창 이미지
    [SerializeField] TextMeshProUGUI checkUnitNameTxt;  // 정보 확인 유닛 이름 텍스트
    [SerializeField] Slider checkUnitHpBar;             // 정보 확인 유닛 체력바 이미지
    [SerializeField] TextMeshProUGUI checkUnitHpTxt;    // 정보 확인 유닛 체력 텍스트
    [SerializeField] TextMeshProUGUI checkUnitStatTxt;  // 정보 확인 유닛 스탯

    [Header("Skill UI")]
    [SerializeField] Image skill_1_Icon;               // 스킬 1 아이콘
    [SerializeField] TextMeshProUGUI skill_1_CoolTxt;  // 스킬 1 쿨타임 텍스트
    [SerializeField] GameObject skill_1_Info;          // 스킬 1 정보창
    [SerializeField] TextMeshProUGUI skill_1_InfoTxt;  // 스킬 1 설명 텍스트
    [SerializeField] Image skill_2_Icon;               // 스킬 2 아이콘
    [SerializeField] TextMeshProUGUI skill_2_CoolTxt;  // 스킬 1 쿨타임 텍스트
    [SerializeField] GameObject skill_2_Info;          // 스킬 2 정보창
    [SerializeField] TextMeshProUGUI skill_2_InfoTxt;  // 스킬 2 설명 텍스트

    [Header("Button")]
    [SerializeField] Image moveBtn;           // 기동 버튼
    [SerializeField] Image attackBtn;         // 공격 버튼
    [SerializeField] Button skill_1_Btn;      // 스킬 1 버튼
    [SerializeField] Button skill_2_Btn;      // 스킬 2 버튼
    [SerializeField] GameObject turnOverBtn;  // 턴 종료 버튼
    public GameObject TurnOverBtnObject => turnOverBtn;  // 턴 종료 버튼

    [Header("")]
    [SerializeField] Sprite[] moveBtn_Icon;
    [SerializeField] Sprite[] attackBtn_Icon;

    [SerializeField] Sprite[] checkInfoImg;  // 정보 확인 창 이미지
    [SerializeField] Sprite[] unitImg;       // 유닛 일러스트

    // 단계별 출력문
    [SerializeField] StepView stepView;

    // 스킬 버튼 활성화 여부
    public bool SkillBtnOn => skill_1_Btn.gameObject.activeSelf || skill_2_Btn.gameObject.activeSelf;

    #region 턴 관련
    // 코인 토스 진행
    public void ShowCoinToss(bool turn)
    {
        coinToss.transform.parent.gameObject.SetActive(true);

        Vector3 coinPosOrigin = new Vector3(13.3f, 10, -15.63f);

        // 호스트 일 때 위치
        if (NetworkManager.Singleton.IsServer)
        {
            coinPosOrigin.z -= 9.0f;
        }

        // 클라이언트 일 때 위치
        else
        {
            coinPosOrigin.z += 9.0f;
            coinToss.transform.parent.rotation = Quaternion.Euler(0, 180, 0);
        }

        coinToss.transform.parent.position = coinPosOrigin;

        if (NetworkManager.Singleton.IsServer)
        {
            coinToss.CoinTossResult = turn ? CoinTossResult.firstTurn : CoinTossResult.secondTurn;
        }

        else
        {
            coinToss.CoinTossResult = turn ? CoinTossResult.secondTurn : CoinTossResult.firstTurn;
        }
    }

    // 턴 수 출력
    public void PrintTurn(int turnCount)
    {
        turnCountTxt.text = $"{string.Format("{0:00}", turnCount)}턴";
    }

    // 턴 이미지 출력
    public IEnumerator ShowTurnImg()
    {
        Vector2 blue_DeactivePos = new Vector2(-250, 270);  // 비활성 위치
        Vector2 blue_ActivePos = new Vector2(-250, 65);     // 활성 위치

        Vector2 red_DeactivePos = new Vector2(250, 270);  // 비활성 위치
        Vector2 red_ActivePos = new Vector2(250, 65);     // 활성 위치

        float time = 0;         // 현재 시간
        float duration = 0.3f;  // 연출 진행 시간

        // 연출 진행 시간동안 반복
        while (time < duration)
        {
            time += Time.deltaTime;
            float percent = time / duration;  // 퍼센티지 계산

            // 본인 턴인 경우
            if (NetworkManager.Singleton.IsServer == GameManager.instance.IsP1Turn)
            {
                if (redTurnImg.anchoredPosition != red_DeactivePos)
                {
                    redTurnImg.anchoredPosition = Vector2.Lerp(red_ActivePos, red_DeactivePos, percent);
                }

                blueTurnImg.anchoredPosition = Vector2.Lerp(blue_DeactivePos, blue_ActivePos, percent);
            }

            else
            {
                if (blueTurnImg.anchoredPosition != blue_DeactivePos)
                {
                    blueTurnImg.anchoredPosition = Vector2.Lerp(blue_ActivePos, blue_DeactivePos, percent);
                }

                redTurnImg.anchoredPosition = Vector2.Lerp(red_DeactivePos, red_ActivePos, percent);
            }

            yield return null;
        }

        // 정 위치 보정
        if (NetworkManager.Singleton.IsServer == GameManager.instance.IsP1Turn)
        {
            redTurnImg.anchoredPosition = red_DeactivePos;
            blueTurnImg.anchoredPosition = blue_ActivePos;
        }

        else
        {
            blueTurnImg.anchoredPosition = blue_DeactivePos;
            redTurnImg.anchoredPosition = red_ActivePos;
        }
    }
    #endregion

    #region 정보창 관련
    // 선택한 유닛 정보창 활성화
    public void ControlUnitInfo(bool active, Unit unit = null)
    {
        controlUnitInfo.SetActive(active);

        if (active && unit != null)
        {
            // 이미지, 이름, 체력바, 스탯 출력
            controlUnitImg.sprite = unitImg[(int)unit.UnitType];
            controlUnitHpBar.value = unit.NowHealth / unit.MaxHealth;
            controlUnitHpTxt.text = $"{Mathf.CeilToInt(unit.NowHealth)} / {Mathf.CeilToInt(unit.MaxHealth)}";

            if (unit.BuffMaxHealth > 0)
            {
                controlUnitHpTxt.text += $"(<color=blue>+{unit.BuffMaxHealth}</color>)";
            }

            controlUnitNameTxt.text = PrintUnitName(unit);
            controlUnitStatTxt.text = PrintUnitStat(unit);
        }
    }

    // 다른 유닛의 정보창 활성화
    public void CheckUnitInfo(bool active, Unit unit = null)
    {
        checkUnitInfo.gameObject.SetActive(active);

        if (active == true)
        {
            if (unit.IsAlly)
            {
                checkUnitInfoImg.sprite = checkInfoImg[0];
            }

            else
            {
                checkUnitInfoImg.sprite = checkInfoImg[1];
            }

            // 이미지, 체력바, 스탯 출력
            checkUnitImg.sprite = unitImg[(int)unit.UnitType];
            checkUnitHpBar.value = unit.NowHealth / unit.MaxHealth;
            checkUnitHpTxt.text = $"{Mathf.CeilToInt(unit.NowHealth)} / {Mathf.CeilToInt(unit.MaxHealth)}";

            if (unit.BuffMaxHealth > 0)
            {
                checkUnitHpTxt.text += $"(<color=blue>+{unit.BuffMaxHealth}</color>)";
            }

            checkUnitNameTxt.text = PrintUnitName(unit);
            checkUnitStatTxt.text = PrintUnitStat(unit);
        }
    }

    // 유닛 이름 출력
    string PrintUnitName(Unit unit)
    {
        string unitName = "";

        switch (unit.UnitType)
        {
            case UnitType.Infantry: unitName = "보병"; break;
            case UnitType.Cavalry: unitName = "경기병"; break;
            case UnitType.HeavyCavalry: unitName = "중장기병"; break;
            case UnitType.Trebuchet: unitName = "트레뷰셋"; break;
            case UnitType.General: 
                General general = unit as General;
                unitName = general.GeneralName;
                break;
            case UnitType.King: unitName = "왕"; break;
        }

        return unitName;
    }

    // 유닛 스탯 출력
    string PrintUnitStat(Unit unit)
    {
        string attackType = "";
        switch (unit.AttackType)
        {
            case AttackType.CloseCombat: attackType = "일반"; break;
            case AttackType.DirectFire: attackType = "직사"; break;
            case AttackType.IndirectFire: attackType = "곡사"; break;
        }

        string info = "";
        info += $"공격 방식 : {attackType}";
        info += $"\n공격력 : {unit.AttackPower}";

        if (unit.BuffAttackPower > 0)
        {
            info += $"(<color=blue>+{unit.BuffAttackPower}</color>)";
        }

        if (unit.MinAttackRange != 0)
        {
            info += $"\n공격 사거리 : {unit.MinAttackRange} ~ {unit.AttackRange}";
        }

        else
        {
            info += $"\n공격 사거리 : {unit.AttackRange}";
        }

        info += $"\n방어력 : {unit.Defense}";

        if (unit.BuffDefense > 0)
        {
            info += $"(<color=blue>+{unit.BuffDefense}</color>)";
        }

        info += $"\n기동력 : {unit.Movement}";

        if (unit.BuffMovement > 0)
        {
            info += $"(<color=blue>+{unit.BuffMovement}</color>)";
        }

        return info;
    }
    #endregion

    #region 버튼 관련
    // 조작 버튼 활성화
    public void ActionButton(bool onoff)
    {
        // 기동 버튼 활성화
        moveBtn.gameObject.SetActive(onoff);

        // 공격 버튼 활성화
        attackBtn.gameObject.SetActive(onoff);
    }

    // 기동 버튼 아이콘 상태 변경
    public void ChangeMoveBtnState(ButtonState buttonState)
    {
        moveBtn.sprite = moveBtn_Icon[(int)buttonState];
    }

    // 공격 버튼 아이콘 상태 변경
    public void ChangeAttackBtnState(ButtonState buttonState)
    {
        attackBtn.sprite = attackBtn_Icon[(int)buttonState];
    }

    // 스킬 버튼 활성화
    public void SkillButton(bool onoff)
    {
        if (onoff)
        {
            // 기동, 공격 버튼 위치 변경
            moveBtn.transform.localPosition = new Vector3(-150, moveBtn.transform.localPosition.y, moveBtn.transform.localPosition.z);
            attackBtn.transform.localPosition = new Vector3(-50, attackBtn.transform.localPosition.y, attackBtn.transform.localPosition.z);
        }

        else
        {
            // 기동, 공격 버튼 위치 초기화
            moveBtn.transform.localPosition = new Vector3(-50, moveBtn.transform.localPosition.y, moveBtn.transform.localPosition.z);
            attackBtn.transform.localPosition = new Vector3(50, attackBtn.transform.localPosition.y, attackBtn.transform.localPosition.z);
        }

        // 스킬 버튼 활성화
        skill_1_Btn.gameObject.SetActive(onoff);
        skill_2_Btn.gameObject.SetActive(onoff);
        
        // 스킬 아이콘 적용
        if (skill_1_Icon.sprite == null && m_Player.Ally_General.Skill[0].SkillIcon != null)
        {
            SkillData skill = m_Player.Ally_General.Skill[0];
            skill_1_Icon.sprite = skill.SkillIcon;

            if (skill.ActivateTiming == ActivateTiming.GameStart)
            {
                skill_1_Icon.rectTransform.sizeDelta *= new Vector2(1.04f, 1.04f);
            }
        }

        if (skill_2_Icon.sprite == null && m_Player.Ally_General.Skill[1].SkillIcon != null)
        {
            SkillData skill = m_Player.Ally_General.Skill[1];
            skill_2_Icon.sprite = skill.SkillIcon;

            if (skill.ActivateTiming == ActivateTiming.GameStart)
            {
                skill_2_Icon.rectTransform.sizeDelta *= new Vector2(1.04f, 1.04f);
            }
        }
    }

    // 스킬 버튼 변경
    public void ChangeSkillBtn(int skillIndex, bool skillUsed)
    {
        Color hideColor = new Color(0.5f, 0.5f, 0.5f, 1);

        if (skillUsed)
        {
            if (skillIndex == 0)
            {
                skill_1_Icon.color = hideColor;
            }

            else
            {
                skill_2_Icon.color = hideColor;
            }
        }

        else
        {
            if (skillIndex == 0)
            {
                skill_1_Icon.color = Color.white;
            }

            else
            {
                skill_2_Icon.color = Color.white;
            }
        }
    }

    // 스킬 쿨타임 UI 표시
    public void ShowSkillCool(int skillIndex, int skillCoolTurn)
    {
        if (skillIndex == 0)
        {
            skill_1_CoolTxt.gameObject.SetActive(true);
            skill_1_CoolTxt.text = skillCoolTurn.ToString();
        }

        else
        {
            skill_2_CoolTxt.gameObject.SetActive(true);
            skill_2_CoolTxt.text = skillCoolTurn.ToString();
        }
    }

    // 스킬 쿨타임 UI 초기화
    public void ResetSkillCool()
    {
        if (skill_1_CoolTxt.gameObject.activeSelf)
        {
            skill_1_CoolTxt.gameObject.SetActive(false);
        }

        if (skill_2_CoolTxt.gameObject.activeSelf)
        {
            skill_2_CoolTxt.gameObject.SetActive(false);
        }
    }
    #endregion

    #region 버튼 연결 함수
    // 기동 버튼
    public void MoveBtn()
    {
        if (m_Player.IsActing) return;

        if (m_Player.ControlState != ControlState.UnitMove)
        {
            // 기동 상태
            m_Player.MoveState();
        }
    }

    // 공격 버튼
    public void AttackBtn()
    {
        if (m_Player.IsActing) return;

        if (m_Player.ControlState != ControlState.UnitAttack)
        {
            // 공격 상태
            m_Player.AttackState();
        }
    }

    // 스킬 1 버튼 이벤트 함수
    public void Skill_1_Info(bool onoff)
    {
        skill_1_Info.SetActive(onoff);
    }

    // 스킬 2 버튼 이벤트 함수
    public void Skill_2_Info(bool onoff)
    {
        skill_2_Info.SetActive(onoff);
    }

    // 턴 종료 버튼
    public void TurnOverBtn()
    {
        if (m_Player.IsActing) return;

        // 턴 넘김 버튼 비활성
        turnOverBtn.SetActive(false);

        GameManager.instance.TurnOver();
    }
    #endregion

    #region 스킬 정보 관련
    // 스킬 설명 설정
    public void SetSkillInfo(SkillData skill_1, SkillData skill_2)
    {
        skill_1_InfoTxt.text = PrintSkillInfo(skill_1);
        skill_2_InfoTxt.text = PrintSkillInfo(skill_2);
    }

    // 스킬 정보
    string PrintSkillInfo(SkillData skill)
    {
        string info = "";
        info += $"스킬 이름 : {skill.SkillName}";

        string activateTiming = "패시브";

        if (skill.ActivateTiming == ActivateTiming.Activate)
        {
            activateTiming = "액티브";
        }

        else if (skill.ActivateTiming == ActivateTiming.OutNumbered)
        {
            activateTiming = "발동형";
        }

        info += $"\n스킬 종류 : {activateTiming}";

        string skillEffect = "";

        for (int i = 0; i < skill.Effect.Count; i++)
        {
            Effect effect = skill.Effect[i];

            switch (effect.EffectKey)
            {
                case EffectKey.AttackPower: skillEffect += "공격력"; break;
                case EffectKey.Accuracy: skillEffect += "명중률"; break;
                case EffectKey.Penetration: skillEffect += "방어 관통력"; break;
                case EffectKey.Defense: skillEffect += "방어력"; break;
                case EffectKey.Health: skillEffect += "체력"; break;
                case EffectKey.Movement: skillEffect += "기동력"; break;
                case EffectKey.DamageNegation: skillEffect += "데미지 무효화 확률"; break;
                case EffectKey.ExtraTurn: skillEffect += "추가 턴 진행 확률"; break;
            }

            if (effect.EffectKey < EffectKey.DamageNegation)
            {
                skillEffect += $" + {effect.EffectValue}";
            }

            else
            {
                skillEffect += $" {effect.EffectValue}%";
            }

            if (i != skill.Effect.Count - 1)
            {
                skillEffect += ", ";
            }
        }

        info += $"\n효과 : {skillEffect}";

        return info;
    }
    #endregion

    // 단계별 출력
    public void PrintStep(GameReadyStep step)
    {
        stepView.gameObject.SetActive(true);
        stepView.Step = step;
    }

    // 턴 관련 UI 초기화
    public void ResetTurnUI(bool previousValue, bool newValue)
    {
        // 호스트인 경우
        if (GameManager.instance.IsServer)
        {
            // 턴 종료 버튼
            turnOverBtn.SetActive(newValue);

            // 내 턴 활성 비활성
            StartCoroutine(ShowTurnImg());
        }

        else
        {
            // 턴 종료 버튼
            turnOverBtn.SetActive(!newValue);

            // 내 턴 활성 비활성
            StartCoroutine(ShowTurnImg());
        }
    }

    // 조작 UI 초기화
    public void ResetControlUI()
    {
        if (controlUnitInfo.activeSelf)
        {
            ControlUnitInfo(false);
        }

        if (checkUnitInfo.gameObject.activeSelf)
        {
            CheckUnitInfo(false);
        }

        if (moveBtn.gameObject.activeSelf || attackBtn.gameObject.activeSelf)
        {
            ActionButton(false);
        }

        if (SkillBtnOn)
        {
            SkillButton(false);
        }

        if (skill_1_Info.activeSelf)
        {
            skill_1_Info.SetActive(false);
        }

        if (skill_2_Info.activeSelf)
        {
            skill_2_Info.SetActive(false);
        }

        // 버튼 아이콘 상태 초기
        ChangeMoveBtnState(ButtonState.Default);
        ChangeAttackBtnState(ButtonState.Default);
    }

    #region 마우스가 UI 위에 있는지 확인하는 함수
    public bool IsPointerOverUI()
    {
        // 이벤트 시스템의 포인터 데이터를 생성
        PointerEventData pointer = new PointerEventData(EventSystem.current);

        // 현재 마우스의 좌표를 데이터에 할당
        pointer.position = Input.mousePosition;

        // 레이캐스트 결과를 담을 리스트 생성
        List<RaycastResult> results = new List<RaycastResult>();

        // 모든 UI 요소를 대상으로 마우스 위치에 무엇이 있는지 확인
        EventSystem.current.RaycastAll(pointer, results);

        // UI 위에 포인터가 있을 경우 true 반환
        return (0 < results.Count);
    }
    #endregion
}
