using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    Image healthBar;
    public float maxHealth = 100f;
    public float HP;
    
    void Start()
    {
        healthBar = GetComponent<Image>();
        HP = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.fillAmount = HP / maxHealth;
    }
}
