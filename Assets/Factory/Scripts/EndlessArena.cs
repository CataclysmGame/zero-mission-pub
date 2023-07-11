using UnityEngine;

public class EndlessArena : MonoBehaviour
{
    [SerializeField] private Collider2D _arenaCollider;

    private RaycastHit2D[] _hits = new RaycastHit2D[5];

    public bool IsPlayerOutsideArea
    {
        get
        {
            if (_arenaCollider == null)
            {
                Debug.LogError("The arena collider is not set.");
                return false;
            }

            var player = GameManager.Instance.Player;

            if (player == null)
            {
                return false;
            }

            return !CollidersOverlappingMoreThanThreshold(_arenaCollider, player.WallsCollider, 0.8f);
        }
    }

    private bool CollidersOverlappingMoreThanThreshold(Collider2D c1, Collider2D c2, float areaThresholdPercentage)
    {
        var c1_bounds = c1.bounds;
        var c2_bounds = c2.bounds;

        float c1_l = c1_bounds.center.x - c1_bounds.extents.x;
        float c1_r = c1_bounds.center.x + c1_bounds.extents.x;
        float c1_t = c1_bounds.center.y + c1_bounds.extents.y;
        float c1_b = c1_bounds.center.y - c1_bounds.extents.y;

        float c2_l = c2_bounds.center.x - c2_bounds.extents.x;
        float c2_r = c2_bounds.center.x + c2_bounds.extents.x;
        float c2_t = c2_bounds.center.y + c2_bounds.extents.y;
        float c2_b = c2_bounds.center.y - c2_bounds.extents.y;

        // Calculate the area of overlap
        float overlapWidth = Mathf.Max(0, Mathf.Min(c1_r, c2_r) - Mathf.Max(c1_l, c2_l));
        float overlapHeight = Mathf.Max(0, Mathf.Min(c1_t, c2_t) - Mathf.Max(c1_b, c2_b));
        float overlapArea = overlapWidth * overlapHeight;

        // Calculate the area of each rectangle
        float area1 = (c1_r - c1_l) * (c1_t - c1_b);
        float area2 = (c2_r - c2_l) * (c2_t - c2_b);

        // Calculate the percentage of overlap
        float overlapPercentage = overlapArea / Mathf.Min(area1, area2);

        return overlapPercentage >= areaThresholdPercentage;
    }

    public bool isPointInsideArea(Vector3 location)
    {
        return _arenaCollider.OverlapPoint(location);
    }

    public Vector3 GetNearestPointInsideArenaFromOutside(Vector2 outsidePoint)
    {
        var areaPos = transform.position;

        var horizontalDir = new Vector2(areaPos.x - outsidePoint.x, 0).normalized;
        var verticalDir = new Vector2(0, areaPos.y - outsidePoint.y).normalized;

        var mask = LayerMask.GetMask("EndlessArena");
        const float distance = 100.0f;

        var hits = Physics2D.RaycastNonAlloc(
            outsidePoint,
            horizontalDir,
            _hits,
            distance,
            mask
        );

        for (var i = 0; i < hits; ++i)
        {
            if (_hits[i].transform.name != "EndlessArena")
            {
                continue;
            }

            // Shift the point a bit inside
            var point = _hits[i].point + horizontalDir * 0.5f;
            return point;
        }

        hits = Physics2D.RaycastNonAlloc(
            outsidePoint,
            verticalDir,
            _hits,
            distance,
            mask
        );

        for (var i = 0; i < hits; ++i)
        {
            if (_hits[i].transform.name != "EndlessArena")
            {
                continue;
            }

            // Shift the point a bit inside
            var point = _hits[i].point + verticalDir * 0.5f;
            return point;
        }

        // Spawns the player at the center of the arena if no point is found
        return areaPos;
    }
}