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
        public const double StringerWidth = 75.0;
        public const string StringerProfile = "PFC-180*75*20";
        public const string TreadProfile = "PL10";
        public const string KickerProfile = "PL6";
        public const string TreadSidePlateProfile = "PL8";
        public const string EndPlateProfile = "PL10";
        public const string PlatformSupportAngleProfile = "RSA50*50*6";
        public const double PlatformSupportAngleLeg = 50.0;
        public const double PlatformSupportAngleEndClearance = 10.0;
        public const string Material = "S355JR";

        // Tread detail: horizontal PL10 tread with a vertical plate at each side.
        public const double TreadSidePlateLength = 250.0;
        public const double TreadSidePlateHeight = 70.0;
        public const double TreadSidePlateThickness = 8.0;
        public const double TreadSidePlateSnipeHorizontal = 75.0;
        public const double TreadSidePlateSnipeVertical = 50.0;
        public const double TreadPlateThickness = 10.0;
        public const double TreadBoltSize = 12.0;
        public const double TreadBoltSpacing = 125.0;
        public const double TreadFirstBoltFromLeadingEdge = 30.0;
        public const double TreadBoltDownFromTop = 58.0;
        public const double KickerUpstand = 50.0;
        public const double KickerBelowTread = 10.0;
        public const double KickerThickness = 6.0;

        // Top and bottom connections.
        public const double WallPlateWidth = 70.0;
        public const double WallPlateHeight = 170.0;
        // Base plate is deliberately kept fully inside the PFC footprint.
        public const double BasePlateLength = 170.0;
        public const double BasePlateInsideProjection = 80.0;
        public const double BasePlateWidth = StringerWidth + BasePlateInsideProjection;
        public const double BasePlateAnchorInsetFromWeb = 45.0;
        public const double BasePlateThickness = 10.0;
        public const double StringerJointPlateThickness = 10.0;
        public const string JoiningPlateProfile = "PL10*70";
        public const double JoiningPlateWidth = 70.0;
        public const double JoiningPlateLength = 180.0;
        public const double JoiningPlateWeldGap = 5.0;
        public const double BottomJoiningPlateLowerHeight = 117.8;
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
        public const double BaseAnchorBoltSpacing = 90.0;
        public const string BoltStandard = "8.8XOX";

        // Welds.
        public const double FilletWeldSize = 6.0;

        // Guarding.
        //
        // The supplied standard handrail details use 25 mm nominal-bore tube,
        // 33.7 mm outside diameter, with an 82 mm centre-line radius for formed
        // bends. Use that fabrication system consistently through posts, rails
        // and returns.
        public const double HandrailOutsideDiameter = 33.7;
        public const double StairHandrailHeight = 900.0;
        public const double PlatformGuardHeight = 900.0;
        public const double MidRailHeight = 550.0;
        public const string RailProfile = "CHS33.7*3.2";

        // Handrail posts are shop-welded to PL10 footplates and site-bolted
        // through the top flange of the PFC stringers. The supplied top-mounted
        // standard detail uses 2-M16 at 100 mm centres, so use that as our guide.
        public const double HandrailPostBaseLength = 160.0;
        public const double HandrailPostBaseWidth = 70.0;
        public const double HandrailPostBaseThickness = 10.0;
        public const double HandrailPostBoltSpacing = 100.0;
        public const double HandrailPostBoltSize = 16.0;

        // Landing: two posts only.
        public const double PlatformPostEndOffset = 150.0;

        // Sloping post footplates remain deliberately asymmetric about the
        // vertical CHS post. The plate is 160 long: 90 uphill / 70 downhill.
        // Widen the flight-only M16 centres slightly for better CHS clearance:
        // 65 uphill / 45 downhill = 110 mm c/c, leaving 25 mm end distance.
        public const double FlightPostBaseUphill = 90.0;
        public const double FlightPostBaseDownhill = 70.0;
        public const double FlightPostUphillBoltOffset = 65.0;
        public const double FlightPostDownhillBoltOffset = 45.0;

        public const double FlightPostEndOffset = 250.0;
        public const double FlightPostSpacing = 1000.0;
        public const double HandrailReferenceMaxPostSpacing = 1500.0;

        // 33.7 OD reference bend data:
        // formed bend R82 on centre line; 90-degree welded elbow R38.1.
        public const double HandrailReturnBendRadius = 82.0;
        public const double HandrailWeldedElbowRadius = 38.1;
        public const double HandrailReturnProjection = 164.0;
        public const double HandrailReturnCornerOffset = 82.0;

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
            StringerTop,
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

            Beam leftSupportAngle = CreatePlatformSupportAngle(
                -halfStringerSpacing,
                true,
                "L");

            Beam rightSupportAngle = CreatePlatformSupportAngle(
                halfStringerSpacing,
                false,
                "R");

            CreateFilletWeld(leftStringer, leftSupportAngle, "PLATFORM SUPPORT ANGLE L TO PFC");
            CreateFilletWeld(rightStringer, rightSupportAngle, "PLATFORM SUPPORT ANGLE R TO PFC");

            CreateFilletWeld(deck, leftSupportAngle, "PLATFORM DECK TO SUPPORT ANGLE L");
            CreateFilletWeld(deck, rightSupportAngle, "PLATFORM DECK TO SUPPORT ANGLE R");

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

            CreateBottomJoiningPlate(
                leftStringer,
                leftBottomPost,
                -halfStringerSpacing,
                true,
                "L");

            CreateBottomJoiningPlate(
                rightStringer,
                rightBottomPost,
                halfStringerSpacing,
                false,
                "R");

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

                bool isBottomTread = i == StairSettings.TreadCount - 1;

                CreateTreadSidePlateAndBolts(
                    tread,
                    leftStringer,
                    isBottomTread ? leftBottomPost : null,
                    x1,
                    treadTopZ,
                    -halfStringerSpacing,
                    true,
                    i + 1);

                CreateTreadSidePlateAndBolts(
                    tread,
                    rightStringer,
                    isBottomTread ? rightBottomPost : null,
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
            Beam bottomVerticalPfc,
            double treadStartX,
            double treadTopZ,
            double stringerY,
            bool leftSide,
            int treadNumber)
        {
            bool isBottomTread = bottomVerticalPfc != null;

            // Normal treads use PL8 end plates.
            // The bottom tread uses PL10 because this plate also forms the clean
            // connection between the sloping PFC and the short vertical PFC.
            string sidePlateProfile = isBottomTread
                ? StairSettings.EndPlateProfile
                : StairSettings.TreadSidePlateProfile;

            double sidePlateThickness = isBottomTread
                ? StairSettings.StringerJointPlateThickness
                : StairSettings.TreadSidePlateThickness;

            double sidePlateX1 = treadStartX;
            double sidePlateX2 = treadStartX + StairSettings.Going;

            double sidePlateTopZ =
                treadTopZ - StairSettings.TreadPlateThickness;

            double sidePlateBottomZ =
                sidePlateTopZ - StairSettings.TreadSidePlateHeight;

            double sidePlateY = leftSide
                ? stringerY + (sidePlateThickness / 2.0)
                : stringerY - (sidePlateThickness / 2.0);

            // Snipe the uphill/top corner so the tread end plate follows the
            // sloping PFC cleanly instead of projecting past it: 75 mm along the
            // top edge and 50 mm down the rear edge.
            ContourPlate sidePlate = new ContourPlate
            {
                Name = "PLATE",
                Class = "99"
            };

            sidePlate.Profile.ProfileString =
                sidePlateProfile;

            sidePlate.Material.MaterialString =
                StairSettings.Material;

            sidePlate.Position.Depth =
                Position.DepthEnum.MIDDLE;

            sidePlate.AddContourPoint(
                new ContourPoint(
                    LocalPoint(
                        sidePlateX1 + StairSettings.TreadSidePlateSnipeHorizontal,
                        sidePlateY,
                        sidePlateTopZ),
                    null));

            sidePlate.AddContourPoint(
                new ContourPoint(
                    LocalPoint(
                        sidePlateX2,
                        sidePlateY,
                        sidePlateTopZ),
                    null));

            sidePlate.AddContourPoint(
                new ContourPoint(
                    LocalPoint(
                        sidePlateX2,
                        sidePlateY,
                        sidePlateBottomZ),
                    null));

            sidePlate.AddContourPoint(
                new ContourPoint(
                    LocalPoint(
                        sidePlateX1,
                        sidePlateY,
                        sidePlateBottomZ),
                    null));

            sidePlate.AddContourPoint(
                new ContourPoint(
                    LocalPoint(
                        sidePlateX1,
                        sidePlateY,
                        sidePlateTopZ - StairSettings.TreadSidePlateSnipeVertical),
                    null));

            InsertOrThrow(
                sidePlate,
                "TREAD " + treadNumber +
                (leftSide ? " LEFT" : " RIGHT") +
                " END PLATE [75 HORIZONTAL / 50 VERTICAL SNIPE]");

            CreateFilletWeld(
                tread,
                sidePlate,
                "TREAD " + treadNumber +
                (leftSide ? " LEFT" : " RIGHT") +
                " END PLATE TO TREAD");

            double frontBoltX =
                sidePlateX2 - StairSettings.TreadFirstBoltFromLeadingEdge;

            double rearBoltX =
                frontBoltX - StairSettings.TreadBoltSpacing;

            double boltZ =
                treadTopZ - StairSettings.TreadBoltDownFromTop;

            Point frontBolt =
                LocalPoint(frontBoltX, sidePlateY, boltZ);

            Point rearBolt =
                LocalPoint(rearBoltX, sidePlateY, boltZ);

            if (!isBottomTread)
            {
                CreateTwoBoltArray(
                    sidePlate,
                    stringer,
                    frontBolt,
                    rearBolt,
                    StairSettings.TreadBoltSize,
                    BoltPlaneKind.StairSide,
                    "TREAD " + treadNumber +
                    (leftSide ? " LEFT" : " RIGHT") +
                    " END PLATE - 2 M12 BOLTS TO PFC");

                return;
            }

            // Bottom tread only:
            // the front bolt passes through the short vertical PFC,
            // while the rear bolt passes through the sloping PFC.
            // They must therefore be separate Tekla bolt groups.
            CreateSingleBolt(
                sidePlate,
                bottomVerticalPfc,
                frontBolt,
                rearBolt,
                StairSettings.TreadBoltSize,
                BoltPlaneKind.StairSide,
                "BOTTOM TREAD " +
                (leftSide ? "LEFT" : "RIGHT") +
                " FRONT M12 TO VERTICAL PFC");

            CreateSingleBolt(
                sidePlate,
                stringer,
                rearBolt,
                frontBolt,
                StairSettings.TreadBoltSize,
                BoltPlaneKind.StairSide,
                "BOTTOM TREAD " +
                (leftSide ? "LEFT" : "RIGHT") +
                " REAR M12 TO SLOPING PFC");
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

        private Beam CreatePlatformSupportAngle(
            double stringerY,
            bool leftSide,
            string side)
        {
            double halfLeg =
                StairSettings.PlatformSupportAngleLeg / 2.0;

            // The flat PFC web face is the +/-450 datum. Keep the complete
            // 50x50 angle inside the landing: vertical leg against the web and
            // horizontal leg immediately below the PL10 deck.
            double supportY = leftSide
                ? stringerY + halfLeg
                : stringerY - halfLeg;

            double deckUndersideZ =
                StairSettings.TotalRise - StairSettings.TreadPlateThickness;

            double supportZ =
                deckUndersideZ - halfLeg;

            Point nominalStart = LocalPoint(
                StairSettings.PlatformSupportAngleEndClearance,
                supportY,
                supportZ);

            Point nominalEnd = LocalPoint(
                StairSettings.PlatformLength -
                    StairSettings.PlatformSupportAngleEndClearance,
                supportY,
                supportZ);

            // Reverse one side so the asymmetric angle section mirrors correctly
            // and keeps its vertical leg against the PFC web on both sides.
            Point start = leftSide ? nominalStart : nominalEnd;
            Point end = leftSide ? nominalEnd : nominalStart;

            Beam angle = new Beam(start, end)
            {
                Name = "ANGLE",
                Class = "16"
            };

            angle.Profile.ProfileString =
                StairSettings.PlatformSupportAngleProfile;

            angle.Material.MaterialString =
                StairSettings.Material;

            angle.Position.Plane = Position.PlaneEnum.MIDDLE;
            angle.Position.Depth = Position.DepthEnum.MIDDLE;

            // TOP put the horizontal leg below the support and the vertical leg
            // on the wrong side. BELOW rotates the RSA 180 degrees within the
            // same 50 x 50 envelope: horizontal leg directly under the landing
            // plate, vertical leg hanging down against the PFC web.
            angle.Position.Rotation = Position.RotationEnum.BELOW;

            InsertOrThrow(
                angle,
                "PLATFORM SUPPORT ANGLE " + side + " [model name: ANGLE, class 16, " +
                StairSettings.PlatformSupportAngleProfile +
                ", rotation BELOW, 10 mm clear each end]");

            return angle;
        }

        private void CreateWallConnection(Beam stringer, double y, string side)
        {
            double halfWidth = StairSettings.WallPlateWidth / 2.0;
            double halfHeight = StairSettings.WallPlateHeight / 2.0;

            // The platform PFC top is 10 mm below finished landing level because
            // the PL10 deck finishes exactly at 3170. Keep the 170 mm end plate
            // wholly within the 180 mm PFC depth: 5 mm clear top and bottom.
            double stringerTopZ =
                StairSettings.TotalRise - StairSettings.TreadPlateThickness;

            double centreZ =
                stringerTopZ - (StairSettings.StringerDepth / 2.0);

            bool leftSide = y < 0.0;

            // Same transverse setting-out as the base and joining plates:
            // 70 mm plate flush to the outside of the 75 mm PFC footprint,
            // leaving 5 mm at the web side for the 6 mm fillet weld.
            double plateY =
                GetJoiningPlateCentreY(y, leftSide);

            // The user's first pick is the wall face at X = 0.
            // A PL10 contour plate is centred on its contour plane, so X = 5
            // puts the back face of the plate exactly on the picked wall plane.
            double plateX =
                StairSettings.BasePlateThickness / 2.0;

            ContourPlate wallPlate = CreatePlate(
                "BEACOM WALL END PLATE " + side,
                StairSettings.EndPlateProfile,
                LocalPoint(plateX, plateY - halfWidth, centreZ - halfHeight),
                LocalPoint(plateX, plateY + halfWidth, centreZ - halfHeight),
                LocalPoint(plateX, plateY + halfWidth, centreZ + halfHeight),
                LocalPoint(plateX, plateY - halfWidth, centreZ + halfHeight),
                "8");

            // The back face of the PL10 is exactly on the clicked wall datum
            // (X = 0). Fit the channel to the inside/front face at X = 10 so
            // the PFC does not run through the end plate.
            double plateInsideFaceX =
                plateX + (StairSettings.BasePlateThickness / 2.0);

            CreateWallEndFitting(
                stringer,
                plateInsideFaceX,
                y,
                centreZ,
                side);

            CreateFilletWeld(
                stringer,
                wallPlate,
                "WALL END PLATE " + side + " TO PFC");

            Point lowerBolt = LocalPoint(
                plateX,
                plateY,
                centreZ - (StairSettings.BaseAnchorBoltSpacing / 2.0));

            Point upperBolt = LocalPoint(
                plateX,
                plateY,
                centreZ + (StairSettings.BaseAnchorBoltSpacing / 2.0));

            CreateTwoBoltArray(
                wallPlate,
                wallPlate,
                lowerBolt,
                upperBolt,
                StairSettings.AnchorBoltSize,
                BoltPlaneKind.Wall,
                "WALL END PLATE " + side + " - 2 M16 ANCHORS");
        }

        private void CreateWallEndFitting(
            Beam channel,
            double x,
            double y,
            double z,
            string side)
        {
            Fitting fitting = new Fitting
            {
                Father = channel,
                Plane = new Tekla.Structures.Model.Plane()
            };

            fitting.Plane.Origin =
                LocalPoint(x, y, z);

            // Y-Z plane: exactly parallel to the wall/end plate.
            fitting.Plane.AxisX = new Vector(
                _yAxis.X,
                _yAxis.Y,
                _yAxis.Z);

            fitting.Plane.AxisY = new Vector(
                0.0,
                0.0,
                1.0);

            InsertOrThrow(
                fitting,
                "CHANNEL " + side +
                " FIT TO INSIDE FACE OF WALL END PLATE");
        }

        private void CreateBaseConnection(Beam stringer, double y, string side)
        {
            double halfLength =
                StairSettings.BasePlateLength / 2.0;

            // Keep the 170 mm plate length within the 180 mm PFC depth:
            // 5 mm clear at each end.
            double x =
                StairSettings.OverallLength -
                (StairSettings.StringerDepth / 2.0);

            bool leftSide =
                y < 0.0;

            // The base plate now covers the full 75 mm PFC footprint and then
            // projects 80 mm inward into the clear stair width for site drilling.
            //
            // The PFC reference line is its inward web face:
            // left side  -> channel footprint runs toward -Y, clear stair is +Y
            // right side -> channel footprint runs toward +Y, clear stair is -Y
            double outsideY = leftSide
                ? y - StairSettings.StringerWidth
                : y + StairSettings.StringerWidth;

            double insideY = leftSide
                ? y + StairSettings.BasePlateInsideProjection
                : y - StairSettings.BasePlateInsideProjection;

            double minY =
                Math.Min(outsideY, insideY);

            double maxY =
                Math.Max(outsideY, insideY);

            double basePlateZ =
                StairSettings.BasePlateThickness / 2.0;

            ContourPlate basePlate = CreatePlate(
                "BEACOM BASE PLATE " + side,
                StairSettings.EndPlateProfile,
                LocalPoint(x - halfLength, minY, basePlateZ),
                LocalPoint(x + halfLength, minY, basePlateZ),
                LocalPoint(x + halfLength, maxY, basePlateZ),
                LocalPoint(x - halfLength, maxY, basePlateZ),
                "8");

            CreateFilletWeld(
                stringer,
                basePlate,
                "BASE PLATE " + side + " TO PFC");

            // Put both M16 anchors on the clear inward projection so the stringer
            // can be erected first and drilled straight down before the bottom
            // tread is installed. The anchor line is 45 mm inside the PFC web,
            // leaving 35 mm to the free edge of the 80 mm projection.
            double anchorY = leftSide
                ? y + StairSettings.BasePlateAnchorInsetFromWeb
                : y - StairSettings.BasePlateAnchorInsetFromWeb;

            Point firstBolt = LocalPoint(
                x - (StairSettings.BaseAnchorBoltSpacing / 2.0),
                anchorY,
                basePlateZ);

            Point secondBolt = LocalPoint(
                x + (StairSettings.BaseAnchorBoltSpacing / 2.0),
                anchorY,
                basePlateZ);

            CreateTwoBoltArray(
                basePlate,
                basePlate,
                firstBolt,
                secondBolt,
                StairSettings.AnchorBoltSize,
                BoltPlaneKind.Horizontal,
                "BASE PLATE " + side +
                " - 2 M16 SITE-DRILL ANCHORS ON INWARD PROJECTION");
        }

        private void CreateGuarding(PlatformParts platform, FlightParts flight)
        {
            double halfStringerSpacing =
                StairSettings.StringerSpacing / 2.0;

            // The PFC insertion line is the inward web face. Move half the 75 mm
            // channel width outward so every handrail post sits on the centre of
            // the PFC top flange, keeping the full 900 mm clear stair width.
            double leftRailY =
                -halfStringerSpacing - (StairSettings.StringerWidth / 2.0);

            double rightRailY =
                halfStringerSpacing + (StairSettings.StringerWidth / 2.0);

            CreateSideGuard(
                leftRailY,
                "L",
                platform.LeftStringer,
                flight.LeftStringer);

            CreateSideGuard(
                rightRailY,
                "R",
                platform.RightStringer,
                flight.RightStringer);
        }

        private void CreateSideGuard(
            double y,
            string side,
            Beam platformStringer,
            Beam flightStringer)
        {
            // Private-stair arrangement: keep the handrail continuous at the
            // same nominal height over both the landing and the pitch line.
            // This removes the old 200 mm step in the rail at the top of flight.
            double platformTopRailZ =
                StairSettings.TotalRise + StairSettings.PlatformGuardHeight;

            double platformMidRailZ =
                StairSettings.TotalRise + StairSettings.MidRailHeight;

            Beam platformTopRail = CreateBeam(
                "HANDRAIL",
                StairSettings.RailProfile,
                LocalPoint(0.0, y, platformTopRailZ),
                LocalPoint(StairSettings.PlatformLength, y, platformTopRailZ),
                "7");

            Beam platformMidRail = CreateBeam(
                "HANDRAIL",
                StairSettings.RailProfile,
                LocalPoint(0.0, y, platformMidRailZ),
                LocalPoint(StairSettings.PlatformLength, y, platformMidRailZ),
                "7");

            Beam stairTopRail = CreateBeam(
                "HANDRAIL",
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
                "HANDRAIL",
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

            // The horizontal and sloping rails now meet at one clean mitred joint.
            CreateFilletWeld(
                platformTopRail,
                stairTopRail,
                "HANDRAIL " + side + " TOP LANDING/FLIGHT JOINT");

            CreateFilletWeld(
                platformMidRail,
                stairMidRail,
                "HANDRAIL " + side + " MID LANDING/FLIGHT JOINT");

            PolyBeam bottomReturn = CreateHandrailBottomReturn(
                y,
                side);

            CreateFilletWeld(
                stairTopRail,
                bottomReturn,
                "HANDRAIL " + side + " TOP RAIL TO D-RETURN");

            CreateFilletWeld(
                stairMidRail,
                bottomReturn,
                "HANDRAIL " + side + " MID RAIL TO D-RETURN");

            // Two posts are enough on the 1000 mm landing. Keeping them
            // 150 mm in from each end gives a clean 700 mm clear spacing.
            double[] platformPostXs =
            {
                StairSettings.PlatformPostEndOffset,
                StairSettings.PlatformLength - StairSettings.PlatformPostEndOffset
            };

            foreach (double x in platformPostXs)
            {
                Beam post = CreatePlatformHandrailPost(
                    x,
                    y,
                    side,
                    platformStringer,
                    platformTopRailZ);

                CreateFilletWeld(
                    post,
                    platformTopRail,
                    "HANDRAIL POST " + side + " TO TOP RAIL");

                CreateFilletWeld(
                    post,
                    platformMidRail,
                    "HANDRAIL POST " + side + " TO MID RAIL");
            }

            // Flight posts start/end 250 mm clear of the two fabricated PFC joints
            // and run at 1000 mm horizontal centres: 1250 / 2250 / 3250 / 4250.
            for (double nominalX =
                    StairSettings.PlatformLength + StairSettings.FlightPostEndOffset;
                 nominalX <=
                    StairSettings.OverallLength - StairSettings.FlightPostEndOffset + 0.1;
                 nominalX += StairSettings.FlightPostSpacing)
            {
                Beam post = CreateFlightHandrailPost(
                    nominalX,
                    y,
                    side,
                    flightStringer);

                CreateFilletWeld(
                    post,
                    stairTopRail,
                    "HANDRAIL POST " + side + " TO SLOPING TOP RAIL");

                CreateFilletWeld(
                    post,
                    stairMidRail,
                    "HANDRAIL POST " + side + " TO SLOPING MID RAIL");
            }
        }

        private Beam CreatePlatformHandrailPost(
            double x,
            double y,
            string side,
            Beam platformStringer,
            double topRailZ)
        {
            double halfLength =
                StairSettings.HandrailPostBaseLength / 2.0;

            double halfWidth =
                StairSettings.HandrailPostBaseWidth / 2.0;

            // The platform PFC top is at 3160; the post footplate sits directly
            // on that top flange, outside the 900 mm wide landing deck.
            double pfcTopZ =
                StairSettings.TotalRise - StairSettings.TreadPlateThickness;

            double plateCentreZ =
                pfcTopZ + (StairSettings.HandrailPostBaseThickness / 2.0);

            ContourPlate footPlate = CreatePlate(
                "HANDRAIL POST BASE " + side,
                StairSettings.EndPlateProfile,
                LocalPoint(x - halfLength, y - halfWidth, plateCentreZ),
                LocalPoint(x + halfLength, y - halfWidth, plateCentreZ),
                LocalPoint(x + halfLength, y + halfWidth, plateCentreZ),
                LocalPoint(x - halfLength, y + halfWidth, plateCentreZ),
                "99");

            double halfBoltSpacing =
                StairSettings.HandrailPostBoltSpacing / 2.0;

            CreateTwoBoltArray(
                footPlate,
                platformStringer,
                LocalPoint(x - halfBoltSpacing, y, plateCentreZ),
                LocalPoint(x + halfBoltSpacing, y, plateCentreZ),
                StairSettings.HandrailPostBoltSize,
                BoltPlaneKind.Horizontal,
                "HANDRAIL POST BASE " + side + " - 2 M16 @ 100 C/C TO PLATFORM CHANNEL");

            double postBaseZ =
                pfcTopZ + StairSettings.HandrailPostBaseThickness;

            Beam post = CreateBeam(
                "HANDRAIL",
                StairSettings.RailProfile,
                LocalPoint(x, y, postBaseZ),
                LocalPoint(x, y, topRailZ),
                "7");

            CreateFilletWeld(
                footPlate,
                post,
                "HANDRAIL POST " + side + " TO BASE PLATE");

            return post;
        }

        private Beam CreateFlightHandrailPost(
            double nominalX,
            double y,
            string side,
            Beam flightStringer)
        {
            double slope =
                StairSettings.Rise / StairSettings.Going;

            double lengthFactor =
                Math.Sqrt(1.0 + (slope * slope));

            // +tangent runs downhill; -tangent runs uphill.
            double tangentX =
                1.0 / lengthFactor;

            double tangentZ =
                -slope / lengthFactor;

            double normalX =
                slope / lengthFactor;

            double normalZ =
                1.0 / lengthFactor;

            double pfcTopZ =
                FlightStringerTopZ(nominalX);

            double halfPlateThickness =
                StairSettings.HandrailPostBaseThickness / 2.0;

            // Mid-plane point directly below the vertical CHS post.
            double plateAtPostX =
                nominalX + (normalX * halfPlateThickness);

            double plateAtPostZ =
                pfcTopZ + (normalZ * halfPlateThickness);

            double halfWidth =
                StairSettings.HandrailPostBaseWidth / 2.0;

            // Because the CHS post is vertical while the plate slopes, give the
            // uphill end extra length. This puts the post deliberately off-centre
            // on the 160 mm plate: 90 mm uphill / 70 mm downhill.
            double uphillX =
                plateAtPostX -
                (tangentX * StairSettings.FlightPostBaseUphill);

            double uphillZ =
                plateAtPostZ -
                (tangentZ * StairSettings.FlightPostBaseUphill);

            double downhillX =
                plateAtPostX +
                (tangentX * StairSettings.FlightPostBaseDownhill);

            double downhillZ =
                plateAtPostZ +
                (tangentZ * StairSettings.FlightPostBaseDownhill);

            ContourPlate footPlate = CreatePlate(
                "HANDRAIL POST BASE " + side,
                StairSettings.EndPlateProfile,
                LocalPoint(uphillX, y - halfWidth, uphillZ),
                LocalPoint(downhillX, y - halfWidth, downhillZ),
                LocalPoint(downhillX, y + halfWidth, downhillZ),
                LocalPoint(uphillX, y + halfWidth, uphillZ),
                "99");

            // Push both flight bolts slightly farther away from the vertical CHS.
            // 65 mm uphill / 45 mm downhill gives 110 mm c/c while retaining
            // 25 mm from each bolt centre to the end of the 160 mm plate.
            Point uphillBolt = LocalPoint(
                plateAtPostX -
                    (tangentX * StairSettings.FlightPostUphillBoltOffset),
                y,
                plateAtPostZ -
                    (tangentZ * StairSettings.FlightPostUphillBoltOffset));

            Point downhillBolt = LocalPoint(
                plateAtPostX +
                    (tangentX * StairSettings.FlightPostDownhillBoltOffset),
                y,
                plateAtPostZ +
                    (tangentZ * StairSettings.FlightPostDownhillBoltOffset));

            CreateTwoBoltArray(
                footPlate,
                flightStringer,
                uphillBolt,
                downhillBolt,
                StairSettings.HandrailPostBoltSize,
                BoltPlaneKind.StringerTop,
                "HANDRAIL POST BASE " + side +
                " - 2 M16 @ 110 C/C TO FLIGHT CHANNEL");

            // Start the vertical CHS on the top face of the sloping PL10 footplate.
            double postX =
                nominalX +
                (normalX * StairSettings.HandrailPostBaseThickness);

            double postBaseZ =
                pfcTopZ +
                (normalZ * StairSettings.HandrailPostBaseThickness);

            double postTopZ =
                FlightPitchZ(postX) +
                StairSettings.StairHandrailHeight;

            Point postBasePoint =
                LocalPoint(postX, y, postBaseZ);

            Beam post = CreateBeam(
                "HANDRAIL",
                StairSettings.RailProfile,
                postBasePoint,
                LocalPoint(postX, y, postTopZ),
                "7");

            // A vertical CHS has a square end by default, which does not sit
            // correctly on an inclined footplate. Fit the bottom of the post to
            // the exact top-face plane of the sloping PL10 base plate.
            CreateSlopingPostBaseFitting(
                post,
                postBasePoint,
                tangentX,
                tangentZ,
                side);

            CreateFilletWeld(
                footPlate,
                post,
                "HANDRAIL POST " + side + " TO SLOPING BASE PLATE");

            return post;
        }

        private void CreateSlopingPostBaseFitting(
            Beam post,
            Point plateTopPoint,
            double tangentX,
            double tangentZ,
            string side)
        {
            // Plane follows the sloping footplate top face:
            // one axis across the stair, the other along the flight pitch.
            Fitting fitting = new Fitting
            {
                Father = post,
                Plane = new Tekla.Structures.Model.Plane()
            };

            fitting.Plane.Origin =
                plateTopPoint;

            fitting.Plane.AxisX = new Vector(
                _yAxis.X,
                _yAxis.Y,
                0.0);

            fitting.Plane.AxisY = new Vector(
                (_xAxis.X * tangentX),
                (_xAxis.Y * tangentX),
                tangentZ);

            InsertOrThrow(
                fitting,
                "HANDRAIL POST " + side +
                " FIT TO SLOPING BASE-PLATE TOP FACE");
        }

        private PolyBeam CreateHandrailBottomReturn(
            double y,
            string side)
        {
            double topZ =
                StairSettings.Rise + StairSettings.StairHandrailHeight;

            double midZ =
                StairSettings.Rise + StairSettings.MidRailHeight;

            double frontX =
                StairSettings.OverallLength +
                StairSettings.HandrailReturnProjection;

            // One continuous CHS33.7 polybeam closes the top and mid rails.
            // The supplied standard detail uses an R82 centre-line formed bend
            // for 33.7 OD tube, so use that radius at both D-return corners.
            PolyBeam returnRail =
                new PolyBeam(PolyBeam.PolyBeamTypeEnum.BEAM)
                {
                    Name = "HANDRAIL",
                    Class = "7"
                };

            returnRail.Profile.ProfileString =
                StairSettings.RailProfile;

            returnRail.Material.MaterialString =
                StairSettings.Material;

            returnRail.Position.Plane =
                Position.PlaneEnum.MIDDLE;

            returnRail.Position.Depth =
                Position.DepthEnum.MIDDLE;

            returnRail.Position.Rotation =
                Position.RotationEnum.TOP;

            returnRail.AddContourPoint(
                new ContourPoint(
                    LocalPoint(
                        StairSettings.OverallLength,
                        y,
                        topZ),
                    null));

            returnRail.AddContourPoint(
                new ContourPoint(
                    LocalPoint(
                        frontX,
                        y,
                        topZ - StairSettings.HandrailReturnCornerOffset),
                    new Chamfer(
                        StairSettings.HandrailReturnCornerOffset,
                        StairSettings.HandrailReturnCornerOffset,
                        Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING)));

            returnRail.AddContourPoint(
                new ContourPoint(
                    LocalPoint(
                        frontX,
                        y,
                        midZ + StairSettings.HandrailReturnCornerOffset),
                    new Chamfer(
                        StairSettings.HandrailReturnCornerOffset,
                        StairSettings.HandrailReturnCornerOffset,
                        Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING)));

            returnRail.AddContourPoint(
                new ContourPoint(
                    LocalPoint(
                        StairSettings.OverallLength,
                        y,
                        midZ),
                    null));

            InsertOrThrow(
                returnRail,
                "HANDRAIL " + side +
                " BOTTOM D-RETURN [CHS33.7, R82 centre-line bends]");

            return returnRail;
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
            // Same detailing philosophy as the bottom joint:
            // PL10x70 two-point plate, exact 180 mm finished length, outside edge
            // flush with the outside of the 75 mm PFC footprint, leaving 5 mm
            // clear at the web side for the 6 mm fillet weld.
            double plateY = GetJoiningPlateCentreY(stringerY, leftSide);

            double jointX = StairSettings.PlatformLength;
            double jointZ =
                StairSettings.TotalRise - StairSettings.TreadPlateThickness;

            // At the top joint the member directions away from the intersection are:
            // platform PFC -> back toward the wall (-X)
            // flight PFC   -> down the stair (+X, -Z)
            //
            // Their unit-vector sum gives the internal angle bisector, which puts
            // the joining plate naturally between the two PFC webs.
            double slope =
                StairSettings.Rise / StairSettings.Going;

            double flightLengthFactor =
                Math.Sqrt(1.0 + (slope * slope));

            double flightUx =
                1.0 / flightLengthFactor;

            double flightUz =
                -slope / flightLengthFactor;

            double bisectorX =
                -1.0 + flightUx;

            double bisectorZ =
                flightUz;

            double bisectorLength =
                Math.Sqrt(
                    (bisectorX * bisectorX) +
                    (bisectorZ * bisectorZ));

            bisectorX /= bisectorLength;
            bisectorZ /= bisectorLength;

            // Use one full PFC depth vertically to establish the untrimmed line,
            // then round the finished two-point plate to exactly 180 mm.
            double rawLength =
                StairSettings.StringerDepth / Math.Abs(bisectorZ);

            Point rawStart = LocalPoint(
                jointX,
                plateY,
                jointZ);

            Point rawEnd = LocalPoint(
                jointX + (bisectorX * rawLength),
                plateY,
                jointZ + (bisectorZ * rawLength));

            GetTrimmedJoiningPlatePoints(
                rawStart,
                rawEnd,
                out Point trimmedStart,
                out Point trimmedEnd);

            Beam joiningPlate = CreateTwoPointWebPlate(
                "BEACOM TOP JOINING PLATE " + side,
                trimmedStart,
                trimmedEnd);

            // Platform PFC is on the upper side of this plate; flight PFC is on
            // the lower side. Cut them parallel to the plate, 5 mm either side.
            CreateJoiningPlateClearanceCuts(
                platformStringer,
                flightStringer,
                trimmedStart,
                trimmedEnd,
                "TOP PFC CLEARANCE " + side);

            CreateFilletWeld(
                platformStringer,
                joiningPlate,
                "TOP JOINING PLATE " + side + " TO PLATFORM PFC");

            CreateFilletWeld(
                flightStringer,
                joiningPlate,
                "TOP JOINING PLATE " + side + " TO FLIGHT PFC");
        }

        private void CreateBottomJoiningPlate(
            Beam flightStringer,
            Beam verticalPfc,
            double stringerY,
            bool leftSide,
            string side)
        {
            // Keep the bottom geometry already proven in the model, but make the
            // plate PL10x70 and place it flush with the outside of the 75 mm PFC
            // footprint. This leaves 5 mm clear at the web side for welding.
            double plateY = GetJoiningPlateCentreY(stringerY, leftSide);

            Point jointPoint = LocalPoint(
                StairSettings.OverallLength,
                plateY,
                StairSettings.BottomStringerJointHeight);

            Point lowerBackPoint = LocalPoint(
                StairSettings.OverallLength - StairSettings.StringerDepth,
                plateY,
                StairSettings.BottomJoiningPlateLowerHeight);

            Point trimmedStart;
            Point trimmedEnd;

            GetTrimmedJoiningPlatePoints(
                jointPoint,
                lowerBackPoint,
                out trimmedStart,
                out trimmedEnd);

            Beam joiningPlate = CreateTwoPointWebPlate(
                "BEACOM BOTTOM JOINING PLATE " + side,
                trimmedStart,
                trimmedEnd);

            CreateJoiningPlateClearanceCuts(
                flightStringer,
                verticalPfc,
                trimmedStart,
                trimmedEnd,
                "BOTTOM PFC CLEARANCE " + side);

            CreateFilletWeld(
                flightStringer,
                joiningPlate,
                "BOTTOM JOINING PLATE " + side + " TO SLOPING PFC");

            CreateFilletWeld(
                verticalPfc,
                joiningPlate,
                "BOTTOM JOINING PLATE " + side + " TO VERTICAL PFC");
        }

        private double GetJoiningPlateCentreY(
            double stringerY,
            bool leftSide)
        {
            // PFC reference line is the inward web-face datum.
            // PL10x70 is pushed outward until its outside edge is flush with the
            // 75 mm PFC footprint. That leaves exactly 5 mm at the web side.
            double centreOffset =
                StairSettings.StringerWidth -
                (StairSettings.JoiningPlateWidth / 2.0);

            return leftSide
                ? stringerY - centreOffset
                : stringerY + centreOffset;
        }

        private void GetTrimmedJoiningPlatePoints(
            Point rawStart,
            Point rawEnd,
            out Point trimmedStart,
            out Point trimmedEnd)
        {
            double dx = rawEnd.X - rawStart.X;
            double dy = rawEnd.Y - rawStart.Y;
            double dz = rawEnd.Z - rawStart.Z;

            double currentLength = Math.Sqrt(
                (dx * dx) +
                (dy * dy) +
                (dz * dz));

            if (currentLength <= StairSettings.JoiningPlateLength)
            {
                throw new InvalidOperationException(
                    "Joining plate target length is longer than the available geometry.");
            }

            double totalTrim =
                currentLength - StairSettings.JoiningPlateLength;

            // Keep the exact 180 mm fabrication length. As agreed at the bottom,
            // give the upper/start end 2 mm more clearance than the lower/end.
            double startTrim = (totalTrim / 2.0) + 1.0;
            double endTrim = (totalTrim / 2.0) - 1.0;

            double ux = dx / currentLength;
            double uy = dy / currentLength;
            double uz = dz / currentLength;

            trimmedStart = new Point(
                rawStart.X + (ux * startTrim),
                rawStart.Y + (uy * startTrim),
                rawStart.Z + (uz * startTrim));

            trimmedEnd = new Point(
                rawEnd.X - (ux * endTrim),
                rawEnd.Y - (uy * endTrim),
                rawEnd.Z - (uz * endTrim));
        }

        private void CreateJoiningPlateClearanceCuts(
            Beam upperPart,
            Beam lowerPart,
            Point plateStart,
            Point plateEnd,
            string description)
        {
            Vector plateDirection = new Vector(
                plateEnd.X - plateStart.X,
                plateEnd.Y - plateStart.Y,
                plateEnd.Z - plateStart.Z);

            plateDirection.Normalize();

            Vector acrossStair = new Vector(
                _yAxis.X,
                _yAxis.Y,
                0.0);

            acrossStair.Normalize();

            // acrossStair x plateDirection gives a vector in the stair side plane
            // perpendicular to the joining plate centreline.
            Vector perpendicular =
                acrossStair.Cross(plateDirection);

            perpendicular.Normalize();

            // Make "positive" perpendicular consistently mean upward in Z.
            if (perpendicular.Z < 0.0)
            {
                perpendicular = new Vector(
                    -perpendicular.X,
                    -perpendicular.Y,
                    -perpendicular.Z);
            }

            Point midPoint = new Point(
                (plateStart.X + plateEnd.X) / 2.0,
                (plateStart.Y + plateEnd.Y) / 2.0,
                (plateStart.Z + plateEnd.Z) / 2.0);

            double halfPlateThickness =
                StairSettings.StringerJointPlateThickness / 2.0;

            Point upperCutOrigin = new Point(
                midPoint.X + (perpendicular.X * halfPlateThickness),
                midPoint.Y + (perpendicular.Y * halfPlateThickness),
                midPoint.Z + (perpendicular.Z * halfPlateThickness));

            Point lowerCutOrigin = new Point(
                midPoint.X - (perpendicular.X * halfPlateThickness),
                midPoint.Y - (perpendicular.Y * halfPlateThickness),
                midPoint.Z - (perpendicular.Z * halfPlateThickness));

            // Upper member: cut on the upper side of the PL10 plate.
            CreateParallelFitting(
                upperPart,
                upperCutOrigin,
                acrossStair,
                plateDirection,
                description + " - FLIGHT +5MM");

            // Lower member: cut on the lower side of the PL10 plate.
            CreateParallelFitting(
                lowerPart,
                lowerCutOrigin,
                acrossStair,
                plateDirection,
                description + " - VERTICAL -5MM");
        }

        private void CreateParallelFitting(
            Beam beam,
            Point origin,
            Vector acrossStair,
            Vector plateDirection,
            string description)
        {
            Fitting fitting = new Fitting
            {
                Father = beam,
                Plane = new Tekla.Structures.Model.Plane()
            };

            // These two axes define a plane whose side-elevation trace is exactly
            // parallel to the diagonal joining plate.
            fitting.Plane.Origin = origin;
            fitting.Plane.AxisX = new Vector(
                acrossStair.X,
                acrossStair.Y,
                acrossStair.Z);

            fitting.Plane.AxisY = new Vector(
                plateDirection.X,
                plateDirection.Y,
                plateDirection.Z);

            InsertOrThrow(
                fitting,
                description + " [parallel to joining plate]");
        }

        private Beam CreateTwoPointWebPlate(
            string name,
            Point startPoint,
            Point endPoint)
        {
            WorkPlaneHandler workPlaneHandler = _model.GetWorkPlaneHandler();
            TransformationPlane originalPlane =
                workPlaneHandler.GetCurrentTransformationPlane();

            try
            {
                Point globalStart =
                    originalPlane.TransformationMatrixToGlobal.Transform(startPoint);

                Point globalEnd =
                    originalPlane.TransformationMatrixToGlobal.Transform(endPoint);

                Vector globalXAxis =
                    ToGlobalVector(_xAxis, originalPlane);

                Vector globalZAxis =
                    ToGlobalVector(new V3(0.0, 0.0, 1.0), originalPlane);

                // Vertical stair-side plane (run x vertical). Creating the plate in
                // this work plane makes Rotation FRONT put the broad plate face in
                // the PFC web plane.
                TransformationPlane webPlane =
                    new TransformationPlane(
                        globalStart,
                        globalXAxis,
                        globalZAxis);

                workPlaneHandler.SetCurrentTransformationPlane(webPlane);

                Point localStart =
                    webPlane.TransformationMatrixToLocal.Transform(globalStart);

                Point localEnd =
                    webPlane.TransformationMatrixToLocal.Transform(globalEnd);

                Beam plate = new Beam(localStart, localEnd)
                {
                    Name = "PLATE",
                    Class = "99"
                };

                plate.Profile.ProfileString =
                    StairSettings.JoiningPlateProfile;

                plate.Material.MaterialString =
                    StairSettings.Material;

                plate.Position.Plane = Position.PlaneEnum.MIDDLE;
                plate.Position.Depth = Position.DepthEnum.MIDDLE;
                plate.Position.Rotation = Position.RotationEnum.TOP;

                InsertOrThrow(
                    plate,
                    name + " [model name: PLATE, class 99, " +
                    StairSettings.JoiningPlateProfile +
                    ", two-point plate, Rotation TOP, outside edge flush with PFC]");

                return plate;
            }
            finally
            {
                workPlaneHandler.SetCurrentTransformationPlane(originalPlane);
            }
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
                    Name = "CHANNEL",
                    Class = "9"
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
                if (leftSide)
                {
                    // Physical left-hand upright:
                    // Vertical = Down  -> Plane RIGHT
                    // Rotation = Top   -> Rotation TOP
                    // Horizontal = Right -> Depth FRONT
                    //
                    // For COLUMN-type members Tekla maps the column Vertical
                    // control through Position.Plane and the Horizontal control
                    // through Position.Depth.
                    column.Position.Plane = Position.PlaneEnum.RIGHT;
                    column.Position.Depth = Position.DepthEnum.FRONT;
                    column.Position.Rotation = Position.RotationEnum.TOP;
                }
                else
                {
                    // Physical right-hand upright is already correct:
                    // Up / Below / Right. Leave it untouched.
                    column.Position.Plane = Position.PlaneEnum.LEFT;
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
                Name = "CHANNEL",
                Class = "9"
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
                name + " [model name: CHANNEL, class 9, profile: " +
                StairSettings.StringerProfile +
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
                name + " [class " + modelClass + ", profile: " + profile +
                ", material: " + StairSettings.Material + "]");

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
                Name = "PLATE",
                Class = "99"
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

                    case BoltPlaneKind.StringerTop:
                        // Plane of the sloping PFC top flange. The bolt axis is
                        // normal to the flange/footplate rather than vertical.
                        planeAxisX = ToGlobalVector(
                            new V3(
                                1.0,
                                0.0,
                                -(StairSettings.Rise / StairSettings.Going)),
                            originalPlane);

                        planeAxisY = globalYAxis;
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

        private void CreateSingleBolt(
            Part partToBeBolted,
            Part partToBoltTo,
            Point boltPosition,
            Point orientationPosition,
            double boltSize,
            BoltPlaneKind boltPlaneKind,
            string description)
        {
            WorkPlaneHandler workPlaneHandler = _model.GetWorkPlaneHandler();
            TransformationPlane originalPlane =
                workPlaneHandler.GetCurrentTransformationPlane();

            try
            {
                Point globalBolt =
                    originalPlane.TransformationMatrixToGlobal.Transform(boltPosition);

                Point globalOrientation =
                    originalPlane.TransformationMatrixToGlobal.Transform(orientationPosition);

                Vector globalXAxis = ToGlobalVector(_xAxis, originalPlane);
                Vector globalYAxis = ToGlobalVector(_yAxis, originalPlane);
                Vector globalZAxis = ToGlobalVector(new V3(0.0, 0.0, 1.0), originalPlane);

                Vector planeAxisX;
                Vector planeAxisY;

                switch (boltPlaneKind)
                {
                    case BoltPlaneKind.StairSide:
                        planeAxisX = globalXAxis;
                        planeAxisY = globalZAxis;
                        break;

                    case BoltPlaneKind.Wall:
                        planeAxisX = globalYAxis;
                        planeAxisY = globalZAxis;
                        break;

                    default:
                        planeAxisX = globalXAxis;
                        planeAxisY = globalYAxis;
                        break;
                }

                TransformationPlane boltPlane =
                    new TransformationPlane(globalBolt, planeAxisX, planeAxisY);

                workPlaneHandler.SetCurrentTransformationPlane(boltPlane);

                Point localBolt =
                    boltPlane.TransformationMatrixToLocal.Transform(globalBolt);

                Point localOrientation =
                    boltPlane.TransformationMatrixToLocal.Transform(globalOrientation);

                BoltArray bolt = new BoltArray
                {
                    PartToBeBolted = partToBeBolted,
                    PartToBoltTo = partToBoltTo,
                    FirstPosition = localBolt,
                    SecondPosition = localOrientation,
                    BoltSize = boltSize,
                    Tolerance = 2.0,
                    BoltStandard = StairSettings.BoltStandard,
                    BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE,
                    CutLength = 200.0,
                    ExtraLength = 0.0,
                    ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES,
                    Bolt = true,
                    Washer1 = false,
                    Washer2 = true,
                    Washer3 = false,
                    Nut1 = true,
                    Nut2 = false
                };

                bolt.Position.Depth = Position.DepthEnum.MIDDLE;
                bolt.Position.Plane = Position.PlaneEnum.MIDDLE;
                bolt.Position.Rotation = Position.RotationEnum.FRONT;

                // A zero distance in each direction creates one bolt at the
                // FirstPosition while SecondPosition only defines the array axis.
                bolt.AddBoltDistX(0.0);
                bolt.AddBoltDistY(0.0);

                InsertOrThrow(
                    bolt,
                    description + " [single SITE M" + boltSize.ToString("0") +
                    ", cut: 200, extra: 0, thread in material]");
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
