using System.Collections.Generic;
using UnityEngine;

namespace LoopSubdivision
{
    public class LoopSubdivision : MonoBehaviour
    {
        public Mesh mesh;
        public ShowCase showCase;
        
        private Vector3[] m_oldMeshVertices;
        private Vector3[] m_newMeshVertices;
        private GizmosVertex m_oldVerticeGizmos;
        private GizmosVertex m_newVerticeGizmos;
        private int m_loopCount = 1;    // Loop 细分迭代次数 //
        private Mesh m_lastMesh;
        
        void Start()
        {
            m_oldVerticeGizmos = new GizmosVertex();
            m_newVerticeGizmos = new GizmosVertex();
            m_lastMesh = mesh;
            if (showCase != null)
            {
                showCase.SetBooth(mesh, ShowCase.EShowType.Origin);
            }
        }

        private void OnGUI()
        {
            m_loopCount = (int)GUI.HorizontalSlider(new Rect(0, 50, 200, 50), m_loopCount, 1, 5);
            GUI.Label(new Rect(250, 50, 200, 50), "迭代次数: " + m_loopCount);
            
            if (GUI.Button(new Rect(0, 100, 200, 100), "Subdivide"))
            {
                Mesh targetMesh = this.mesh;
                for (int i = 0; i < m_loopCount; i++)
                {
                    targetMesh = Subdivide(targetMesh);
                    int loopNum = i + 1;
                    Utility.Save(targetMesh, "Loop" + loopNum);
                    if (showCase != null)
                    {
                        showCase.SetBooth(targetMesh, (ShowCase.EShowType)loopNum);
                    }   
                }
            }
        }

        private void OnValidate()
        {
            if(mesh != m_lastMesh)
            {
                m_lastMesh = mesh;
                if (showCase != null)
                {
                    showCase.SetBooth(mesh, ShowCase.EShowType.Origin);
                }
            }
        }
        
        // private void OnDrawGizmos()
        // {
        //     if (m_oldVerticeGizmos != null)
        //     {
        //         Color gizColor = Gizmos.color;
        //         Gizmos.color= Color.red;
        //         m_oldVerticeGizmos.DrawGizmos(m_oldMeshVertices);
        //         Gizmos.color = gizColor;
        //     }
        //
        //     if (m_newVerticeGizmos != null)
        //     {
        //         Color gizColor = Gizmos.color;
        //         Gizmos.color= Color.blue;
        //         m_newVerticeGizmos.DrawGizmos(m_newMeshVertices);
        //         Gizmos.color = gizColor;
        //     }
        // }

        private Mesh Subdivide(Mesh mesh)
        {
            if (mesh == null)
            {
                return null;
            }

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            Vector3[] distinctPositions;
            int[] vertexToDistinctPosIndexs;
            RemoveDuplicateVertex(vertices, out distinctPositions, out vertexToDistinctPosIndexs);

            int[] distinctTriangles;
            RemapTriangles(triangles, vertexToDistinctPosIndexs, out distinctTriangles);
            
            // 初始化旧顶点 //
            OldVertexInfo[] oldVertexInfos = null;
            InitOldVertexInfo(vertices, triangles, distinctPositions, vertexToDistinctPosIndexs, distinctTriangles, out oldVertexInfos);

            // 初始化新顶点 //
            NewVertexInfo[] newVertexInfos = null;
            InitNewVertexInfo(vertices, triangles, distinctTriangles, out newVertexInfos);

            // 计算新mesh的vertices和triangle //
            Vector3[] newMeshVertices;
            int[] newMeshTriangle;
            CalculateVertexAndTriangle(triangles, newVertexInfos, oldVertexInfos, out newMeshVertices, out newMeshTriangle);

            // // 测试 //
            // m_newMeshVertices = newMeshVertices;
            // m_oldMeshVertices = new Vector3[oldVertexInfos.Length];
            // for (int i = 0; i < oldVertexInfos.Length; i++)
            // {
            //     m_oldMeshVertices[i] = oldVertexInfos[i].position;
            // }
            //
            // m_newMeshVertices = new Vector3[newVertexInfos.Length];
            // for (int i = 0; i < newVertexInfos.Length; i++)
            // {
            //     m_newMeshVertices[i] = newVertexInfos[i].position;
            // }

            Mesh resultMesh = CreateMesh(newMeshVertices, newMeshTriangle);
            return resultMesh;
        }

