using System.Collections;
using UnityEngine;

public class LaserEnemy : Enemy
{
    [Header("Laser System")]
    [SerializeField] private Transform firePoint1;
    [SerializeField] private Transform firePoint2;
    [SerializeField] private GameObject[] laserSparkEffects = new GameObject[2];
    [SerializeField] private LineRenderer[] laserBeamRenderers = new LineRenderer[2]; 
    [SerializeField] private float laserDamageTickInterval = 0.1f;
    [SerializeField] private float laserBeamWidth = 0.03f; // Smaller than player's 0.06f
    [SerializeField] private float laserBeamWidthScale = 0.3f; // Smaller than player's 0.5f
    
    private bool lasering, cooldown = false;
    private float laserTickTimer = 0f;

    protected override void Start()
    {
        base.Start();
        health = new Health(1 * gameManager.multiplierEnemyHealth * (isBoss ? 30 : 1), 0, 1 * gameManager.multiplierEnemyHealth * (isBoss ? 30 : 1));
        pointsValue = 3 * (int)gameManager.multiplierPoint * (isBoss ? 30 : 1);
        speed *= gameManager.multiplierEnemySpeed;
        gameObject.transform.localScale = gameObject.transform.localScale * (isBoss ? 5 : 1);
        
        // Initialize arrays if they're null
        if (laserSparkEffects == null)
            laserSparkEffects = new GameObject[2];
        if (laserBeamRenderers == null)
            laserBeamRenderers = new LineRenderer[2];
        
        // Initialize dual laser system
        for (int i = 0; i < 2; i++)
        {
            if (i < laserSparkEffects.Length && laserSparkEffects[i] != null)
                laserSparkEffects[i].SetActive(false);
            if (i < laserBeamRenderers.Length && laserBeamRenderers[i] != null)
            {
                laserBeamRenderers[i].enabled = false;
                laserBeamRenderers[i].positionCount = 2;
                laserBeamRenderers[i].useWorldSpace = true;
                ApplyEnemyLaserBeamWidth(i);
            }
        }
        
        if (isBoss)
        {
            FindAnyObjectByType<UIManager>().SetBossMaxHealth(health.GetMaxHealth());
            FindAnyObjectByType<UIManager>().UpdateEnemyHealth(health.GetMaxHealth());
        }
    }

