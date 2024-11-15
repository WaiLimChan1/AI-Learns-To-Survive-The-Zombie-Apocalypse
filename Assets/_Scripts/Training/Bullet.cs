using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //Events
    public event Action<GameObject> OnBulletCollision;

    [Header("Environment")]
    [SerializeField] private MLEnvironment MLEnvironment;

    [Header("Bullet Stats")]
    [SerializeField] public float speed = 25.0f;

    public void SetUp(MLEnvironment MLEnvironment)
    {
        this.MLEnvironment = MLEnvironment;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject collidedWithGO = other.gameObject;
        OnBulletCollision?.Invoke(collidedWithGO);
        OnBulletCollision = null;
        MLEnvironment.DestroyAndRemoveBullet(this.gameObject);
    }

    private void FixedUpdate()
    {
        transform.localPosition += transform.forward * speed * Time.fixedDeltaTime;
    }
}
