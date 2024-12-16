using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class MonsterController : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Attack, Investigate }
    
    [Header("State Settings")]
    public State currentState;

    [Header("Attack Settings")]
    public List<GameObject> attackParts;
    public List<float> attackDamages;
    private Dictionary<GameObject, int> _attackIndexDictionary = new Dictionary<GameObject, int>();
    private List<BoxCollider> _attackColliders = new List<BoxCollider>();
    public float attackCoolDown = 3f;
    public List<float> attackAnimationLengths;
    private bool isAttackHit = false;
    
    [Header("Player Settings")]
    public GameObject player;
    private Transform target;
    private Collider targetCollider;
    private PlayerStatusController playerStatusController;
    private int playerLayerIndex = 10;

    [Header("Self Settings")]
    private NavMeshAgent agent;
    private Animator animator;
    // TODO: private PlayerController player;
    public Transform headTransform;
    // Local Axis of Head Direction
    public int headForwardDirectionIdx = 1;
    public int headVerticalDirectionIdx = 0;
    public bool headDirectionInverted = false;
    private LayerMask _monsterInvertedMask = ~(1 << 9);
    private Vector3 _headPositionOffset;
    
    [Header("Detect Settings")]
    public float detectionRange = 25f;
    public float viewRangeinChasing = 35f;
    private float _detectionRate = 0.5f;
    public float fovHorizontalNormal = 100f;
    public float fovVerticalNormal = 120f;
    public float fovHorizontalAttacking = 240f;
    public float fovVerticalAttacking = 240f;
    private float fovHorizontal;
    private float fovVertical;
    public float chaseRange = 20f;
    public float attackRange = 3f;
    private float _targetCheckRate = 0.5f;
    private bool _hasTarget = false;

    [Header("Movement Range Settings")]
    public Vector3 patrolCenterPoint;
    public float patrolAreaRadius = 500f;
    public float distanceLowerBound = 5f;
    public float distanceUpperBound = 15f;
    
    [Header("Movement Speed Settings")]
    public float investigateSpeed = 5f;
    public float chaseSpeed = 8f;
    public float patrolSpeed = 3f;
    
    // TODO: control parameters of navmesh agent
    void Start()
    {
        target = player.transform;
        targetCollider = player.GetComponent<CapsuleCollider>();
        playerStatusController = player.GetComponent<PlayerStatusController>();

        ChangeFov(true);
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        Initialize();
    }

    void Update()
    {
        float currentSpeed = agent.velocity.magnitude;
        animator.SetFloat("Speed", currentSpeed / chaseSpeed);
    }

    void Initialize()
    {
        // TODO: player = target.gameObject.GetComponent<PlayerController>();

        for (int i = 0; i < attackParts.Count; i++)
        {
            var part = attackParts[i];
            _attackColliders.Add(part.GetComponent<BoxCollider>());
            AttackCollisionController attackCollider = part.GetComponent<AttackCollisionController>();

            _attackIndexDictionary[part] = i;
            
            attackCollider.OnHit += HandleHit;
        }
        
        NavMeshHit hit;
        NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas);
        
        agent.Warp(hit.position);
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
        print("Idle");

        if (Random.Range(0, 10) <= 7)
        {
            animator.SetBool("LookAround_b", true);
        }
        StartCoroutine("LookAround");
        
        int numCheck = Mathf.RoundToInt(idleTime / _targetCheckRate);

        for (int i = 0; i < numCheck; i++)
        {
            if (_hasTarget)
            {
                currentState = State.Chase;
                StopCoroutine("LookAround");
                animator.SetBool("LookAround_b", false);
                yield break;
            }
            yield return new WaitForSeconds(_targetCheckRate);
        }
        
        animator.SetBool("LookAround_b", false);
        currentState = State.Patrol;
    }

    IEnumerator Patrol(Vector3 destination, float speed)
    {
        print("Patrol");
        agent.speed = speed;
        
        if (Random.Range(0, 10) <= 7)
        {
            animator.SetBool("LookAround_b", true);
        }
        
        StartCoroutine("LookAround");
        
        agent.SetDestination(destination);
        
        while (agent.remainingDistance > agent.stoppingDistance)
        {
            if (_hasTarget)
            {
                currentState = State.Chase;
                StopCoroutine("LookAround");
                animator.SetBool("LookAround_b", false);
                yield break;
            }
            yield return null;
        }
        
        animator.SetBool("LookAround_b", false);
        currentState = State.Idle;
    }

    Vector3 SampleRandomDestination()
    {
        // if the gameObject is too far from the patrol Area Center, sample the next orientation in [Orientation to the center] +- 90
        Vector3 distanceToCenter = patrolCenterPoint - transform.position;
        float bound = patrolAreaRadius - distanceToCenter.magnitude < distanceUpperBound ? 90 : 180;

        while (true)
        {
            
            float randomAngle = Random.Range(-bound, bound);
        
            Quaternion angleOffset = Quaternion.Euler(0, randomAngle, 0);
            Vector3 patrolDirection = angleOffset * distanceToCenter.normalized;
        
            float patrolDistance = Random.Range(distanceLowerBound, distanceUpperBound);

            if (NavMesh.SamplePosition(transform.position + patrolDirection * patrolDistance, out NavMeshHit hit, 5f,
                    agent.areaMask))
            {
                return hit.position;
            }
        }
    }

    IEnumerator Chase()
    {
        print("Chase");
        ChangeFov(false);
        agent.speed = chaseSpeed;
        
        while (true)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            if (distanceToTarget < chaseRange && NavMesh.SamplePosition(target.position, out NavMeshHit hit, 3, agent.areaMask))
            {
                if (distanceToTarget < attackRange)
                {
                    currentState = State.Attack;
                    
                    ChangeFov(true);
                    yield break;
                }

                Vector3 forwardDir = GetHeadDirection(0);
                
                if (!IsTargetInView(forwardDir, viewRangeinChasing))
                {
                    _hasTarget = false;
                    currentState = State.Investigate;
                    ChangeFov(true);
                    yield break;
                }
                
                agent.SetDestination(hit.position);
            }
            else
            {
                _hasTarget = false;
                currentState = State.Patrol;
                ChangeFov(true);
                yield break;
            }
            yield return new WaitForSeconds(_detectionRate);
        }
    }

    // Coroutine in Idle & Patrol state
    IEnumerator LookAround()
    {
        print("LookAround");
        // TODO: add head rotation or animation; forward direction -> head forward direction
        
        while (true)
        {
            Vector3 forwardDir = GetHeadDirection(0);
            
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
        if ((target.position - headTransform.position).sqrMagnitude > range * range)
        {
            return false;
        }
        
        Vector3 directionToTarget = (targetCollider.bounds.center - headTransform.position).normalized;
        
        Vector3 horizontalDirection = GetHeadDirection(1);
        Vector3 verticalDirection = GetHeadDirection(2);
            
        Vector3 directionHorizontal = Vector3.ProjectOnPlane(directionToTarget, verticalDirection);
        
        float angleHorizontal = Vector3.Angle(forwardDirection, directionHorizontal);
        
        if (angleHorizontal < fovHorizontal / 2)
        {
            Vector3 directionVertical = Vector3.ProjectOnPlane(directionToTarget, horizontalDirection);
            float angleVertical = Vector3.Angle(forwardDirection, directionVertical);

            if (angleVertical < fovVertical / 2)
            {
                RaycastHit hit;
                if (Physics.Raycast(headTransform.position, directionToTarget, out hit, range,
                        Physics.DefaultRaycastLayers & _monsterInvertedMask, QueryTriggerInteraction.Collide))
                {
                    if (hit.transform.gameObject.layer == playerLayerIndex)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    IEnumerator Attack()
    {
        print("Attack");

        float checkRate = 0.5f;
        
        float waitTime = 0.5f;
        
        while (true)
        {
            // sample attack motion
            // activate attacking part collider
            // set animation trigger for attack
            var attackIndex = Random.Range(0, attackParts.Count);
            isAttackHit = false;
            _attackColliders[attackIndex].enabled = true;
            animator.SetTrigger("Attack" + attackIndex);
            yield return new WaitForSeconds(attackAnimationLengths[attackIndex]);
            // deactivate attacking part collider
            _attackColliders[attackIndex].enabled = false;
            isAttackHit = false;

            int numCheck = Mathf.RoundToInt((attackCoolDown - attackAnimationLengths[attackIndex]) / checkRate);

            for (int i = 0; i < numCheck; i++)
            {
                if (Vector3.Distance(transform.position, target.position) > attackRange)
                {
                    yield return new WaitForSeconds(waitTime);
                    currentState = State.Chase;
                    yield break;
                }

                if (NavMesh.SamplePosition(target.position, out NavMeshHit hit, 3f, agent.areaMask))
                {
                    agent.SetDestination(hit.position);
                }
                
                yield return new WaitForSeconds(checkRate);
            }
            agent.SetDestination(transform.position);
        }
    }

    IEnumerator Investigate()
    {
        print("Investigate");
        ChangeFov(false);
        
        animator.SetBool("LookAroundAggressive_b", true);
        
        Vector3 destination = agent.destination;
        agent.SetDestination(transform.position);

        yield return Idle(3);

        if (currentState == State.Chase)
        {
            animator.SetBool("LookAroundAggressive_b", false);
            ChangeFov(true);
            yield break;
        }

        yield return Patrol(destination, investigateSpeed);

        if (currentState == State.Chase)
        {
            animator.SetBool("LookAroundAggressive_b", false);
            ChangeFov(true);
            yield break;
        }
        
        animator.SetBool("LookAroundAggressive_b", false);
        ChangeFov(true);
        currentState = State.Idle;
    }
    
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx">0: head direction, 1: horizontal direction, 2: vertical direction</param>
    /// <returns></returns>
    Vector3 GetHeadDirection(int idx)
    {
        int multiplier = headDirectionInverted ? -1 : 1;
        int resIdx;

        switch (idx)
        {
            case 0:
                resIdx = headForwardDirectionIdx;
                break;
            case 1:
                resIdx = 3 - (headForwardDirectionIdx + headVerticalDirectionIdx);
                break;
            default:
                resIdx = headVerticalDirectionIdx;
                break;
        }
        
        switch (resIdx)
        {
            case 0:
                return headTransform.right * multiplier;
            case 1:
                return headTransform.up * multiplier;
            default:
                return headTransform.forward * multiplier;
        }
    }

    void ChangeFov(bool isNormal)
    {
        if (isNormal)
        {
            fovHorizontal = fovHorizontalNormal;
            fovVertical = fovVerticalNormal;
        }
        else
        {
            fovHorizontal = fovHorizontalAttacking;
            fovVertical = fovVerticalAttacking;
        }
    }

    void HandleHit(Collider other, GameObject hitPart)
    {
        if (!isAttackHit && other.gameObject.layer == playerLayerIndex)
        {
            var idx = _attackIndexDictionary[hitPart];
            playerStatusController.UpdateHP(-attackDamages[idx]);
            isAttackHit = true;
        }
    }
}