        private void InitOldVertexInfo(Vector3[] vertices, int[] triangles, Vector3[] distinctPositions, int[] vertexToDistinctPosIndexs, int[] remappedTriangle, out OldVertexInfo[] vertexInfos)
        {
            if (vertices == null || triangles == null || distinctPositions == null || vertexToDistinctPosIndexs == null ||
                remappedTriangle == null || vertices.Length != vertexToDistinctPosIndexs.Length ||
                remappedTriangle.Length != triangles.Length)
            {
                vertexInfos = null;
                return;
            }

            List<int>[] temp = new List<int>[distinctPositions.Length];     // 存放真实顶点相连接的顶点位置索引 //
            for (int i = 0; i < remappedTriangle.Length; i++)
            {
                int realVertexIndex = remappedTriangle[i];
                
                List<int> list = null;
                if (temp[realVertexIndex] == null)
                {
                    list = new List<int>();
                    temp[realVertexIndex] = list;
                }
                else
                {
                    list = temp[realVertexIndex];
                }
             
                // int vertexIndex = triangles[i];
                for (int j = 0; j < remappedTriangle.Length; j += 3)
                {
                    int flag = 7;   // 111 //
                    if (remappedTriangle[j] == realVertexIndex)
                    {
                        flag = 6;      // 110 //
                    }
                    if (remappedTriangle[j + 1] == realVertexIndex)
                    {
                        flag = 5;      // 101 //
                    }
                    if (remappedTriangle[j + 2] == realVertexIndex)
                    {
                        flag = 3;      // 011 //
                    }

                    int offset = 0;
                    while (flag != 7 && offset < 3)
                    {
                        if ((flag & (1 << offset)) > 0)
                        {
                            int distinctPosIndex = remappedTriangle[j + offset];
                            if (!list.Contains(distinctPosIndex))
                            {
                                list.Add(distinctPosIndex);
                            }
                        }
                        offset++;
                    }
                }
            }

            vertexInfos = new OldVertexInfo[vertices.Length];
            for (int i = 0; i < vertexInfos.Length; i++)
            {
                OldVertexInfo info = new OldVertexInfo();
                info.position = vertices[i];
                int realVertexIndex = vertexToDistinctPosIndexs[i];
                if (temp != null && realVertexIndex < temp.Length && temp[realVertexIndex] != null)
                {
                    int degree = temp[realVertexIndex].Count;
                    info.degree = degree;
                    info.linkedPoints = new OldVertexInfo[degree]; 
                    for (int j = 0; j < degree; j++)
                    {
                        int realPosIndex = temp[realVertexIndex][j];
                        info.linkedPoints[j] = new OldVertexInfo() {position = distinctPositions[realPosIndex]};
                    }
                }
                vertexInfos[i] = info;
            }

            AdjustVertexPositionOld(vertexInfos);
        }

