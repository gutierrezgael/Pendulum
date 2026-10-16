using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Shooting")]
    public float fireRate = 3f;      // shots per second (higher = faster)
    public float bulletSpeed = 15f;  // how fast the ball travels
    public float bulletSize = 0.3f;  // ball diameter
    public float range = 15f;        // distance in units before the ball disappears
    public int damage = 1;           // damage per ball 
}