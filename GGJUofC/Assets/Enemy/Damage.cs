using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    public float damage = 10f;

    private bool _hasHitThisAttack = false;

    // Every time the hitbox turns ON for a new attack, allow one hit again
    private void OnEnable()
    {
        _hasHitThisAttack = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (_hasHitThisAttack) return;

        if (!other.CompareTag("Player")) return;

        // Works even if the collider is on a child
        PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
        if (ph == null)
            ph = other.transform.root.GetComponent<PlayerHealth>();

        if (ph != null)
        {
            ph.TakeDamage(damage);
            _hasHitThisAttack = true; // only once per attack window
        }
    }
}