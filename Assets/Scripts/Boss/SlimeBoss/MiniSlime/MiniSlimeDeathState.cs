using UnityEngine;

public class MiniSlimeDeathState : State<EnemyInputs>
{
    private MiniSlimeController slime;
    private float duration = 1f;
    private float timer = 0f;
    private Vector3 originalPos;

    public MiniSlimeDeathState(MiniSlimeController slime)
    {
        this.slime = slime;
    }

    public override void Awake()
    {
        base.Awake();
        originalPos = slime.transform.position;
        timer = 0f;
        Debug.Log("Entró al MiniSlimeDeathState");
    }

    public override void Execute()
    {
        timer += Time.deltaTime;

        float offsetX = Random.Range(-0.05f, 0.05f);
        float offsetY = Random.Range(-0.05f, 0.05f);
        slime.transform.position = originalPos + new Vector3(offsetX, offsetY, 0);

        if (timer >= duration)
        {
            slime.transform.position = originalPos;

            GameObject prefab = slime.miniSlimePrefab;
            GameObject.Instantiate(prefab, slime.transform.position + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
            GameObject.Instantiate(prefab, slime.transform.position + new Vector3(-0.5f, -0.5f, 0), Quaternion.identity);

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