        private void InitNewVertexInfo(Vector3[] vertices, int[] triangles, int[] realVertexIndexs, out NewVertexInfo[] newVertexInfos)
        {
            if (vertices == null || triangles == null || realVertexIndexs == null || triangles.Length != realVertexIndexs.Length)
            {
                newVertexInfos = null;
                return;
            }

            newVertexInfos = new NewVertexInfo[triangles.Length];
            for (int i = 0; i < triangles.Length; i+=3)
            {
                int pointIndex0 = triangles[i];
                int pointIndex1 = triangles[i+1];
                int pointIndex2 = triangles[i+2];
                // Utility.Sort(ref pointIndex0, ref pointIndex1, ref pointIndex2);

                int realVertexIndex0 = realVertexIndexs[i];
                int realVertexIndex1 = realVertexIndexs[i+1];
                int realVertexIndex2 = realVertexIndexs[i+2];
                
                int faceIndex = i / 3 * 10;
                NewVertexInfo info0 = new NewVertexInfo()
                {
                    position = (vertices[pointIndex0] + vertices[pointIndex1]) * 0.5f,
                    edgeIndex = faceIndex,
                    linkedRealVertexIndex0 = realVertexIndex0,
                    linkedRealVertexIndex1 = realVertexIndex1,
                    linkedPosition0 = vertices[pointIndex0],
                    linkedPosition1 = vertices[pointIndex1],
                    oppositePosition0 = vertices[pointIndex2],
                };

                NewVertexInfo info1 = new NewVertexInfo()
                {
                    position = (vertices[pointIndex1] + vertices[pointIndex2]) * 0.5f,
                    edgeIndex = faceIndex + 1,
                    linkedRealVertexIndex0 = realVertexIndex1,
                    linkedRealVertexIndex1 = realVertexIndex2,
                    linkedPosition0 = vertices[pointIndex1],
                    linkedPosition1 = vertices[pointIndex2],
                    oppositePosition0 = vertices[pointIndex0],
                };
                
                NewVertexInfo info2 = new NewVertexInfo()
                {
                    position = (vertices[pointIndex2] + vertices[pointIndex0]) * 0.5f,
                    edgeIndex = faceIndex + 2,
                    linkedRealVertexIndex0 = realVertexIndex2,
                    linkedRealVertexIndex1 = realVertexIndex0,
                    linkedPosition0 = vertices[pointIndex2],
                    linkedPosition1 = vertices[pointIndex0],
                    oppositePosition0 = vertices[pointIndex1],
                };
                
                newVertexInfos[i] = info0;
                newVertexInfos[i+1] = info1;
                newVertexInfos[i+2] = info2;
            }

            for (int i = 0; i < newVertexInfos.Length; i++)
            {
                NewVertexInfo srcInfo = newVertexInfos[i];
                for (int j = 0; j < newVertexInfos.Length; j++)
                {
                    if (j == i)
                    {
                        continue;
                    }
                    
                    NewVertexInfo dstInfo = newVertexInfos[j];
                    if (Utility.IsOnSameEdge(srcInfo, dstInfo))
                    {
                        srcInfo.oppositePosition1 = dstInfo.oppositePosition0;
                        // dstInfo.oppositePosition1 = srcInfo.oppositePosition0;
                    }
                }
            }
            
            AdjustVertexPositionNew(newVertexInfos);
        }

        private void CalculateVertexAndTriangle(int[] triangles, NewVertexInfo[] newVertexInfos, OldVertexInfo[] oldVertexInfos, out Vector3[] newMeshVertices, out int[] newMeshTriangle)
        {
            if (triangles == null || newVertexInfos == null || oldVertexInfos == null || triangles.Length != newVertexInfos.Length)
            {
                newMeshVertices = null;
                newMeshTriangle = null;
                return;
            }

            int oldVertexCount = oldVertexInfos.Length;
            newMeshVertices = new Vector3[oldVertexCount + newVertexInfos.Length];
            for (int i = 0; i < oldVertexCount; i++)
            {
                newMeshVertices[i] = oldVertexInfos[i].position;
            }
            for (int i = 0; i < newVertexInfos.Length; i++)
            {
                newMeshVertices[i + oldVertexCount] = newVertexInfos[i].position;
            }
            
            //      1
            //      |   \
            //      A   -   B
            //      |           \
            //      0   -   C   -   2
            // 顺时针为正方向 //
            newMeshTriangle = new int[triangles.Length * 4];
            int triangleIndex = 0;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int faceIndex = i / 3;
                int offset = faceIndex * 3;
                
                // 0 A C //
                newMeshTriangle[triangleIndex++] = triangles[i];
                newMeshTriangle[triangleIndex++] = oldVertexCount + offset;
                newMeshTriangle[triangleIndex++] = oldVertexCount + offset + 2;
                
                // A 1 B //
                newMeshTriangle[triangleIndex++] = oldVertexCount + offset;
                newMeshTriangle[triangleIndex++] = triangles[i + 1];
                newMeshTriangle[triangleIndex++] = oldVertexCount + offset + 1;
                
                // C B 2 //
                newMeshTriangle[triangleIndex++] = oldVertexCount + offset + 2;
                newMeshTriangle[triangleIndex++] = oldVertexCount + offset + 1;
                newMeshTriangle[triangleIndex++] = triangles[i+2];
                
                // C A B //
                newMeshTriangle[triangleIndex++] = oldVertexCount + offset + 2;
                newMeshTriangle[triangleIndex++] = oldVertexCount + offset;
                newMeshTriangle[triangleIndex++] = oldVertexCount + offset + 1;
            }
        }

