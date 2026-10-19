using System.Collections;
using System.Collections.Generic;
using UltEvents;
using Unity.Netcode;
using UnityEngine;

// 외형 적용 상태
public enum SetMatMode
{
    Default,
    OutLine,
    Concealment
}

public struct Damage
{
    public float value;
    public bool isMiss;
    public bool isNegation;
}

public class Unit : MonoBehaviour
{
    // 유닛 매니저
    UnitManager m_Unit;
    public UnitManager M_Unit => m_Unit;

    // 맵 매니저
    MapManager m_Map;
    public MapManager M_Map => m_Map;

    // 플레이어 매니저
    PlayerManager m_Player;
    public PlayerManager M_Player => m_Player;

    // 애니메이터
    [SerializeField] Animator anim;
    public Animator Anim => anim;

    // 애니메이션 이벤트
    [SerializeField] UnitAnimEvent unitAnimEvent;
    public UnitAnimEvent UnitAnimEvent => unitAnimEvent;

    // 유닛 매쉬
    [SerializeField] UnitMesh unitMesh;
    public UnitMesh UnitMesh => unitMesh;

    // 선택 화살표
    [SerializeField] GameObject selectArrow;

    // 발판
    [SerializeField] UnitBase unitBase;
    public UnitBase UnitBase { get => unitBase; set => unitBase = value; }

    // 데미지
    [SerializeField] DamageManager damageManager;

    // 기동 이펙트
    [SerializeField] GameObject moveVFX;
    public GameObject MoveVFX => moveVFX;

    // 은폐 이펙트
    [SerializeField] GameObject concealmentVFX;

    // 근접 공격 이펙트
    public virtual GameObject CloseCombatVFX => null;

    // 스킬 적용 이펙트
    [SerializeField] List<GameObject> skillVFXList = new List<GameObject>();
    public List<GameObject> SkillVFXList => skillVFXList;

    // 화살
    public virtual Arrow Arrow => null;

    // 돌
    public virtual Rock Rock => null;

    // 기본 외형 메테리얼
    Material meshMat;
    public Material MeshMat { get => meshMat; set => meshMat = value; }

    // 외곽선 메테리얼
    Material outlineMat;
    public Material OutlineMat { get => outlineMat; set => outlineMat = value; }

    // 외곽선 메테리얼
    Material concealmentMat;
    public Material ConcealmentMat { get => concealmentMat; set => concealmentMat = value; }

    #region 유닛 스탯
    [Header("Stat")]
    [SerializeField] UnitType unitType;      // 유닛 종류
    [SerializeField] int valuePoint;         // 가치 점수
    [SerializeField] AttackType attackType;  // 공격 방식
    [SerializeField] float attackPower;      // 공격력
    [SerializeField] int attackRange;        // 공격 사거리
    [SerializeField] float penetration;      // 방어 관통력
    [SerializeField] int accuracy = 80;      // 공격 명중률
    [SerializeField] float defense;          // 방어력
    [SerializeField] float maxHealth;        // 최대 체력
    [SerializeField] float nowHealth;        // 현재 체력
    [SerializeField] int movement;           // 기동력

    public UnitType UnitType => unitType;
    public int ValuePoint => valuePoint;
    public AttackType AttackType => attackType;
    public float AttackPower { get => attackPower; set => attackPower = value; }
    public virtual int MinAttackRange => 0;
    public int AttackRange { get => attackRange; set => attackRange = value; }
    public float Penetration { get => penetration; set => penetration = value; }
    public int Accuracy => accuracy;
    public float Defense { get => defense; set => defense = value; }
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
    public float NowHealth { get => nowHealth; set => nowHealth = value; }
    public int Movement { get => movement; set => movement = value; }
    #endregion

