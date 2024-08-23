using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private Transform _playerTransform;

    private float bulletSpeed = 10f;

    private float _timer;
    private float _timeBetweenShots = 2f;

    private float _exitTimer;
    private float _timeTillExit = 3f;
    private float _distanceToCountExit = 3f;
    public EnemyAttackState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override void EnterState() { }
    public override void ExitState() { }
    public override void FrameUpdate()  
    {
        base.FrameUpdate();

        enemy.MoveEnemy(Vector2.zero);

        if(_timer > _timeBetweenShots)
        {
            _timer = 0;
            Vector2 dir = (_playerTransform.position - enemy.transform.position).normalized;

            Rigidbody2D bullet = GameObject.Instantiate(enemy.bulletPrefab, enemy.transform.position, Quaternion.identity);
            bullet.velocity = dir * bulletSpeed;
        }

        if(Vector2.Distance(_playerTransform.position, enemy.transform.position) > _distanceToCountExit)
        {
            _exitTimer += Time.deltaTime;

            if(_exitTimer > _timeTillExit)
                enemy.StateMachine.ChangeState(enemy.ChaseState);
        }
        else
        {
            _exitTimer = 0;
        }

        _timer += Time.deltaTime;
    }
    public override void PhysicsUpdate() { }
    public override void AnimationTriggerEvent(Enemy.AnimationtriggerType triggerType) { }
}