        private void AdjustVertexPositionNew(NewVertexInfo[] vertexInfos)
        {
            if (vertexInfos == null)
            {
                return;
            }

            for (int i = 0; i < vertexInfos.Length; i++)
            {
                NewVertexInfo info = vertexInfos[i];
                if (info == null)
                {
                    continue;
                }
                
                info.position = 0.375f * (info.linkedPosition0 + info.linkedPosition1) + 
                                0.125f * (info.oppositePosition0 + info.oppositePosition1);
            }
        }

        private void AdjustVertexPositionOld(OldVertexInfo[] vertexInfos)
        {
            if (vertexInfos == null)
            {
                return;
            }

            const float _3_16 = 3.0f / 16.0f;
            for (int i = 0; i < vertexInfos.Length; i++)
            {
                OldVertexInfo info = vertexInfos[i];
                if (info == null)
                {
                    continue;
                }

                int degree = info.degree;
                Vector3 neighborPosSum = Vector3.zero;
                for (int j = 0; j < info.linkedPoints.Length; j++)
                {
                    neighborPosSum += info.linkedPoints[j].position;
                }
                
                float u = degree == 3 ? _3_16 : 3.0f / (8 * degree);
                info.position = (1 - degree * u) * info.position + u * neighborPosSum;
            }
        }

        // 去重 //
        private void RemoveDuplicateVertex(Vector3[] vertices, out Vector3[] distinctPositions, out int[] vertexRemapIndex)
        {
            if (vertices == null)
            {
                distinctPositions = null;
                vertexRemapIndex = null;
                return;
            }


            int curVertexCount = 0;
            vertexRemapIndex = new int[vertices.Length];
            List<Vector3> distinctVertexList = new List<Vector3>();

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 curVertex = vertices[i];
                bool repeqated = false;
                for (int j = 0; j < distinctVertexList.Count; j++)
                {
                    if (Utility.Vector3Equal(curVertex, distinctVertexList[j]))
                    {
                        repeqated = true;
                        break;
                    }
                }

                if (!repeqated)
                {
                    distinctVertexList.Add(vertices[i]);
                }

                vertexRemapIndex[i] = distinctVertexList.IndexOf(vertices[i]);
            }

            distinctPositions = distinctVertexList.ToArray();
        }

        private void RemapTriangles(int[] triangles, int[] distinctVertexIndexs, out int[] remappedTriangles)
        {
            if (triangles == null)
            {
                remappedTriangles = null;
                return;
            }

            remappedTriangles = new int[triangles.Length];
            for (int i = 0; i < triangles.Length; i++)
            {
                int originVertexIndex = triangles[i];
                int distinctVertexIndex = distinctVertexIndexs[originVertexIndex];
                remappedTriangles[i] = distinctVertexIndex;
            }
        }

        public Mesh CreateMesh(Vector3[] vertices, int[] triangles)
        {
            if (vertices == null || triangles == null)
            {
                return null;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            return mesh;
        }
    }
}