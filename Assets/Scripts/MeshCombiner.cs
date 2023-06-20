using System.Collections;
using System.Collections.Generic;
using Autodesk.Fbx;
using Parabox.CSG;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;
using UnityMeshSimplifier;

public class MeshCombiner : MonoBehaviour
{

    private Mesh createdMesh;
     public IEnumerator Combining3DMesh2(MeshFilter[] roomMeshFilters,MeshFilter[] corridorsMeshFilters)
   {

       Mesh mesh1 = CombineMeshes(roomMeshFilters);
       GameObject combinedMeshObject1 = new GameObject("CombinedMesh");
       combinedMeshObject1.transform.position = Vector3.zero;

       // Create a new MeshFilter and MeshRenderer for the combined mesh
       MeshFilter combinedMeshFilter1 = combinedMeshObject1.AddComponent<MeshFilter>();
       MeshRenderer combinedMeshRenderer1 = combinedMeshObject1.AddComponent<MeshRenderer>();
       combinedMeshRenderer1.material = GetComponent<MeshRenderer>().material;
       combinedMeshFilter1.sharedMesh = mesh1;
        
       Mesh mesh2 = CombineMeshes(corridorsMeshFilters);
       GameObject combinedMeshObject = new GameObject("CombinedMesh");
       combinedMeshObject.transform.position = Vector3.zero;

       // Create a new MeshFilter and MeshRenderer for the combined mesh
       MeshFilter combinedMeshFilter = combinedMeshObject.AddComponent<MeshFilter>();
       MeshRenderer combinedMeshRenderer = combinedMeshObject.AddComponent<MeshRenderer>();
       combinedMeshRenderer.material = GetComponent<MeshRenderer>().material;

       combinedMeshFilter.sharedMesh = mesh2;
        
       //  
       Mesh mesh=CSG.Union(combinedMeshObject1, combinedMeshObject).mesh;
       GetComponent<MeshFilter>().sharedMesh = mesh;
       
       if (mesh != null)
       {
           Vector3[] normals = mesh.normals;
           for (int i = 0; i < normals.Length; i++)
           {
               normals[i] = -normals[i];
           }
        
           mesh.normals = normals;
        
           for (int i = 0; i < mesh.subMeshCount; i++)
           {
               int[] triangles = mesh.GetTriangles(i);
               for (int j = 0; j < triangles.Length; j += 3)
               {
                   (triangles[j], triangles[j + 1]) = (triangles[j + 1], triangles[j]);
               }
        
               mesh.SetTriangles(triangles, i);
           }
       }
       DestroyImmediate(combinedMeshObject1);
       DestroyImmediate(combinedMeshObject);
       yield return null;

    }
     
