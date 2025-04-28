using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public GameObject ExplosionVFX;

    public void Start()
    {
        StartCoroutine(Boom());
    }

    private IEnumerator Boom()
    {
        yield return new WaitForSeconds(3);
        Explode();
    }

    private void Explode()
    {
        var explosion = Instantiate(ExplosionVFX, transform.position, Quaternion.identity);
        Destroy(transform, 5);
        var colliders = Physics.OverlapSphere(transform.position, 3);
        foreach (var collider in colliders)
        {
            if (collider.GetComponent<TDCharater>())
            {
                collider.GetComponent<TDCharater>().GetDamage(50);
            }
        }
        Destroy(gameObject, 3);
        Destroy(explosion, 3);
    }
}
