using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualTargetingSystem : Singleton<ManualTargetingSystem>
{
    [SerializeField] private ArrowView arrowView;
    [SerializeField] private LayerMask targetLayerMask;

    public void StartTargeting(Vector3 startPosition)
    {
        arrowView.gameObject.SetActive(true);
        arrowView.SetupArrow(startPosition);
    }

    public EnemyView EndTargeting(Vector3 endPosition)
    {
        arrowView.gameObject.SetActive(false);

        return TryGetTargetedEnemy(endPosition, out EnemyView enemyView) ? enemyView : null;
    }

    private bool TryGetTargetedEnemy(Vector3 worldPoint, out EnemyView enemyView)
    {
        if (Physics.Raycast(worldPoint, Vector3.forward, out RaycastHit hit, 10f, targetLayerMask)
            && hit.collider != null
            && hit.transform.TryGetComponent(out enemyView))
        {
            return true;
        }

        enemyView = null;
        return false;
    }
}