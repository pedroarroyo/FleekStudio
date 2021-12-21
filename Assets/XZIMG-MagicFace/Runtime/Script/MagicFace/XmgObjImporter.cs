/// Imported from wiki.unity3d.com, revamped by XZIMG Limited

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
 
public class XmgObjImporter
{ 
    private struct localMeshStruct
    {
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector2[] uv;
        public int[] triangles;
        public int[] faceVerts;
        public int[] trianglesUV;
        public string name;
        public string assetName;
    }
 
    /// Import an asset
	public Mesh Import(string assetName = "XZIMG/Models/face-model-obj") {
        localMeshStruct lMesh = createMeshFromAsset(assetName);
        populateMeshStruct(ref lMesh);

		Mesh unityMesh = new Mesh(); 
        unityMesh.vertices = lMesh.vertices;     
        Vector2[] mesh_uv  = new Vector2[lMesh.vertices.Length];
        unityMesh.triangles = lMesh.triangles;
 
        // Get face uvs from faces and convert to vertices uvs
        Vector2[] newUVs = new Vector2[lMesh.trianglesUV.Length];
        int i=0;
        foreach (int uv_idx in lMesh.trianglesUV) {
            int v_idx = lMesh.triangles[i];
            mesh_uv[v_idx] = lMesh.uv[uv_idx];
            i++;
        } 
        unityMesh.uv = mesh_uv; 
        unityMesh.RecalculateBounds(); 
        unityMesh.RecalculateNormals();
		return unityMesh;
	}
 
    private static localMeshStruct createMeshFromAsset(string assetName = "XZIMG/Models/face-model-obj")
    {
        int triangles = 0;
        int vertices = 0;
        int vt = 0;
        int vn = 0;
        localMeshStruct mesh = new localMeshStruct();
        mesh.assetName = assetName;
        TextAsset content = Resources.Load(assetName) as TextAsset;

        string entireText = content.text;

        // System.IO.File.Find()
        // string text = System.IO.File.ReadAllText(@"face-model-obj.txt");
        // Debug.Log(text);

        using (StringReader reader = new StringReader(entireText))
        {
            string currentText = reader.ReadLine();
            char [] splitIdentifier = { ' ' };
            string [] brokenString;
            while (currentText != null)
            {
                if (
                    !currentText.StartsWith("f ") &&
                    !currentText.StartsWith("v ") &&
                    !currentText.StartsWith("vt ") && !currentText.StartsWith("vn ")
                ) {
                    // Skip the line
                    currentText = reader.ReadLine();
                    if (currentText != null)
                    {
                        currentText = currentText.Replace("  ", " ");
                    }
                }
                else
                {
                    // count elements
                    currentText = currentText.Trim();                           
                    brokenString = currentText.Split(splitIdentifier, 50);
                    switch (brokenString[0])
                    {
                        case "v":
                            vertices++;
                            break;
                        case "vt":
                            vt++;
                            break;
                        case "vn":
                            vn++;
                            break;
                        case "f":
                            //face = face + brokenString.Length - 1;
                            triangles = triangles + 3;                            
                            break;
                    }
                    currentText = reader.ReadLine();
                    if (currentText != null)
                    {
                        currentText = currentText.Replace("  ", " ");
                    }
                }
            }
        }
        mesh.triangles = new int[triangles];
        mesh.trianglesUV = new int[triangles];
        mesh.vertices = new Vector3[vertices];
        mesh.uv = new Vector2[vt];
        mesh.normals = new Vector3[vn];
        return mesh;
    }

