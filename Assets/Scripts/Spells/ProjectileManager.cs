using UnityEngine;
using System;

public class ProjectileManager : MonoBehaviour
{
    public GameObject[] projectiles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.projectileManager = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //I usually prefer using default parameters, but this still might come in handy - Eisig
    //public void CreateProjectile(string trajectory, Vector3 where, Vector3 direction, float speed, Action<Hittable,Vector3> onHit)
    //{
    //    GameObject new_projectile = Instantiate(projectiles[0], where + direction.normalized*1.1f, Quaternion.Euler(0,0,Mathf.Atan2(direction.y, direction.x)*Mathf.Rad2Deg));
    //    new_projectile.GetComponent<ProjectileController>().movement = MakeMovement(trajectory, speed);
    //    new_projectile.GetComponent<ProjectileController>().OnHit += onHit;
    //}

    public void CreateProjectile(string trajectory, Vector3 where, Vector3 direction, float speed, Action<Hittable, Vector3, int> onHit, float lifetime = -1, int num_splits = 0, Hittable filter = null)
    {
        GameObject new_projectile = Instantiate(projectiles[0], where + direction.normalized * 1.1f, Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
        new_projectile.GetComponent<ProjectileController>().movement = MakeMovement(trajectory, speed);
        new_projectile.GetComponent<ProjectileController>().OnHit += onHit;
        if (lifetime >= 0) new_projectile.GetComponent<ProjectileController>().SetLifetime(lifetime);
        new_projectile.GetComponent<ProjectileController>().splits = num_splits;
        if (filter != null) new_projectile.GetComponent<ProjectileController>().SetFilter(filter);
    }

    public ProjectileMovement MakeMovement(string name, float speed)
    {
        if (name == "straight")
        {
            return new StraightProjectileMovement(speed);
        }
        if (name == "homing")
        {
            return new HomingProjectileMovement(speed);
        }
        if (name == "spiraling")
        {
            return new SpiralingProjectileMovement(speed);
        }
        return null;
    }

}
