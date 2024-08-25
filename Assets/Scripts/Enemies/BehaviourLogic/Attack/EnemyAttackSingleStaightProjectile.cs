using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack-Straight-Single Projectile", menuName = "Enemy Logic/Attack Logic/Straight Single Projectile")]
public class EnemyAttackSingleStaightProjectile : EnemyAttackSOBase
{
    [SerializeField] private Rigidbody2D bulletPrefab;
    private float bulletSpeed = 10f;

    [SerializeField] private float _timer;
    [SerializeField] private float _timeBetweenShots = 2f;

    [SerializeField] private float _exitTimer;
    [SerializeField] private float _timeTillExit = 3f;
    [SerializeField] private float _distanceToCountExit = 3f;
    public override void DoAnimationTriggerEventLogic(Enemy.AnimationtriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        enemy.MoveEnemy(Vector2.zero);

        if (_timer > _timeBetweenShots)
        {
            _timer = 0;
            Vector2 dir = (playerTransform.position - enemy.transform.position).normalized;

            Rigidbody2D bullet = GameObject.Instantiate(bulletPrefab, enemy.transform.position, Quaternion.identity);
            bullet.velocity = dir * bulletSpeed;
        }

        if (Vector2.Distance(playerTransform.position, enemy.transform.position) > _distanceToCountExit)
        {
            _exitTimer += Time.deltaTime;

            if (_exitTimer > _timeTillExit)
                enemy.StateMachine.ChangeState(enemy.ChaseState);
        }
        else
        {
            _exitTimer = 0;
        }

        _timer += Time.deltaTime;
    }

    public override void DoPhysicsUpdateLogic()
    {
        base.DoPhysicsUpdateLogic();
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }
}
