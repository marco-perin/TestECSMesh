using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Mathematics;
using UnityEditor;

namespace Tests
{

	public class LineUtilsTest
	{
		// A Test behaves as an ordinary method
		[Test]
		public void TestLineLeftness()
		{
			var l1 = new float2(0, 0);
			var l2 = new float2(0, 1);
			var psx = new float2(-.05f);
			var pdx = new float2(.05f);
			Assert.IsTrue(LineUtilities.IsOnLeftOfLine(l1, l2, psx));
			Assert.IsFalse(LineUtilities.IsOnLeftOfLine(l1, l2, pdx));
		}
	}

	public class PlaneCustomTest
	{
		[Test]
		public void CutsThroughtPlane()
		{
			var u1 = new float3(0, 1, 0);
			var u2 = new float3(0, 1, 1);
			var d1 = new float3(0, -1, 0);
			var d2 = new float3(0, -1, -1);
			//var psx = new float2(-.05f);
			//var pdx = new float2(.05f);

			var plane = new PlaneCustom() { n = new float3(0, 1, 0), pt = 0 };

			Assert.IsTrue(PlaneCustom.CutsThroughPlane(plane, u1, d1));
			Assert.IsFalse(PlaneCustom.CutsThroughPlane(plane, u1, u2));
			Assert.IsTrue(PlaneCustom.CutsThroughPlane(plane, 100 * u1, 100 * d1));
			Assert.IsFalse(PlaneCustom.CutsThroughPlane(plane, 100 * u1, 100 * u2));
		}
	}

	public class ArrayUtilsTester
	{

		[Test]
		public void GetEdgeeAtIndexTest()
		{
			int[] tris6 = new int[6] { 0, 1, 2, 0, 1, 3 };

			var tri0 = ArrayUtils.GetEdgeAtIndex(tris6, 0);
			var tri1 = ArrayUtils.GetEdgeAtIndex(tris6, 2);
			Assert.IsTrue(tri0[0] == 0);
			Assert.IsTrue(tri0[1] == 1);
			Debug.Log(tri0);
			Debug.Log(tri1);
			Assert.IsTrue(tri1[0] == 2);
			Assert.IsTrue(tri1[1] == 0);
		}

		[Test]
		public void GetTriangleAtIndexTest()
		{
			int[] tris6 = new int[6] { 0, 1, 2, 0, 1, 3 };

			var tri0 = ArrayUtils.GetTriangleAtIndex(tris6, 0);
			var tri1 = ArrayUtils.GetTriangleAtIndex(tris6, 3);
			Assert.IsTrue(tri0[0] == 0);
			Assert.IsTrue(tri0[2] == 2);
			Debug.Log(tri1);
			Assert.IsTrue(tri1[1] == 1);
			Assert.IsTrue(tri1[2] == 3);
		}

		[Test]
		public void GetTrianglesTest()
		{
			int[] tris1 = new int[1] { 0 };
			int[] tris6 = new int[6] { 0, 1, 2, 0, 1, 3 };
			Assert.IsNull(ArrayUtils.GetTriangles(tris1));
			var tris = ArrayUtils.GetTriangles(tris6);

			Assert.IsTrue(tris.Length == 2);
			Assert.IsTrue(tris[0][0] == 0);
			Assert.IsTrue(tris[0][2] == 2);
			Debug.Log("t0: " + tris[0]);
			Debug.Log("t1: " + tris[1]);
			Assert.IsTrue(tris[1][1] == 1);
			Assert.IsTrue(tris[1][2] == 3);
		}
	}

	public class TriangleUtilsTests
	{
		// A Test behaves as an ordinary method
		[Test]
		public void TestCut()
		{
			float3 A = new float3(0, 0, 0);
			float3 B = new float3(1, 0, 0);
			float3 C = new float3(1, 1, 0);

			float3 Q = new float3(0.5f, 0.5f, 1);
			float3 Q1 = new float3(0.5f, 0.5f, -1);
			Assert.IsTrue(
			TriangleUtils.Cuts(A, B, C, Q, Q1)
			);
		}

		[Test]
		public void TestCutROR()
		{
			float3 A = new float3(0, 0, 0);
			float3 B = new float3(1, 0, 0);
			float3 C = new float3(1, 1, 0);

			float3 Q = new float3(0.5f, 0.5f, 1);
			float3 Q1 = new float3(0.5f, 0.5f, -1);
			Assert.IsTrue(
			TriangleUtils.Cuts(C, A, B, Q, Q1)
			);
		}

		[Test]
		public void TestCutInverse()
		{
			float3 A = new float3(0, 0, 0);
			float3 B = new float3(1, 0, 0);
			float3 C = new float3(1, 1, 0);

			float3 Q = new float3(0.5f, 0.5f, 1);
			float3 Q1 = new float3(0.5f, 0.5f, -1);
			Assert.IsTrue(
			TriangleUtils.Cuts(A, B, C, Q1, Q)
			);
		}

		[Test]
		public void TestNoCutThrought()
		{
			float3 A = new float3(0, 0, 0);
			float3 B = new float3(1, 0, 0);
			float3 C = new float3(1, 1, 0);

			float3 Q = new float3(0.5f, 0.5f, 1);
			float3 Q1 = new float3(10f, 0.5f, -1);
			Assert.IsFalse(
			TriangleUtils.Cuts(A, B, C, Q, Q1)
			);
		}

		[Test]
		public void TestNoCutUp()
		{
			float3 A = new float3(0, 0, 0);
			float3 B = new float3(1, 0, 0);
			float3 C = new float3(1, 1, 0);

			float3 Q = new float3(0.5f, 0.5f, 1);
			float3 Q1 = new float3(0.5f, 1, 1);
			Assert.IsFalse(
			TriangleUtils.Cuts(A, B, C, Q, Q1)
			);
		}

		[Test]
		public void TestNoCutDown()
		{
			float3 A = new float3(0, 0, 0);
			float3 B = new float3(10, 0, 0);
			float3 C = new float3(10, 10, 0);

			float3 Q = new float3(5, 5, 1);
			float3 Q1 = new float3(5, 5, 5);
			Assert.IsFalse(
			TriangleUtils.Cuts(A, B, C, Q, Q1)
			);
		}

	}
}

