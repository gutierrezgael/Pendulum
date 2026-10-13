using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStats))]
public class PlayerShooter : MonoBehaviour
{
    public Projectile projectilePrefab;
    public Transform shootPoint;

    PlayerStats stats;
    float nextShotTime;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.isPressed && Time.time >= nextShotTime)
        {
            Shoot();
            nextShotTime = Time.time + 1f / stats.fireRate;
        }
    }

    void Shoot()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Plane aimPlane = new Plane(Vector3.up, shootPoint.position);
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (aimPlane.Raycast(ray, out float distance))
        {
            Vector3 target = ray.GetPoint(distance);
            Vector3 direction = target - shootPoint.position;
            direction.y = 0f;
            direction.Normalize();

            Projectile p = Instantiate(projectilePrefab, shootPoint.position, Quaternion.LookRotation(direction));
            p.Launch(direction, stats.bulletSpeed, stats.bulletSize, stats.range, stats.damage);
        }
    }
}