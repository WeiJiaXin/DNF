using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeDrawer : MonoForDebug
{
    [SortingLayer]
    public string sortingLayer;
    public int sortingOrder;
    public Color color = new Color(1,1,1,0.67f);
    public Vector3 originEuler = new Vector3(-90,0,0);

    //
    public float angle = 360;

    public float minRadius = 0;

    public float maxRadius = 3;
    //

    private GameObject go;
    private MeshFilter mf;
    private MeshRenderer mr;
    private Shader shader;

    public void Display_EB() => Display(true);

    public void Display(bool reDraw=false)
    {
        if (!go || reDraw)
        {
            DrawCircleSolid(transform, Vector3.zero, angle, minRadius, maxRadius);
        }

        go.SetActive(true);
    }

    public void Disable()
    {
        if (!go)
            return;
        go.SetActive(false);
    }

    private void DrawCircleSolid(Transform t, Vector3 center, float angle, float minRadius, float maxRadius)
    {
        int pointAmount = 30; //点的数目，值越大曲线越平滑 
        float eachAngle = angle / pointAmount;
        Vector3 forward = t.forward;

        List<Vector3> vertices = new List<Vector3>();
        List<Color> colors = new List<Color>();
        float GradientFactor;
        if (minRadius <= 0)
        {
            GradientFactor = -3;
            vertices.Add(center);
            colors.Add(new Color(color.r, color.g, color.b, GradientFactor * color.a));

            for (int i = 0; i <= pointAmount; i++)
            {
                Vector3 pos = Quaternion.Euler(0f, eachAngle * i, 0f) * forward * maxRadius + center;
                vertices.Add(pos);
                colors.Add(color);
            }

            CreateMesh(vertices, colors, true);
        }
        else
        {
            GradientFactor = -0.1f;
            var cenRadius = (maxRadius - minRadius) * 0.33f + minRadius;
            var cenColor = new Color(color.r, color.g, color.b, GradientFactor * maxRadius * color.a);
            for (int i = 0; i <= pointAmount; i++)
            {
                var dir = Quaternion.Euler(0f, eachAngle * i, 0f) * forward;
                Vector3 pos = dir * minRadius + center;
                vertices.Add(pos);
                colors.Add(color);
                pos = dir * cenRadius + center;
                vertices.Add(pos);
                colors.Add(cenColor);
                pos = dir * maxRadius + center;
                vertices.Add(pos);
                colors.Add(color);
            }

            CreateMesh(vertices, colors, false);
        }
    }

    private void CreateMesh(List<Vector3> vertices, List<Color> colors, bool byPoint)
    {
        if (go == null)
        {
            go = new GameObject("Range Mesh");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0, 0, 0);
            go.transform.eulerAngles = originEuler;
            mf = go.AddComponent<MeshFilter>();
            mr = go.AddComponent<MeshRenderer>();
            shader = Shader.Find("Sprites/Default");
            mr.material.shader = shader;
            mr.material.renderQueue = 5000;
        }

        mr.sortingLayerName = sortingLayer;
        mr.sortingOrder = sortingOrder;

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = byPoint ? GetTrianglesByPoint(vertices) : GetTrianglesByCircle(vertices);
        if (colors != null)
            mesh.SetColors(colors);

        mf.mesh = mesh;
    }

    private int[] GetTrianglesByPoint(List<Vector3> vertices)
    {
        int[] triangles;

        int triangleAmount = vertices.Count - 2;
        triangles = new int[3 * triangleAmount];

        //根据三角形的个数，来计算绘制三角形的顶点顺序（索引）   
        //顺序必须为顺时针或者逆时针   
        for (int i = 0; i < triangleAmount; i++)
        {
            triangles[3 * i] = 0; //固定第一个点   
            triangles[3 * i + 1] = i + 1;
            triangles[3 * i + 2] = i + 2;
        }

        return triangles;
    }

    private int[] GetTrianglesByCircle(List<Vector3> vertices)
    {
        int[] triangles;

        int triangleAmount = (vertices.Count / 3 - 1) * 4;
        triangles = new int[3 * triangleAmount];

        //根据三角形的个数，来计算绘制三角形的顶点顺序（索引）   
        //顺序必须为顺时针或者逆时针   
        for (int i = 0; i < triangleAmount; i += 3)
        {
            if (4 * i + 0 >= triangles.Length)
                break;
            triangles[4 * i + 0] = i + 0;
            triangles[4 * i + 1] = i + 1;
            triangles[4 * i + 2] = i + 3;
            triangles[4 * i + 3] = i + 3;
            triangles[4 * i + 4] = i + 1;
            triangles[4 * i + 5] = i + 4;
            triangles[4 * i + 6] = i + 1;
            triangles[4 * i + 7] = i + 2;
            triangles[4 * i + 8] = i + 4;
            triangles[4 * i + 9] = i + 4;
            triangles[4 * i + 10] = i + 2;
            triangles[4 * i + 11] = i + 5;
        }

        return triangles;
    }
}