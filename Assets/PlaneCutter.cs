using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
//using UnityEngine;

public class PlaneCutter : MonoBehaviour
{
	public MeshFilter mf;
	private Mesh m;

	public float3 n;
	public float3 pt;

	public float3[] vs;
	public List<int> vs_up;
	public List<int> vs_down;
	public int[] ts;
	public int2[] edges;

	public List<int3> cuttingTris;


	void Start()
	{
		m = mf.sharedMesh;

		vs_up = new List<int>(m.vertexCount / 2);
		vs_down = new List<int>(m.vertexCount / 2);
		cuttingTris = new List<int3>(20);
	}


	void Update()
	{

		PlaneCustom p = new PlaneCustom() { n = math.normalizesafe(n), pt = pt };

		vs = m.vertices.Select(v => (float3)v).ToArray();
		ts = m.triangles;

		vs_up.Clear();
		vs_down.Clear();
		cuttingTris.Clear();

		for (int i = 0; i < vs.Length; i++)
		{
			if (PlaneCustom.CrossWithNormal(p, mf.transform.TransformPoint(vs[i])) > 0)
				vs_up.Add(i);
			else
				vs_down.Add(i);
		}

		int3[] alltris = ArrayUtils.GetTriangles(ts);


		foreach (var t in alltris)
		{
			bool a = PlaneCustom.CutsThroughPlane(p, mf.transform.TransformPoint(vs[t[0]]), mf.transform.TransformPoint(vs[t[1]]));
			bool b = PlaneCustom.CutsThroughPlane(p, mf.transform.TransformPoint(vs[t[1]]), mf.transform.TransformPoint(vs[t[2]]));
			bool c = PlaneCustom.CutsThroughPlane(p, mf.transform.TransformPoint(vs[t[2]]), mf.transform.TransformPoint(vs[t[0]]));
			//var tt = ArrayUtils.GetEdgesFromTriangle(t);
			if (a && b || b && c || a && c)
				cuttingTris.Add(t);
		}


		m.vertices = vs.Select(f => (Vector3)f).ToArray();
		m.triangles = ts;

	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;

		Gizmos.DrawLine(pt, pt + math.normalizesafe(n));

		//CustomGizmos.DrawArrow(pt, pt + n, 15f, n);
		//Gizmos.color = Color.green;


		foreach (var pt in vs_up)
		{
			Gizmos.DrawSphere(mf.transform.TransformPoint(vs[pt]), 0.015f);
		}

		Gizmos.color = Color.blue;

		foreach (var pt in vs_down)
		{
			Gizmos.DrawSphere(mf.transform.TransformPoint(vs[pt]), 0.015f);
		}

		Gizmos.color = Color.red;

		foreach (var ct in cuttingTris)
		{
			foreach (var e in ArrayUtils.GetEdgesFromTriangle(ct))
			{
				Gizmos.DrawLine(mf.transform.TransformPoint(vs[e[0]]), mf.transform.TransformPoint(vs[e[1]]));
			}
		}

		Gizmos.color = Color.green;

		Gizmos.DrawLine(pt, pt + math.normalizesafe(n));

	}
}
