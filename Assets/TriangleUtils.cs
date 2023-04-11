using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;


public static class TriangleUtils
{

	public static float3 Intersection(float3x3 p, float3x2 q)
		=> Intersection(p[0], p[1], p[2], q);

	public static float3 Intersection(float3 p1, float3 p2, float3 p3, float3x2 q)
	{
		var n = math.cross(p2 - p1, p3 - p1);

		var t = -math.dot(q[0] - p1, n) / math.dot(q[1] - q[0], n);

		return q[0] + t * (q[1] - q[0]);
	}

	public static bool Cuts(float3 A, float3 B, float3 C, float3x2 Q)
		=> Cuts(A, B, C, Q[0], Q[1]);

	public static bool Cuts(float3 A, float3 B, float3 C, float3 Q, float3 Q1)
	{
		return
		SignedVolume(Q, A, B, C)
		*
		SignedVolume(Q1, A, B, C) <= 0

		&&

			SignedVolume(Q, Q1, A, B) *
			SignedVolume(Q, Q1, B, C) >= 0
			&&
			SignedVolume(Q, Q1, B, C) *
			SignedVolume(Q, Q1, C, A) >= 0;
	}

	public static float SignedVolume(float3 A, float3 B, float3 C, float3 D)
	{
		return math.dot(B - A, math.cross(C - A, D - A)) / 6;

		//return SignedVolume(ToMatrix(A, B, C, D));
	}

	public static float3 TriangleOrientation(float3 p1, float3 p2, float3 p3)
	{
		return math.cross(p2 - p1, p3 - p2);
	}

	//public static float SignedVolume(float4x4 mat)
	//{
	//	return 1 / 6 * math.determinant(mat);
	//}

	//public static float4x4 ToMatrix(float3 A, float3 B, float3 C, float3 D)
	//{
	//	return new float4x4(new float4(A, 1), new float4(B, 1), new float4(C, 1), new float4(D, 1));
	//}

	//public static float4 Tofloat4(float3 f, float w) => new float4(f, w);
}

public static class PolygonUtils
{

	public static List<float3[]> GetSeparatedPoints(float3[] pts, int[] edges)
	{
		var ret = new List<List<float3>>();

		var edgs = ArrayUtils.GetEdges(edges);

		var graph = new Graph()
		{
			edges = edgs,
			verts = pts
		};

		Graph.DFS(graph, 0);

		ret.Add(graph.verts.Where((v, i) => graph.vertexTypes[i] == Graph.VertexType.Visited).ToList());


		return ret.Select(l => l.ToArray()).ToList();
	}

	public static int[] ConvexHullIdx(float2[] s)
	{
		List<float2> P = new List<float2>(s.Count());
		List<int> I = new List<int>(s.Count());

		// Get leftmost point
		float3 pointOnHull_i = s.Select((ss, ii) => new float3(ss, ii)).Aggregate((leftmost, next) => next.x < leftmost.x ? next : leftmost);
		float2 ptOnHull = new float2(pointOnHull_i.x, pointOnHull_i.y);
		int POHIndex = (int)pointOnHull_i.z;

		int i = 0;
		float2 endpoint = s[0];
		int endpointI = 0;
		do
		{
			P.Add(ptOnHull);
			I.Add(POHIndex);
			for (int j = 0; j < s.Length; j++)
			{
				if (LineUtilities.IsOnLeftOfLine(P[i], endpoint, s[j]) || math.distancesq(endpoint, ptOnHull) < 0.0001f)
				{
					endpointI = j;
					endpoint = s[endpointI];
				}

			}
			i++;
			POHIndex = endpointI;
			ptOnHull = endpoint;


		} while (math.distancesq(endpoint, P[0]) > 0.0001f);
		//} while (endpointI != 0);

		return I.ToArray();
	}
}
public static class ArrayUtils
{
	public static float3 MidPoint(IEnumerable<float3> pts)
	{
		var ret = new float3(0);
		foreach (var p in pts)
			ret += p;

		ret /= pts.Count();

		return ret;
	}

