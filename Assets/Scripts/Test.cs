using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.CSG;
using UnityMeshSimplifier;

public class Test : MonoBehaviour
{
    [SerializeField] private MeshFilter[] cubes;
    void Start()
    {
        StartCoroutine(Combining3DMesh(cubes));
    }

    public IEnumerator Combining3DMesh(MeshFilter[] meshFilters)
   {

        Vector3 startMeshFilterPos = meshFilters[0].transform.parent.position;
        meshFilters[0].transform.parent.position -= meshFilters[0].transform.position;
        Debug.Log(meshFilters[0].transform.position);
        Vector3 startPos = transform.position;
        transform.position = meshFilters[0].transform.position;
        Mesh firstMesh = meshFilters[0].sharedMesh;
        MeshFilter coreFilter = meshFilters[0];
        coreFilter.sharedMesh = meshFilters[0].sharedMesh;
        Mesh mesh= null;
        Vector3 startScale = meshFilters[0].transform.localScale;
        //meshFilters[0].transform.localScale=Vector3.one;
        for (int i = 1; i < meshFilters.Length; i++)
        {
            mesh = CSG.Union(coreFilter.gameObject, meshFilters[i].gameObject).mesh;
            coreFilter.sharedMesh = mesh;
            meshFilters[0].transform.localScale=Vector3.one;
            // var meshSimplifier = new MeshSimplifier();
            // meshSimplifier.Initialize(mesh);
            // meshSimplifier.SimplifyMesh(0.5f);
            // mesh = meshSimplifier.ToMesh();
        }

        meshFilters[0].transform.parent.position = startMeshFilterPos;
        meshFilters[0].sharedMesh = firstMesh;
        meshFilters[0].transform.localScale=startScale;

        foreach (var filter in meshFilters)
        {
            filter.gameObject.SetActive(false);
        }
        //Розвертаємо грані всередину

        // if (mesh != null)
        // {
        //     Vector3[] normals = mesh.normals;
        //     for (int i = 0; i < normals.Length; i++)
        //     {
        //         normals[i] = -normals[i];
        //     }
        //
        //     mesh.normals = normals;
        //
        //     for (int i = 0; i < mesh.subMeshCount; i++)
        //     {
        //         int[] triangles = mesh.GetTriangles(i);
        //         for (int j = 0; j < triangles.Length; j += 3)
        //         {
        //             (triangles[j], triangles[j + 1]) = (triangles[j + 1], triangles[j]);
        //         }
        //
        //         mesh.SetTriangles(triangles, i);
        //     }
        // }

        // Присвоюємо створений меш MeshFilter головного об'єкту
        GetComponent<MeshFilter>().sharedMesh = mesh;

        // Відображаємо головний об'єкт
        gameObject.SetActive(true);
        transform.position = startPos;
        yield return null;

    }
}
