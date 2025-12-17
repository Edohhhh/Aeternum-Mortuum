using UnityEngine;

[CreateAssetMenu(fileName = "LaRosaPowerUp", menuName = "PowerUps/LaRosa")]
public class LaRosaPowerUp : PowerUp
{
    public int reflectDamage = 1;

    public override void Apply(PlayerController player)
    {
        // Creamos/obtenemos el manager persistente
        var mgrGo = GameObject.Find("LaRosa_Manager");
        LaRosaManager mgr;

        if (mgrGo == null)
        {
            mgrGo = new GameObject("LaRosa_Manager");
            mgr = mgrGo.AddComponent<LaRosaManager>();
            Object.DontDestroyOnLoad(mgrGo);
        }
        else
        {
            mgr = mgrGo.GetComponent<LaRosaManager>();
            if (mgr == null) mgr = mgrGo.AddComponent<LaRosaManager>();
        }

        mgr.reflectDamage = reflectDamage;
        mgr.SetEnabled(true);
        mgr.RebindNow(); // por si se aplicó en medio de la escena
    }

    public override void Remove(PlayerController player)
    {
        var mgrGo = GameObject.Find("LaRosa_Manager");
        if (mgrGo != null)
        {
            var mgr = mgrGo.GetComponent<LaRosaManager>();
            if (mgr != null) mgr.SetEnabled(false);
        }
    }
}
