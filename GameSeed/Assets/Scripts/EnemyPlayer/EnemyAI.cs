using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider placementAreaCollider;
    [SerializeField] private ThrowEnemy throwEnemyScript;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody rigid;

    [Header("AI Settings")]
    [SerializeField] private float thinkDelay = 1.5f;
    [SerializeField] private int monteCarloSimulations = 20;
    private bool hasPlaced = false;

    private WaitForSeconds delayWait;
    private WaitForSeconds shortWait;

    private List<Vector3> debugPredictedPositions = new List<Vector3>();
    private Vector3 debugBestFinalPosition;
    public Vector3 refEnemyPosition;

    //bawah ini struct MoveScenario
    //Jujur ini pertama kalinya gw bikin struct di C#
    //Kek anjay akhirnya kepake juga alpro
    //emang bajingan cuma anjay kepake juga

    public struct MoveScenario
    {
        public Vector3 placementPosition;
        public float hitPoint;
        public float throwDirectionZ;
        public float velocityScale;
        public float score;
    }

    void Awake()
    {
        delayWait = new WaitForSeconds(thinkDelay);
        shortWait = new WaitForSeconds(1.0f);
    }

    public void StartTurn()
    {
        StartCoroutine(ExecuteAITurn());
    }
    private Vector3 PlaceStickRandomly()
    {
        if (placementAreaCollider == null) return transform.position;
        Bounds bounds = placementAreaCollider.bounds;
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        Vector3 targetPosition = new Vector3(randomX, bounds.center.y, randomZ);
        return targetPosition;
    }

    private IEnumerator ExecuteAITurn()
    {
        yield return new WaitForSeconds(thinkDelay);
        if (!hasPlaced)
        {
            Vector3 randomPos = PlaceStickRandomly();
            transform.position = randomPos;
            refEnemyPosition = randomPos;
            TurnManager.Instance.SetState(TurnState.PlayerThrowing);
            hasPlaced = true;
        }

        if (throwEnemyScript != null && TurnManager.Instance.GetCurrentState() == TurnState.EnemyTurn)
        {
            throwEnemyScript.enabled = true;
            throwEnemyScript.OnStickPlaced();
        }

        yield return new WaitForSeconds(1.0f);

        if (throwEnemyScript != null && TurnManager.Instance.GetCurrentState() == TurnState.EnemyTurn)
        {
            MoveScenario bestScene = RunMonteCarlo();
            throwEnemyScript.SetAIHitPoint(bestScene.hitPoint * -1f);
            throwEnemyScript.SetAIThrowDirection(bestScene.throwDirectionZ);
            throwEnemyScript.StickDataRef.velocityScale = bestScene.velocityScale;
            yield return new WaitForSeconds(thinkDelay);
            throwEnemyScript.Throw();
        }
    }

    private MoveScenario RunMonteCarlo()
    {
        debugPredictedPositions.Clear();

        MoveScenario bestScenario = new MoveScenario();
        bestScenario.score = float.MinValue;

        Vector3 stableForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            if (stableForward == Vector3.zero) stableForward = Vector3.forward;
        Vector3 stableRight = Vector3.Cross(Vector3.up, stableForward).normalized;

        float gravity = Mathf.Abs(Physics.gravity.y);
        float exactAirTime = (2f * throwEnemyScript.StickDataRef.up) / gravity;

        for (int i = 0; i < monteCarloSimulations; i++)
        {
            MoveScenario testScenario = new MoveScenario();

            testScenario.hitPoint = Random.Range(-0.5f, 0.5f);
            testScenario.placementPosition = transform.position;
            testScenario.velocityScale = Random.Range(0f, 1f);

            Vector3 toPlayer = playerTransform.position - testScenario.placementPosition;
            float dotForward = Vector3.Dot(toPlayer.normalized, stableForward);
            testScenario.throwDirectionZ = dotForward > 0.5f ? 1f : -1f;

            testScenario.score = EvaluateScore(testScenario);

            // dari sini
            float calcForce = throwEnemyScript.StickDataRef.launchForce * testScenario.velocityScale;
            Vector3 forwardVelocity = stableForward * (calcForce * testScenario.throwDirectionZ);
            float sideDeflectionPower = 5f;
            Vector3 sidewaysVelocity = stableRight * (testScenario.hitPoint * sideDeflectionPower * testScenario.throwDirectionZ);

            Vector3 combinedHorizontalVelocity = forwardVelocity + sidewaysVelocity;

            Vector3 baseLandingPos = testScenario.placementPosition + (combinedHorizontalVelocity * exactAirTime);
            float groundSpinDriftMultiplier = 1.0f;
            Vector3 groundSpinDrift = stableRight * (testScenario.hitPoint * groundSpinDriftMultiplier);

            Vector3 estimatedLandingPos = baseLandingPos + groundSpinDrift;
            // 4. Titik jatuh final yang udah ngebaca spin
            Vector3 landingPos = baseLandingPos + groundSpinDrift;

            debugPredictedPositions.Add(landingPos);
            //sampe sini hapus kalau mau build
            //gizmos juga

            if (testScenario.score > bestScenario.score)
            {
                bestScenario = testScenario;
                debugBestFinalPosition = landingPos;
            }
        }
        return bestScenario;
    }

    private float EvaluateScore(MoveScenario scenario)
    {
        float score = 0f;
        Vector3 stableForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (stableForward == Vector3.zero) stableForward = Vector3.forward;

        float gravity = Mathf.Abs(Physics.gravity.y);
        float exactAirTime = (2f * throwEnemyScript.StickDataRef.up) / gravity;

        Vector3 stableRight = Vector3.Cross(Vector3.up, stableForward).normalized;
        float calcForce = throwEnemyScript.StickDataRef.launchForce * scenario.velocityScale;
        Vector3 forwardVelocity = stableForward * (calcForce * scenario.throwDirectionZ);

        float sideDeflectionPower = 5f;
        Vector3 sidewaysVelocity = stableRight * (scenario.hitPoint * sideDeflectionPower * scenario.throwDirectionZ);

        Vector3 combinedHorizontalVelocity = forwardVelocity + sidewaysVelocity;
        Vector3 baseLandingPos = scenario.placementPosition + (combinedHorizontalVelocity * exactAirTime);
        
        float groundSpinDriftMultiplier = 1.0f;
        Vector3 groundSpinDrift = stableRight * (scenario.hitPoint * groundSpinDriftMultiplier);

        Vector3 estimatedLandingPos = baseLandingPos + groundSpinDrift; 

        float distanceToPlayer = Vector3.Distance(estimatedLandingPos, playerTransform.position);

        score += 100f / (distanceToPlayer + 1f);

        RaycastHit hit;
        Vector3 directionToPlayer = playerTransform.position - scenario.placementPosition;
        if(Physics.Raycast(scenario.placementPosition, directionToPlayer, out hit, 15f))
        {
            if(hit.transform == playerTransform)
            {
                score += 50;
            }
        }

        float checkRadius = throwEnemyScript.StickDataRef.stickLength * 0.25f;
        Collider[] hitCollider = Physics.OverlapSphere(estimatedLandingPos, checkRadius);

        foreach (Collider col in hitCollider)
        {
            if (col.CompareTag("OutOfBound"))
            {
                Debug.Log("Out of bound detected!");
                score -= 500;
            }
        }

        Vector3 playerForward = playerTransform.forward;
        Vector3 directionFromPlayer = (estimatedLandingPos - playerTransform.position).normalized;
        float alignment = Vector3.Dot(playerForward, directionFromPlayer); 
        
        if (alignment > 0.7f)
        {
            score -= 30f;
        }

        return score;
    }

    private void OnDrawGizmos()
    {
        if (debugPredictedPositions == null || debugPredictedPositions.Count == 0) return;

        Gizmos.color = Color.yellow;

        foreach(Vector3 pos in debugPredictedPositions)
        {
            Gizmos.DrawSphere(pos + Vector3.up * 0.5f, 0.3f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, debugBestFinalPosition);
        Gizmos.DrawSphere(debugBestFinalPosition, 0.4f);
    }
}