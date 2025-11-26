using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class FireBookBurnOnEnemy : MonoBehaviour
{
    private EnemyHealth hp;
    private Coroutine burnRoutine;

    private void Awake()
    {
        hp = GetComponent<EnemyHealth>();
    }

    public void StartBurn(float dps, float duration)
    {
        if (hp == null)
        {
            hp = GetComponent<EnemyHealth>();
            if (hp == null) return;
        }

        if (burnRoutine != null)
            StopCoroutine(burnRoutine);

        burnRoutine = StartCoroutine(Burn(dps, duration));
    }

    private IEnumerator Burn(float dps, float duration)
    {
        float t = 0f;

        while (t < duration && hp.GetCurrentHealth() > 0)
        {
            int dmg = Mathf.CeilToInt(dps);
            hp.TakeDamage(dmg, Vector2.zero, 0f);

            yield return new WaitForSeconds(1f);
            t += 1f;
        }

        burnRoutine = null;
    }
}