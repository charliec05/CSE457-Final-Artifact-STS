using UnityEngine;

public class AttackOrbEffect : MonoBehaviour
{
    public Transform target;
    public float speed = 8f;

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.3f)
        {
            Destroy(gameObject);
        }
    }
}
