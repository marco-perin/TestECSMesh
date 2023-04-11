using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using System.Linq;
using System;

public class TriangleIntersectionViewer : MonoBehaviour
{
	public Transform intersection;

	public bool buildM1 = true;
	public MeshFilter mf1;

	public bool buildM2 = true;
	public MeshFilter mf2;

	public MeshFilter mfi;

	private Mesh m1;
	private Mesh m2;
	private Mesh mi;

	public float3[] vert1;
	public float3[] vert2;


	public List<float3x2> collidingEdges = new List<float3x2>(10);
	public List<int> collisionEdges = new List<int>(20);
	public List<float3> collisionPts = new List<float3>(10);

	// Start is called before the first frame update
	void Start()
	{
		if (buildM1)
			m1 = new Mesh();
		else
		{
			m1 = mf1.sharedMesh;
			vert1 = m1.vertices.Select(v => (float3)v).ToArray();
		}

		if (buildM2)
			m2 = new Mesh();
		else
		{
			m2 = mf2.sharedMesh;
			vert2 = m2.vertices.Select(v => (float3)v).ToArray();
		}
		mi = new Mesh();
		mi.MarkDynamic();

		if (buildM1)
			ConstructMesh(vert1, ref m1);

		if (buildM2)
			ConstructMesh(vert2, ref m2);

		// apply meshes
		if (buildM1)
			mf1.sharedMesh = m1;

		if (buildM2)
			mf2.sharedMesh = m2;





		//first = true;
	}
	//bool first = true;
	private void Update()
	{
		//if (!first)
		//	return;


		//caculate collisions
		collidingEdges.Clear();
		collisionEdges.Clear();
		collisionPts.Clear();
		var vert1 = m1.vertices;
		var vert2 = m2.vertices;
		var tris1 = m1.triangles;
		var tris2 = m2.triangles;

		int cPsIndex = 0;



		// find intersection edges and pts
		for (int i = 0; i < tris1.Count(); i += 3)
		{
			float3 pt1 = mf1.transform.TransformPoint(vert1[tris1[i]]);
			float3 pt2 = mf1.transform.TransformPoint(vert1[tris1[i + 1]]);
			float3 pt3 = mf1.transform.TransformPoint(vert1[tris1[i + 2]]);

			for (int j = 0; j < tris2.Count(); j += 3)
			{
				float3x2 e1 = new float3x2(new float3(mf2.transform.TransformPoint(vert2[tris2[j]])), new float3(mf2.transform.TransformPoint(vert2[tris2[j + 1]])));
				float3x2 e2 = new float3x2(new float3(mf2.transform.TransformPoint(vert2[tris2[j]])), new float3(mf2.transform.TransformPoint(vert2[tris2[j + 2]])));
				float3x2 e3 = new float3x2(new float3(mf2.transform.TransformPoint(vert2[tris2[j + 1]])), new float3(mf2.transform.TransformPoint(vert2[tris2[j + 2]])));

				bool tc1 = TriangleUtils.Cuts(pt1, pt2, pt3, e1);
				bool tc2 = TriangleUtils.Cuts(pt1, pt2, pt3, e2);
				bool tc3 = TriangleUtils.Cuts(pt1, pt2, pt3, e3);

				float3 cp1 = 0;
				int cp1i = 0;
				float3 cp2 = 0;
				int cp2i = 0;
				float3 cp3 = 0;
				int cp3i = 0;

				if (tc1)
				{
					if (!collidingEdges.Contains(e1) && !collidingEdges.Contains(new float3x2(e1.c1, e1.c0)))
					{
						collidingEdges.Add(e1);
						cp1 = TriangleUtils.Intersection(pt1, pt2, pt3, e1);
						cp1i = cPsIndex++;
						collisionPts.Add(cp1);
					}
					else
					{
						if (!collisionPts.Any(p => math.lengthsq(p - cp1) < 0.0001f))
						{
							cp1 = TriangleUtils.Intersection(pt1, pt2, pt3, e1);
							cp1i = cPsIndex++;
							collisionPts.Add(cp1);
						}
						if (tc2 || tc3)
						{
							cp1 = TriangleUtils.Intersection(pt1, pt2, pt3, e1);
							cp1i = collisionPts.FindIndex(p => math.lengthsq(p - cp1) < 0.0001f);
						}
					}
				}
				if (tc2)
				{

					if (!collidingEdges.Contains(e2) && !collidingEdges.Contains(new float3x2(e2.c1, e2.c0)))
					{
						collidingEdges.Add(e2);
						cp2 = TriangleUtils.Intersection(pt1, pt2, pt3, e2);
						cp2i = cPsIndex++;
						collisionPts.Add(cp2);
					}
					else
					{
						if (!collisionPts.Any(p => math.lengthsq(p - cp2) < 0.0001f))
						{
							cp2 = TriangleUtils.Intersection(pt1, pt2, pt3, e2);
							cp2i = cPsIndex++;
							collisionPts.Add(cp2);
						}
						else if (tc1 || tc3)
						{
							cp2 = TriangleUtils.Intersection(pt1, pt2, pt3, e2);
							cp2i = collisionPts.FindIndex(p => math.lengthsq(p - cp2) < 0.0001f);
						}
					}
				}
				if (tc3)
				{
					if (!collidingEdges.Contains(e3) && !collidingEdges.Contains(new float3x2(e3.c1, e3.c0)))
					{
						collidingEdges.Add(e3);
						cp3 = TriangleUtils.Intersection(pt1, pt2, pt3, e3);
						cp3i = cPsIndex++;
						collisionPts.Add(cp3);
					}
					else
					{
						if (!collisionPts.Any(p => math.lengthsq(p - cp3) < 0.0001f))
						{
							cp3 = TriangleUtils.Intersection(pt1, pt2, pt3, e3);
							cp3i = cPsIndex++;
							collisionPts.Add(cp3);
						}
						else if (tc1 || tc2)
						{
							cp3 = TriangleUtils.Intersection(pt1, pt2, pt3, e3);
							cp3i = collisionPts.FindIndex(p => math.lengthsq(p - cp3) < 0.0001f);
						}
					}

				}
				if (tc1 && tc2)
				{
					collisionEdges.Add(cp1i);
					collisionEdges.Add(cp2i);
				}
				if (tc2 && tc3)
				{
					collisionEdges.Add(cp2i);
					collisionEdges.Add(cp3i);
				}
				if (tc3 && tc1)
				{
					collisionEdges.Add(cp3i);
					collisionEdges.Add(cp1i);
				}

			}
		}

		if (!collisionPts.Any())
			return;

		// TODO: check for when pts are connected
		//var connectedL = PolygonUtils.GetSeparatedPoints(collisionPts.Distinct().ToArray(), collisionEdges.ToArray());
		//var connected = connectedL[0];

		var connected = collisionPts;//.Distinct();

		float3 midPoint = ArrayUtils.MidPoint(connected);

		//GeometryUtility.TryCreatePlaneFromPolygon(collisionPts.Select(f => (Vector3)f).ToArray(), out Plane p);
		PlaneCustom? pln = GeometryUtilities.PlaneFromPoints(connected);

		if (pln == null)
		{
			Debug.LogError("PlaneIsNull!");
		}
		PlaneCustom pl = pln.Value;

		//	throw new ApplicationException("The plane is null you dumb ass");

		if (math.dot(pl.n, pl.pt - (float3)Camera.main.transform.position) < 1)
			pl.n *= -1;

		//intersection.position = pl.Value.pt;


		var M = GeometryUtilities.ToPlaneCoordMatrix(pl);

		float2[] pts2d = connected.Select(pt => math.mul(M, new float4(pt, 1))).Select(p => new float2(p.x, p.y)).ToArray();

		var convexhullptIndexes = PolygonUtils.ConvexHullIdx(pts2d).Select(i => collisionPts[i]);
		//var convexhullptIndexes = collisionPts;


		ConstructMeshFromMidpoint(convexhullptIndexes.ToArray(), pl.pt, pl.n, ref mi);

		mfi.sharedMesh = mi;
		//first = false;
	}


	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		// vert 1 ball
		foreach (var v in vert1)
		{
			Gizmos.DrawSphere(mf1.transform.TransformPoint(v), 0.01f);
		}

