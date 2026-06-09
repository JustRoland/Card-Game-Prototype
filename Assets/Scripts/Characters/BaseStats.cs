using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Stats", menuName = "Base Stats")]
public class BaseStats : ScriptableObject
{
    [Header("Movement")]
    public float acceleration = 135;
    public float walkSpeed = 4.5f;
    public float crouchSpeed = 3;
    public float sprintSpeed = 10;
    public float slowedSpeed = 2;
    public float jumpForce = 8;
    public float airAcceleration = 2;
    public float dashForce = 40;
    public float dashAirDistance = 3;
    public float dashCooldown = 2;
    
    [Header("Combat")]
    public float health = 100f;
    public float damageMultiplier = 1f;
    public float defense = 10f;

}