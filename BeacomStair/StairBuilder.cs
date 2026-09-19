using System;
using System.Collections.Generic;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace BeacomStair
{
    internal static class StairSettings
    {
        // Agreed stair geometry.
        public const double TotalRise = 3170.0;
        public const int RiseCount = 15;
        public const int TreadCount = 14;
        public const double Going = 250.0;
        public const double FlightRun = 3500.0;
        public const double PlatformLength = 1000.0;
        public const double OverallLength = 4500.0;
        public const double ClearBetweenHandrails = 900.0;

        // Working fabrication defaults - intentionally kept here for easy adjustment.
        public const double TreadWidth = 1050.0;
        public const double StringerSpacing = 900.0;
        public const double HandrailOutsideDiameter = 42.4;
        public const double StairHandrailHeight = 900.0;
        public const double PlatformGuardHeight = 1100.0;
        public const double MidRailHeight = 550.0;
        public const double PlatformBeamReferenceDrop = 100.0;

        public const string StringerProfile = "PFC200X75X23";
        public const string TreadProfile = "PL8";
        public const string RiserProfile = "PL6";
        public const string RailProfile = "CHS42.4*3.2";
        public const string Material = "S355JR";

        public static double Rise
        {
            get { return TotalRise / RiseCount; }
        }

        public static double RailCentreOffset
        {
            get { return (ClearBetweenHandrails / 2.0) + (HandrailOutsideDiameter / 2.0); }
        }
    }

    internal sealed class StairBuildResult
    {
        public double Rise { get; set; }
        public int InsertedObjectCount { get; set; }
    }

    internal sealed class StairBuilder
    {
        private readonly Model _model;

        private V3 _origin;
        private V3 _xAxis;
        private V3 _yAxis;

        private int _insertedObjectCount;
        private readonly List<ModelObject> _insertedObjects = new List<ModelObject>();

        public StairBuilder(Model model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public StairBuildResult Build(Point startPoint, Point directionPoint)
        {
            if (startPoint == null)
                throw new ArgumentNullException(nameof(startPoint));

            if (directionPoint == null)
                throw new ArgumentNullException(nameof(directionPoint));

            CreateCoordinateSystem(startPoint, directionPoint);

            try
            {
                CreatePlatform();
                CreateFlight();
                CreateGuarding();

                return new StairBuildResult
                {
                    Rise = StairSettings.Rise,
                    InsertedObjectCount = _insertedObjectCount
                };
            }
            catch
            {
                DeleteInsertedObjects();
                throw;
            }
        }

        private void CreateCoordinateSystem(Point startPoint, Point directionPoint)
        {
            V3 start = V3.FromPoint(startPoint);
            V3 direction = V3.FromPoint(directionPoint) - start;

            // The first pick is the centre of the top platform back edge.
            // Only the horizontal direction of the second pick matters.
            _origin = start;
            direction = new V3(direction.X, direction.Y, 0.0);

            if (direction.Length < 1.0)
            {
                throw new InvalidOperationException(
                    "The direction point is too close to the start point. " +
                    "Pick a second point clearly in the direction the stair should run.");
            }

            _xAxis = direction.Normalised();

            // Width is fixed, so the cross-stair axis is simply 90 degrees to the run.
            _yAxis = new V3(-_xAxis.Y, _xAxis.X, 0.0);
        }

        private void CreatePlatform()
        {
            double halfWidth = StairSettings.TreadWidth / 2.0;
            double halfStringerSpacing = StairSettings.StringerSpacing / 2.0;

            // 1000 mm top platform plate, centred on the picked start point at its back edge.
            CreatePlate(
                "BEACOM PLATFORM",
                StairSettings.TreadProfile,
                LocalPoint(0.0, -halfWidth, 0.0),
                LocalPoint(StairSettings.PlatformLength, -halfWidth, 0.0),
                LocalPoint(StairSettings.PlatformLength, halfWidth, 0.0),
                LocalPoint(0.0, halfWidth, 0.0),
                "3");

            // Side supports below the platform.
            double beamZ = -StairSettings.PlatformBeamReferenceDrop;

            CreateBeam(
                "BEACOM PLATFORM STRINGER L",
                StairSettings.StringerProfile,
                LocalPoint(0.0, -halfStringerSpacing, beamZ),
                LocalPoint(StairSettings.PlatformLength, -halfStringerSpacing, beamZ),
                "2");

            CreateBeam(
                "BEACOM PLATFORM STRINGER R",
                StairSettings.StringerProfile,
                LocalPoint(0.0, halfStringerSpacing, beamZ),
                LocalPoint(StairSettings.PlatformLength, halfStringerSpacing, beamZ),
                "2");
        }

        private void CreateFlight()
        {
            double rise = StairSettings.Rise;
            double halfWidth = StairSettings.TreadWidth / 2.0;
            double halfStringerSpacing = StairSettings.StringerSpacing / 2.0;

            // Sloping PFC stringers. Reference line runs from platform level to floor level.
            CreateBeam(
                "BEACOM FLIGHT STRINGER L",
                StairSettings.StringerProfile,
                LocalPoint(StairSettings.PlatformLength, -halfStringerSpacing, 0.0),
                LocalPoint(StairSettings.OverallLength, -halfStringerSpacing, -StairSettings.TotalRise),
                "2");

            CreateBeam(
                "BEACOM FLIGHT STRINGER R",
                StairSettings.StringerProfile,
                LocalPoint(StairSettings.PlatformLength, halfStringerSpacing, 0.0),
                LocalPoint(StairSettings.OverallLength, halfStringerSpacing, -StairSettings.TotalRise),
                "2");

            // 14 horizontal treads. The platform itself forms the 15th/top walking level.
            for (int i = 0; i < StairSettings.TreadCount; i++)
            {
                double x1 = StairSettings.PlatformLength + (i * StairSettings.Going);
                double x2 = x1 + StairSettings.Going;
                double z = -((i + 1) * rise);

                CreatePlate(
                    "BEACOM TREAD " + (i + 1),
                    StairSettings.TreadProfile,
                    LocalPoint(x1, -halfWidth, z),
                    LocalPoint(x2, -halfWidth, z),
                    LocalPoint(x2, halfWidth, z),
                    LocalPoint(x1, halfWidth, z),
                    "4");
            }

            // 15 closed risers, including the riser below the platform and the bottom riser.
            for (int i = 0; i < StairSettings.RiseCount; i++)
            {
                double x = StairSettings.PlatformLength + (i * StairSettings.Going);
                double zTop = -(i * rise);
                double zBottom = -((i + 1) * rise);

                // The final riser is at the foot rather than one going short.
                if (i == StairSettings.RiseCount - 1)
                    x = StairSettings.OverallLength;

                CreatePlate(
                    "BEACOM RISER " + (i + 1),
                    StairSettings.RiserProfile,
                    LocalPoint(x, -halfWidth, zTop),
                    LocalPoint(x, halfWidth, zTop),
                    LocalPoint(x, halfWidth, zBottom),
                    LocalPoint(x, -halfWidth, zBottom),
                    "5");
            }
        }

        private void CreateGuarding()
        {
            double railOffset = StairSettings.RailCentreOffset;

            CreateSideGuard(-railOffset, "L");
            CreateSideGuard(railOffset, "R");
        }

        private void CreateSideGuard(double y, string side)
        {
            // Platform top rail and mid rail.
            CreateBeam(
                "BEACOM PLATFORM TOP RAIL " + side,
                StairSettings.RailProfile,
                LocalPoint(0.0, y, StairSettings.PlatformGuardHeight),
                LocalPoint(StairSettings.PlatformLength, y, StairSettings.PlatformGuardHeight),
                "7");

            CreateBeam(
                "BEACOM PLATFORM MID RAIL " + side,
                StairSettings.RailProfile,
                LocalPoint(0.0, y, StairSettings.MidRailHeight),
                LocalPoint(StairSettings.PlatformLength, y, StairSettings.MidRailHeight),
                "7");

            // Three platform posts per side.
            CreateRailPost(0.0, y, 0.0, StairSettings.PlatformGuardHeight, side, "PLATFORM");
            CreateRailPost(500.0, y, 0.0, StairSettings.PlatformGuardHeight, side, "PLATFORM");
            CreateRailPost(StairSettings.PlatformLength, y, 0.0, StairSettings.PlatformGuardHeight, side, "PLATFORM");

            // Flight rails follow the pitch line.
            double flightStartX = StairSettings.PlatformLength;
            double flightEndX = StairSettings.OverallLength;

            CreateBeam(
                "BEACOM STAIR TOP RAIL " + side,
                StairSettings.RailProfile,
                LocalPoint(flightStartX, y, StairSettings.StairHandrailHeight),
                LocalPoint(
                    flightEndX,
                    y,
                    -StairSettings.TotalRise + StairSettings.StairHandrailHeight),
                "7");

            CreateBeam(
                "BEACOM STAIR MID RAIL " + side,
                StairSettings.RailProfile,
                LocalPoint(flightStartX, y, StairSettings.MidRailHeight),
                LocalPoint(
                    flightEndX,
                    y,
                    -StairSettings.TotalRise + StairSettings.MidRailHeight),
                "7");

            // Flight posts at 500 mm horizontal centres including both ends.
            for (double x = flightStartX; x <= flightEndX + 0.1; x += 500.0)
            {
                double baseZ = FlightPitchZ(x);
                CreateRailPost(
                    x,
                    y,
                    baseZ,
                    baseZ + StairSettings.StairHandrailHeight,
                    side,
                    "STAIR");
            }

            // Close the 200 mm height change between landing guard and stair handrail.
            CreateBeam(
                "BEACOM RAIL TRANSITION " + side,
                StairSettings.RailProfile,
                LocalPoint(
                    StairSettings.PlatformLength,
                    y,
                    StairSettings.StairHandrailHeight),
                LocalPoint(
                    StairSettings.PlatformLength,
                    y,
                    StairSettings.PlatformGuardHeight),
                "7");
        }

        private void CreateRailPost(
            double x,
            double y,
            double baseZ,
            double topZ,
            string side,
            string zone)
        {
            CreateBeam(
                "BEACOM " + zone + " POST " + side,
                StairSettings.RailProfile,
                LocalPoint(x, y, baseZ),
                LocalPoint(x, y, topZ),
                "7");
        }

        private double FlightPitchZ(double x)
        {
            double distanceAlongFlight = x - StairSettings.PlatformLength;
            double ratio = distanceAlongFlight / StairSettings.FlightRun;
            return -(ratio * StairSettings.TotalRise);
        }

        private Point LocalPoint(double x, double y, double z)
        {
            V3 result =
                _origin +
                (_xAxis * x) +
                (_yAxis * y) +
                new V3(0.0, 0.0, z);

            return result.ToPoint();
        }

        private void CreateBeam(
            string name,
            string profile,
            Point start,
            Point end,
            string modelClass)
        {
            Beam beam = new Beam(start, end)
            {
                Name = name,
                Class = modelClass
            };

            beam.Profile.ProfileString = profile;
            beam.Material.MaterialString = StairSettings.Material;
            beam.Position.Plane = Position.PlaneEnum.MIDDLE;
            beam.Position.Depth = Position.DepthEnum.MIDDLE;
            beam.Position.Rotation = Position.RotationEnum.FRONT;

            InsertOrThrow(
                beam,
                name + " [profile: " + profile + ", material: " + StairSettings.Material + "]");
        }

        private void CreatePlate(
            string name,
            string profile,
            Point p1,
            Point p2,
            Point p3,
            Point p4,
            string modelClass)
        {
            ContourPlate plate = new ContourPlate
            {
                Name = name,
                Class = modelClass
            };

            plate.Profile.ProfileString = profile;
            plate.Material.MaterialString = StairSettings.Material;
            plate.Position.Depth = Position.DepthEnum.MIDDLE;

            plate.AddContourPoint(new ContourPoint(p1, null));
            plate.AddContourPoint(new ContourPoint(p2, null));
            plate.AddContourPoint(new ContourPoint(p3, null));
            plate.AddContourPoint(new ContourPoint(p4, null));

            InsertOrThrow(
                plate,
                name + " [profile: " + profile + ", material: " + StairSettings.Material + "]");
        }

        private void InsertOrThrow(ModelObject modelObject, string description)
        {
            if (!modelObject.Insert())
                throw new InvalidOperationException("Tekla failed to insert: " + description);

            _insertedObjects.Add(modelObject);
            _insertedObjectCount++;
        }

        private void DeleteInsertedObjects()
        {
            for (int i = _insertedObjects.Count - 1; i >= 0; i--)
            {
                try
                {
                    _insertedObjects[i].Delete();
                }
                catch
                {
                    // Keep cleaning up the remaining objects.
                }
            }

            _model.CommitChanges();
            _insertedObjects.Clear();
            _insertedObjectCount = 0;
        }

        private struct V3
        {
            public V3(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public double X { get; }
            public double Y { get; }
            public double Z { get; }

            public double Length
            {
                get { return Math.Sqrt((X * X) + (Y * Y) + (Z * Z)); }
            }

            public V3 Normalised()
            {
                double length = Length;

                if (length < 0.000001)
                    throw new InvalidOperationException("Cannot normalise a zero-length direction.");

                return this * (1.0 / length);
            }

            public Point ToPoint()
            {
                return new Point(X, Y, Z);
            }

            public static V3 FromPoint(Point point)
            {
                return new V3(point.X, point.Y, point.Z);
            }

            public static double Dot(V3 a, V3 b)
            {
                return (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);
            }

            public static V3 operator +(V3 a, V3 b)
            {
                return new V3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
            }

            public static V3 operator -(V3 a, V3 b)
            {
                return new V3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
            }

            public static V3 operator *(V3 value, double scale)
            {
                return new V3(value.X * scale, value.Y * scale, value.Z * scale);
            }
        }
    }
}