    #region 버프 스탯
    [Header("Buff Stat")]
    [SerializeField] float buffAttackPower;  // 공격력
    [SerializeField] float buffPenetration;  // 방어 관통력
    [SerializeField] int buffAccuracy;       // 공격 명중률
    [SerializeField] float buffDefense;      // 방어력
    [SerializeField] float buffNowHealth;    // 현재 체력
    [SerializeField] float buffMaxHealth;    // 최대 체력
    [SerializeField] int buffMovement;       // 기동력

    public float BuffAttackPower { get => buffAttackPower; set => buffAttackPower = value; }
    public float BuffPenetration { get => buffPenetration; set => buffPenetration = value; }
    public int BuffAccuracy { get => buffAccuracy; set => buffAccuracy = value; }
    public float BuffDefense { get => buffDefense; set => buffDefense = value; }
    public float BuffMaxHealth { get => buffMaxHealth; set => buffMaxHealth = value; }
    public int BuffMovement { get => buffMovement; set => buffMovement = value; }
    #endregion

    [Header("")]

    // 적용 중인 스킬
    [SerializeField] List<SkillData> appliedSkill;
    public List<SkillData> AppliedSkill => appliedSkill;

    // 범위 스킬 확인 대리자
    [SerializeField] UltEvent checkAuraInfluence;
    public UltEvent CheckAuraInfluence { get => checkAuraInfluence; set => checkAuraInfluence = value; }

    // 데미지 무효화 확인 대리자
    [SerializeField] UltEvent<SkillValue<Damage>> checkDamageNegation;
    public UltEvent<SkillValue<Damage>> CheckDamageNegation { get => checkDamageNegation; set => checkDamageNegation = value; }

    [Header("")]

    // 유닛 고유 ID
    [SerializeField] string unitID;
    public string UnitID { get => unitID; set => unitID = value; }

    // 유닛 진영
    [SerializeField] bool isAlly = true;
    public bool IsAlly { get => isAlly; set => isAlly = value; }

    // 유닛이 위치한 타일 정보
    [SerializeField] Tile tile;
    public Tile Tile { get => tile; set => tile = value; }

    // 타일 좌표 프러퍼티
    public Vector2Int Coord => tile.TileData.Coord;

    // 선택 된 상태
    [SerializeField] bool isSelected = false;

    // 기동 중인 상태
    [SerializeField] bool nowMoving = false;
    public bool NowMoving { get => nowMoving; set => nowMoving = value; }

    // 공격 중인 상태
    [SerializeField] bool nowAttacking = false;
    public bool NowAttacking { get => nowAttacking; set => nowAttacking = value; }

    // 기동 한 상태
    [SerializeField] bool endMove = false;
    public bool EndMove { get => endMove; set => endMove = value; }

    // 공격 한 상태
    [SerializeField] bool endAttack = false;
    public bool EndAttack { get => endAttack; set => endAttack = value; }

    // 은폐 상태
    [SerializeField] bool isConcealment;
    public bool IsConcealment => isConcealment;

    // 기동 시간
    float moveDuration = 1.5f;
    public float MoveDuration { get => moveDuration; set => moveDuration = value; }

    // 회전 시간
    float rotDuration = 0.1f;
    public float RotDuration { get => rotDuration; set => rotDuration = value; }

    #region 효과음 아이디
    [Header("SFX ID")]
    [SerializeField] string moveSFX;
    [SerializeField] string attackSFX;
    [SerializeField] string damageSFX;
    [SerializeField] string missSFX;
    [SerializeField] string dieSFX;

    public string MoveSFX => moveSFX;
    public string AttackSFX => attackSFX;
    public string DamageSFX => damageSFX;
    public string MissSFX => missSFX;
    public string DieSFX => dieSFX;
    public virtual string ProjectileSFX => null;
    #endregion

    public virtual void Start()
    {
        SetPos();
    }

