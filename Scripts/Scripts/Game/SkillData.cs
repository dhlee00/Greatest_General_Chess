using System.Collections.Generic;
using UltEvents;
using UnityEngine;

// 발동 타이밍
public enum ActivateTiming
{
    GameStart,   // 게임 시작 시
    Activate,    // 발동 시
    OutNumbered  // 수적 열세 시
}

// 스킬 대상
public enum SkillTarget
{
    Global,        // 전역
    AllyUnit,      // 아군 유닛
    HeavyCavalry,  // 중장기병
    Trebuchet,     // 트레뷰셋
}

// 적용 효과
public enum EffectKey
{
    AttackPower,
    Accuracy,
    Penetration,
    Defense,
    Health,
    Movement,
    DamageNegation,
    ExtraTurn
}

// 적용되는 효과
[System.Serializable]
public struct Effect
{
    // 적용할 효과 대상
    [SerializeField] EffectKey effectKey;
    public EffectKey EffectKey { get => effectKey; set => effectKey = value; }

    // 효과 값
    [SerializeField] float effectValue;
    public float EffectValue { get => effectValue; set => effectValue = value; }
}

// 스킬값 클래스
public class SkillValue<T>
{
    public T value;
}

// 스킬 데이터
[System.Serializable]
public class SkillData
{
    // 스킬 아이콘
    [SerializeField] Sprite skillIcon;
    public Sprite SkillIcon => skillIcon;

    // 스킬 이펙트
    [SerializeField] GameObject skillVFXPrefab;
    public GameObject SkillVFXPrefab => skillVFXPrefab;

    // 대상 이펙트
    [SerializeField] GameObject targetVFXPrefab;
    public GameObject TargetVFXPrefab => targetVFXPrefab;

    // 효과음 아이디
    [SerializeField] string sfxID;
    public string SfxID => sfxID;

    // 스킬 설명
    [SerializeField][TextArea(1, 2)] string skillInfo;
    public string SkillInfo => skillInfo;

    [Header("")]

    // 스킬 아이디
    [SerializeField] string skillID;
    public string SkillID { get => skillID; set => skillID = value; }

    // 스킬 이름
    [SerializeField] string skillName;
    public string SkillName { get => skillName; set => skillName = value; }

    // 발동 타이밍
    [SerializeField] ActivateTiming activateTiming;
    public ActivateTiming ActivateTiming { get => activateTiming; set => activateTiming = value; }

    // 발동 조건
    [SerializeField] UltEvent<General, SkillData, bool> activateCondition;
    public UltEvent<General, SkillData, bool> ActivateCondition => activateCondition;

    // 스킬 대상
    [SerializeField] SkillTarget skillTarget;
    public SkillTarget SkillTarget { get => skillTarget; set => skillTarget = value; }

    // 스킬 범위 (0이면 전 범위)
    [SerializeField] int rangeValue;
    public int RangeValue { get => rangeValue; set => rangeValue = value; }

    // 지속 턴 (0이면 장군 전사시까지 지속)
    [SerializeField] int durationTurn;
    public int DurationTurn { get => durationTurn; set => durationTurn = value; }

    // 적용되는 효과
    [SerializeField] List<Effect> effect = new List<Effect>();
    public List<Effect> Effect { get => effect; set => effect = value; }

    // 스킬 기능
    [SerializeField] UltEvent<General, SkillData, bool> effectMethod;

    [Header("")]

    // 적용 중
    [SerializeField] bool isActivate;
    public bool IsActivate { get => isActivate; set => isActivate = value; }

    [SerializeField] Unit unit;

    // 생성자
    public SkillData(SkillData data)
    {
        skillIcon = data.skillIcon;                  // 스킬 아이콘
        skillVFXPrefab = data.skillVFXPrefab;        // 스킬 이펙트
        targetVFXPrefab = data.targetVFXPrefab;      // 스킬 대상 이펙트
        sfxID = data.sfxID;                          // 효과음 아이디
        skillID = data.skillID;                      // 스킬 아이디
        skillName = data.skillName;                  // 스킬 이름
        activateTiming = data.activateTiming;        // 발동 타이밍
        skillTarget = data.skillTarget;              // 스킬 대상
        rangeValue = data.rangeValue;                // 스킬 범위 (0이면 전 범위)
        durationTurn = data.durationTurn;            // 지속 턴 (0이면 장군 전사시까지 지속)
        effect = new List<Effect>(data.effect);      // 적용되는 효과
        activateCondition = data.activateCondition;  // 스킬 기능
        effectMethod = data.effectMethod;            // 적용 중
    }
    public SkillData() { }

    // 스킬 실행
    public void Activate(General general)
    {
        // 특수 스킬은 대리자에 등록
        if (effectMethod != null && effectMethod.HasCalls)
        {
            effectMethod.Invoke(general, this, true);
        }

        // 버프 스킬은 바로 실행
        else
        {
            AddStat(general, this);
        }

        isActivate = true;
    }

    // 스킬 해제
    public void Deactivate(General general)
    {
        // 특수 스킬은 대리자에서 제거
        if (effectMethod != null && effectMethod.HasCalls)
        {
            effectMethod.Invoke(general, this, false);
        }

        // 버프 스킬 해제
        else
        {
            SubtractStat(general, this);
        }

        isActivate = false;
    }

    // 유닛 능력치 증가
    public void AddStat(General general, SkillData skillData)
    {
        // 유닛 리스트 순환
        foreach (Unit unit in general.CheckTarget(skillTarget, rangeValue))
        {
            // 능력치 증가
            unit.BuffStat(skillData);

            if (skillData.targetVFXPrefab != null)
            {
                // 이펙트 적용
                unit.ApplySkillVFX(skillData.targetVFXPrefab);
            }
        }
    }

    // 유닛 능력치 감소
    public void SubtractStat(General general, SkillData skillData)
    {
        // 유닛 리스트 순환
        foreach (Unit unit in general.CheckTarget(skillTarget, rangeValue))
        {
            if (unit.AppliedSkill.Contains(skillData))
            {
                // 능력치 감소
                unit.DebuffStat(skillData);

                if (skillData.targetVFXPrefab != null)
                {
                    // 이펙트 끄기
                    unit.OffSkillVFX(skillData.targetVFXPrefab);
                }
            }
        }
    }
}
