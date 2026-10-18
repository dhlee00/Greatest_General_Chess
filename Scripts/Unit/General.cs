using System.Collections;
using System.Collections.Generic;
using UltEvents;
using UnityEngine;

public class General : Unit
{
    [SerializeField] string projectileSFX;
    public override string ProjectileSFX => projectileSFX;

    [Header("")]
    // 장군 이름
    [SerializeField] string generalName;
    public string GeneralName { get => generalName; set => generalName = value; }

    // 보유 스킬들
    [SerializeField] SkillData[] skill;
    public SkillData[] Skill { get => skill; set => skill = value; }

    // 아군 유닛 리스트
    public Dictionary<string, Unit> UnitList => IsAlly ? M_Unit.AllyUnitList : M_Unit.EnemyUnitList;

    // 범위 스킬 대리자
    [SerializeField] UltEvent auraInfluence;
    public UltEvent AuraInfluence { get => auraInfluence; set => auraInfluence = value; }

    [SerializeField] Arrow arrow;
    public override Arrow Arrow => arrow;

    // 스탯 + 스킬 설정
    public override void SetStat(UnitData unitData)
    {
        base.SetStat(unitData);

        GeneralData generalData = unitData as GeneralData;

        generalName = generalData.GeneralName;

        skill = new SkillData[2];
        skill[0] = new SkillData(generalData.SkillData[0]);
        skill[1] = new SkillData(generalData.SkillData[1]);
    }

    // 스킬 대상 확인
    public List<Unit> CheckTarget(SkillTarget skillTarget, int rangeValue)
    {
        List<Unit> targetList = new List<Unit>();

        foreach (Unit target in UnitList.Values)
        {
            if (skillTarget == SkillTarget.AllyUnit)
            {
                targetList.Add(target);
            }

            else if ((skillTarget == SkillTarget.HeavyCavalry) && (target.UnitType == UnitType.HeavyCavalry))
            {
                targetList.Add(target);
            }

            else if ((skillTarget == SkillTarget.Trebuchet) && (target.UnitType == UnitType.Trebuchet))
            {
                targetList.Add(target);
            }
        }

        if (rangeValue == 0)
        {
            return targetList;
        }

        else
        {
            List<Unit> inRangetargetList = new List<Unit>();

            foreach (Unit target in targetList)
            {
                if (M_Map.FindPath.GetHexDis(Coord, target.Coord) <= rangeValue)
                {
                    inRangetargetList.Add(target);
                }
            }

            return inRangetargetList;
        }
    }

    // 기동
    public override IEnumerator Move(Tile destination)
    {
        // 범위 스킬 확인
        if (auraInfluence != null && auraInfluence.HasCalls)
        {
            auraInfluence.Invoke();
        }

        NowMoving = true;

        M_Player.IsActing = true;

        // 이전 위치 타일에서 자기자신 제거
        Tile.Unit = null;

        // 애니메이션 재생
        StartCoroutine(PlayMoveAnim());

        // 효과음 재생
        SFXManager.instance.PlaySFX(MoveSFX);

        // 경로 가져오기
        List<Vector2Int> pathTiles = M_Map.FindPath.Path(Tile.TileData.Coord, destination.TileData.Coord);

        // 이펙트 출력
        StartCoroutine(GenerateMoveVFX(pathTiles.Count));

        // 경로 반복
        foreach (Vector2Int pathTile in pathTiles)
        {
            // 시작 타일 제외
            if (pathTile == Tile.Coord) continue;

            // 시작 지점
            Vector3 startPos = transform.position;

            // 도착 지점
            Vector3 endPos = M_Map.TileList[pathTile].TileData.TilePos;

            // 만약 도착 지점이 층수가 차이나는 경우 유닛 발판 투명도 조절
            if (Tile.TileData.TileFloor != M_Map.TileList[pathTile].TileData.TileFloor)
            {
                UnitBase.SetFade(10f);
            }

            // 방향
            Vector3 dir = (new Vector3(endPos.x, 0, endPos.z) - new Vector3(startPos.x, 0, startPos.z)).normalized;

            if (Vector3.Angle(UnitMesh.transform.forward, dir) > 0f)
            {
                // 현재 회전값
                Quaternion startRot = UnitMesh.transform.rotation;

                // 기동하려는 지점 쪽을 바라보는 회전값
                Quaternion endRot = Quaternion.LookRotation(dir);

                // 회전 중인 시간
                float rotTime = 0f;

                while (rotTime < RotDuration)
                {
                    rotTime += Time.deltaTime;
                    float timer = rotTime / RotDuration;

                    // 현재 회전 방향에서 이동하려는 타일 쪽을 서서히 향함
                    UnitMesh.transform.rotation = Quaternion.Slerp(startRot, endRot, timer);
                    yield return null;
                }

                // 최종 회전값 보정
                UnitMesh.transform.rotation = endRot;
            }

            // 기동 진행 시간
            float moveDuPerTileCount = MoveDuration / pathTiles.Count;

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
            Tile = M_Map.TileList[pathTile];

            // 타일 확인
            CheckTile(Tile);

            // 발판 투명도 원래대로
            UnitBase.SetFade(100f);
        }

        // 정면 바라보기
        Quaternion nowRot = UnitMesh.transform.localRotation;
        float time = 0;

        while (time < 0.1f)
        {
            time += Time.deltaTime;
            float timer = time / 0.1f;

            UnitMesh.transform.localRotation = Quaternion.Slerp(nowRot, Quaternion.Euler(0, 0, 0), timer);
            yield return null;
        }

        NowMoving = false;
        
        // 범위 스킬 확인
        if (auraInfluence != null && auraInfluence.HasCalls)
        {
            auraInfluence.Invoke();
        }

        // 기동하려는 타일에 자기자신 전달
        destination.Unit = this;

        // 모든 유닛 은폐 확인
        M_Player.AllUnitCheckCanConcealment();

        // 기동 한 상태
        EndMove = true;

        // 효과음 종료
        SFXManager.instance.StopSFX(MoveSFX);

        // 행동 종료
        M_Player.EndAction();
    }

    public override void Concealment(bool concealment)
    {
        base.Concealment(concealment);

        if (!IsAlly)
        {
            if (auraInfluence != null && auraInfluence.HasCalls)
            {
                foreach (SkillData skill in skill)
                {
                    foreach (GameObject effect in SkillVFXList)
                    {
                        if (effect.name == skill.SkillVFXPrefab.name)
                        {
                            effect.SetActive(!concealment);
                            return;
                        }
                    }
                }
            }
        }
    }

    // 사망 확인
    public override void CheckDie(Damage damage)
    {
        // 체력이 0이면
        if (NowHealth <= 0)
        {
            for (int skillIndex = 0; skillIndex < 2; skillIndex++)
            {
                if (skill[skillIndex].ActivateTiming == ActivateTiming.OutNumbered)
                {
                    skill[skillIndex].ActivateCondition.Invoke(this, skill[skillIndex], false);
                }
            }

            // 스킬 효과 삭제
            skill[0].Deactivate(this);
            skill[1].Deactivate(this);
        }

        base.CheckDie(damage);
    }
}
