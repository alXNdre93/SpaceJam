using UnityEngine;

public class Health
{

    private float currentHealth;
    private float maxHealth;
    private float healthRegenRate;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Health( float _maxHealth, float _healthRegenRate, float _currentHealt = 100)
    {
        currentHealth = _currentHealt;
        maxHealth = _maxHealth;
        healthRegenRate = _healthRegenRate;
    }

    public Health()
    {
        //Empty contructor
    }

    public void AddHealth(float value)
    {
        currentHealth += value;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    public void DeductHealth(float value)
    {
        currentHealth -= value;
        if (currentHealth < 0)
            currentHealth = 0;
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public void RegenHealth()
    {
        AddHealth(healthRegenRate * Time.deltaTime);
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }
}
