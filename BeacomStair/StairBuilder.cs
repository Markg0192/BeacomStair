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

        // Main steelwork.
        // The PFC reference lines represent the two inward web faces.
        // This gives a genuine 900 mm clear tread width between stringers.
        public const double TreadWidth = 900.0;
        public const double StringerSpacing = 900.0;
        public const double StringerDepth = 150.0;
        public const string StringerProfile = "PFC-150*75*18";
        public const string TreadProfile = "PL10";
        public const string RiserProfile = "PL6";
        public const string TreadSidePlateProfile = "PL8";
        public const string EndPlateProfile = "PL10";
        public const string Material = "S355JR";

        // Tread detail: horizontal PL10 tread with a vertical plate at each side.
        public const double TreadSidePlateLength = 200.0;
        public const double TreadSidePlateHeight = 100.0;
        public const double TreadSidePlateThickness = 8.0;
        public const double TreadPlateThickness = 10.0;
        public const double TreadBoltSize = 12.0;
        public const double TreadBoltSpacing = 100.0;

        // Top and bottom connections.
        public const double WallPlateWidth = 180.0;
        public const double WallPlateHeight = 220.0;
        public const double BasePlateLength = 220.0;
        public const double BasePlateWidth = 180.0;
        public const double AnchorBoltSize = 16.0;
        public const double AnchorBoltSpacing = 100.0;
        public const string BoltStandard = "8.8XOX";

        // Welds.
        public const double FilletWeldSize = 6.0;

        // Guarding.
        public const double HandrailOutsideDiameter = 42.4;
        public const double StairHandrailHeight = 900.0;
        public const double PlatformGuardHeight = 1100.0;
        public const double MidRailHeight = 550.0;
        public const string RailProfile = "CHS42.4*3.2";

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
        private enum BoltPlaneKind
        {
            Horizontal,
            StairSide,
            Wall
        }

        private sealed class PlatformParts
        {
            public ContourPlate Deck { get; set; }
            public Beam LeftStringer { get; set; }
            public Beam RightStringer { get; set; }
        }

        private sealed class FlightParts
        {
            public Beam LeftStringer { get; set; }
            public Beam RightStringer { get; set; }
            public List<ContourPlate> Treads { get; } = new List<ContourPlate>();
        }

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

        public StairBuildResult Build(Point wallGroundPoint, Point towardFootPoint)
        {
            if (wallGroundPoint == null)
                throw new ArgumentNullException(nameof(wallGroundPoint));

            if (towardFootPoint == null)
                throw new ArgumentNullException(nameof(towardFootPoint));

            CreateCoordinateSystem(wallGroundPoint, towardFootPoint);

            try
            {
                PlatformParts platform = CreatePlatform();
                FlightParts flight = CreateFlight(platform);

                CreateEndConnections(platform, flight);
                CreateGuarding(platform, flight);

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

        private void CreateCoordinateSystem(Point wallGroundPoint, Point towardFootPoint)
        {
            V3 start = V3.FromPoint(wallGroundPoint);
            V3 direction = V3.FromPoint(towardFootPoint) - start;

            // The first pick is on the ground at the wall, directly below the top landing.
            // The second pick only defines the horizontal direction out toward the stair foot.
            _origin = start;
            direction = new V3(direction.X, direction.Y, 0.0);

            if (direction.Length < 1.0)
            {
                throw new InvalidOperationException(
                    "The direction point is too close to the wall point. " +
                    "Pick the second point clearly out in the direction of the stair foot.");
            }

            _xAxis = direction.Normalised();
            _yAxis = new V3(-_xAxis.Y, _xAxis.X, 0.0);
        }

        private PlatformParts CreatePlatform()
        {
            double halfWidth = StairSettings.TreadWidth / 2.0;
            double halfStringerSpacing = StairSettings.StringerSpacing / 2.0;
            double platformZ = StairSettings.TotalRise;
            double stringerTopZ = platformZ - (StairSettings.TreadPlateThickness / 2.0);

            ContourPlate deck = CreatePlate(
                "BEACOM PLATFORM",
                StairSettings.TreadProfile,
                LocalPoint(0.0, -halfWidth, platformZ),
                LocalPoint(StairSettings.PlatformLength, -halfWidth, platformZ),
                LocalPoint(StairSettings.PlatformLength, halfWidth, platformZ),
                LocalPoint(0.0, halfWidth, platformZ),
                "3");

            // PFC flat webs face inward toward the stair and the open channel/toes
            // face outward. Left/right use opposite start/end directions because
            // Tekla defines channel handedness from start handle to end handle.
            Beam leftStringer = CreatePfcBeam(
                "BEACOM PLATFORM STRINGER L",
                LocalPoint(0.0, -halfStringerSpacing, stringerTopZ),
                LocalPoint(StairSettings.PlatformLength, -halfStringerSpacing, stringerTopZ),
                "2",
                false);

            Beam rightStringer = CreatePfcBeam(
                "BEACOM PLATFORM STRINGER R",
                LocalPoint(0.0, halfStringerSpacing, stringerTopZ),
                LocalPoint(StairSettings.PlatformLength, halfStringerSpacing, stringerTopZ),
                "2",
                true);

            CreateFilletWeld(leftStringer, deck, "PLATFORM DECK TO LEFT PFC");
            CreateFilletWeld(rightStringer, deck, "PLATFORM DECK TO RIGHT PFC");

            return new PlatformParts
            {
                Deck = deck,
                LeftStringer = leftStringer,
                RightStringer = rightStringer
            };
        }

        private FlightParts CreateFlight(PlatformParts platform)
        {
            double rise = StairSettings.Rise;
            double halfWidth = StairSettings.TreadWidth / 2.0;
            double halfStringerSpacing = StairSettings.StringerSpacing / 2.0;

            // The stringer reference line follows the tread-pitch line from the
            // platform edge to the front of the bottom tread. Depth=BEHIND keeps
            // the PFC below that line instead of centring it through the treads.
            Beam leftStringer = CreatePfcBeam(
                "BEACOM FLIGHT STRINGER L",
                LocalPoint(StairSettings.PlatformLength, -halfStringerSpacing, StairSettings.TotalRise),
                LocalPoint(StairSettings.OverallLength, -halfStringerSpacing, StairSettings.StringerDepth),
                "2",
                false);

            Beam rightStringer = CreatePfcBeam(
                "BEACOM FLIGHT STRINGER R",
                LocalPoint(StairSettings.PlatformLength, halfStringerSpacing, StairSettings.TotalRise),
                LocalPoint(StairSettings.OverallLength, halfStringerSpacing, StairSettings.StringerDepth),
                "2",
                true);

            CreateFilletWeld(platform.LeftStringer, leftStringer, "LEFT PLATFORM PFC TO FLIGHT PFC");
            CreateFilletWeld(platform.RightStringer, rightStringer, "RIGHT PLATFORM PFC TO FLIGHT PFC");

            FlightParts result = new FlightParts
            {
                LeftStringer = leftStringer,
                RightStringer = rightStringer
            };

            for (int i = 0; i < StairSettings.TreadCount; i++)
            {
                double x1 = StairSettings.PlatformLength + (i * StairSettings.Going);
                double x2 = x1 + StairSettings.Going;
                double z = StairSettings.TotalRise - ((i + 1) * rise);

                ContourPlate tread = CreatePlate(
                    "BEACOM TREAD " + (i + 1),
                    StairSettings.TreadProfile,
                    LocalPoint(x1, -halfWidth, z),
                    LocalPoint(x2, -halfWidth, z),
                    LocalPoint(x2, halfWidth, z),
                    LocalPoint(x1, halfWidth, z),
                    "4");

                result.Treads.Add(tread);

                CreateTreadSidePlateAndBolts(
                    tread,
                    leftStringer,
                    x1,
                    z,
                    -halfStringerSpacing,
                    true,
                    i + 1);

                CreateTreadSidePlateAndBolts(
                    tread,
                    rightStringer,
                    x1,
                    z,
                    halfStringerSpacing,
                    false,
                    i + 1);
            }

            CreateRisers(platform, result);

            return result;
        }

        private void CreateTreadSidePlateAndBolts(
            ContourPlate tread,
            Beam stringer,
            double treadStartX,
            double treadZ,
            double stringerY,
            bool leftSide,
            int treadNumber)
        {
            // A conventional tread end plate: vertical plate below the tread edge,
            // welded to the PL10 tread and bolted through the PFC web.
            double sidePlateX1 =
                treadStartX +
                ((StairSettings.Going - StairSettings.TreadSidePlateLength) / 2.0);

            double sidePlateX2 =
                sidePlateX1 + StairSettings.TreadSidePlateLength;

            double sidePlateTopZ =
                treadZ - (StairSettings.TreadPlateThickness / 2.0);

            double sidePlateBottomZ =
                sidePlateTopZ - StairSettings.TreadSidePlateHeight;

            // Centre the PL8 plate just inside the stringer reference plane so its
            // outer face sits on the PFC web plane.
            double sidePlateY = leftSide
                ? stringerY + (StairSettings.TreadSidePlateThickness / 2.0)
                : stringerY - (StairSettings.TreadSidePlateThickness / 2.0);

            ContourPlate sidePlate = CreatePlate(
                "BEACOM TREAD SIDE PLATE " + treadNumber + (leftSide ? " L" : " R"),
                StairSettings.TreadSidePlateProfile,
                LocalPoint(sidePlateX1, sidePlateY, sidePlateTopZ),
                LocalPoint(sidePlateX2, sidePlateY, sidePlateTopZ),
                LocalPoint(sidePlateX2, sidePlateY, sidePlateBottomZ),
                LocalPoint(sidePlateX1, sidePlateY, sidePlateBottomZ),
                "6");

            CreateFilletWeld(
                tread,
                sidePlate,
                "TREAD " + treadNumber +
                (leftSide ? " LEFT" : " RIGHT") +
                " SIDE PLATE TO TREAD");

            double boltZ = sidePlateTopZ - (StairSettings.TreadSidePlateHeight / 2.0);
            double firstBoltX =
                treadStartX +
                ((StairSettings.Going - StairSettings.TreadBoltSpacing) / 2.0);

            double secondBoltX = firstBoltX + StairSettings.TreadBoltSpacing;

            CreateTwoBoltArray(
                sidePlate,
                stringer,
                LocalPoint(firstBoltX, sidePlateY, boltZ),
                LocalPoint(secondBoltX, sidePlateY, boltZ),
                StairSettings.TreadBoltSize,
                false,
                BoltPlaneKind.StairSide,
                "TREAD " + treadNumber +
                (leftSide ? " LEFT" : " RIGHT") +
                " SIDE PLATE - 2 M12 BOLTS TO PFC");
        }

        private void CreateRisers(PlatformParts platform, FlightParts flight)
        {
            double rise = StairSettings.Rise;
            double halfWidth = StairSettings.TreadWidth / 2.0;

            for (int i = 0; i < StairSettings.RiseCount; i++)
            {
                double x = StairSettings.PlatformLength + (i * StairSettings.Going);
                double zTop = StairSettings.TotalRise - (i * rise);
                double zBottom = StairSettings.TotalRise - ((i + 1) * rise);

                if (i == StairSettings.RiseCount - 1)
                    x = StairSettings.OverallLength;

                ContourPlate riser = CreatePlate(
                    "BEACOM RISER " + (i + 1),
                    StairSettings.RiserProfile,
                    LocalPoint(x, -halfWidth, zTop),
                    LocalPoint(x, halfWidth, zTop),
                    LocalPoint(x, halfWidth, zBottom),
                    LocalPoint(x, -halfWidth, zBottom),
                    "5");

                // Upper walking surface.
                Part upperPart = i == 0
                    ? (Part)platform.Deck
                    : flight.Treads[i - 1];

                CreateFilletWeld(upperPart, riser, "RISER " + (i + 1) + " TO UPPER PLATE");

                // Lower walking surface, except for the final riser down to ground.
                if (i < StairSettings.TreadCount)
                {
                    CreateFilletWeld(
                        flight.Treads[i],
                        riser,
                        "RISER " + (i + 1) + " TO LOWER TREAD");
                }
            }
        }

        private void CreateEndConnections(PlatformParts platform, FlightParts flight)
        {
            double halfStringerSpacing = StairSettings.StringerSpacing / 2.0;

            CreateWallConnection(platform.LeftStringer, -halfStringerSpacing, "L");
            CreateWallConnection(platform.RightStringer, halfStringerSpacing, "R");

            CreateBaseConnection(flight.LeftStringer, -halfStringerSpacing, "L");
            CreateBaseConnection(flight.RightStringer, halfStringerSpacing, "R");
        }

        private void CreateWallConnection(Beam stringer, double y, string side)
        {
            double centreZ = StairSettings.TotalRise - (StairSettings.StringerDepth / 2.0);
            double halfWidth = StairSettings.WallPlateWidth / 2.0;
            double halfHeight = StairSettings.WallPlateHeight / 2.0;

            ContourPlate wallPlate = CreatePlate(
                "BEACOM WALL END PLATE " + side,
                StairSettings.EndPlateProfile,
                LocalPoint(0.0, y - halfWidth, centreZ - halfHeight),
                LocalPoint(0.0, y + halfWidth, centreZ - halfHeight),
                LocalPoint(0.0, y + halfWidth, centreZ + halfHeight),
                LocalPoint(0.0, y - halfWidth, centreZ + halfHeight),
                "8");

            CreateFilletWeld(stringer, wallPlate, "WALL END PLATE " + side + " TO PFC");

            Point lowerBolt = LocalPoint(0.0, y, centreZ - (StairSettings.AnchorBoltSpacing / 2.0));
            Point upperBolt = LocalPoint(0.0, y, centreZ + (StairSettings.AnchorBoltSpacing / 2.0));

            // There is no concrete wall object selected by this tool. Self-referencing
            // the plate lets Tekla create the two site bolt objects as the anchor
            // representation through the plate.
            CreateTwoBoltArray(
                wallPlate,
                wallPlate,
                lowerBolt,
                upperBolt,
                StairSettings.AnchorBoltSize,
                true,
                BoltPlaneKind.Wall,
                "WALL END PLATE " + side + " - 2 M16 ANCHORS");
        }

        private void CreateBaseConnection(Beam stringer, double y, string side)
        {
            double halfLength = StairSettings.BasePlateLength / 2.0;
            double halfWidth = StairSettings.BasePlateWidth / 2.0;
            double x = StairSettings.OverallLength;

            ContourPlate basePlate = CreatePlate(
                "BEACOM BASE PLATE " + side,
                StairSettings.EndPlateProfile,
                LocalPoint(x - halfLength, y - halfWidth, 0.0),
                LocalPoint(x + halfLength, y - halfWidth, 0.0),
                LocalPoint(x + halfLength, y + halfWidth, 0.0),
                LocalPoint(x - halfLength, y + halfWidth, 0.0),
                "8");

            CreateFilletWeld(stringer, basePlate, "BASE PLATE " + side + " TO PFC");

            Point firstBolt = LocalPoint(
                x - (StairSettings.AnchorBoltSpacing / 2.0),
                y,
                0.0);

            Point secondBolt = LocalPoint(
                x + (StairSettings.AnchorBoltSpacing / 2.0),
                y,
                0.0);

            CreateTwoBoltArray(
                basePlate,
                basePlate,
                firstBolt,
                secondBolt,
                StairSettings.AnchorBoltSize,
                true,
                BoltPlaneKind.Horizontal,
                "BASE PLATE " + side + " - 2 M16 ANCHORS");
        }

        private void CreateGuarding(PlatformParts platform, FlightParts flight)
        {
            double railOffset = StairSettings.RailCentreOffset;

            CreateSideGuard(
                -railOffset,
                "L",
                platform.Deck,
                flight.LeftStringer);

            CreateSideGuard(
                railOffset,
                "R",
                platform.Deck,
                flight.RightStringer);
        }

        private void CreateSideGuard(
            double y,
            string side,
            ContourPlate platformDeck,
            Beam flightStringer)
        {
            double platformZ = StairSettings.TotalRise;

            Beam platformTopRail = CreateBeam(
                "BEACOM PLATFORM TOP RAIL " + side,
                StairSettings.RailProfile,
                LocalPoint(0.0, y, platformZ + StairSettings.PlatformGuardHeight),
                LocalPoint(StairSettings.PlatformLength, y, platformZ + StairSettings.PlatformGuardHeight),
                "7");

            Beam platformMidRail = CreateBeam(
                "BEACOM PLATFORM MID RAIL " + side,
                StairSettings.RailProfile,
                LocalPoint(0.0, y, platformZ + StairSettings.MidRailHeight),
                LocalPoint(StairSettings.PlatformLength, y, platformZ + StairSettings.MidRailHeight),
                "7");

            List<Beam> platformPosts = new List<Beam>
            {
                CreateRailPost(0.0, y, platformZ, platformZ + StairSettings.PlatformGuardHeight, side, "PLATFORM"),
                CreateRailPost(500.0, y, platformZ, platformZ + StairSettings.PlatformGuardHeight, side, "PLATFORM"),
                CreateRailPost(StairSettings.PlatformLength, y, platformZ, platformZ + StairSettings.PlatformGuardHeight, side, "PLATFORM")
            };

            foreach (Beam post in platformPosts)
            {
                CreateFilletWeld(platformDeck, post, "PLATFORM POST " + side + " TO DECK");
                CreateFilletWeld(post, platformTopRail, "PLATFORM POST " + side + " TO TOP RAIL");
                CreateFilletWeld(post, platformMidRail, "PLATFORM POST " + side + " TO MID RAIL");
            }

            Beam stairTopRail = CreateBeam(
                "BEACOM STAIR TOP RAIL " + side,
                StairSettings.RailProfile,
                LocalPoint(
                    StairSettings.PlatformLength,
                    y,
                    StairSettings.TotalRise + StairSettings.StairHandrailHeight),
                LocalPoint(
                    StairSettings.OverallLength,
                    y,
                    StairSettings.Rise + StairSettings.StairHandrailHeight),
                "7");

            Beam stairMidRail = CreateBeam(
                "BEACOM STAIR MID RAIL " + side,
                StairSettings.RailProfile,
                LocalPoint(
                    StairSettings.PlatformLength,
                    y,
                    StairSettings.TotalRise + StairSettings.MidRailHeight),
                LocalPoint(
                    StairSettings.OverallLength,
                    y,
                    StairSettings.Rise + StairSettings.MidRailHeight),
                "7");

            for (double x = StairSettings.PlatformLength;
                 x <= StairSettings.OverallLength + 0.1;
                 x += 500.0)
            {
                double baseZ = FlightStringerTopZ(x);
                double topZ = FlightPitchZ(x) + StairSettings.StairHandrailHeight;

                Beam post = CreateRailPost(
                    x,
                    y,
                    baseZ,
                    topZ,
                    side,
                    "STAIR");

                CreateFilletWeld(flightStringer, post, "STAIR POST " + side + " TO PFC");
                CreateFilletWeld(post, stairTopRail, "STAIR POST " + side + " TO TOP RAIL");
                CreateFilletWeld(post, stairMidRail, "STAIR POST " + side + " TO MID RAIL");
            }

            Beam transition = CreateBeam(
                "BEACOM RAIL TRANSITION " + side,
                StairSettings.RailProfile,
                LocalPoint(
                    StairSettings.PlatformLength,
                    y,
                    StairSettings.TotalRise + StairSettings.StairHandrailHeight),
                LocalPoint(
                    StairSettings.PlatformLength,
                    y,
                    StairSettings.TotalRise + StairSettings.PlatformGuardHeight),
                "7");

            CreateFilletWeld(platformTopRail, transition, "PLATFORM/STAIR RAIL TRANSITION " + side);
            CreateFilletWeld(stairTopRail, transition, "STAIR RAIL/TRANSITION " + side);
        }

        private Beam CreateRailPost(
            double x,
            double y,
            double baseZ,
            double topZ,
            string side,
            string zone)
        {
            return CreateBeam(
                "BEACOM " + zone + " POST " + side,
                StairSettings.RailProfile,
                LocalPoint(x, y, baseZ),
                LocalPoint(x, y, topZ),
                "7");
        }

        private double FlightPitchZ(double x)
        {
            double distance = x - StairSettings.PlatformLength;
            double ratio = distance / StairSettings.FlightRun;

            return StairSettings.TotalRise -
                   (ratio * (StairSettings.TotalRise - StairSettings.Rise));
        }

        private double FlightStringerTopZ(double x)
        {
            double distance = x - StairSettings.PlatformLength;
            double ratio = distance / StairSettings.FlightRun;

            return StairSettings.TotalRise -
                   (ratio * (StairSettings.TotalRise - StairSettings.StringerDepth));
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

        private Beam CreatePfcBeam(
            string name,
            Point nominalStart,
            Point nominalEnd,
            string modelClass,
            bool reverseDirection)
        {
            Point start = reverseDirection ? nominalEnd : nominalStart;
            Point end = reverseDirection ? nominalStart : nominalEnd;

            Beam beam = new Beam(start, end)
            {
                Name = name,
                Class = modelClass
            };

            beam.Profile.ProfileString = StairSettings.StringerProfile;
            beam.Material.MaterialString = StairSettings.Material;
            // TOP keeps the channel upright.
            //
            // Both PFCs are deliberately modelled in opposite directions. With that
            // handedness, RIGHT puts the entire section outside the reference line,
            // so the reference line becomes the inward web face on both sides.
            //
            // BEHIND keeps the section below the tread/pitch reference line.
            beam.Position.Plane = Position.PlaneEnum.RIGHT;
            beam.Position.Depth = Position.DepthEnum.BEHIND;
            beam.Position.Rotation = Position.RotationEnum.TOP;

            InsertOrThrow(
                beam,
                name + " [profile: " + StairSettings.StringerProfile +
                ", plane: RIGHT, rotation: TOP, depth: BEHIND]");

            return beam;
        }

        private Beam CreateBeam(
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
            beam.Position.Rotation = Position.RotationEnum.TOP;

            InsertOrThrow(
                beam,
                name + " [profile: " + profile + ", material: " + StairSettings.Material + "]");

            return beam;
        }

        private ContourPlate CreatePlate(
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

            return plate;
        }

        private void CreateTwoBoltArray(
            Part partToBeBolted,
            Part partToBoltTo,
            Point firstPosition,
            Point secondPosition,
            double boltSize,
            bool siteBolt,
            BoltPlaneKind boltPlaneKind,
            string description)
        {
            WorkPlaneHandler workPlaneHandler = _model.GetWorkPlaneHandler();
            TransformationPlane originalPlane =
                workPlaneHandler.GetCurrentTransformationPlane();

            try
            {
                // The input points and stair axes are expressed in the user's current
                // work plane. Convert them to global coordinates before creating the
                // temporary work plane used by the bolt group.
                Point globalFirst =
                    originalPlane.TransformationMatrixToGlobal.Transform(firstPosition);

                Point globalSecond =
                    originalPlane.TransformationMatrixToGlobal.Transform(secondPosition);

                Vector globalXAxis = ToGlobalVector(_xAxis, originalPlane);
                Vector globalYAxis = ToGlobalVector(_yAxis, originalPlane);
                Vector globalZAxis = ToGlobalVector(new V3(0.0, 0.0, 1.0), originalPlane);

                Vector planeAxisX;
                Vector planeAxisY;

                switch (boltPlaneKind)
                {
                    case BoltPlaneKind.StairSide:
                        // X-Z plane. Bolt axis is across the stair through the PFC web.
                        planeAxisX = globalXAxis;
                        planeAxisY = globalZAxis;
                        break;

                    case BoltPlaneKind.Wall:
                        // Y-Z wall plane. Bolt axis runs into the wall.
                        planeAxisX = globalYAxis;
                        planeAxisY = globalZAxis;
                        break;

                    default:
                        // Horizontal X-Y plane. Bolt axis is vertical into the floor.
                        planeAxisX = globalXAxis;
                        planeAxisY = globalYAxis;
                        break;
                }

                TransformationPlane boltPlane =
                    new TransformationPlane(globalFirst, planeAxisX, planeAxisY);

                workPlaneHandler.SetCurrentTransformationPlane(boltPlane);

                Point localFirst =
                    boltPlane.TransformationMatrixToLocal.Transform(globalFirst);

                Point localSecond =
                    boltPlane.TransformationMatrixToLocal.Transform(globalSecond);

                BoltArray bolts = new BoltArray
                {
                    PartToBeBolted = partToBeBolted,
                    PartToBoltTo = partToBoltTo,
                    FirstPosition = localFirst,
                    SecondPosition = localSecond,
                    BoltSize = boltSize,
                    Tolerance = 2.0,
                    BoltStandard = StairSettings.BoltStandard,
                    BoltType = siteBolt
                        ? BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE
                        : BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP,
                    CutLength = 100.0,
                    ExtraLength = 10.0,
                    ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_NO,
                    Bolt = true,
                    Washer1 = true,
                    Washer2 = false,
                    Washer3 = false,
                    Nut1 = true,
                    Nut2 = false
                };

                bolts.Position.Depth = Position.DepthEnum.MIDDLE;
                bolts.Position.Plane = Position.PlaneEnum.MIDDLE;
                bolts.Position.Rotation = Position.RotationEnum.FRONT;

                bolts.AddBoltDistX(Distance(localFirst, localSecond));
                bolts.AddBoltDistY(0.0);

                InsertOrThrow(
                    bolts,
                    description + " [bolt: M" + boltSize.ToString("0") +
                    ", standard: " + StairSettings.BoltStandard + "]");
            }
            finally
            {
                workPlaneHandler.SetCurrentTransformationPlane(originalPlane);
            }
        }

        private static Vector ToGlobalVector(
            V3 vector,
            TransformationPlane sourcePlane)
        {
            Point localOrigin = new Point(0.0, 0.0, 0.0);
            Point localVectorEnd = new Point(vector.X, vector.Y, vector.Z);

            Point globalOrigin =
                sourcePlane.TransformationMatrixToGlobal.Transform(localOrigin);

            Point globalVectorEnd =
                sourcePlane.TransformationMatrixToGlobal.Transform(localVectorEnd);

            return new Vector(
                globalVectorEnd.X - globalOrigin.X,
                globalVectorEnd.Y - globalOrigin.Y,
                globalVectorEnd.Z - globalOrigin.Z);
        }

        private void CreateFilletWeld(Part mainPart, Part secondaryPart, string description)
        {
            Weld weld = new Weld
            {
                MainObject = mainPart,
                SecondaryObject = secondaryPart,
                TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET,
                SizeAbove = StairSettings.FilletWeldSize,
                ShopWeld = true,
                AroundWeld = false
            };

            InsertOrThrow(
                weld,
                description + " [6 mm fillet weld]");
        }

        private static double Distance(Point a, Point b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double dz = b.Z - a.Z;

            return Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
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