	public static int2[] GetEdges(int[] edges)
	{
		if (edges.Length % 2 != 0)
			return null;

		var ret = new int2[edges.Length / 2];

		for (int i = 0; i < edges.Length / 2 - 1; i++)
		{
			ret[i] = GetEdgeAtIndex(edges, i * 2);
		}

		return ret;
	}


	/// <summary>
	/// [ t0[3], t1[3],.. ] -> [ e0[2], e1[2], ..]
	/// </summary>
	/// <param name="t"></param>
	/// <returns></returns>
	public static int2[] GetEdgeFromTris(int3[] t)
	{
		var ret = new int2[t.Length];

		for (int i = 0; i < t.Length; i += 3)
		{
			var tr = GetEdgesFromTriangle(t[i]);
			ret[i] = tr[0];
			ret[i + 1] = tr[1];
			ret[i + 2] = tr[2];
		}

		return ret;
	}

	public static int3[] GetTriangles(int[] triangles)
	{
		if (triangles.Length % 3 != 0)
			return null;

		var ret = new int3[triangles.Length / 3];

		for (int i = 0; i < triangles.Length / 3; i++)
		{
			ret[i] = GetTriangleAtIndex(triangles, i * 3);
		}

		return ret;
	}

	public static int3 GetTriangleAtIndex(int[] tris, int i)
	{
		return new int3(tris[i], tris[i + 1], tris[i + 2]);
	}

	public static int2 GetEdgeAtIndex(int[] edges, int i)
	{
		return new int2(edges[i], edges[i + 1]);
	}

	public static int2[] GetEdgesFromTriangle(int3 t)
	{
		return new int2[3] {
			new int2(t[0],t[1]),
			new int2(t[1],t[2]),
			new int2(t[2],t[0]),
		};
	}

	//public static int2x3 GetEdgesFromTriangleFixed(int3 t)
	//{
	//	return new int2x3(
	//		new int2(t[0], t[1]),
	//		new int2(t[1], t[2]),
	//		new int2(t[2], t[0])
	//	);
	//}
}

public static class LineUtilities
{
	public static bool IsOnLeftOfLine(float2 l1, float2 l2, float2 p)
	{
		var n = new float2(l2.y - l1.y, -(l2.x - l1.x));
		var d = p - l1;

		return math.dot(d, n) < 0;
	}
}

public static class GeometryUtilities
{
	public static float4x4 ToPlaneCoordMatrix(float3 A, float3 B, float3 C)
	{
		float3 uAB = math.normalizesafe(B - A);
		float3 uAC = math.normalizesafe(C - A);
		float3 uN = math.cross(uAB, uAC);

		float3 u = A + uAB;
		float3 v = A + uAC;
		float3 n = A + uN;

		return ToPlaneCoordMatrix(A, u, v, n);

	}

	public static float4x4 ToPlaneCoordMatrix(PlaneCustom p)
	{
		float3 u = p.pt + p.n[0] * 1;
		float3 v = p.pt + p.n[1] * 1;
		return ToPlaneCoordMatrix(p.pt, u, v, p.n);
	}

	public static float4x4 ToPlaneCoordMatrix(float3 pt, float3 u, float3 v, float3 n)
	{

		//	    [Ax ux vx nx]  	[0 1 0 0]
		// M * [Ay uy vy ny] = [0 0 1 0]
		//     [Az uz vz nz]   [0 0 0 1]
		//     [1  1  1  1 ]   [1 1 1 1]
		// M *       S       =     D

		float4x4 D = new float4x4(0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1);
		float4x4 S = new float4x4(
			new float4(pt, 1),
			new float4(u, 1),
			new float4(v, 1),
			new float4(n, 1)
			);

		return D * math.inverse(S);
	}

