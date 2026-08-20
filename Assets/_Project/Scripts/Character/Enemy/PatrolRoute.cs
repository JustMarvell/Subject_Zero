using UnityEngine;

namespace SubjectZero.Character.Enemy
{
    /// <summary>
    /// Holds an ordered set of waypoints (its own child Transforms) for a patrol loop.
    /// </summary>
    public class PatrolRoute : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;

        public int WaypointCount => waypoints != null ? waypoints.Length : 0;

        public Transform GetWaypoint(int index)
        {
            if (WaypointCount == 0) return null;
            int wrapped = ((index % WaypointCount) + WaypointCount) % WaypointCount;
            return waypoints[wrapped];
        }
    }
}