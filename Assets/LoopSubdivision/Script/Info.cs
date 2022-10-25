using UnityEngine;

namespace LoopSubdivision
{
    public class OldVertexInfo
    {
        public Vector3 position;        // 位置 //
        public int degree;              // 顶点的度 //
        public OldVertexInfo[] linkedPoints;  // 相连接的点 //
        
        private static int COUNTER = 0;
        private readonly int hashCode;

        public OldVertexInfo()
        {
            hashCode = COUNTER++;
            linkedPoints = null;
        }

        public static bool operator == (OldVertexInfo vertex0, OldVertexInfo vertex1)
        {
            return !object.ReferenceEquals(vertex0, null) && 
                   !object.ReferenceEquals(vertex1, null) &&
                   Utility.Vector3Equal(vertex0.position, vertex1.position);
        }

        public static bool operator != (OldVertexInfo vertex0, OldVertexInfo vertex1)
        {
            return !(vertex0 == vertex1);
        }

        public override bool Equals(object obj)
        {
            OldVertexInfo oldVertexInfo = obj as OldVertexInfo;
            return oldVertexInfo != null && oldVertexInfo == this;
        }

        public override int GetHashCode()
        {
            return hashCode;
        }
    }
    
    public class NewVertexInfo
    {
        public Vector3 position;
        public int edgeIndex;
        public int linkedRealVertexIndex0;
        public int linkedRealVertexIndex1;
        public Vector3 linkedPosition0;
        public Vector3 linkedPosition1;
        public Vector3 oppositePosition0;
        public Vector3 oppositePosition1;

        private static int COUNTER = 0;
        private readonly int hashCode;

        public NewVertexInfo()
        {
            hashCode = COUNTER++;
        }

        public static bool operator == (NewVertexInfo vertex0, NewVertexInfo vertex1)
        {
            // return !(vertex0 is null) && !(vertex1 is null) && Utility.Vector3Equal(vertex0.position, vertex1.position);
            return !object.ReferenceEquals(vertex0, null) && !object.ReferenceEquals(vertex1, null) && 
                   Utility.Vector3Equal(vertex0.position, vertex1.position);
        }

        public static bool operator != (NewVertexInfo vertex0, NewVertexInfo vertex1)
        {
            return !(vertex0 == vertex1);
        }

        public override bool Equals(object obj)
        {
            NewVertexInfo vertexInfo = obj as NewVertexInfo;
            return vertexInfo != null && vertexInfo == this;
        }

        public override int GetHashCode()
        {
            return hashCode;
        }
    }
}