using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Stats", menuName = "Base Stats")]
public class BaseStats : ScriptableObject
{
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
    public int damage = 10;
    public int defense = 10;

}