using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MonsterController : MonoBehaviour
{
    private enum State { Idle, Patrol, Chase, Attack, Investigate }
    private State currentState;

    public List<GameObject> attackParts;
    private Dictionary<GameObject, float> _attackDamages;
    private List<BoxCollider> _attackColliders;

    public Transform[] patrolPoints;
    private NavMeshAgent agent;

    public float handDamage = 10f;
    public float mouthDamage = 20f;
    
    public Transform target;
    // TODO: private PlayerController player;
    
    public float detectionRange = 15f;
    public float viewRangeinChasing = 25f;
    private float _detectionRate = 1f;
    public float fovHorizontal = 180f;
    public float fovVertical = 150f;
    public float chaseRange = 20f;
    public float attackRange = 3f;

    public Vector3 patrolCenterPoint;
    public float patrolAreaRadius;
    public float distanceLowerBound = 5f;
    public float distanceUpperBound = 15f;
    // TODO: synchronize layer index and 1 << idx
    private LayerMask _targetMask = 1 << 3;
    private Collider[] _targetsInRange = new Collider[1];
    private Vector3 _headPositionOffset;

    private float _attackCoolDown;
    private float _attackAnimationLength;

    public float investigateSpeed = 5f;
    public float chaseSpeed = 8f;
    public float patrolSpeed = 3f;

    private float _targetCheckRate = 0.5f;

    private bool _hasTarget = false;
    
    
    // TODO: control parameters of navmesh agent
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // TODO: player = target.gameObject.GetComponent<PlayerController>();

        foreach (GameObject part in attackParts)
        {
            _attackColliders.Add(part.GetComponent<BoxCollider>());
            AttackCollisionController attackCollider = part.GetComponent<AttackCollisionController>();
            
            if (part.CompareTag("Hand"))
            {
                _attackDamages[part] = handDamage;
            }
            else if (part.CompareTag("Mouth"))
            {
                _attackDamages[part] = mouthDamage;
            }
            
            attackCollider.onHit += HandleHit;
        }
        
        
        currentState = State.Idle;
        StartCoroutine(StateRoutine());
    }

    IEnumerator StateRoutine()
    {
        while (true)
        {
            switch (currentState)
            {
                case State.Idle:
                    yield return Idle(Random.Range(5f, 8f));
                    break;
                case State.Patrol:
                    yield return Patrol(SampleRandomDestination(), patrolSpeed);
                    break;
                case State.Chase:
                    yield return Chase();
                    break;
                case State.Attack:
                    yield return Attack();
                    break;
                case State.Investigate:
                    yield return Investigate();
                    break;
            }
            yield return null;
        }
    }

    IEnumerator Idle(float idleTime)
    {
        StartCoroutine("LookAround");
        
        int numCheck = Mathf.RoundToInt(idleTime / _targetCheckRate);

        for (int i = 0; i < numCheck; i++)
        {
            if (_hasTarget)
            {
                currentState = State.Chase;
                StopCoroutine("LookAround");
                yield break;
            }
            yield return new WaitForSeconds(_targetCheckRate);
        }
        
        currentState = State.Patrol;
    }

    IEnumerator Patrol(Vector3 destination, float speed)
    {
        agent.speed = speed;
        StartCoroutine("LookAround");

        NavMeshHit hit;
        NavMesh.SamplePosition(destination, out hit, 0, agent.areaMask);
        
        agent.SetDestination(hit.position);
        
        while (agent.remainingDistance > agent.stoppingDistance)
        {
            if (_hasTarget)
            {
                currentState = State.Chase;
                StopCoroutine("LookAround");
                yield break;
            }
            yield return null;
        }
        
        currentState = State.Idle;
    }

    Vector3 SampleRandomDestination()
    {
        // if the gameObject is too far from the patrol Area Center, sample the next orientation in [Orientation to the center] +- 90
        Vector3 distanceToCenter = patrolCenterPoint - transform.position;
        float bound = patrolAreaRadius - distanceToCenter.magnitude < distanceUpperBound ? 90 : 180;
        float randomAngle = Random.Range(-bound, bound);
        
        Quaternion angleOffset = Quaternion.Euler(0, randomAngle, 0);
        Vector3 patrolDirection = angleOffset * distanceToCenter.normalized;
        
        float patrolDistance = Random.Range(distanceLowerBound, distanceUpperBound);
        
        return transform.position + patrolDirection * patrolDistance;
    }

    IEnumerator Chase()
    {
        agent.speed = chaseSpeed;
        
        while (true)
        {

            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            if (distanceToTarget < chaseRange && NavMesh.SamplePosition(target.position, out NavMeshHit hit, 0, agent.areaMask))
            {
                if (distanceToTarget < attackRange)
                {
                    currentState = State.Attack;
                    yield break;
                }
                if (!IsTargetInView(transform.forward, chaseRange))
                {
                    _hasTarget = false;
                    currentState = State.Investigate;
                    yield break;
                }
                
                agent.SetDestination(hit.position);
            }
            else
            {
                _hasTarget = false;
                currentState = State.Patrol;
                yield break;
            }
            yield return new WaitForSeconds(_detectionRate);
        }
    }

    // Coroutine in Idle & Patrol state
    IEnumerator LookAround()
    {
        // TODO: add head rotation or animation; forward direction -> head forward direction
        
        while (true)
        {
            Vector3 forwardDir = transform.forward;

            if (IsTargetInView(forwardDir, detectionRange))
            {
                _hasTarget = true;
                yield break;
            }
            
            yield return new WaitForSeconds(_detectionRate);
        }
    }

    private bool IsTargetInView(Vector3 forwardDirection, float range)
    {
        int numTargets = Physics.OverlapSphereNonAlloc(transform.position, range, _targetsInRange, _targetMask);
        
        for (int i = 0; i < numTargets; i++)
        {
            Transform targetTransform = _targetsInRange[i].transform;
            Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
            
            Vector3 directionHorizontal = Vector3.ProjectOnPlane(directionToTarget, transform.up);
        
            float angleHorizontal = Vector3.Angle(forwardDirection, directionHorizontal);
            
            if (angleHorizontal < fovHorizontal / 2)
            {
                Vector3 directionVertical = Vector3.ProjectOnPlane(directionToTarget, transform.right);
                float angleVertical = Vector3.Angle(forwardDirection, directionVertical);

                if (angleVertical < fovVertical / 2)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + _headPositionOffset, directionToTarget, out hit, range))
                    {
                        // TODO: tag name synchronization
                        if (hit.transform.CompareTag("Player"))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    IEnumerator Attack()
    {
        // TODO: add box colliders to the attacking parts & add several animations & sample attack motion & make lists for animation length and attack cooldown

        float checkRate = 0.1f;
        float waitTime = 0.5f;
        
        while (true)
        {
            // TODO: use Animation Event?
            // sample attack motion
            // activate attacking part collider
            // set animation trigger for attack
            yield return new WaitForSeconds(_attackAnimationLength);
            // deactivate attacking part collider

            int numCheck = Mathf.RoundToInt((_attackCoolDown - _attackAnimationLength) / checkRate);

            for (int i = 0; i < numCheck; i++)
            {
                if (Vector3.Distance(transform.position, target.position) > attackRange)
                {
                    yield return new WaitForSeconds(waitTime);
                    currentState = State.Chase;
                    yield break;
                }

                yield return new WaitForSeconds(checkRate);
            }
        }
    }

    IEnumerator Investigate()
    {
        Vector3 destination = agent.destination;
        agent.SetDestination(transform.position);

        yield return Idle(4);

        if (currentState == State.Chase)
        {
            yield break;
        }

        yield return Patrol(destination, investigateSpeed);

        if (currentState == State.Chase)
        {
            yield break;
        }
        
        currentState = State.Idle;
    }

    void HandleHit(Collider other, GameObject hitPart)
    {
        // TODO: tag name synchronization
        if (other.CompareTag("Player"))
        {
            float damage = _attackDamages[hitPart];
            // TODO: player.TakeDamage(damage);
        }
    }
}
