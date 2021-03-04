using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPistol : Gun
{
    [SerializeField] LayerMask solid;
    [SerializeField] float bulletRadius;
    [SerializeField] Transform stretchBulletPrefab;
    [SerializeField] float bulletLifetime = 1/60f;
    [SerializeField] float bulletHeightWorldUnits;    

    public override void MidFire()
    {       
        float angle = GetShootingAngle(recoil + motionSpread);
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        RaycastHit2D hit = Physics2D.CircleCast(firePoint.position, bulletRadius, dir, Mathf.Infinity, solid);
        
        if (hit.collider != null)
        {
            Toggle toggle = hit.collider.GetComponent<Toggle>();
            if (toggle != null)
            {
                bool dead = toggle.Damage();
                if (dead && gameObject.CompareTag("Player"))
                {                    
                    TemperatureBoost.Instance.InduceStress(1f);                    
                }                    
            }
            
            // Shadow piercing mechanic.
            if (hit.collider.gameObject.CompareTag("Shadow"))
            {
                RaycastHit2D pierceHit = Physics2D.CircleCast(firePoint.position, bulletRadius, dir, Mathf.Infinity, solid);
                if (pierceHit.collider != null)
                {
                    Toggle pierceToggle = pierceHit.collider.GetComponent<Toggle>();
                    if (pierceToggle != null)
                    {
                        bool pierceDead = pierceToggle.Damage();
                        if (pierceDead && gameObject.CompareTag("Player"))
                            TemperatureBoost.Instance.InduceStress(1f);
                    }
                }

                hit = pierceHit;
            }
        }        

        Transform stretchBulletInstance = Instantiate(stretchBulletPrefab, firePoint.position, Quaternion.identity);
        stretchBulletInstance.localEulerAngles = new Vector3(0, 0, -90f + angle * Mathf.Rad2Deg);
        float d = hit.collider != null ? Vector3.Distance(firePoint.position, hit.point) : 20f;
        stretchBulletInstance.localScale = new Vector3(1, d / bulletHeightWorldUnits, 1);
        stretchBulletInstance.localPosition += 0.5f * d * stretchBulletInstance.up;

        StartCoroutine(stretchBulletInstance.GetComponent<Toggle>().DeactivateToggle(bulletLifetime));

        float distToCamera = (hit.point - (Vector2)Camera.main.transform.position).magnitude;
        float x = Mathf.Clamp01(distToCamera / 12f);
        CameraShake.Instance.InduceStress(1 - 0.4f * x * x);               
    }
}
