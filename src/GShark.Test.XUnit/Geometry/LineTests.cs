﻿using FluentAssertions;
using GShark.Core;
using GShark.ExtendedMethods;
using GShark.Geometry;
using System;
using System.Linq;
using GShark.Geometry.Interfaces;
using Xunit;
using Xunit.Abstractions;

namespace GShark.Test.XUnit.Geometry
{
    public class LineTests
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly Line _exampleLine;
        public LineTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;

            #region example
            // Initializes a line by start and end point.
            Vector3 pt1 = new Vector3 { 5, 0, 0 };
            Vector3 pt2 = new Vector3 { 15, 15, 0 };
            _exampleLine = new Line(pt1, pt2);

            // Initializes a line by a starting point a direction and a length.
            Line line = new Line(pt1, Vector3.XAxis, 15);
            #endregion
        }

        [Fact]
        public void It_Returns_A_Line()
        {
            // Arrange
            Vector3 pt1 = new Vector3 { 5, 0, 0 };

            // Act
            Line line = _exampleLine;

            // Assert
            line.Should().NotBeNull();
            line.Start.All(pt1.Contains).Should().BeTrue();
        }

        [Fact]
        public void It_Throws_An_Exception_If_Inputs_Are_Not_Valid_Or_Equals()
        {
            // Arrange
            Vector3 pt = new Vector3 {5, 5, 0};

            // Act
            Func<Line> func0 = () => new Line(pt, pt);
            Func<Line> func1 = () => new Line(pt, Vector3.Unset);

            // Assert
            func0.Should().Throw<Exception>();
            func1.Should().Throw<Exception>().WithMessage("Inputs are not valid, or are equal");
        }

        [Fact]
        public void It_Returns_A_Line_By_A_Starting_Point_Direction_Length()
        {
            // Arrange
            Vector3 startingPoint = new Vector3 { 0, 0, 0 };
            int lineLength = 15;
            Vector3 expectedDirection = new Vector3 {1, 0, 0};
            Vector3 expectedStartPt = new Vector3 { lineLength, 0, 0 };

            // Act
            Line line1 = new Line(startingPoint, Vector3.XAxis, lineLength);
            Line line2 = new Line(startingPoint, Vector3.XAxis, -lineLength);

            // Assert
            line1.Length.Should().Be(line2.Length).And.Be(lineLength);
            line1.Start.Should().BeEquivalentTo(line2.Start).And.BeEquivalentTo(startingPoint);

            line1.Direction.Should().BeEquivalentTo(expectedDirection);
            line1.End.Should().BeEquivalentTo(expectedStartPt);

            line2.Direction.Should().BeEquivalentTo(Vector3.Reverse(expectedDirection));
            line2.End.Should().BeEquivalentTo(Vector3.Reverse(expectedStartPt));
        }

        [Fact]
        public void It_Throws_An_Exception_If_Length_Is_Zero()
        {
            // Arrange
            Vector3 startingPoint = new Vector3 { 0, 0, 0 };

            // Act
            Func<Line> func = () => new Line(startingPoint, Vector3.XAxis, 0);

            // Assert
            func.Should().Throw<Exception>().WithMessage("Length must not be 0.0");
        }

        [Fact]
        public void It_Returns_The_Length_Of_The_Line()
        {
            // Arrange
            double expectedLength = 18.027756;

            // Act
            Line line = _exampleLine;

            // Assert
            line.Length.Should().BeApproximately(expectedLength, 5);
        }

        [Fact]
        public void It_Returns_The_Line_Direction()
        {
            // Arrange
            Vector3 expectedDirection = new Vector3 { 0.5547, 0.83205, 0};

            // Act
            Vector3 dir = _exampleLine.Direction;

            // Assert
            dir.IsEqualRoundingDecimal(expectedDirection, 5).Should().BeTrue();
        }

        [Fact]
        public void It_Returns_The_ClosestPoint()
        {
            // Arrange
            Vector3 pt = new Vector3 { 5, 8, 0 };
            Vector3 expectedPt = new Vector3 { 8.692308, 5.538462, 0 };

            // Act
            Vector3 closestPt = _exampleLine.ClosestPt(pt);

            // Assert
            closestPt.IsEqualRoundingDecimal(expectedPt, 6).Should().BeTrue();
        }

        [Fact]
        public void PointAt_Throw_An_Exception_If_Parameter_Outside_The_Curve_Domain()
        {
            // Act
            Func<Vector3> func = () => _exampleLine.PointAt(2);

            // Assert
            func.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Parameter is outside the domain 0.0 to 1.0 *");
        }

        [Theory]
        [InlineData(0.0, new[] { 5.0, 0, 0 })]
        [InlineData(0.15, new[] { 6.5, 2.25, 0 })]
        [InlineData(0.36, new[] { 8.6, 5.4, 0 })]
        [InlineData(0.85, new[] { 13.5, 12.75, 0 })]
        [InlineData(1.0, new[] { 15.0, 15.0, 0 })]
        public void It_Returns_The_Evaluated_Point_At_The_Given_Parameter(double t, double[] ptExpected)
        {
            // Act
            Vector3 ptEvaluated = _exampleLine.PointAt(t);

            // Assert
            ptEvaluated.Equals(ptExpected.ToVector()).Should().BeTrue();
        }

        [Theory]
        [InlineData(0.323077, new[] { 5.0, 7.0, 0 })]
        [InlineData(0.338462, new[] { 7.0, 6.0, 0 })]
        [InlineData(0.415385, new[] { 5.0, 9.0, 0 })]
        public void It_Returns_The_Parameter_On_The_Line_Closest_To_The_Point(double expectedParam, double[] pts)
        {
            // Arrange
            Vector3 pt = new Vector3(pts);

            // Act
            double parameter = _exampleLine.ClosestParameter(pt);

            // Assert
            parameter.Should().BeApproximately(expectedParam, GeoSharpMath.MAX_TOLERANCE);
        }

        [Fact]
        public void It_Returns_A_Flipped_Line()
        {
            // Act
            Line flippedLine = _exampleLine.Flip();

            // Assert
            flippedLine.Start.Equals(_exampleLine.End).Should().BeTrue();
            flippedLine.End.Equals(_exampleLine.Start).Should().BeTrue();
        }

        [Fact]
        public void It_Returns_An_Extend_Line()
        {
            // Act
            Line extendedLine = _exampleLine.Extend(0, -5);

            // Assert
            extendedLine.Length.Should().BeApproximately(13.027756, GeoSharpMath.MAX_TOLERANCE);
            extendedLine.Start.Should().BeEquivalentTo(_exampleLine.Start);
        }

        [Fact]
        public void It_Checks_If_Two_Lines_Are_Equals()
        {
            // Act
            Line lineFlip = _exampleLine.Flip();
            Line lineFlippedBack = lineFlip.Flip();

            // Assert
            lineFlip.Equals(lineFlippedBack).Should().BeFalse();
            lineFlippedBack.Equals(_exampleLine).Should().BeTrue();
        }

        [Fact]
        public void It_Returns_A_Transformed_Line()
        {
            // Arrange
            Vector3 translatedVec = new Vector3 { 10, 10, 0 };
            Transform transform = Transform.Translation(translatedVec);

            // Act
            Line transformedLine = _exampleLine.Transform(transform);

            // Assert
            transformedLine.Start.Should().BeEquivalentTo(new Vector3 { 15, 10, 0 });
            transformedLine.End.Should().BeEquivalentTo(new Vector3 { 25, 25, 0 });
        }

        [Fact]
        public void It_Returns_A_NurbsCurve_Data_From_The_Line()
        {
            // Arrange
            ICurve line = _exampleLine;

            // Assert
            line.ControlPoints.Count.Should().Be(2);
            line.Degree.Should().Be(1);
        }
    }
}