    // 위치 세팅
    public void SetPos()
    {
        // 타일에 자신이 위치한 것 알림
        tile.Unit = this;

        // 초기 위치 설정
        transform.position = new Vector3(tile.TileData.TilePos.x, tile.TileHeigh / 2.0f + 0.25f, tile.TileData.TilePos.z);

        // 적인 경우 반대로
        if (NetworkManager.Singleton.IsServer != isAlly)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    // 스탯 증가
    public void BuffStat(SkillData skill)
    {
        appliedSkill.Add(skill);

        foreach (Effect effect in skill.Effect)
        {
            switch (effect.EffectKey)
            {
                case EffectKey.AttackPower:
                    attackPower += effect.EffectValue;
                    buffAttackPower += effect.EffectValue;
                    break;

                case EffectKey.Defense:
                    defense += effect.EffectValue;
                    buffDefense += effect.EffectValue;
                    break;

                case EffectKey.Movement:
                    movement += (int)effect.EffectValue;
                    buffMovement += (int)effect.EffectValue;
                    break;

                case EffectKey.Health:
                    nowHealth += effect.EffectValue;
                    buffNowHealth += effect.EffectValue;

                    maxHealth += effect.EffectValue;
                    buffMaxHealth += effect.EffectValue;
                    break;

                case EffectKey.Accuracy:
                    accuracy += (int)effect.EffectValue;
                    buffAccuracy += (int)effect.EffectValue;
                    break;

                case EffectKey.Penetration:
                    penetration += effect.EffectValue;
                    buffPenetration += effect.EffectValue;
                    break;
            }
        }
    }

    // 스탯 감소
    public void DebuffStat(SkillData skill)
    {
        appliedSkill.Remove(skill);

        foreach (Effect effect in skill.Effect)
        {
            switch (effect.EffectKey)
            {
                case EffectKey.AttackPower:
                    attackPower -= effect.EffectValue;
                    buffAttackPower -= effect.EffectValue;
                    break;

                case EffectKey.Defense:
                    defense -= effect.EffectValue;
                    buffDefense -= effect.EffectValue;
                    break;

                case EffectKey.Movement:
                    movement -= (int)effect.EffectValue;
                    buffMovement -= (int)effect.EffectValue;
                    break;

                case EffectKey.Health:

                    if (buffNowHealth > 0)
                    {
                        if (buffNowHealth >= effect.EffectValue)
                        {
                            nowHealth -= effect.EffectValue;
                            buffNowHealth -= effect.EffectValue;
                        }

                        else
                        {
                            nowHealth -= buffNowHealth;
                            buffNowHealth = 0;
                        }
                    }

                    maxHealth -= effect.EffectValue;
                    buffMaxHealth -= effect.EffectValue;
                    break;

                case EffectKey.Accuracy:
                    accuracy -= (int)effect.EffectValue;
                    buffAccuracy -= (int)effect.EffectValue;
                    break;

                case EffectKey.Penetration:
                    penetration -= effect.EffectValue;
                    buffPenetration -= effect.EffectValue;
                    break;
            }
        }
    }

    // 선택 됨
    public void Selected(bool selected)
    {
        isSelected = selected;

        if (isAlly || !isConcealment)
        {
            // 유닛 베이스 변경
            unitBase.SetMat(selected, isAlly, isConcealment);

            // 화살표
            selectArrow.SetActive(selected);
        }
    }

    // 기동 준비
    public void ReadyMove(bool selected)
    {
        m_Map.ShowMoveHighlight(this, selected);
    }

    // 공격 준비
    public void ReadyAttack(bool selected)
    {
        m_Map.ShowAttackHighlight(this, selected);
    }

    // 정보 확인 중
    public void Checked(bool isCheck)
    {
        if (isCheck)
        {
            SetMat(SetMatMode.OutLine);
        }

        else
        {
            if (isConcealment)
            {
                SetMat(SetMatMode.Concealment);
            }

            else
            {
                SetMat(SetMatMode.Default);
            }
        }
    }

    // 행동 상태 초기화
    public void ResetAction()
    {
        endMove = false;
        endAttack = false;
    }

    // 유닛 기동
    public virtual IEnumerator Move(Tile destination)
    {
        nowMoving = true;

        m_Player.IsActing = true;

        // 이전 위치 타일에서 자기자신 제거
        tile.Unit = null;

        // 애니메이션 재생
        StartCoroutine(PlayMoveAnim());

        // 효과음 재생
        SFXManager.instance.PlaySFX(moveSFX);

        // 경로 가져오기
        List<Vector2Int> pathTiles = m_Map.FindPath.Path(tile.TileData.Coord, destination.TileData.Coord);

        // 이펙트 출력
        StartCoroutine(GenerateMoveVFX(pathTiles.Count));

        // 경로 반복
        foreach (Vector2Int pathTile in pathTiles)
        {
            // 시작 타일 제외
            if (pathTile == tile.Coord) continue;

            // 시작 지점
            Vector3 startPos = transform.position;

            // 도착 지점
            Vector3 endPos = m_Map.TileList[pathTile].TileData.TilePos;

            // 만약 도착 지점이 층수가 차이나는 경우 유닛 발판 투명도 조절
            if (tile.TileData.TileFloor != m_Map.TileList[pathTile].TileData.TileFloor)
            {
                unitBase.SetFade(10f);
            }

            // 방향
            Vector3 dir = (new Vector3(endPos.x, 0, endPos.z) - new Vector3(startPos.x, 0, startPos.z)).normalized;

            if (Vector3.Angle(unitMesh.transform.forward, dir) > 0f)
            {
                // 현재 회전값
                Quaternion startRot = unitMesh.transform.rotation;

                // 기동하려는 지점 쪽을 바라보는 회전값
                Quaternion endRot = Quaternion.LookRotation(dir);

                // 회전 중인 시간
                float rotTime = 0f;

                while (rotTime < rotDuration)
                {
                    rotTime += Time.deltaTime;
                    float timer = rotTime / rotDuration;

                    // 현재 회전 방향에서 이동하려는 타일 쪽을 서서히 향함
                    unitMesh.transform.rotation = Quaternion.Slerp(startRot, endRot, timer);
                    yield return null;
                }

                // 최종 회전값 보정
                unitMesh.transform.rotation = endRot;
            }

            // 기동 진행 시간
            float moveDuPerTileCount = moveDuration / pathTiles.Count;

            // 기동 중인 시간
            float moveTime = 0f;

            while (moveTime < moveDuPerTileCount)
            {
                moveTime += Time.deltaTime;
                float timer = moveTime / moveDuPerTileCount;

                // 서서히 기동
                transform.position = Vector3.Lerp(startPos, endPos, timer);
                yield return null;
            }

            // 도착 위치 보정
            transform.position = endPos;
            tile = m_Map.TileList[pathTile];

            // 타일 확인
            CheckTile(tile);

            // 발판 투명도 원래대로
            unitBase.SetFade(100f);
        }

        // 정면 바라보기
        Quaternion nowRot = unitMesh.transform.localRotation;
        float time = 0;

        while (time < 0.1f)
        {
            time += Time.deltaTime;
            float timer = time / 0.1f;

            unitMesh.transform.localRotation = Quaternion.Slerp(nowRot, Quaternion.Euler(0, 0, 0), timer);
            yield return null;
        }

        nowMoving = false;

        // 기동하려는 타일에 자기자신 전달
        destination.Unit = this;

        // 모든 유닛 은폐 확인
        m_Player.AllUnitCheckCanConcealment();

        // 범위 스킬 받는 중인 경우 범위 내인지 확인
        if (checkAuraInfluence != null && checkAuraInfluence.HasCalls)
        {
            checkAuraInfluence.Invoke();
        }

        // 기동 한 상태
        endMove = true;

        // 효과음 종료
        SFXManager.instance.StopSFX(moveSFX);

        // 행동 종료
        m_Player.EndAction();
    }

    // 기동 애니메이션 재생
    public IEnumerator PlayMoveAnim()
    {
        while (nowMoving)
        {
            anim.SetBool("run", true);
            yield return null;
        }

        anim.SetBool("run", false);
    }

    // 기동 이펙트 생성
    public virtual IEnumerator GenerateMoveVFX(int pathTilesCount)
    {
        yield return new WaitForSeconds(0.11f);

        bool left = true;

        while (!endMove)
        {
            Vector3 effectPos = transform.position;
            effectPos.x += left ? -0.2f : 0.2f;
            effectPos.y = 1;

            bool checkSide = NetworkManager.Singleton.IsServer == isAlly;
            float effectRotY = unitMesh.transform.localEulerAngles.y;

            if (isAlly || !isConcealment)
            {
                GameObject effect = Instantiate(moveVFX);

                effect.transform.position = effectPos;
                effect.transform.rotation = Quaternion.Euler(checkSide ? -90 : 90, effectRotY, 0);
            }

            yield return new WaitForSeconds(1.0f / pathTilesCount);

            left = !left;
        }
    }

    // 현재 타일 확인
    public void CheckTile(Tile tile)
    {
        // 은폐 이펙트 생성
        if (isConcealment != (tile.tileEv == TileEnvironment.Bush))
        {
            Instantiate(concealmentVFX, transform.position, Quaternion.identity);

            // 효과음 재생
            SFXManager.instance.PlaySFX("SFX_CONCEALMENT");
        }

        Concealment(tile.tileEv == TileEnvironment.Bush);
    }

    // 은폐
    public virtual void Concealment(bool concealment)
    {
        isConcealment = concealment;

        if (concealment)
        {
            SetMat(SetMatMode.Concealment);
        }

        else
        {
            SetMat(SetMatMode.Default);
        }

        if (!isAlly)
        {
            unitMesh.HideMesh(concealment);

            if (isSelected)
            {
                selectArrow.SetActive(!concealment);
            }

            unitBase.gameObject.SetActive(!concealment);

            foreach (SkillData skill in appliedSkill)
            {
                foreach (GameObject effect in skillVFXList)
                {
                    if (skill.SkillVFXPrefab != null && skill.SkillVFXPrefab.name == effect.name)
                    {
                        effect.SetActive(!m_Player.Enemy_General.IsConcealment && !concealment);
                    }

                    if (skill.TargetVFXPrefab != null && skill.TargetVFXPrefab.name == effect.name)
                    {
                        effect.SetActive(!m_Player.Enemy_General.IsConcealment && !concealment);
                    }
                }
            }
        }
    }

    // 은폐 잠시 해제
    public IEnumerator ShowUnit()
    {
        unitMesh.HideMesh(false);
        if (isSelected) selectArrow.SetActive(true);
        unitBase.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.0f);

        selectArrow.SetActive(false);
        CheckCanConcealment();
    }

