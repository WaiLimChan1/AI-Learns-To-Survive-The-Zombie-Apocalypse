using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletInUse : MonoBehaviour
{
    [Header("Bullet Stats")]
    [SerializeField] public float speed = 25.0f;

    private void OnTriggerEnter(Collider other)
    {
        GameObject collidedWithGO = other.gameObject;
        if (collidedWithGO.tag == "Zombie")
        {
            Destroy(collidedWithGO);
        }
        Destroy(this.gameObject);
    }

    private void FixedUpdate()
    {
        transform.localPosition += transform.forward * speed * Time.fixedDeltaTime;
    }
}
