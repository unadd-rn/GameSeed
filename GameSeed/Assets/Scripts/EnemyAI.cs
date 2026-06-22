using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider placementAreaCollider;
    [SerializeField] private ThrowEnemy throwEnemyScript;

    [Header("AI Settings")]
    [SerializeField] private float thinkDelay = 1.5f;
    private bool hasPlaced = false;

    public void StartTurn()
    {
        StartCoroutine(ExecuteAITurn());
    }

    private IEnumerator ExecuteAITurn()
    {
        yield return new WaitForSeconds(thinkDelay);
        if (!hasPlaced)
        {
            PlaceStickRandomly();
            hasPlaced = true;
            yield return new WaitForSeconds(1.0f);
        }
        float randomHitPoint = Random.Range(-0.5f, 0.5f);
        throwEnemyScript.SetAIHitPoint(randomHitPoint);
        float randomDirection = Random.value > 0.5f ? 1f : -1f;
        throwEnemyScript.SetAIThrowDirection(randomDirection);
        yield return new WaitForSeconds(thinkDelay);
        throwEnemyScript.Throw();
    }

    private void PlaceStickRandomly()
    {
        if (placementAreaCollider == null) return;
        Bounds bounds = placementAreaCollider.bounds;
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        Vector3 targetPosition = new Vector3(randomX, bounds.center.y, randomZ);
        transform.position = targetPosition;
        if (throwEnemyScript != null)
        {
            throwEnemyScript.enabled = true;
            throwEnemyScript.OnStickPlaced();
        }
    }
}