    // 은폐 확인
    public void CheckCanConcealment()
    {
        if (tile.tileEv != TileEnvironment.Bush) return;

        // 본인 주변 타일 확인
        foreach (Tile tile in m_Map.FindPath.NeighborTiles(Coord))
        {
            // 근처에 상대 유닛이 하나라도 있으면
            if (m_Map.TileList[tile.Coord].Unit != null && m_Map.TileList[tile.Coord].Unit.isAlly != isAlly)
            {
                Concealment(false);
                return;
            }
        }

        Concealment(true);
    }

    // 적 유닛 공격
    public virtual IEnumerator Attack(Tile attackTile, Damage damage)
    {
        nowAttacking = true;

        m_Player.IsActing = true;

        // 공격 대상 쪽 방향
        Vector3 tilePos = attackTile.transform.position;
        Vector3 unitPos = transform.position;
        tilePos.y = unitPos.y;

        Vector3 dir = (tilePos - unitPos).normalized;

        // 현재 회전값
        Quaternion startRot = unitMesh.transform.rotation;

        // 공격하려는 쪽을 바라보는 회전값
        Quaternion endRot = Quaternion.LookRotation(dir);

        // 회전 중인 시간
        float rotTime = 0f;

        while (rotTime < 0.1f)
        {
            rotTime += Time.deltaTime;
            float timer = rotTime / 0.1f;

            // 현재 회전 방향에서 이동하려는 타일 쪽을 서서히 향함
            unitMesh.transform.rotation = Quaternion.Slerp(startRot, endRot, timer);
            yield return null;
        }

        // 최종 회전값 보정
        unitMesh.transform.rotation = endRot;

        // 대상과 데미지 전달
        UnitAnimEvent.SetTargetAndDamage(attackTile, damage);

        // 애니메이션 재생
        anim.SetTrigger("attack");
    }

