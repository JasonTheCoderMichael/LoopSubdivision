using UnityEngine;

namespace LoopSubdivision
{
    public class ShowCase : MonoBehaviour
    {
        public Transform origin;
        public Transform loop1;
        public Transform loop2;
        public Transform loop3;
        public Transform loop4;
        public Transform loop5;

        private GameObject[] m_objs;
        private Transform[] m_transforms;
        private MeshFilter[] m_meshFilters;
        private MeshRenderer[] m_meshRenderers;
        private Material[] m_materials;
        
        private const int SHOW_CASE_NUM = 6;
        
        private void OnEnable()
        {
            m_objs = new GameObject[SHOW_CASE_NUM];
            m_meshFilters = new MeshFilter[SHOW_CASE_NUM];
            m_meshRenderers = new MeshRenderer[SHOW_CASE_NUM];
            m_materials = new Material[SHOW_CASE_NUM];
            m_transforms = new Transform[SHOW_CASE_NUM]
            {
                origin, loop1, loop2, loop3, loop4, loop5,
            };

            for (int i = 0; i < SHOW_CASE_NUM; i++)
            {
                Material material = new Material(Shader.Find("LoopSubdivide/FaceColor"));
                
                string objName = i == 0 ? "Origin" : "Loop" + i;
                GameObject obj = new GameObject(objName);
                MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
                meshFilter.mesh = null;
                meshRenderer.material = material;
                
                m_objs[i] = obj;
                m_meshFilters[i] = meshFilter;
                m_meshRenderers[i] = meshRenderer;
                m_materials[i] = material;
                
                Utility.SetParent(obj.transform, m_transforms[i]);
                obj.SetActive(false);
            }
        }

        public void SetBooth(Mesh mesh, EShowType type)
        {
            int index = (int) type;
            if (m_meshFilters != null && index < m_meshFilters.Length && m_meshFilters[index] != null)
            {
                m_meshFilters[index].mesh = mesh;
            }

            if (m_objs != null && index < m_objs.Length && m_objs[index] != null)
            {
                m_objs[index].SetActive(true);
            }

            if (mesh != null)
            {
                int triangleCount = mesh.triangles.Length / 3;
                if (m_materials != null && index < m_materials.Length && m_materials[index] != null)
                {
                    m_materials[index].SetInt("_Triangle_Count", triangleCount);
                }
            }
        }

        private void OnDisable()
        {
            m_transforms = null;
            m_objs = null;
            m_meshFilters = null;
            m_materials = null;
        }
        
        public enum EShowType
        {
            Origin = 0,
            Loop1,
            Loop2,
            Loop3,
            Loop4,
            Loop5,
        }

    }
}