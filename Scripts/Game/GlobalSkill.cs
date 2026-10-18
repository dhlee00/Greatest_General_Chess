using UnityEngine;

public static class GlobalSkill
{
    // 수적 열세 확인
    public static void CheckOutNumbered(General general, SkillData skillData, bool onoff)
    {
        if (onoff)
        {
            general.M_Player.CheckOutNumbered += () =>
            {
                if ((general.M_Player.EnemyUnit_ValuePoint - general.M_Player.AllyUnit_ValuePoint) >= 8)
                {
                    if (!skillData.IsActivate)
                    {
                        skillData.Activate(general);
                        GameManager.instance.ActivateSkillRpc(skillData.SkillID, true);
                    }
                }

                else
                {
                    if (skillData.IsActivate)
                    {
                        skillData.Deactivate(general);
                        GameManager.instance.ActivateSkillRpc(skillData.SkillID, false);
                    }
                }
            };
        }

        else
        {
            general.M_Player.CheckOutNumbered = null;
        }
    }

    // 범위 스킬
    public static void AuraInfluence(General general, SkillData skillData, bool onoff)
    {
        if (onoff)
        {
            general.ApplySkillVFX(skillData.SkillVFXPrefab);
            skillData.AddStat(general, skillData);
            skillData.IsActivate = true;

            general.AuraInfluence += () =>
            {
                if (skillData.IsActivate)
                {
                    skillData.SubtractStat(general, skillData);
                    skillData.IsActivate = false;
                }

                else
                {
                    skillData.AddStat(general, skillData);
                    skillData.IsActivate = true;

                    if (!general.IsAlly && general.IsConcealment)
                    {
                        foreach (Unit unit in general.CheckTarget(skillData.SkillTarget, skillData.RangeValue))
                        {
                            foreach (GameObject effect in unit.SkillVFXList)
                            {
                                if (effect.name == skillData.TargetVFXPrefab.name)
                                {
                                    effect.SetActive(false);
                                    break;
                                }
                            }
                        }
                    }
                }
            };

            foreach (Unit target in general.UnitList.Values)
            {
                target.CheckAuraInfluence += () =>
                {
                    if (target.M_Map.FindPath.GetHexDis(target.Tile.Coord, general.Tile.Coord) <= skillData.RangeValue)
                    {
                        if (!target.AppliedSkill.Contains(skillData))
                        {
                            target.BuffStat(skillData);

                            if (target.IsAlly || !general.IsConcealment)
                            {
                                target.ApplySkillVFX(skillData.TargetVFXPrefab);
                            }

                            if (!target.IsAlly && general.IsConcealment)
                            {
                                target.OffSkillVFX(skillData.TargetVFXPrefab);
                            }
                        }
                    }

                    else
                    {
                        if (target.AppliedSkill.Contains(skillData))
                        {
                            target.DebuffStat(skillData);
                            target.OffSkillVFX(skillData.TargetVFXPrefab);
                        }
                    }
                };
            }
        }

        else
        {
            general.AuraInfluence = null;

            foreach (Unit target in general.UnitList.Values)
            {
                if (target.AppliedSkill.Contains(skillData))
                {
                    target.DebuffStat(skillData);
                    target.OffSkillVFX(skillData.TargetVFXPrefab);
                }

                target.CheckAuraInfluence = null;
            }
        }
    }

    // 데미지 무효화
    public static void CheckDamageNegation(General general, SkillData skillData, int value, bool onoff)
    {
        foreach (Unit target in general.UnitList.Values)
        {
            if (onoff)
            {
                target.CheckDamageNegation += (damage) =>
                {
                    float ran = Random.Range(0f, 1f);

                    if ((ran * 100) <= value)
                    {
                        damage.value.value = 0.0f;
                        damage.value.isNegation = true;
                    }
                };

                target.AppliedSkill.Add(skillData);
            }

            else
            {
                target.CheckDamageNegation = null;
            }
        }
    }

    // 추가 턴 진행
    public static void CheckExtraTurn(General general, SkillData skillData, int value, bool onoff)
    {
        if (onoff)
        {
            if (general.IsAlly)
            {
                GameManager.instance.CheckExtraTurn += (extraTurn) =>
                {
                    float ran = Random.Range(0f, 1f);

                    if ((ran * 100) <= value)
                    {
                        extraTurn.value = true;
                    }
                };
            }
        }

        else
        {
            GameManager.instance.CheckExtraTurn = null;
        }
    }
}