    // 데미지 공식
    public virtual Damage CalculateDamage(Unit enemyUnit, bool isSplash = false)
    {
        // 아군 유닛의 타일 층 수
        int allyFloor = (int)tile.TileData.TileFloor;

        // 적군 유닛의 타일 층 수
        int enemyFloor = (int)enemyUnit.tile.TileData.TileFloor;

        // (공격력 + (타일 층 수 차 * 5)) - 공격 대상의 방어력 - 방어구 관통력
        float damageValue = 0;

        damageValue = attackPower + ((allyFloor - enemyFloor) * 5);

        // 스플래시 데미지 : 공격력의 절반
        if (isSplash)
        {
            damageValue *= 0.5f;
        }

        // 보병 + 트레뷰셋의 범위 공격을 제외한 나머지는 (방어력 - 방어 관통력) 계산
        if (unitType != UnitType.Infantry && !isSplash)
        {
            float enemyDefense = enemyUnit.defense - penetration;

            // 공격 대상의 방어력 - 방어구 관통력 값이 음수여도 데미지에 + 없게
            if (enemyDefense > 0)
            {
                damageValue -= enemyDefense;
            }
        }

        // 명중 확인
        int checkHit = Random.Range(1, 101);

        // 빗나간 경우 최종데미지의 20%만 적용
        if (checkHit > accuracy)  // 80% 확률
        {
            damageValue *= 0.2f;
        }

        Damage damage = new Damage
        {
            value = (float)System.Math.Round(damageValue, 1, System.MidpointRounding.AwayFromZero),
            isMiss = checkHit > accuracy,
            isNegation = false
        };

        return damage;
    }

