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
    public State currentState;

    public List<GameObject> attackParts;
    private Dictionary<GameObject, float> _attackDamages = new Dictionary<GameObject, float>();
    private List<BoxCollider> _attackColliders = new List<BoxCollider>();

    private NavMeshAgent agent;

    public float handDamage = 10f;
    public float mouthDamage = 20f;

    private Animator animator;
    
    public Transform target;

    public Collider targetCollider;
    // TODO: private PlayerController player;
    public Transform headTransform;
    // Local Axis of Head Direction
    public int headDirectionIdx = 1;
    
    public float detectionRange = 25f;
    public float viewRangeinChasing = 35f;
    private float _detectionRate = 0.5f;
    public float fovHorizontal = 160f;
    public float fovVertical = 170f;
    public float chaseRange = 20f;
    public float attackRange = 3f;

    public Vector3 patrolCenterPoint;
    public float patrolAreaRadius = 500f;
    public float distanceLowerBound = 5f;
    public float distanceUpperBound = 15f;
    // TODO: synchronize layer index and 1 << idx
    private LayerMask _monsterInvertedMask = ~(1 << 9);
    private int playerLayerIndex = 10;
    private Vector3 _headPositionOffset;

    private float _attackCoolDown = 3f;
    private float _attackAnimationLength = 0.8f;

    public float investigateSpeed = 5f;
    public float chaseSpeed = 8f;
    public float patrolSpeed = 3f;

    private float _targetCheckRate = 0.5f;

    private bool _hasTarget = false;
    
    
    // TODO: control parameters of navmesh agent
    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(Initialize());
    }

    void Update()
    {
        float currentSpeed = agent.velocity.magnitude;
        animator.SetFloat("Speed", currentSpeed / chaseSpeed);
    }

    IEnumerator Initialize()
    {
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
        
        NavMeshHit hit;
        NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas);
        
        agent.Warp(hit.position);
        currentState = State.Idle;
        StartCoroutine(StateRoutine());
        yield return null;
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
        agent.speed = chaseSpeed;
        
        while (true)
        {

            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            if (distanceToTarget < chaseRange && NavMesh.SamplePosition(target.position, out NavMeshHit hit, 3, agent.areaMask))
            {
                if (distanceToTarget < attackRange)
                {
                    currentState = State.Attack;
                    yield break;
                }
                Vector3 forwardDir = Vector3.zero;
                switch (headDirectionIdx)
                {
                    case 0:
                        forwardDir = headTransform.right;
                        break;
                    case 1:
                        forwardDir = headTransform.up;
                        break;
                    case 2:
                        forwardDir = headTransform.forward;
                        break;
                }
                
                if (!IsTargetInView(forwardDir, viewRangeinChasing))
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
        print("LookAround");
        // TODO: add head rotation or animation; forward direction -> head forward direction
        
        while (true)
        {
            Vector3 forwardDir = Vector3.zero;
            switch (headDirectionIdx)
            {
                case 0:
                    forwardDir = headTransform.right;
                    break;
                case 1:
                    forwardDir = headTransform.up;
                    break;
                case 2:
                    forwardDir = headTransform.forward;
                    break;
            }
            
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

        Vector3 verticalDirection = Vector3.zero;
        Vector3 horizontalDirection = Vector3.zero;

        // TODO: other models can differ
        
        switch (headDirectionIdx)
        {
            case 0:
                verticalDirection = headTransform.forward;
                horizontalDirection = headTransform.up;
                break;
            case 1:
                verticalDirection = headTransform.right;
                horizontalDirection = headTransform.forward;
                break;
            case 2:
                verticalDirection = headTransform.up;
                horizontalDirection = headTransform.right;
                break;
        }
        
            
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
        // TODO: add box colliders to the attacking parts & add several animations & sample attack motion & make lists for animation length and attack cooldown
        print("Attack");

        float checkRate = 0.5f;
        
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
        animator.SetBool("LookAroundAggressive_b", true);
        
        Vector3 destination = agent.destination;
        agent.SetDestination(transform.position);

        yield return Idle(3);

        if (currentState == State.Chase)
        {
            animator.SetBool("LookAroundAggressive_b", false);
            yield break;
        }

        yield return Patrol(destination, investigateSpeed);

        if (currentState == State.Chase)
        {
            animator.SetBool("LookAroundAggressive_b", false);
            yield break;
        }
        
        animator.SetBool("LookAroundAggressive_b", false);
        currentState = State.Idle;
    }

    void HandleHit(Collider other, GameObject hitPart)
    {
        if (other.gameObject.layer == playerLayerIndex)
        {
            float damage = _attackDamages[hitPart];
            // TODO: player.TakeDamage(damage);
        }
    }
}
