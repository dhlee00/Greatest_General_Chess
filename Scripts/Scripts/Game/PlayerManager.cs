using System.Collections.Generic;
using UltEvents;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

// 플레이어의 조작 상태
public enum ControlState
{
    UnitSelect,  // 유닛 선택
    UnitMove,    // 유닛 기동
    UnitAttack   // 유닛 공격
}

// 예약 스킬
[System.Serializable]
public class ReserveSkill
{
    [SerializeField] General general;
    [SerializeField] int skillIndex;

    public General General { get => general; set => general = value; }
    public int SkillIndex { get => skillIndex; set => skillIndex = value; }
}

// 사용한 스킬
[System.Serializable]
public class UsedSkill
{
    [SerializeField] General general;
    [SerializeField] int skillIndex;
    [SerializeField] int turnCoount;

    public General General { get => general; set => general = value; }
    public int SkillIndex { get => skillIndex; set => skillIndex = value; }
    public int TurnCoount { get => turnCoount; set => turnCoount = value; }
}

// 쿨타임 스킬
[System.Serializable]
public class CoolSkill
{
    [SerializeField] General general;
    [SerializeField] int skillIndex;
    [SerializeField] int coolTurn;

    public General General { get => general; set => general = value; }
    public int SkillIndex { get => skillIndex; set => skillIndex = value; }
    public int CoolTurn { get => coolTurn; set => coolTurn = value; }
}

public class PlayerManager : MonoBehaviour
{
    // 테스트용
    bool alwaysMiss = false;
    bool alwaysDamageNegation = false;
    [HideInInspector] public bool alwaysExtraTurn = false;
    // 테스트용

    // UI 매니저
    [SerializeField] UIManager m_UI;

    // 유닛 매니저
    [SerializeField] UnitManager m_Unit;

    // 커서 매니저
    [SerializeField] CursorManager m_Cursor;

    // 플레이어 조작 상태
    [SerializeField] ControlState controlState = ControlState.UnitSelect;
    public ControlState ControlState { get => controlState; set => controlState = value; }

    // 레이어 마스크
    [SerializeField] LayerMask controlMask;
    [SerializeField] LayerMask checkMask;

    // 나의 장군
    [SerializeField] General ally_General;
    public General Ally_General { get => ally_General; set => ally_General = value; }

    // 상대방의 장군
    [SerializeField] General enemy_General;
    public General Enemy_General { get => enemy_General; set => enemy_General = value; }

    // 커서 변경을 위한 타일 정보
    Tile cursorTile;

    // 조작 중인 유닛
    [SerializeField] Unit controlUnit;

    // 정보 확인 유닛
    Unit checkUnit;

    // 상대가 조작 중인 유닛
    [SerializeField] Unit enemyControlUnit;
    public Unit EnemyControlUnit { get => enemyControlUnit; set => enemyControlUnit = value; }

    // 유닛 가치 점수 합
    [SerializeField] int allyUnit_ValuePoint;   // 아군
    public int AllyUnit_ValuePoint { get => allyUnit_ValuePoint; set => allyUnit_ValuePoint = value; }

    [SerializeField] int enemyUnit_ValuePoint;  // 적군
    public int EnemyUnit_ValuePoint { get => enemyUnit_ValuePoint; set => enemyUnit_ValuePoint = value; }

    [SerializeField] bool isActing;   // 행동 중
    public bool IsActing { get => isActing; set => isActing = value; }

    [SerializeField] bool endMove;    // 이동
    [SerializeField] bool endAttack;  // 공격

    #region 스킬 관련
    [Header("Skill")]

    // 액티브 스킬 사용한 상태
    [SerializeField] bool isUseActiveSkill = false;

    // 사용 예약한 스킬 인덱스
    [SerializeField] List<ReserveSkill> reserveSkillList = new List<ReserveSkill>();
    public List<ReserveSkill> ReserveSkillList => reserveSkillList;

    // 사용한 턴 제한 스킬의 턴 수
    [SerializeField] List<UsedSkill> turnLimitSkillList = new List<UsedSkill>();

    // 쿨타임인 스킬
    [SerializeField] List<CoolSkill> coolSkillList = new List<CoolSkill>();