    // 피격
    public void TakeDamage(Damage damage)
    {
        if (!damage.isNegation)
        {
            // 받은 데미지만큼 체력 감소 (버프 체력이 있으면 먼저 감소)
            if (buffNowHealth > 0)
            {
                if (buffNowHealth >= damage.value)
                {
                    buffNowHealth -= damage.value;
                    nowHealth -= damage.value;
                }

                else
                {
                    float pureNowHealthDamage = damage.value - buffNowHealth;

                    nowHealth -= buffNowHealth;
                    buffNowHealth = 0;

                    nowHealth -= pureNowHealthDamage;
                }
            }

            else
            {
                nowHealth -= damage.value;
            }

            // 데미지 출력
            damageManager.ShowDamage(damage);

            // 사망 확인
            CheckDie(damage);
        }

        else
        {
            // 데미지 무효화 애니메이션 재생
            anim.SetTrigger("damageNegation");

            // 데미지 무효화 효과음 재생
            SFXManager.instance.PlaySFX("SFX_DAMAGENEGATION");
        }

        damageManager.ShowVFX(damage.isNegation);
    }

    // 사망 확인
    public virtual void CheckDie(Damage damage)
    {
        // 체력이 0이면
        if (nowHealth <= 0)
        {
            // 은폐 상태인 경우 은폐 해제
            if (isConcealment)
            {
                Concealment(false);
            }

            // 위치했던 타일에서 자기자신 제거
            tile.Unit = null;

            // 자신이 위치한 타일 제거
            tile = null;

            // 가치 점수 빼기 + 유닛 리스트에서 제거
            if (isAlly)
            {
                m_Player.AllyUnit_ValuePoint -= valuePoint;
                m_Unit.AllyUnitList.Remove(unitID);
            }

            else
            {
                m_Player.EnemyUnit_ValuePoint -= valuePoint;
                m_Unit.EnemyUnitList.Remove(unitID);
            }

            // 애니메이션 재생
            anim.SetInteger("death", Random.Range(1, 3));

            // 효과음 재생
            SFXManager.instance.PlaySFX(dieSFX);

            // 가치 점수 확인
            if (M_Player.CheckOutNumbered != null)
            {
                M_Player.CheckOutNumbered.Invoke();
            }
        }

        else
        {
            // 애니메이션 재생
            anim.SetTrigger("damage");

            if (damage.isMiss)
            {
                SFXManager.instance.PlaySFX(missSFX);
            }

            else
            {
                SFXManager.instance.PlaySFX(damageSFX);
            }
        }
    }

