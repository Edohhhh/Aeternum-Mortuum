using System.Collections;
using UnityEngine;

public class SlimeDeathState : State<EnemyInputs>
{
    private SlimeController slime;
    private float duration = 1f;
    private float timer = 0f;
    private Vector3 originalPos;

    public SlimeDeathState(SlimeController slime)
    {
        this.slime = slime;
    }

    public override void Awake()
    {
        base.Awake();
        originalPos = slime.transform.position;
        timer = 0f;
        Debug.Log("Entró al SlimeDeathState");
    }

    public override void Execute()
    {
        timer += Time.deltaTime;

        // Vibración visual durante 1 segundo
        float offsetX = Random.Range(-0.1f, 0.1f);
        float offsetY = Random.Range(-0.1f, 0.1f);
        slime.transform.position = originalPos + new Vector3(offsetX, offsetY, 0);

        if (timer >= duration)
        {
            slime.transform.position = originalPos; // Restaurar posición

            // Instanciar nuevos slimes
            GameObject prefab = slime.GetMiniSlimePrefab();
            GameObject.Instantiate(prefab, slime.transform.position + Vector3.right * 0.5f, Quaternion.identity);
            GameObject.Instantiate(prefab, slime.transform.position + Vector3.left * 0.5f, Quaternion.identity);

            // Desregistrar y destruir
            EnemyManager.Instance.UnregisterEnemy();
            GameObject.Destroy(slime.gameObject);
        }
    }

    public override void Sleep()
    {
        base.Sleep();
        slime.transform.position = originalPos;
    }
}
