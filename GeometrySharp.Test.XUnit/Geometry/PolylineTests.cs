﻿using FluentAssertions;
using GeometrySharp.Core;
using GeometrySharp.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace GeometrySharp.Test.XUnit.Geometry
{
    public class PolylineTests
    {
        private readonly ITestOutputHelper _testOutput;
        public PolylineTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }
        public static Vector3[] ExamplePts => new[] { new Vector3 { 5, 0, 0 }, new Vector3 { 15, 15, 0 }, new Vector3 { 20, 5, 0 }, new Vector3 { 30, 10, 0 } };

        [Fact]
        public void It_Returns_A_Polyline()
        {
            Polyline polyline = new Polyline(ExamplePts);

            polyline.Count.Should().Be(ExamplePts.Length);
            polyline[0].Should().BeEquivalentTo(ExamplePts[0]);
        }

        [Fact]
        public void Polyline_Throws_An_Exception_If_Vertex_Count_Is_Less_Than_Two()
        {
            Vector3[] pts = new[] {new Vector3 {5, 0, 0}};

            Func<Polyline> func = () => new Polyline(pts);

            func.Should().Throw<Exception>().WithMessage("Insufficient points for a polyline.");
        }

        [Fact]
        public void It_Returns_A_Polyline_Removing_Short_Segments()
        {
            Vector3[] pts = new[] { new Vector3 { 5, 5, 0 }, new Vector3 { 5, 10, 0 }, new Vector3 { 5, 10, 0 }, 
                new Vector3 { 15,12,0 }, new Vector3 { 20,-20,0 }, new Vector3 { 20, -20, 0 } };

            Vector3[] ptsExpected = new[] { new Vector3 { 5, 5, 0 }, new Vector3 { 5, 10, 0 }, new Vector3 { 15,12,0 }, new Vector3 { 20,-20,0 }};

            Polyline polyline = new Polyline(pts);

            polyline.Should().BeEquivalentTo(ptsExpected);
            _testOutput.WriteLine(polyline.ToString());
        }

        [Fact]
        public void It_Returns_The_Length_Of_A_Polyline()
        {
            Polyline polyline = new Polyline(ExamplePts);

            double length = polyline.Length();

            length.Should().BeApproximately(40.388436, GeoSharpMath.MAXTOLERANCE);
        }

        [Fact]
        public void It_Returns_A_Collection_Of_Lines()
        {
            Polyline polyline = new Polyline(ExamplePts);

            Line[] segments = polyline.Segments();

            segments.Length.Should().Be(3);
            segments[1].Length.Should().Be(segments[2].Length).And.BeApproximately(11.18034, GeoSharpMath.MAXTOLERANCE);
        }

        [Theory]
        [InlineData(0.0, new double[]{ 5, 0, 0 }, new double[] { 0.5547, 0.83205, 0 })]
        [InlineData(0.25, new double[] { 12.5, 11.25, 0 }, new double[] { 0.5547, 0.83205, 0 })]
        [InlineData(0.55, new double[] { 18.25, 8.5, 0 }, new double[] { 0.447214, -0.894427, 0 })]
        [InlineData(1.0, new double[] { 30, 10, 0 }, new double[] { 0.894427, 0.447214, 0 })]
        public void It_Returns_A_Point_At_The_Given_Parameter(double t, double[] ptExpected, double[] tangentExpected)
        {
            Polyline polyline = new Polyline(ExamplePts);

            Vector3 pt = polyline.PointAt(t, out Vector3 tangent);

            Vector3 tang = new Vector3(tangentExpected);
            tangent.IsEqualRoundingDecimal(tang, 6).Should().BeTrue();
            pt.Should().BeEquivalentTo(new Vector3(ptExpected));
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.05)]
        public void PointAt_Throws_An_Exception_If_Parameter_Is_Smaller_Than_Zero_And_Bigger_Than_One(double t)
        {
            Polyline polyline = new Polyline(ExamplePts);

            Func<Vector3> func = () => polyline.PointAt(t, out _);

            func.Should().Throw<Exception>();
        }

        [Fact]
        public void It_Returns_A_Transformed_Polyline()
        {
            Transform translation = Transform.Translation(new Vector3 { 10, 15, 0 });
            Transform rotation = Transform.Rotation(GeoSharpMath.ToRadians(30), new Vector3 { 0, 0, 0 });
            Transform combinedTransformations = translation.Combine(rotation);
            double[] distanceToCheck = new[] {19.831825, 20.496248, 24.803072, 28.67703};
            Polyline polyline = new Polyline(ExamplePts);

            Polyline transformedPoly = polyline.Transform(combinedTransformations);

            double[] lengths = polyline.Select((pt, i) => pt.DistanceTo(transformedPoly[i])).ToArray();
            lengths.Select((val, i) => val.Should().BeApproximately(distanceToCheck[i], GeoSharpMath.MAXTOLERANCE));
        }
    }
}
