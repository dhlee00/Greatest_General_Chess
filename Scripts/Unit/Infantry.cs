using UnityEngine;

public class Infantry : Unit
{
    [Header("")]
    [SerializeField] GameObject closeCombatVFX;
    public override GameObject CloseCombatVFX => closeCombatVFX;
}
