using UnityEngine;

namespace LoopSubdivision
{
    public class GizmosVertex
    {
        public void DrawGizmos(Vector3[] positions)
        {
            if (positions == null)
            {
                return;
            }

            for (int i = 0; i < positions.Length; i++)
            {
                Gizmos.DrawSphere(positions[i], 0.05f);
            }
        }
    }
}