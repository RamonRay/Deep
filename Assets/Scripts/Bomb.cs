using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {
    [SerializeField] string collideTag;
    [SerializeField] string submarineTag;
    [SerializeField] float radius=5f;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] int damageToSub=50;
    private void OnCollisionEnter(Collision collision)
    {
        string tag = collision.gameObject.tag;
        if(tag==collideTag)
        {
            try
            {
                SubmarineVacuum.vacuums[0].UnRegisterObject(gameObject);
            }
            catch
            {
                //
            }
            try
            {
                SubmarineVacuum.vacuums[1].UnRegisterObject(gameObject);
            }
            catch
            {
                //
            }
            BombEffectManager.Explode(transform.position);
            ExplodeDamage();
            Destroy(gameObject);
        }
        if(tag == submarineTag)
        {
            try
            {
                SubmarineVacuum.vacuums[0].UnRegisterObject(gameObject);
            }
            catch
            {
                //
            }
            try
            {
                SubmarineVacuum.vacuums[1].UnRegisterObject(gameObject);
            }
            catch
            {
                //
            }
            SubmarineVaccumCollector.collectors[0].Damage(damageToSub);
            SubmarineVaccumCollector.collectors[1].Damage(damageToSub);
            SubmarineShaker.instance.Shake();
            BombEffectManager.Explode(transform.position);
            ExplodeDamage();
            Destroy(gameObject);

        }
    }

    private void OnTriggerEnter(Collider other)
    {

        string tag = other.gameObject.tag;
        if (tag == collideTag)
        {
            try
            {
                SubmarineVacuum.vacuums[0].UnRegisterObject(gameObject);
            }
            catch
            {
                //
            }
            try
            {
                SubmarineVacuum.vacuums[1].UnRegisterObject(gameObject);
            }
            catch
            {
                //
            }
            BombEffectManager.Explode(transform.position);
            ExplodeDamage();
            Destroy(gameObject);
        }
    }

    private void ExplodeDamage()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, enemyLayer);
        foreach (var collider in colliders)
        {
            Enemy enemy = collider.gameObject.GetComponent<Enemy>();
            TentacleController tc = collider.gameObject.GetComponentInParent<TentacleController>();
            try
            {
                if (enemy != null)
                {
                    enemy.Dead();
                }
                if(tc!=null)
                {
                    tc.KillTentacle();
                }
            }
            catch
            {
                Debug.Log(collider.gameObject.name + "Has no corresponding component");
            }
        }
    }
}
