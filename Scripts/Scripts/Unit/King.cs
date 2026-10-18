using UnityEngine;

public class King : Unit
{
    [Header("")]
    [SerializeField] GameObject closeCombatVFX;
    public override GameObject CloseCombatVFX => closeCombatVFX;
}