    // 스킬 이펙트 적용
    public void ApplySkillVFX(GameObject effect)
    {
        // 이미 있는 이펙트인지 확인
        foreach (GameObject haveEffect in skillVFXList)
        {
            if (haveEffect != null && haveEffect.name == effect.name)
            {
                // 은폐 상태면 끄고, 은폐가 아니면 켬
                haveEffect.SetActive(isAlly || !isConcealment);
                return;
            }
        }

        // 없으면 생성
        GameObject skillEffect = Instantiate(effect, transform);
        skillEffect.name = effect.name;
        skillEffect.transform.position = transform.position;

        skillEffect.SetActive(isAlly || !isConcealment);

        skillVFXList.Add(skillEffect);
    }

    // 스킬 이펙트 끄기
    public void OffSkillVFX(GameObject effect)
    {
        foreach (GameObject skillEffect in skillVFXList)
        {
            if (skillEffect != null && skillEffect.name == effect.name)
            {
                skillEffect.SetActive(false);
                break;
            }
        }
    }

    // 외형 적용
    public void SetMat(SetMatMode setMatMode)
    {
        foreach (MeshRenderer mr in unitMesh.Mr)
        {
            switch (setMatMode)
            {
                case SetMatMode.Default:
                    mr.material = meshMat;
                    break;

                case SetMatMode.OutLine:
                    mr.material = outlineMat;
                    break;

                case SetMatMode.Concealment:
                    mr.material = concealmentMat;
                    break;
            }
        }

        foreach (SkinnedMeshRenderer smr in unitMesh.Smr)
        {
            switch (setMatMode)
            {
                case SetMatMode.Default:
                    smr.material = meshMat;
                    break;

                case SetMatMode.OutLine:
                    smr.material = outlineMat;
                    break;

                case SetMatMode.Concealment:
                    smr.material = concealmentMat;
                    break;
            }
        }

        unitBase.SetMat(isSelected, isAlly, isConcealment);
    }

    #region 정보 받아오기
    // 유닛 매니저 받아오기
    public void GetUnitManager(UnitManager unitManager)
    {
        m_Unit = unitManager;
    }

    // 맵 매니저 받아오기
    public void GetMapManager(MapManager mapManager)
    {
        m_Map = mapManager;
    }

    // 플레이어 매니저 받아오기
    public void GetPlayerManager(PlayerManager playerManager)
    {
        m_Player = playerManager;
    }

    // 스탯 받아오기
    public virtual void SetStat(UnitData unitData)
    {
        unitType = unitData.UnitType;
        valuePoint = unitData.ValuePoint;
        attackType = unitData.AttackType;
        attackPower = unitData.AttackPower;
        attackRange = unitData.AttackRange;
        penetration = unitData.Penetration;
        defense = unitData.Defense;
        maxHealth = unitData.Health;
        nowHealth = maxHealth;
        movement = unitData.Movement;
    }
    #endregion
}