	public static PlaneCustom? PlaneFromPoints(IEnumerable<float3> points)
	{
		if (points.Count() < 3)
		{
			return null; // At least three points required
		}

		var centroid = ArrayUtils.MidPoint(points);

		// Calc full 3x3 covariance matrix, excluding symmetries:
		float xx = 0; float xy = 0; float xz = 0;
		float yy = 0; float yz = 0; float zz = 0;

		foreach (var p in points)
		{
			var r = p - centroid;
			xx += r.x * r.x;
			xy += r.x * r.y;
			xz += r.x * r.z;
			yy += r.y * r.y;
			yz += r.y * r.z;
			zz += r.z * r.z;
		}

		var det_x = yy * zz - yz * yz;
		var det_y = xx * zz - xz * xz;
		var det_z = xx * yy - xy * xy;

		var det_max = math.max(math.max(det_x, det_y), det_z);

		if (det_max <= 0.0)
		{
			return null; // The points don't span a plane
		}

		// Pick path with best conditioning:
		float3 dir = 0;
		if (det_max == det_x)
			dir = new float3(det_x, xz * yz - xy * zz, xy * yz - xz * yy);

		else if (det_max == det_y)
		{
			dir = new float3(xz * yz - xy * zz, det_y, xy * xz - yz * xx);
		}
		else
		{
			dir = new float3(xy * yz - xz * yy, xy * xz - yz * xx, det_z);
		}

		return new PlaneCustom()
		{
			pt = centroid,
			n = math.normalizesafe(dir)
		};

	}
}

public class Graph
{
	public float3[] verts;
	public int2[] edges;
	public int[][] adjacencyLists;

	public EdgeType[] edgeTypes;

	public VertexType[] vertexTypes;

	public enum VertexType
	{
		None = 0,
		Visited = 1
	}

	public enum EdgeType
	{
		None = 0,
		Discovery = 1,
		BackEdge = 2
	}

	public static void DFS(Graph g, int vIndex)
	{
		if (g.adjacencyLists == null || g.adjacencyLists.Length == 0)
			g.ComputeAdjacency();
		if (g.edgeTypes == null)
			g.edgeTypes = new EdgeType[g.edges.Length];
		if (g.vertexTypes == null)
			g.vertexTypes = new VertexType[g.verts.Length];

		g.vertexTypes[vIndex] = VertexType.Visited;
		foreach (var (ed, indx) in g.incidentEdges(vIndex))
		{
			if (g.edgeTypes[indx] != EdgeType.None)
				continue;

			int wIndex = g.Opposite(ed, vIndex);
			if (g.vertexTypes[wIndex] == VertexType.None)
			{
				g.edgeTypes[indx] = EdgeType.Discovery;
				DFS(g, wIndex);
			}
			else
				g.edgeTypes[indx] = EdgeType.BackEdge;


		}

	}

	public void ComputeAdjacency()
	{
		adjacencyLists = new int[verts.Length][];

		for (int i = 0; i < verts.Length; i++)
		{
			adjacencyLists[i] = edges.Where(e => e.x == i || e.y == i).Select((e, j) => j).ToArray();
		}
	}

	private int Opposite(int2 edge, int vIndex)
		=> vIndex == edge.x ? edge.y : edge.x;
	/// <summary>
	/// returns indexes of edges connected to verts[vIndex] and corresponding edges
	/// </summary>
	/// <param name="vIndex"></param>
	/// <returns></returns>
	private IEnumerable<(int2, int)> incidentEdges(int vIndex)
	{
		return adjacencyLists[vIndex].Select(i => (edges[i], i));
	}
}

public struct PlaneCustom
{
	public float3 n;
	public float3 pt;

	public static bool CutsThroughPlane(PlaneCustom plane, float3 ptA, float3 ptB)
	{
		float ca = CrossWithNormal(plane, ptA);
		float cb = CrossWithNormal(plane, ptB);
		if (ca == 0 || cb == 0)
			return false;

		return ca * cb < 0;
	}

	public static float CrossWithNormal(PlaneCustom plane, float3 pt)
	{
		return math.dot(plane.n, pt - plane.pt);
	}
}