     public IEnumerator Combining3DMesh3(MeshFilter[] roomMeshFilters,MeshFilter[] corridorsMeshFilters)
   {

       Mesh mesh1 = CombineMeshes(roomMeshFilters);
       GameObject combinedMeshObject1 = new GameObject("CombinedMesh");
       combinedMeshObject1.transform.position = Vector3.zero;

       // Create a new MeshFilter and MeshRenderer for the combined mesh
       MeshFilter combinedMeshFilter1 = combinedMeshObject1.AddComponent<MeshFilter>();
       MeshRenderer combinedMeshRenderer1 = combinedMeshObject1.AddComponent<MeshRenderer>();
       combinedMeshRenderer1.material = GetComponent<MeshRenderer>().material;
       combinedMeshFilter1.sharedMesh = mesh1;
        
 
       Vector3 startMeshFilterPos = combinedMeshFilter1.transform.parent.position;
       combinedMeshFilter1.transform.parent.position -= combinedMeshFilter1.transform.position;
       Vector3 startPos = transform.position;
       transform.position = combinedMeshFilter1.transform.position;
       Mesh firstMesh =combinedMeshFilter1.sharedMesh;
       MeshFilter coreFilter = combinedMeshFilter1;

       MeshFilter thisFilter = GetComponent<MeshFilter>();
       coreFilter.sharedMesh = combinedMeshFilter1.sharedMesh;
       Mesh mesh= null;
       Vector3 startScale = combinedMeshFilter1.transform.localScale;
       //meshFilters[0].transform.localScale=Vector3.one;
       for (int i = 1; i < corridorsMeshFilters.Length; i++)
       {
           mesh = CSG.Union(combinedMeshObject1, corridorsMeshFilters[i].gameObject).mesh;
           mesh.Optimize();
           coreFilter.sharedMesh = mesh;
           corridorsMeshFilters[0].transform.localScale=Vector3.one;
            
           //thisFilter.sharedMesh = mesh;
           // transform.position+=Vector3.up*3;
           // yield return new WaitForSeconds(0.5f);
           // transform.position-=Vector3.up*3;
       }

       corridorsMeshFilters[0].transform.parent.position = startMeshFilterPos;
       corridorsMeshFilters[0].sharedMesh = firstMesh;
       corridorsMeshFilters[0].transform.localScale=startScale;

       foreach (var filter in corridorsMeshFilters)
       {
           filter.gameObject.SetActive(false);
       }
       //  
       GetComponent<MeshFilter>().sharedMesh = mesh;
       
       if (mesh != null)
       {
           Vector3[] normals = mesh.normals;
           for (int i = 0; i < normals.Length; i++)
           {
               normals[i] = -normals[i];
           }
        
           mesh.normals = normals;
        
           for (int i = 0; i < mesh.subMeshCount; i++)
           {
               int[] triangles = mesh.GetTriangles(i);
               for (int j = 0; j < triangles.Length; j += 3)
               {
                   (triangles[j], triangles[j + 1]) = (triangles[j + 1], triangles[j]);
               }
        
               mesh.SetTriangles(triangles, i);
           }
       }
       DestroyImmediate(combinedMeshObject1);
       yield return null;

    }
     private Mesh CombineMeshes(MeshFilter[] meshFilters)
     {
         // Create a list to hold the CombineInstance objects
         List<CombineInstance> combineInstances = new List<CombineInstance>();

         // Iterate through each cube GameObject
         foreach (MeshFilter cube in meshFilters)
         {
             // Extract the mesh data from the cube
             MeshFilter cubeMeshFilter = cube;
             MeshRenderer cubeMeshRenderer = cube.GetComponent<MeshRenderer>();
             Mesh cubeMesh = cubeMeshFilter.sharedMesh;

             // Create a new CombineInstance and set its transform
             CombineInstance combineInstance = new CombineInstance();
             combineInstance.mesh = cubeMesh;
             combineInstance.transform = cube.transform.localToWorldMatrix;

             // Add the CombineInstance to the list
             combineInstances.Add(combineInstance);

             // Optionally, remove the cube GameObject from the scene
             cube.gameObject.SetActive(false);
         }

         // Create a new empty GameObject to hold the combined mesh
       
         // Combine the meshes into a single mesh
         Mesh combinedMesh = new Mesh();
         combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);

         // Perform Boolean operation to delete inner parts using a library like Unity Boolean
         // (Please refer to the documentation of the specific Boolean library you're using for the exact steps)

         // Assign the combined and modified mesh to the MeshFilter component
         //GetComponent<MeshFilter>().sharedMesh = combinedMesh;
         return combinedMesh;
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
        MeshFilter thisFilter = GetComponent<MeshFilter>();
        coreFilter.sharedMesh = meshFilters[0].sharedMesh;
        Mesh mesh= null;
        Vector3 startScale = meshFilters[0].transform.localScale;
        //meshFilters[0].transform.localScale=Vector3.one;
        for (int i = 1; i < meshFilters.Length; i++)
        {
            mesh = CSG.Union(coreFilter.gameObject, meshFilters[i].gameObject).mesh;
            //mesh.Optimize();
            coreFilter.sharedMesh = mesh;
            meshFilters[0].transform.localScale=Vector3.one;
            
            //thisFilter.sharedMesh = mesh;
            // transform.position+=Vector3.up*3;
            // yield return new WaitForSeconds(0.5f);
            // transform.position-=Vector3.up*3;
        }

        meshFilters[0].transform.parent.position = startMeshFilterPos;
        meshFilters[0].sharedMesh = firstMesh;
        meshFilters[0].transform.localScale=startScale;

        foreach (var filter in meshFilters)
        {
            filter.gameObject.SetActive(false);
        }
        //Розвертаємо грані всередину

        if (mesh != null)
        {
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = -normals[i];
            }
        
            mesh.normals = normals;
        
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] triangles = mesh.GetTriangles(i);
                for (int j = 0; j < triangles.Length; j += 3)
                {
                    (triangles[j], triangles[j + 1]) = (triangles[j + 1], triangles[j]);
                }
        
                mesh.SetTriangles(triangles, i);
            }
        }

        // Присвоюємо створений меш MeshFilter головного об'єкту
        thisFilter.sharedMesh = mesh;
        createdMesh = mesh;
        // Відображаємо головний об'єкт
        gameObject.SetActive(true);
        transform.position = startPos;
        yield return null;

    }

    public void Save3DObjectAsFbx()
    {
        
    }

}