    private static void populateMeshStruct(ref localMeshStruct mesh)
    {
        TextAsset content = Resources.Load(mesh.assetName) as TextAsset;
        string entireText = content.text;

        using (StringReader reader = new StringReader(entireText))
        {
            string currentText = reader.ReadLine();
 
            char[] splitIdentifier = { ' ' };
            char[] splitIdentifier2 = { '/' };
            string[] brokenString;
            int f = 0;
            int uv_idx = 0;
            int v = 0;
            int vn = 0;
            int vt = 0;
            int vt1 = 0;
            int vt2 = 0;

            while (currentText != null)
            {
                if (
                    !currentText.StartsWith("f ") && 
                    !currentText.StartsWith("v ") && 
                    !currentText.StartsWith("vt ") &&
                    !currentText.StartsWith("vn ") && 
                    !currentText.StartsWith("g ") && 
                    !currentText.StartsWith("usemtl ") &&
                    !currentText.StartsWith("mtllib ") && 
                    !currentText.StartsWith("vt1 ") && 
                    !currentText.StartsWith("vt2 ") &&
                    !currentText.StartsWith("vc ") && 
                    !currentText.StartsWith("usemap ")
                ) {
                    currentText = reader.ReadLine();
                    if (currentText != null)
                    {
                        currentText = currentText.Replace("  ", " ");
                    }
                }
                else
                {
                    currentText = currentText.Trim();
                    brokenString = currentText.Split(splitIdentifier, 50);
                    switch (brokenString[0])
                    {
                        case "g":
                            break;
                        case "usemtl":
                            break;
                        case "usemap":
                            break;
                        case "mtllib":
                            break;
                        case "v":
                            mesh.vertices[v] = new Vector3(
                                System.Convert.ToSingle(brokenString[1]),
                                System.Convert.ToSingle(brokenString[2]),
                                System.Convert.ToSingle(brokenString[3]));
                            if (v==0) {
                                Debug.Log(mesh.vertices[v]);
                            }

                            v++;
                            break;
                        case "vt":
                            mesh.uv[vt] = new Vector2(
                                System.Convert.ToSingle(brokenString[1]), 
                                System.Convert.ToSingle(brokenString[2]));
                            vt++;
                            break;
                        case "vt1":
                            mesh.uv[vt1] = new Vector2(
                                System.Convert.ToSingle(brokenString[1]),
                                System.Convert.ToSingle(brokenString[2]));
                            vt1++;
                            break;
                        case "vt2":
                            mesh.uv[vt2] = new Vector2(
                                System.Convert.ToSingle(brokenString[1]),
                                System.Convert.ToSingle(brokenString[2]));
                            vt2++;
                            break;
                        case "vn":
                            mesh.normals[vn] = new Vector3(
                                System.Convert.ToSingle(brokenString[1]),
                                System.Convert.ToSingle(brokenString[2]),
                                System.Convert.ToSingle(brokenString[3]));
                            vn++;
                            break;
                        case "vc":
                            break;

                        case "f":
                            Vector3 temp = new Vector3();
                            string [] arrStr1 = brokenString[1].Split(splitIdentifier2, 3); 
                            string [] arrStr2 = brokenString[2].Split(splitIdentifier2, 3); 
                            string [] arrStr3 = brokenString[3].Split(splitIdentifier2, 3); 
                            mesh.triangles[f] = System.Convert.ToInt32(arrStr1[0]) - 1;
                            f++;
                            mesh.triangles[f] = System.Convert.ToInt32(arrStr2[0]) - 1 ;
                            f++;
                            mesh.triangles[f] = System.Convert.ToInt32(arrStr3[0]) - 1;
                            f++;

                            mesh.trianglesUV[uv_idx] = System.Convert.ToInt32(arrStr1[1]) - 1;
                            uv_idx++;
                            mesh.trianglesUV[uv_idx] = System.Convert.ToInt32(arrStr2[1]) - 1 ;
                            uv_idx++;
                            mesh.trianglesUV[uv_idx] = System.Convert.ToInt32(arrStr3[1]) - 1;
                            uv_idx++;


                            break;
                    }
                    currentText = reader.ReadLine();
                    if (currentText != null)
                    {
                        currentText = currentText.Replace("  ", " "); 
                    }
                }
            }
        }
    }
}
