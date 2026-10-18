using UnityEngine;

public class HeavyCavalry : Unit
{
    [Header("")]
    [SerializeField] GameObject closeCombatVFX;
    public override GameObject CloseCombatVFX => closeCombatVFX;
}
