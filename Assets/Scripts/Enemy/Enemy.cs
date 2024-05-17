using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : PooledObject
{
    [SerializeField] public Animator animator;
    [SerializeField] private Collider hitbox;
    public Tile targetTile;
    private Vector3 direction;
    private Vector3 goalTilePos;
    public float maxHP;
    public float currentHP;
    public float baseSpeed;
    public float speed;
    public int goldReward;
    public int damage = 1;
    public bool alive = true;
    public bool walking = true;

    [SerializeField] private string[] walkCycleCounts;
    [SerializeField] private Sprite portraitSprite;
    [SerializeField] private string enemyName;

    public bool clickedOn = false;
    public bool showingHP = false;

    public List<Projectile> incomingProjectiles = new List<Projectile>();

    public bool armoured = false;

    private Coroutine slowCo = null;
    private float slowTimer = 0;
    private float maxSlowTimer = 0;
    public bool buffed = false;
    public bool slowed = false;

    [SerializeField] private GameObject popupHolder;

    public override void ResetObject()
    {
        currentHP = maxHP;
        showingHP = false;
        clickedOn = false;
        incomingProjectiles = new List<Projectile>();
        slowTimer = 0;
        maxSlowTimer = 0;
        buffed = false;
        slowed = false;
        base.ResetObject();
    }

    public virtual void SetBuffedSpeed(bool buffing)
    {
        if(buffing)
        {
            speed = baseSpeed * (slowed ? 0.75f : 1.25f);
        }
        else
        {
            speed = baseSpeed * (slowed ? 0.6f : 1);
        }
    }

    public virtual void SetSlowedSpeed(bool slowing)
    {
        if(slowing)
        {
            speed = baseSpeed * (buffed ? 0.75f : 0.6f);
        }
        else
        {
            speed = baseSpeed * (buffed ? 1.25f : 1f);
        }
    }

    public void CheckForBuffingAoEs()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, 0.5f, 1 << 12);

        if (cols == null || cols.Length == 0)
        {
            SetBuffedSpeed(false);
            buffed = false;
        }
    }

    public void SlowDown(float duration)
    {
        if(slowCo != null) 
        { 
            slowTimer = 0;
            maxSlowTimer = duration;
        }
        slowCo = StartCoroutine(SlowDownCo(duration));
    }

    private IEnumerator SlowDownCo(float duration)
    {
        slowed = true;
        SetSlowedSpeed(true);
        slowTimer = 0;
        maxSlowTimer = duration;
        while (slowTimer < maxSlowTimer)
        {
            while(GameManager.instance.gameState != GameManager.GameState.Playing) { yield return null; }

            slowTimer += Time.deltaTime;
            yield return null;
        }
        SetSlowedSpeed(false);
        slowed = false;
    }

    public void BuffSpeed()
    {
        buffed = true;
        SetBuffedSpeed(true);
    }

    public virtual void Init(Tile spawnTile)
    {
        targetTile = spawnTile.parentTile;
        transform.position += new Vector3(0, 0.5f, 0);
        goalTilePos = targetTile.transform.position;
        goalTilePos.y = transform.position.y;
        direction = (goalTilePos - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        hitbox.enabled = false;
        alive = false;
        speed = baseSpeed;
        walking = true;

    }
    public virtual void InitWithTarget(Tile _targetTile)
    {
        targetTile = _targetTile;
        transform.position += new Vector3(0, 0.5f, 0);
        goalTilePos = targetTile.transform.position;
        goalTilePos.y = transform.position.y;
        direction = (goalTilePos - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        hitbox.enabled = false;
        alive = false;
        speed = baseSpeed;
    }

    public void BeginWalk()
    {
        string walk = "Walk" + walkCycleCounts[Random.Range(0, walkCycleCounts.Length)];
        animator.Play(walk, 0, Random.Range(0f, 1f));
        hitbox.enabled = true;
        alive = true;
    }

    public void OnEnable()
    {
        animator.Play("Spawn");
    }

    public virtual void Update()
    {
        if(alive && GameManager.instance.gameState == GameManager.GameState.Playing && walking)
        {
            transform.position += direction * speed * Time.deltaTime;
            if(Vector3.Distance(transform.position, goalTilePos) < 0.05f)
            {
                targetTile = targetTile.parentTile;
                goalTilePos = targetTile.transform.position;
                goalTilePos.y = transform.position.y;
                direction = (goalTilePos - transform.position).normalized;
                if (gameObject.activeInHierarchy) { StartCoroutine(RotateToFaceNextTile()); }
            }
        }
    }

    private IEnumerator RotateToFaceNextTile()
    {
        Quaternion startRot = transform.rotation;
        Quaternion goalRot = Quaternion.LookRotation(direction);
        float timer = 0;
        while(timer < 0.25f)
        {
            while(GameManager.instance.gameState != GameManager.GameState.Playing) { yield return null; }

            timer += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(startRot, goalRot, timer / 0.25f);
            yield return null;
        }
        transform.rotation = goalRot;
    }

    public virtual void TakeDamage(float damageTaken, Tower attackingTower)
    {
        if(currentHP <= 0) { return; }
        GameManager.instance.DamagePopUp((int)damageTaken, popupHolder.transform.position);
        currentHP -= damageTaken;
        if (showingHP)
        {
            GameManager.instance.GetUIManager().ShowEnemyHPDisplay(portraitSprite, (int)currentHP, (int)maxHP, damage, enemyName, this, armoured);
        }
        if(currentHP <= 0)
        {
            attackingTower.GiveKill();
            Kill();
        }
    }

    public virtual void Kill()
    {
        foreach(Projectile projectile in incomingProjectiles)
        {
            projectile.target = null;
        }
        if(showingHP)
        {
            GameManager.instance.GetUIManager().HideEnemyHPDisplay();
        }
        GameManager.instance.EnemyKilled(goldReward, popupHolder.transform.position);
        alive = false;
        hitbox.enabled = false;
        animator.Play("Death");
        //Release();
    }

    public override void Release()
    {
        GameObject effect = GameManager.instance.GetEffect(3);
        effect.transform.position = transform.position + Vector3.up * 0.25f - transform.forward * 0.25f;
        effect.SetActive(true);
        if(slowCo != null) { StopCoroutine(slowCo); }
        base.Release();
    }

    public float GetDistanceToGoal()
    {
        float dist = 0;
        Tile tile = targetTile;
        while(tile.parentTile != tile)
        {
            dist++;
            tile = tile.parentTile;
        }
        dist += Vector3.Distance(transform.position, goalTilePos);
        return dist;
    }

    private void OnMouseEnter()
    {
        showingHP = true;
        GameManager.instance.GetUIManager().ShowEnemyHPDisplay(portraitSprite, (int)currentHP, (int)maxHP, damage, enemyName, this, armoured);
    }

    private void OnMouseUp()
    {
        clickedOn = true;
    }

    private void OnMouseExit()
    {
        if (clickedOn) { return; }
        showingHP = false;
        GameManager.instance.GetUIManager().HideEnemyHPDisplay();
    }

    public void StopShowingInfo(bool hideDisplay = true)
    {
        clickedOn = false;
        showingHP = false;
        if (hideDisplay) { GameManager.instance.GetUIManager().HideEnemyHPDisplay(); }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Castle"))
        {
            AttackCastle();
        }
    }

    public virtual void AttackCastle()
    {
        animator.Play("Attack");
        walking = false;
    }

    public void DealDamage()
    {
        if (!alive) { return; }
        GameManager.instance.EnemyReachedGoal(damage);
        if (showingHP)
        {
            GameManager.instance.GetUIManager().HideEnemyHPDisplay();
        }
        alive = false;
        hitbox.enabled = false;
        Release();
    }
}