		Gizmos.color = Color.green;

		// vert 2 balls
		foreach (var v in vert2)
		{
			Gizmos.DrawSphere(mf2.transform.TransformPoint(v), 0.01f);
		}

		Gizmos.color = Color.blue;

		// colliding edges
		foreach (var e in collidingEdges)
		{
			Gizmos.DrawLine(e[0], e[1]);
		}

		// collision pts
		foreach (var pt in collisionPts)
		{
			Gizmos.DrawSphere(pt, 0.01f);
		}
		Gizmos.color = Color.black;

		// collision edges
		for (int i = 0; i < collisionEdges.Count(); i += 2)
		{
			Gizmos.DrawLine(collisionPts[collisionEdges[i]], collisionPts[collisionEdges[i + 1]]);
		}

		Gizmos.color = Color.cyan;

		Gizmos.DrawSphere(ArrayUtils.MidPoint(collisionPts), 0.01f);

		Gizmos.color = Color.white;


	}

	// pass vertex from the hull in order ( cw or ccw )
	private void ConstructMeshFromMidpoint(float3[] verts, float3 midPoint, float3 outNormal, ref Mesh m)
	{
		var vlist = verts.ToList();
		vlist.Add(midPoint);

		NativeArray<float3> vs = new NativeArray<float3>(vlist.ToArray(), Allocator.Temp);
		NativeArray<int> tris = new NativeArray<int>(verts.Count() * 3, Allocator.Temp);
		//NativeArray<float3> norms = new NativeArray<float3>(tris.Count(), Allocator.Temp);

		int midIndex = vlist.Count() - 1;
		//Debug.Log(vs.)
		int triI = 0;
		for (int i = 1; i <= verts.Count(); i++)
		{
			bool flipNorm = math.dot(TriangleUtils.TriangleOrientation(vlist[i - 1], vlist[midIndex], vlist[i == verts.Count() ? 0 : i]),
				outNormal) > 0;
			if (!flipNorm)
			{
				tris[triI++] = i - 1;
				tris[triI++] = midIndex;
			}
			else
			{
				tris[triI++] = midIndex;
				tris[triI++] = i - 1;
			}
			// last iteration
			if (i == verts.Count())
				tris[triI++] = 0;
			else
				tris[triI++] = i;



		}

		//for (int i = 0; i < tris.Count(); i += 3)
		//{
		//	float3 n = RecalcNormals(verts[tris[i]], verts[tris[i + 1]], verts[tris[i + 2]]);

		//	if (math.dot(n, outNormal) < 1)
		//		n *= 1;

		//	norms[i] += n;
		//	norms[i + 1] += n;
		//	norms[i + 2] += n;
		//}

		//// normailze normals
		//for (int i = 0; i < norms.Count(); i++)
		//{
		//	norms[i] = math.normalize(norms[i]);
		//}

		//Debug.Log($"vs : {vs.Count()} trisMax: {tris.Max()}");
		//         7          6
		if (vs.Count() < tris.Max() + 1)
		{
			Debug.Log("LOL");
		}

		m.triangles = Array.Empty<int>();
		m.vertices = vs.Select(f3 => (Vector3)f3).ToArray();
		m.triangles = tris.ToArray();

		//m.normals = norms.Select(f => (Vector3)f).ToArray();

		m.RecalculateNormals();




		vs.Dispose();
		tris.Dispose();
		//norms.Dispose();

	}

	public void ConstructMesh(float3[] verts, ref Mesh m)
	{
		NativeArray<float3> vs = new NativeArray<float3>(verts, Allocator.Temp);
		NativeArray<int> tris = new NativeArray<int>(12, Allocator.Temp);

		// Down
		tris[0] = 0;
		tris[1] = 1;
		tris[2] = 2;

		tris[3] = 0;
		tris[4] = 3;
		tris[5] = 1;

		tris[6] = 1;
		tris[7] = 3;
		tris[8] = 2;

		tris[9] = 2;
		tris[10] = 3;
		tris[11] = 0;

		m.vertices = vs.Select(f3 => new Vector3(f3.x, f3.y, f3.z)).ToArray();
		m.triangles = tris.ToArray();

		m.RecalculateNormals();

		vs.Dispose();
		tris.Dispose();
	}

	public float3 RecalcNormals(float3 p1, float3 p2, float3 p3)
	{
		return math.cross(p2 - p1, p3 - p2);
	}
}

//public static class MeshExtension
//{
//	public static float3 getVert(this int)
//	}