    protected override void Update()
    {
        base.Update();
        if (target == null || target.gameObject == null)
        {
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        
        if (distanceToTarget < attackRange && !lasering)
        {
            lasering = true;
            StartCoroutine(Laser());
        }

        // Handle the laser beam rendering and damage
        HandleEnemyLaser();

        if (cooldown)
            StartCoroutine(Cooldown());
    }

    public override void Attack(float interval)
    { }

    public override void GetDamage(float damage)
    {
        base.GetDamage(damage);
    }

    public override void Shoot()
    { }

    public override int GetPointsValue()
    {
        return pointsValue;
    }

    IEnumerator Laser()
    {
        // The heavy lifting is done in HandleEnemyLaser(), this just controls timing
        yield return new WaitForSeconds(attackTime);
        cooldown = true;
    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(attackTime);
        lasering = false;
        cooldown = false;
    }
    
    private void HandleEnemyLaser()
    {
        bool laserShouldBeActive = false;

        if (lasering && !cooldown && target != null && target.gameObject != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            if (distanceToTarget <= attackRange)
            {
                laserShouldBeActive = true;

                laserTickTimer += Time.deltaTime;

                if (laserTickTimer >= Mathf.Max(0.02f, laserDamageTickInterval))
                {
                    laserTickTimer = 0f;
                    DamageWithEnemyLaser();
                }
            }
        }
        else
        {
            laserTickTimer = 0f;
        }

        // Handle both lasers - only if target exists
        if (laserSparkEffects != null && laserBeamRenderers != null)
        {
            Transform[] firePoints = { firePoint1, firePoint2 };
            int laserCount = Mathf.Min(firePoints.Length, Mathf.Min(laserSparkEffects.Length, laserBeamRenderers.Length));
            for (int i = 0; i < laserCount; i++)
            {
                Vector2 beamOrigin = firePoints[i] != null ? (Vector2)firePoints[i].position : (Vector2)transform.position;
                Vector2 beamDirection = Vector2.up; // Default direction
                Vector2 beamEnd = beamOrigin + beamDirection * attackRange;

                // Only calculate direction if target exists
                if (target != null && target.gameObject != null)
                {
                    beamDirection = ((Vector2)target.position - beamOrigin).normalized;
                    beamEnd = beamOrigin + beamDirection * attackRange;

                    if (laserShouldBeActive && TryGetEnemyBeamEndPoint(beamOrigin, beamDirection, out Vector2 hitPoint))
                        beamEnd = hitPoint;
                }

                if (i < laserSparkEffects.Length && laserSparkEffects[i] != null)
                {
                    laserSparkEffects[i].transform.position = beamOrigin;
                    laserSparkEffects[i].transform.up = beamDirection;
                    laserSparkEffects[i].SetActive(laserShouldBeActive);
                }

                if (i < laserBeamRenderers.Length && laserBeamRenderers[i] != null)
                {
                    ApplyEnemyLaserBeamWidth(i);
                    laserBeamRenderers[i].enabled = laserShouldBeActive;
                    if (laserShouldBeActive)
                    {
                        laserBeamRenderers[i].SetPosition(0, beamOrigin);
                        laserBeamRenderers[i].SetPosition(1, beamEnd);
                    }
                }
            }
        }
    }

    private void DamageWithEnemyLaser()
    {
        // Early return if target is null
        if (target == null || target.gameObject == null)
            return;
            
        Transform[] firePoints = { firePoint1, firePoint2 };
        
        // Check damage from both lasers
        for (int laserIndex = 0; laserIndex < firePoints.Length; laserIndex++)
        {
            Vector2 origin = firePoints[laserIndex] != null ? (Vector2)firePoints[laserIndex].position : (Vector2)transform.position;
            Vector2 direction = ((Vector2)target.position - origin).normalized;
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, attackRange);
            
            if (hits == null || hits.Length == 0)
                continue;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hitCollider = hits[i].collider;
                if (hitCollider == null)
                    continue;

                if (!hitCollider.CompareTag("Player"))
                    continue;

                IDamageable damageable = hitCollider.GetComponent<IDamageable>();
                if (damageable == null)
                    continue;

                float scaledDamage = weapon.GetDamage() * (gameManager != null ? gameManager.multiplierEnemyDamage : 1f);
                damageable.GetDamage(scaledDamage);
                return; // Only damage once per tick, even with dual lasers
            }
        }
    }

    private bool TryGetEnemyBeamEndPoint(Vector2 origin, Vector2 direction, out Vector2 endPoint)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, attackRange); // Use attackRange
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider == null)
                continue;
            if (IsEnemySelfCollider(hitCollider))
                continue;

            endPoint = hits[i].point;
            return true;
        }

        endPoint = origin + direction * attackRange; // Use attackRange
        return false;
    }

    private bool IsEnemySelfCollider(Collider2D collider)
    {
        if (collider == null)
            return false;

        Transform hitTransform = collider.transform;
        return hitTransform == transform || hitTransform.IsChildOf(transform);
    }

    private void ApplyEnemyLaserBeamWidth(int laserIndex)
    {
        if (laserBeamRenderers == null || laserIndex >= laserBeamRenderers.Length || laserBeamRenderers[laserIndex] == null)
            return;

        float appliedWidth = Mathf.Max(0.005f, laserBeamWidth * Mathf.Max(0.01f, laserBeamWidthScale));
        laserBeamRenderers[laserIndex].startWidth = appliedWidth;
        laserBeamRenderers[laserIndex].endWidth = appliedWidth;
        laserBeamRenderers[laserIndex].widthMultiplier = appliedWidth;
        laserBeamRenderers[laserIndex].widthCurve = AnimationCurve.Constant(0f, 1f, 1f);
    }

    // Override OnPoolDespawnInternal to ensure laser state is cleaned up
    protected override void OnPoolDespawnInternal()
    {
        // Reset laser state
        lasering = false;
        cooldown = false;
        laserTickTimer = 0f;
        
        // Ensure laser effects are disabled
        if (laserSparkEffects != null)
        {
            for (int i = 0; i < laserSparkEffects.Length; i++)
            {
                if (laserSparkEffects[i] != null)
                {
                    laserSparkEffects[i].SetActive(false);
                }
            }
        }
        
        if (laserBeamRenderers != null)
        {
            for (int i = 0; i < laserBeamRenderers.Length; i++)
            {
                if (laserBeamRenderers[i] != null)
                {
                    laserBeamRenderers[i].enabled = false;
                }
            }
        }
        
        base.OnPoolDespawnInternal();
    }

}