    // 열세 확인 대리자
    [SerializeField] UltEvent checkOutNumbered;
    public UltEvent CheckOutNumbered { get => checkOutNumbered; set => checkOutNumbered = value; }
    #endregion

    // 레이어
    int layer_Tile;         // 타일
    int layer_AllyUnit;     // 아군 유닛 레이어
    int layer_EnemyUnit;    // 적군 유닛 레이어
    int layer_ControlUnit;  // 조작 중인 유닛 레이어

    void Start()
    {
        layer_Tile = LayerMask.NameToLayer("Tile");
        layer_AllyUnit = LayerMask.NameToLayer("AllyUnit");
        layer_EnemyUnit = LayerMask.NameToLayer("EnemyUnit");
        layer_ControlUnit = LayerMask.NameToLayer("ControlUnit");
    }

    void Update()
    {
        if (!GameManager.instance.ReadyComplete) return;

        #region 유닛 호버링 정보확인
        // 왼쪽 Shift 누르는 동안 + 마우스가 유닛의 위치에 있을 때 + 마우스가 UI 위에 없을 때
        if (Input.GetKey(KeyCode.LeftShift))
        {
            // 카메라에서 레이 발사
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity, checkMask) && !m_UI.IsPointerOverUI())
            {
                // 정보 확인 유닛이 다른 유닛인 경우
                if (checkUnit == null || checkUnit.transform != hit.transform)
                {
                    // 유닛 불러오기
                    Unit unit = hit.transform.GetComponent<Unit>();

                    // 은폐 상태인 적 유닛 무시
                    if (!unit.IsAlly && unit.IsConcealment) return;

                    // 조작 중인 유닛과 다른 유닛인 경우
                    if (unit != controlUnit)
                    {
                        if (checkUnit != null)
                        {
                            // 기존 유닛 외곽선 없애기
                            checkUnit.Checked(false);
                        }

                        // 정보 확인 중인 유닛 저장
                        checkUnit = unit;

                        // 정보창 활성화
                        m_UI.CheckUnitInfo(true, checkUnit);

                        // 외곽선 표시
                        checkUnit.Checked(true);
                    }
                }
            }
        }

        else
        {
            if (checkUnit != null)
            {
                checkUnit.Checked(false);
                m_UI.CheckUnitInfo(false);
                checkUnit = null;
            }
        }
        #endregion

        // 마우스 왼쪽 클릭 + 마우스가 UI 위에 없을 때 + 행동 중이 아닐 때
        if (Input.GetMouseButtonDown(0) && !m_UI.IsPointerOverUI() && !isActing)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // 본인 턴 아닐 때 무시
            if (!(NetworkManager.Singleton.IsServer == GameManager.instance.IsP1Turn)) return;

            if (Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity, controlMask))
            {
                // 현재 조작 상태에 따라 조작
                switch (controlState)
                {
                    // 유닛 기동 상태
                    case ControlState.UnitMove:

                        if (controlUnit != null && hit.transform.CompareTag("Tile"))
                        {
                            // 타일 불러오기
                            Tile selectTile = hit.transform.parent.GetComponent<Tile>();

                            // 하이라이트 된 타일인 경우 + 암반 타일이 아닌 경우 + 누군가 위치한 타일이 아닌 경우
                            if (selectTile.MoveHighlight.gameObject.activeSelf &&
                                selectTile.tileEv != TileEnvironment.Bedrock && selectTile.Unit == null)
                            {
                                // 목적지 선택
                                DestinationSelect(selectTile);
                            }
                        }
                        break;

                    // 유닛 공격 상태
                    case ControlState.UnitAttack:

                        Tile target = null;

                        if (hit.transform.CompareTag("Unit"))
                        {
                            Unit unit = hit.transform.GetComponent<Unit>();
                            if (!unit.IsAlly)
                            {
                                target = unit.Tile;
                            }
                        }

                        else if (hit.transform.CompareTag("Tile"))
                        {
                            target = hit.transform.parent.GetComponent<Tile>();
                        }

                        if (target != null && target.AttackHighlight.gameObject.activeSelf)
                        {
                            // 타겟으로 설정
                            TargetSelect(target);
                        }
                        break;
                }

                // 유닛 불러오기
                Unit selectUnit = hit.transform.GetComponent<Unit>();

                // 유닛이 아군인 경우
                if (selectUnit != null && selectUnit.IsAlly)
                {
                    // 만약 현재 조작 중인 유닛이 있을 때
                    if (controlUnit != null)
                    {
                        // 조작 중인 유닛을 행동 시킨 적 없을 때
                        if (!controlUnit.EndMove && !controlUnit.EndAttack)
                        {
                            // 하이라이트 활성화 된 경우 비활성화
                            controlUnit.Selected(false);

                            if (controlState == ControlState.UnitMove)
                            {
                                controlUnit.ReadyMove(false);
                            }

                            else if (controlState == ControlState.UnitAttack)
                            {
                                controlUnit.ReadyAttack(false);
                            }

                            // 유닛 레이어 설정
                            controlUnit.gameObject.layer = layer_AllyUnit;
                            selectUnit.gameObject.layer = layer_ControlUnit;

                            // 해당 유닛 선택
                            UnitSelect(selectUnit);
                        }
                    }

                    else
                    {
                        // 조작 유닛 레이어 설정
                        selectUnit.gameObject.layer = layer_ControlUnit;

                        // 해당 유닛 선택
                        UnitSelect(selectUnit);
                    }
                }
            }
        }

        if (controlUnit != null)
        {
            // 카메라에서 레이 발사
            Ray cursorRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(cursorRay, out RaycastHit cursorHit, float.PositiveInfinity, 1 << layer_Tile) && !m_UI.IsPointerOverUI())
            {
                if (cursorHit.transform.CompareTag("Tile"))
                {
                    if (cursorTile == null || cursorTile.transform != cursorHit.transform.parent)
                    {
                        // 타일 불러오기
                        cursorTile = cursorHit.transform.parent.GetComponent<Tile>();
                    }
                }
            }

            if (cursorTile != null)
            {
                if (cursorTile.MoveHighlight.gameObject.activeSelf)
                {
                    m_Cursor.ChangeCursor(CursorState.Move);
                }

                else if (cursorTile.AttackHighlight.gameObject.activeSelf)
                {
                    m_Cursor.ChangeCursor(CursorState.Attack);
                }

                else
                {
                    m_Cursor.ChangeCursor(CursorState.Default);
                }
            }
        }
    }

    #region 조작 상태
    // 조작 마스크 설정
    void SetControlMask(ControlState controlState)
    {
        controlMask = 1 << layer_AllyUnit;

        switch (controlState)
        {
            case ControlState.UnitMove:

                controlMask |= 1 << layer_Tile;
                break;

            case ControlState.UnitAttack:

                // 조작하는 유닛이 트레뷰셋인 경우
                if (controlUnit.UnitType == UnitType.Trebuchet)
                {
                    controlMask |= 1 << layer_Tile;
                }

                // 나머지 유닛인 경우
                else
                {
                    controlMask |= 1 << layer_EnemyUnit;
                }
                break;
        }
    }

    // 기동 상태
    public void MoveState()
    {
        if (controlUnit != null && !controlUnit.EndMove)
        {
            // 기동 상태
            controlState = ControlState.UnitMove;

            // 마스크 설정
            SetControlMask(ControlState.UnitMove);

            // 유닛 기동 준비 상태
            controlUnit.ReadyAttack(false);
            controlUnit.ReadyMove(true);

            // 버튼 상태 변경
            if (!endAttack)
            {
                m_UI.ChangeAttackBtnState(ButtonState.Default);
            }

            m_UI.ChangeMoveBtnState(ButtonState.Active);
        }
    }

    // 공격 상태
    public void AttackState()
    {
        if (controlUnit != null && !controlUnit.EndAttack)
        {
            // 공격 상태
            controlState = ControlState.UnitAttack;

            // 마스크 설정
            SetControlMask(ControlState.UnitAttack);

            // 유닛 공격 준비 상태
            controlUnit.ReadyMove(false);
            controlUnit.ReadyAttack(true);

            // 버튼 상태 변경
            if (!endMove)
            {
                m_UI.ChangeMoveBtnState(ButtonState.Default);
            }

            m_UI.ChangeAttackBtnState(ButtonState.Active);
        }
    }

    // 상태 초기화
    public void ResetState()
    {
        if (controlUnit != null)
        {
            controlUnit.Selected(false);

            if (controlState == ControlState.UnitMove)
            {
                controlUnit.ReadyMove(false);
            }

            if (controlState == ControlState.UnitAttack)
            {
                controlUnit.ReadyAttack(false);
            }

            // 유닛의 행동 상태 초기화
            controlUnit.ResetAction();

            // 레이어 초기화
            controlMask = 1 << layer_AllyUnit;
            controlUnit.gameObject.layer = layer_AllyUnit;

            // 선택한 유닛 초기화
            controlUnit = null;
        }

        if (enemyControlUnit != null)
        {
            enemyControlUnit.ResetAction();
            enemyControlUnit.Selected(false);
        }

        enemyControlUnit = null;

        // 상태 초기화
        controlState = ControlState.UnitSelect;

        // 행동 여부 초기화
        endMove = false;
        endAttack = false;

        // UI 초기화
        m_UI.ResetControlUI();
    }
    #endregion

    #region 유닛 조작
    // 유닛 선택
    void UnitSelect(Unit unit)
    {
        // 조작 전 정보 확인 때문에 정보 확인 창 활성화 된 경우 비활성화
        if (checkUnit != null)
        {
            checkUnit.Checked(false);
            m_UI.CheckUnitInfo(false);
            checkUnit = null;
        }

        // 조작 중인 유닛
        controlUnit = unit;

        // 유닛 정보창 활성화
        m_UI.ControlUnitInfo(true, unit);

        // 버튼 활성화
        m_UI.ActionButton(true);

        // 만약 유닛이 장군이면 스킬 버튼 활성화 및 스킬 설명 설정
        if (unit.UnitType == UnitType.General)
        {
            General general = unit as General;
            m_UI.SkillButton(true);
            m_UI.SetSkillInfo(general.Skill[0], general.Skill[1]);
        }

        else
        {
            if (m_UI.SkillBtnOn)
            {
                m_UI.SkillButton(false);
            }
        }

        // 유닛 선택한 상태
        controlUnit.Selected(true);

        if (controlState == ControlState.UnitAttack)
        {
            // 공격 상태로 전환
            AttackState();
        }

        else
        {
            // 기동 상태로 전환
            MoveState();
        }
    }

    // 기동 타일 선택
    void DestinationSelect(Tile destination)
    {
        // 하이라이트 끄기
        controlUnit.ReadyMove(false);

        // 유닛 기동
        StartCoroutine(controlUnit.Move(destination));

        // 동기화
        GameManager.instance.UnitMoveRpc(controlUnit.UnitID, destination.Coord);
    }

    // 공격 타일 선택
    void TargetSelect(Tile attackTile)
    {
        // 하이라이트 끄기
        controlUnit.ReadyAttack(false);

        // 데미지 계산
        SkillValue<Damage> damage = new SkillValue<Damage>();

        if (attackTile.Unit != null)
        {
            damage.value = controlUnit.CalculateDamage(attackTile.Unit);

            // 데미지 무효화 확인
            if (attackTile.Unit.CheckDamageNegation != null && attackTile.Unit.CheckDamageNegation.HasCalls)
            {
                attackTile.Unit.CheckDamageNegation.Invoke(damage);
            }
        }

        if (alwaysMiss && !alwaysDamageNegation)
        {
            damage.value.value = 1;
            damage.value.isMiss = true;
        }

        else if (!alwaysMiss && alwaysDamageNegation)
        {
            damage.value.value = 0;
            damage.value.isNegation = true;
        }

        // 유닛 공격
        StartCoroutine(controlUnit.Attack(attackTile, damage.value));

        // 동기화
        GameManager.instance.UnitAttackRpc(controlUnit.UnitID, attackTile.Coord, damage.value.value, damage.value.isMiss, damage.value.isNegation);
    }

    // 행동 종료
    public void EndAction()
    {
        isActing = false;

        if (controlUnit != null)
        {
            // 유닛 정보창 최신화
            m_UI.ControlUnitInfo(true, controlUnit);

            if (controlState == ControlState.UnitMove)
            {
                // 이동 종료
                endMove = true;

                // 기동 버튼 비활성화
                m_UI.ChangeMoveBtnState(ButtonState.Deactive);

                // 공격 상태로 전환
                AttackState();
            }

            else
            {
                // 공격 종료
                endAttack = true;

                // 정보창 비활성화
                m_UI.CheckUnitInfo(false);

                // 공격 버튼 비활성화
                m_UI.ChangeAttackBtnState(ButtonState.Deactive);

                // 기동 상태로 전환
                MoveState();
            }

            // 행동력 감소
            GameManager.instance.SpendActionPoint();
        }
    }

    // 부쉬에 위치한 모든 유닛 주변 타일 확인
    public void AllUnitCheckCanConcealment()
    {
        foreach (Unit allyUnit in m_Unit.AllyUnitList.Values)
        {
            allyUnit.CheckCanConcealment();
        }

        foreach (Unit enemyUnit in m_Unit.EnemyUnitList.Values)
        {
            enemyUnit.CheckCanConcealment();
        }
    }
    #endregion

    #region 스킬 관련
    // 게임 시작 시 발동하는 스킬 사용
    public void ActivateGameStartSkill()
    {
        for (int skillIndex = 0; skillIndex < 2; skillIndex++)
        {
            if (ally_General.Skill[skillIndex].ActivateTiming == ActivateTiming.GameStart)
            {
                ally_General.Skill[skillIndex].Activate(ally_General);
            }

            if (enemy_General.Skill[skillIndex].ActivateTiming == ActivateTiming.GameStart)
            {
                enemy_General.Skill[skillIndex].Activate(enemy_General);
            }
        }
    }

    public void StartCheckOutNumbered()
    {
        for (int skillIndex = 0; skillIndex < 2; skillIndex++)
        {
            if (ally_General.Skill[skillIndex].ActivateTiming == ActivateTiming.OutNumbered)
            {
                ally_General.Skill[skillIndex].ActivateCondition.Invoke(ally_General, ally_General.Skill[skillIndex], true);
            }
        }
    }

    // 장군 액티브 스킬 예약
    public void ReserveActiveSkill(int index)
    {
        if (IsActing) return;

        if (!isUseActiveSkill && GameManager.instance.ActionPoint == 2)
        {
            if (ally_General.Skill[index].ActivateTiming == ActivateTiming.Activate)
            {
                // 스킬 예약
                ReserveSkill reserveSkill = new ReserveSkill
                {
                    General = ally_General,
                    SkillIndex = index
                };

                reserveSkillList.Add(reserveSkill);

                // 상대방 플레이어 매니저에도 예약 걸어두기
                GameManager.instance.ReserveActiveSkillRpc(reserveSkill.SkillIndex);

                // 스킬 사용한 상태
                isUseActiveSkill = true;

                // 스킬 버튼 색 변경
                m_UI.ChangeSkillBtn(index, true);

                // 행동력 2 감소
                GameManager.instance.SpendActionPoint(2);
            }
        }
    }

    // 예약 해둔 스킬 사용
    public void UseReserveSkill()
    {
        if (reserveSkillList.Count > 0)
        {
            // 삭제 목록
            List<ReserveSkill> deleteList = new List<ReserveSkill>();

            foreach (var reserveSkill in reserveSkillList)
            {
                UseActiveSkill(reserveSkill);

                // 예약 스킬 해제
                deleteList.Add(reserveSkill);
            }

            // 사용한 스킬 목록에서 삭제
            if (deleteList.Count > 0)
            {
                foreach (var skill in deleteList)
                {
                    reserveSkillList.Remove(skill);
                }
            }
        }
    }

    // 액티브 스킬 사용 메서드
    public void UseActiveSkill(ReserveSkill reserveSkill)
    {
        General general = reserveSkill.General;
        int index = reserveSkill.SkillIndex;

        general.Skill[index].Activate(general);

        // 애니메이션 재생
        int ran = Random.Range(0, 2);
        general.Anim.SetTrigger($"useSkill_{ran}");

        // 이펙트 생성
        if (general.Skill[index].SkillVFXPrefab != null)
        {
            if (general.IsAlly || !general.IsConcealment)
            {
                Instantiate(general.Skill[index].SkillVFXPrefab, general.transform);
            }
        }

        if (!general.Skill[index].SfxID.IsNullOrEmpty())
        {
            // 효과음 재생
            SFXManager.instance.PlaySFX(general.Skill[index].SfxID);
        }

        UsedSkill usedSkill = new UsedSkill
        {
            General = general,
            SkillIndex = index,
            TurnCoount = general.Skill[index].DurationTurn
        };

        turnLimitSkillList.Add(usedSkill);
    }

    // 스킬 턴 확인
    public void CheckSkillTurn()
    {
        if (turnLimitSkillList.Count > 0)
        {
            // 삭제할 키 목록
            List<UsedSkill> deleteList = new List<UsedSkill>();

            // 사용한 스킬 목록 순회
            foreach (var usedSkill in turnLimitSkillList)
            {
                // 턴 수 감소
                usedSkill.TurnCoount--;

                // 0턴인 경우 스킬 끝
                if (usedSkill.TurnCoount <= 0)
                {
                    // 스킬 효과 종료
                    usedSkill.General.Skill[usedSkill.SkillIndex].Deactivate(usedSkill.General);

                    if (usedSkill.General == ally_General)
                    {
                        // 스킬 쿨타임 시작
                        CoolSkill cSkill = new CoolSkill
                        {
                            General = usedSkill.General,
                            SkillIndex = usedSkill.SkillIndex,
                            CoolTurn = 10
                        };

                        coolSkillList.Add(cSkill);
                    }

                    // 삭제 목록에 추가
                    deleteList.Add(usedSkill);
                }
            }

            // 사용한 스킬 목록에서 삭제
            if (deleteList.Count > 0)
            {
                foreach (var skill in deleteList)
                {
                    turnLimitSkillList.Remove(skill);
                }
            }
        }
    }

    // 스킬 쿨타임
    public void SkillCoolDown()
    {
        if (coolSkillList.Count > 0)
        {
            // 삭제할 스킬 목록
            List<CoolSkill> deleteList = new List<CoolSkill>();

            foreach (CoolSkill coolSkill in coolSkillList)
            {
                if (coolSkill.CoolTurn <= 0)
                {
                    // UI 초기화
                    m_UI.ResetSkillCool();

                    // 스킬 버튼 색 변경
                    m_UI.ChangeSkillBtn(coolSkill.SkillIndex, false);

                    // 스킬 사용하지 않은 상태
                    if (coolSkill.General == ally_General)
                    {
                        isUseActiveSkill = false;
                    }

                    // 삭제 목록에 추가
                    deleteList.Add(coolSkill);
                }

                else
                {
                    // 쿨타임 UI 표시
                    m_UI.ShowSkillCool(coolSkill.SkillIndex, coolSkill.CoolTurn);

                    // 쿨타임 감소
                    coolSkill.CoolTurn--;
                }
            }

            // 사용한 스킬 목록에서 삭제
            if (deleteList.Count > 0)
            {
                foreach (CoolSkill skill in deleteList)
                {
                    coolSkillList.Remove(skill);
                }
            }
        }
    }
    #endregion

    public void AlwaysMiss(GameObject btn)
    {
        if (!alwaysMiss)
        {
            alwaysMiss = true;
            btn.GetComponent<Image>().color = Color.yellow;
            btn.GetComponentInChildren<Text>().text = "모든 공격 빗나감";
        }

        else
        {
            alwaysMiss = false;
            btn.GetComponent<Image>().color = Color.white;
            btn.GetComponentInChildren<Text>().text = "빗나감 테스트";
        }
    }

    public void AlwaysNegation(Button btn)
    {
        if (!alwaysDamageNegation)
        {
            alwaysDamageNegation = true;
            btn.GetComponent<Image>().color = Color.yellow;
            btn.GetComponentInChildren<Text>().text = "모든 공격 무효화";
        }

        else
        {
            alwaysDamageNegation = false;
            btn.GetComponent<Image>().color = Color.white;
            btn.GetComponentInChildren<Text>().text = "데미지 무효화 테스트";
        }
    }

    public void AlwaysExtraTurn(Button btn)
    {
        if (!alwaysExtraTurn)
        {
            alwaysExtraTurn = true;
            btn.GetComponent<Image>().color = Color.yellow;
            btn.GetComponentInChildren<Text>().text = "항상 추가 턴";
        }

        else
        {
            alwaysExtraTurn = false;
            btn.GetComponent<Image>().color = Color.white;
            btn.GetComponentInChildren<Text>().text = "기만술 테스트";
        }
    }
}
