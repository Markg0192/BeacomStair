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
        public const double StringerDepth = 180.0;
        public const string StringerProfile = "PFC-180*75*20";
        public const string TreadProfile = "PL10";
        public const string KickerProfile = "PL6";
        public const string TreadSidePlateProfile = "PL8";
        public const string EndPlateProfile = "PL10";
        public const string Material = "S355JR";

        // Tread detail: horizontal PL10 tread with a vertical plate at each side.
        public const double TreadSidePlateLength = 250.0;
        public const double TreadSidePlateHeight = 70.0;
        public const double TreadSidePlateThickness = 8.0;
        public const double TreadPlateThickness = 10.0;
        public const double TreadBoltSize = 12.0;
        public const double TreadBoltSpacing = 125.0;
        public const double TreadFirstBoltFromLeadingEdge = 30.0;
        public const double TreadBoltDownFromTop = 58.0;
        public const double KickerUpstand = 50.0;
        public const double KickerBelowTread = 10.0;
        public const double KickerThickness = 6.0;

        // Top and bottom connections.
        public const double WallPlateWidth = 180.0;
        public const double WallPlateHeight = 220.0;
        public const double BasePlateLength = 300.0;
        public const double BasePlateWidth = 180.0;
        public const double BasePlateThickness = 10.0;
        public const double StringerJointPlateThickness = 10.0;
        public const double TopJointPlateLength = 180.0;
        public const double TopJointPlateDepth = 160.0;
        public const double BottomJointPlateLength = 120.0;
        public const double BottomJointPlateDepth = 120.0;
        public static double BottomStringerJointHeight
        {
            get
            {
                // Keep the PFC reference line exactly parallel to the stair pitch.
                // Top reference is 10 mm below the 3170 finished landing surface.
                return (TotalRise - TreadPlateThickness) -
                       (FlightRun * (Rise / Going));
            }
        }
        public const double AnchorBoltSize = 16.0;
        public const double WallAnchorBoltSpacing = 100.0;
        public const double BaseAnchorBoltSpacing = 220.0;
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
            public Beam LeftBottomPost { get; set; }
            public Beam RightBottomPost { get; set; }
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
            double platformTopZ = StairSettings.TotalRise;
            double platformPlateZ =
                platformTopZ - (StairSettings.TreadPlateThickness / 2.0);

            double stringerTopZ =
                platformTopZ - StairSettings.TreadPlateThickness;

            ContourPlate deck = CreatePlate(
                "BEACOM PLATFORM",
                StairSettings.TreadProfile,
                LocalPoint(0.0, -halfWidth, platformPlateZ),
                LocalPoint(StairSettings.PlatformLength, -halfWidth, platformPlateZ),
                LocalPoint(StairSettings.PlatformLength, halfWidth, platformPlateZ),
                LocalPoint(0.0, halfWidth, platformPlateZ),
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
            double flightTopZ =
                StairSettings.TotalRise - StairSettings.TreadPlateThickness;

            Beam leftStringer = CreatePfcBeam(
                "BEACOM FLIGHT STRINGER L",
                LocalPoint(StairSettings.PlatformLength, -halfStringerSpacing, flightTopZ),
                LocalPoint(
                    StairSettings.OverallLength,
                    -halfStringerSpacing,
                    StairSettings.BottomStringerJointHeight),
                "2",
                false);

            Beam rightStringer = CreatePfcBeam(
                "BEACOM FLIGHT STRINGER R",
                LocalPoint(StairSettings.PlatformLength, halfStringerSpacing, flightTopZ),
                LocalPoint(
                    StairSettings.OverallLength,
                    halfStringerSpacing,
                    StairSettings.BottomStringerJointHeight),
                "2",
                true);

            double basePlateTopZ = StairSettings.BasePlateThickness;

            Beam leftBottomPost = CreateVerticalPfcBeam(
                "BEACOM BOTTOM VERTICAL PFC L",
                StairSettings.OverallLength,
                -halfStringerSpacing,
                basePlateTopZ,
                StairSettings.BottomStringerJointHeight,
                true);

            Beam rightBottomPost = CreateVerticalPfcBeam(
                "BEACOM BOTTOM VERTICAL PFC R",
                StairSettings.OverallLength,
                halfStringerSpacing,
                basePlateTopZ,
                StairSettings.BottomStringerJointHeight,
                false);

            CreateTopStringerJointPlate(
                platform.LeftStringer,
                leftStringer,
                -halfStringerSpacing,
                true,
                "L");

            CreateTopStringerJointPlate(
                platform.RightStringer,
                rightStringer,
                halfStringerSpacing,
                false,
                "R");

            CreateBottomStringerJointPlate(
                leftStringer,
                leftBottomPost,
                -halfStringerSpacing,
                true,
                "L");

            CreateBottomStringerJointPlate(
                rightStringer,
                rightBottomPost,
                halfStringerSpacing,
                false,
                "R");

            FlightParts result = new FlightParts
            {
                LeftStringer = leftStringer,
                RightStringer = rightStringer,
                LeftBottomPost = leftBottomPost,
                RightBottomPost = rightBottomPost
            };

            for (int i = 0; i < StairSettings.TreadCount; i++)
            {
                double x1 = StairSettings.PlatformLength + (i * StairSettings.Going);
                double x2 = x1 + StairSettings.Going;
                double treadTopZ =
                    StairSettings.TotalRise - ((i + 1) * rise);

                double treadPlateZ =
                    treadTopZ - (StairSettings.TreadPlateThickness / 2.0);

                ContourPlate tread = CreatePlate(
                    "BEACOM TREAD " + (i + 1),
                    StairSettings.TreadProfile,
                    LocalPoint(x1, -halfWidth, treadPlateZ),
                    LocalPoint(x2, -halfWidth, treadPlateZ),
                    LocalPoint(x2, halfWidth, treadPlateZ),
                    LocalPoint(x1, halfWidth, treadPlateZ),
                    "4");

                result.Treads.Add(tread);

                CreateTreadSidePlateAndBolts(
                    tread,
                    leftStringer,
                    x1,
                    treadTopZ,
                    -halfStringerSpacing,
                    true,
                    i + 1);

                CreateTreadSidePlateAndBolts(
                    tread,
                    rightStringer,
                    x1,
                    treadTopZ,
                    halfStringerSpacing,
                    false,
                    i + 1);
            }

            CreateKickers(result);

            return result;
        }

        private void CreateTreadSidePlateAndBolts(
            ContourPlate tread,
            Beam stringer,
            double treadStartX,
            double treadTopZ,
            double stringerY,
            bool leftSide,
            int treadNumber)
        {
            // Full-depth tread end plate:
            // PL10 horizontal tread above a shallow PL8 vertical end plate.
            // The end plate follows the full 250 mm going and bolts to the PFC web.
            double sidePlateX1 = treadStartX;
            double sidePlateX2 = treadStartX + StairSettings.Going;

            double sidePlateTopZ =
                treadTopZ - StairSettings.TreadPlateThickness;

            double sidePlateBottomZ =
                sidePlateTopZ - StairSettings.TreadSidePlateHeight;

            double sidePlateY = leftSide
                ? stringerY + (StairSettings.TreadSidePlateThickness / 2.0)
                : stringerY - (StairSettings.TreadSidePlateThickness / 2.0);

            ContourPlate sidePlate = CreatePlate(
                "BEACOM TREAD END PLATE " + treadNumber + (leftSide ? " L" : " R"),
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
                " END PLATE TO TREAD");

            // Leading edge is the downhill/front edge of the tread (x2).
            // Keep the familiar plate-tread arrangement:
            // first bolt 30 mm back from the leading edge, second 125 mm behind it.
            double firstBoltX =
                sidePlateX2 - StairSettings.TreadFirstBoltFromLeadingEdge;

            double secondBoltX =
                firstBoltX - StairSettings.TreadBoltSpacing;

            double boltZ =
                treadTopZ - StairSettings.TreadBoltDownFromTop;

            CreateTwoBoltArray(
                sidePlate,
                stringer,
                LocalPoint(firstBoltX, sidePlateY, boltZ),
                LocalPoint(secondBoltX, sidePlateY, boltZ),
                StairSettings.TreadBoltSize,
                BoltPlaneKind.StairSide,
                "TREAD " + treadNumber +
                (leftSide ? " LEFT" : " RIGHT") +
                " END PLATE - 2 M12 BOLTS TO PFC");
        }

        private void CreateKickers(FlightParts flight)
        {
            double halfWidth = StairSettings.TreadWidth / 2.0;

            for (int i = 0; i < StairSettings.TreadCount; i++)
            {
                double treadBackEdgeX =
                    StairSettings.PlatformLength + (i * StairSettings.Going);

                // PL6 contour plates are centred on their contour plane.
                // Move the plane half the plate thickness outside the tread edge
                // so the kicker face is flush with the tread edge rather than
                // overlapping the PL10 tread.
                double kickerX =
                    treadBackEdgeX - (StairSettings.KickerThickness / 2.0);

                double treadTopZ =
                    StairSettings.TotalRise - ((i + 1) * StairSettings.Rise);

                double treadBottomZ =
                    treadTopZ - StairSettings.TreadPlateThickness;

                double kickerTopZ =
                    treadTopZ + StairSettings.KickerUpstand;

                double kickerBottomZ =
                    treadBottomZ - StairSettings.KickerBelowTread;

                ContourPlate kicker = CreatePlate(
                    "BEACOM TREAD KICKER " + (i + 1),
                    StairSettings.KickerProfile,
                    LocalPoint(kickerX, -halfWidth, kickerTopZ),
                    LocalPoint(kickerX, halfWidth, kickerTopZ),
                    LocalPoint(kickerX, halfWidth, kickerBottomZ),
                    LocalPoint(kickerX, -halfWidth, kickerBottomZ),
                    "5");

                CreateFilletWeld(
                    flight.Treads[i],
                    kicker,
                    "TREAD " + (i + 1) + " TO 50MM UPSTAND KICKER");
            }
        }

        private void CreateEndConnections(PlatformParts platform, FlightParts flight)
        {
            double halfStringerSpacing = StairSettings.StringerSpacing / 2.0;

            CreateWallConnection(platform.LeftStringer, -halfStringerSpacing, "L");
            CreateWallConnection(platform.RightStringer, halfStringerSpacing, "R");

            CreateBaseConnection(flight.LeftBottomPost, -halfStringerSpacing, "L");
            CreateBaseConnection(flight.RightBottomPost, halfStringerSpacing, "R");
        }

        private void CreateWallConnection(Beam stringer, double y, string side)
        {
            double centreZ = StairSettings.TotalRise - (StairSettings.StringerDepth / 2.0);
            double halfWidth = StairSettings.WallPlateWidth / 2.0;
            double halfHeight = StairSettings.WallPlateHeight / 2.0;

            ContourPlate wallPlate = CreatePlate(
                "BEACOM WALL END PLATE " + side,
                StairSettings.EndPlateProfile,
                LocalPoint(5.0, y - halfWidth, centreZ - halfHeight),
                LocalPoint(5.0, y + halfWidth, centreZ - halfHeight),
                LocalPoint(5.0, y + halfWidth, centreZ + halfHeight),
                LocalPoint(5.0, y - halfWidth, centreZ + halfHeight),
                "8");

            CreateFilletWeld(stringer, wallPlate, "WALL END PLATE " + side + " TO PFC");

            Point lowerBolt = LocalPoint(5.0, y, centreZ - (StairSettings.WallAnchorBoltSpacing / 2.0));
            Point upperBolt = LocalPoint(5.0, y, centreZ + (StairSettings.WallAnchorBoltSpacing / 2.0));

            // There is no concrete wall object selected by this tool. Self-referencing
            // the plate lets Tekla create the two site bolt objects as the anchor
            // representation through the plate.
            CreateTwoBoltArray(
                wallPlate,
                wallPlate,
                lowerBolt,
                upperBolt,
                StairSettings.AnchorBoltSize,
                BoltPlaneKind.Wall,
                "WALL END PLATE " + side + " - 2 M16 ANCHORS");
        }

        private void CreateBaseConnection(Beam stringer, double y, string side)
        {
            double halfLength = StairSettings.BasePlateLength / 2.0;
            double halfWidth = StairSettings.BasePlateWidth / 2.0;
            double x = StairSettings.OverallLength;

            double basePlateZ = StairSettings.BasePlateThickness / 2.0;

            ContourPlate basePlate = CreatePlate(
                "BEACOM BASE PLATE " + side,
                StairSettings.EndPlateProfile,
                LocalPoint(x - halfLength, y - halfWidth, basePlateZ),
                LocalPoint(x + halfLength, y - halfWidth, basePlateZ),
                LocalPoint(x + halfLength, y + halfWidth, basePlateZ),
                LocalPoint(x - halfLength, y + halfWidth, basePlateZ),
                "8");

            CreateFilletWeld(stringer, basePlate, "BASE PLATE " + side + " TO PFC");

            Point firstBolt = LocalPoint(
                x - (StairSettings.BaseAnchorBoltSpacing / 2.0),
                y,
                basePlateZ);

            Point secondBolt = LocalPoint(
                x + (StairSettings.BaseAnchorBoltSpacing / 2.0),
                y,
                basePlateZ);

            CreateTwoBoltArray(
                basePlate,
                basePlate,
                firstBolt,
                secondBolt,
                StairSettings.AnchorBoltSize,
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

            double topZ =
                StairSettings.TotalRise - StairSettings.TreadPlateThickness;

            // This line is deliberately derived from the same rise/going ratio as
            // the treads so the PFC remains parallel to every tread connection.
            return topZ -
                   (distance * (StairSettings.Rise / StairSettings.Going));
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

        private void CreateTopStringerJointPlate(
            Beam platformStringer,
            Beam flightStringer,
            double stringerY,
            bool leftSide,
            string side)
        {
            double plateY = leftSide
                ? stringerY + (StairSettings.StringerJointPlateThickness / 2.0)
                : stringerY - (StairSettings.StringerJointPlateThickness / 2.0);

            double x1 =
                StairSettings.PlatformLength -
                (StairSettings.TopJointPlateLength / 2.0);

            double x2 =
                StairSettings.PlatformLength +
                (StairSettings.TopJointPlateLength / 2.0);

            double topZ =
                StairSettings.TotalRise - StairSettings.TreadPlateThickness;

            double bottomZ = topZ - StairSettings.TopJointPlateDepth;

            ContourPlate plate = CreatePlate(
                "BEACOM TOP STRINGER JOINT PLATE " + side,
                StairSettings.EndPlateProfile,
                LocalPoint(x1, plateY, topZ),
                LocalPoint(x2, plateY, topZ),
                LocalPoint(x2, plateY, bottomZ),
                LocalPoint(x1, plateY, bottomZ),
                "8");

            CreateFilletWeld(
                platformStringer,
                plate,
                "TOP JOINT PLATE " + side + " TO PLATFORM PFC");

            CreateFilletWeld(
                flightStringer,
                plate,
                "TOP JOINT PLATE " + side + " TO FLIGHT PFC");
        }

        private void CreateBottomStringerJointPlate(
            Beam flightStringer,
            Beam verticalPfc,
            double stringerY,
            bool leftSide,
            string side)
        {
            // Small PL10 plate on the inside web face only. It bridges the last
            // piece of the sloping PFC to the short vertical PFC without covering
            // most of the bottom connection.
            double plateY = leftSide
                ? stringerY + (StairSettings.StringerJointPlateThickness / 2.0)
                : stringerY - (StairSettings.StringerJointPlateThickness / 2.0);

            double jointX = StairSettings.OverallLength;
            double jointZ = StairSettings.BottomStringerJointHeight;

            double xLeft =
                jointX - StairSettings.BottomJointPlateLength;

            double xLowerLeft =
                jointX - (StairSettings.BottomJointPlateLength * 0.40);

            double topZ =
                jointZ + (StairSettings.BottomJointPlateDepth * 0.35);

            double bottomZ =
                jointZ - (StairSettings.BottomJointPlateDepth * 0.65);

            ContourPlate plate = CreatePlate(
                "BEACOM BOTTOM STRINGER JOINT PLATE " + side,
                StairSettings.EndPlateProfile,
                LocalPoint(xLeft, plateY, topZ),
                LocalPoint(jointX, plateY, topZ),
                LocalPoint(jointX, plateY, bottomZ),
                LocalPoint(xLowerLeft, plateY, bottomZ),
                "8");

            CreateFilletWeld(
                flightStringer,
                plate,
                "BOTTOM JOINT PLATE " + side + " TO FLIGHT PFC");

            CreateFilletWeld(
                verticalPfc,
                plate,
                "BOTTOM JOINT PLATE " + side + " TO VERTICAL PFC");
        }

        private Beam CreateVerticalPfcBeam(
            string name,
            double x,
            double y,
            double bottomZ,
            double topZ,
            bool leftSide)
        {
            WorkPlaneHandler workPlaneHandler = _model.GetWorkPlaneHandler();
            TransformationPlane originalPlane =
                workPlaneHandler.GetCurrentTransformationPlane();

            try
            {
                // Create the short upright as a true Tekla COLUMN-type member.
                // This gives us the same Vertical / Rotation / Horizontal controls
                // that were proven manually in Tekla.
                Point globalOrigin =
                    originalPlane.TransformationMatrixToGlobal.Transform(
                        _origin.ToPoint());

                Vector globalXAxis =
                    ToGlobalVector(_xAxis, originalPlane);

                Vector globalYAxis =
                    ToGlobalVector(_yAxis, originalPlane);

                TransformationPlane stairPlane =
                    new TransformationPlane(
                        globalOrigin,
                        globalXAxis,
                        globalYAxis);

                Point globalBottom =
                    originalPlane.TransformationMatrixToGlobal.Transform(
                        LocalPoint(x, y, bottomZ));

                Point globalTop =
                    originalPlane.TransformationMatrixToGlobal.Transform(
                        LocalPoint(x, y, topZ));

                workPlaneHandler.SetCurrentTransformationPlane(stairPlane);

                Point localBottom =
                    stairPlane.TransformationMatrixToLocal.Transform(globalBottom);

                Point localTop =
                    stairPlane.TransformationMatrixToLocal.Transform(globalTop);

                Beam column = new Beam(Beam.BeamTypeEnum.COLUMN)
                {
                    StartPoint = localBottom,
                    EndPoint = localTop,
                    Name = name,
                    Class = "2"
                };

                column.Profile.ProfileString = StairSettings.StringerProfile;
                column.Material.MaterialString = StairSettings.Material;

                // Exact Tekla settings proven manually:
                //
                // NOTE: leftSide is the stair-local Y-side flag, not the physical
                // left/right label seen in the user's model view.
                //
                // leftSide == true  -> Down / Top / Right
                // leftSide == false -> Up / Below / Right
                //
                // For COLUMN-type members:
                // Horizontal -> Plane
                // Vertical   -> Depth (Behind = Down, Front = Up)
                column.Position.Plane = Position.PlaneEnum.RIGHT;

                if (leftSide)
                {
                    column.Position.Depth = Position.DepthEnum.BEHIND;
                    column.Position.Rotation = Position.RotationEnum.TOP;
                }
                else
                {
                    column.Position.Depth = Position.DepthEnum.FRONT;
                    column.Position.Rotation = Position.RotationEnum.BELOW;
                }

                InsertOrThrow(
                    column,
                    name +
                    (leftSide
                        ? " [COLUMN: Down / Top / Right]"
                        : " [COLUMN: Up / Below / Right]"));

                return column;
            }
            finally
            {
                workPlaneHandler.SetCurrentTransformationPlane(originalPlane);
            }
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
                    BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE,
                    CutLength = 200.0,
                    ExtraLength = 0.0,
                    ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES,
                    Bolt = true,

                    // Bolt head side: bolt only.
                    // Nut side: one washer and one nut.
                    Washer1 = false,
                    Washer2 = true,
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
                    description + " [SITE M" + boltSize.ToString("0") +
                    ", cut: 200, extra: 0, one nut-side washer + nut]");
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
