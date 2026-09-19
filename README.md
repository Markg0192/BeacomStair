# BeacomStair

Tekla Structures 2023 stair generator for the agreed Beacom hobby-workshop stair.

## Fixed geometry

- Total rise: **3170 mm**
- Flight: **15 equal rises @ 211.33 mm**
- Treads: **14 @ 250 mm going**
- Flight run: **3500 mm**
- Top platform: **1000 mm**
- Overall horizontal length: **4500 mm**
- Clear width between handrails: **900 mm**
- Single straight flight
- Closed risers
- PFC side stringers
- Guarding / handrails both sides

The 1000 mm top platform is included so the stair can bridge the obstruction below the upper floor while keeping the full stair within the 4500 mm horizontal envelope.

## Recommended way to run it

Open a Tekla Structures 2023 model, build **BeacomStair.sln**, then run the `BeacomStair` project.

The tool connects to the currently open Tekla model and asks for only two points:

1. **Wall datum** - pick the ground/floor point at the wall directly below the top landing.
2. **Direction point** - pick outward from the wall toward the stair foot.

The first point fixes the wall plane and ground level. The distance and height of the second point do not matter; it establishes the horizontal direction only. The tool creates the 1000 mm top platform out from the wall, then the 3500 mm flight, giving 4500 mm overall horizontal length.

## Tekla references

The project targets **.NET Framework 4.8 / x64 / C# 7.3** and defaults to:

`C:\Program Files\Tekla Structures\2023.0\bin`

If your Tekla installation is somewhere else, change `TeklaInstallDir` in `BeacomStair/BeacomStair.csproj`.

## Working fabrication defaults

The stair geometry above is fixed. The section/plate defaults below are deliberately kept in one settings class so they are easy to change after the first live model test:

- Stringers/platform side beams: `PFC-180*75*20`
- Treads/platform deck: `PL10`
- Tread end plates: `PL8`, full 250 mm tread depth × 70 mm deep
- Tread fixing: 2 × M12 bolts per end plate; first bolt 30 mm from leading edge, 125 mm centres
- Tread kickers: `PL6`, 50 mm above the tread top and 10 mm below the tread underside; kicker face sits flush to the tread edge with no plate overlap
- Platform/flight joint: 10 mm web plate at each PFC, 6 mm fillet welded to both members
- Top wall connection: 10 mm plate with 2 × M16 site anchors per stringer
- Bottom stringer detail: sloping PFC ends about 250 mm above floor, connects through a 10 mm web plate to a short vertical PFC
- Bottom floor connection: vertical PFC welded to a 10 mm base plate with 2 × M16 site anchors
- Welded joints: 6 mm fillet welds
- Handrail/guard: `CHS42.4*3.2` (42.4 mm OD CHS tube)
- Clear tread/platform width between PFC web faces: 900 mm
- Stringer centres: 900 mm
- Stair handrail height: 900 mm above the pitch line
- Platform guard height: 1100 mm

The 1050 mm plate width is wider than the 900 mm clear handrail width, as agreed.

## First live test

This has been set up specifically for Tekla Structures 2023. The next useful step is to run it in the intended model and inspect profile orientation, end positions and the exact connection/detailing treatment. Those are model/fabrication-detail items rather than changes to the agreed stair rise/going/run.


## Detailing approach

The PFC stringers are modelled with Tekla rotation set to TOP and On plane set to RIGHT. The left and right PFCs use opposite modelling directions so the flat web faces are inward and the open channel/toes face outward. The PFC reference lines are the two inward web-face datums, 900 mm apart.

The tread arrangement is now a conventional shallow end-plate detail: a PL10 horizontal tread spans between the PFC web-face datums. A PL8 end plate runs the full 250 mm tread depth at each side but is only 70 mm deep. Each end plate is fillet welded along the tread edge and fixed to the adjacent PFC web with two M12 bolts. The stringers are PFC-180*75*20 to give the connection a little more usable web depth without making the stair unnecessarily heavy.

The wall and floor anchor bolts are represented as Tekla bolt groups through the connection plates. Because the tool does not ask you to select a concrete wall or slab object, the anchor bolt group is attached to the plate itself as a modelling representation of the site anchors.


## Level convention

The quoted stair levels are finished walking-surface levels. The top face of the PL10 landing is at exactly 3170 mm above the picked wall/floor datum. Horizontal PL10 tread plates are similarly offset by half their thickness so their top faces, rather than their centre planes, land on the calculated tread levels.


## Bolt assembly convention

All generated bolts are SITE bolts. Bolt cut length is 200 mm and extra length is 0 mm. Thread in material is set to YES. The assembly is bolt head only on the head side, with one washer and one nut on the nut side.


## Refined bottom connection

The short bottom PFC is modelled bottom-to-top and uses FRONT/BACK rotation rather than TOP so the left and right channels mirror correctly as vertical members. The PL10 splice at the sloping-to-vertical PFC joint is a compact corner plate on the inside web face. The base plate is 300 × 180 × 10 with the two M16 site anchors at 220 mm centres so they sit outside the PFC footprint rather than clashing with it.


## Proven bottom upright orientation

The two short bottom PFCs are created as Tekla COLUMN-type members so their Position settings match the property pane directly:
- Right: Vertical Down / Rotation Top / Horizontal Right
- Left: Vertical Up / Rotation Below / Horizontal Right

Their model extents remain from 10.00 mm above floor (top of base plate) to the calculated 201.33 mm stringer joint level.


## Bottom tread / upright joint

The separate bottom stringer splice plate has been removed. On the bottom tread only, the tread end plate is PL10 and acts as the clean joint plate at the foot. Its two M12 fixings are separate bolt groups because they connect to different members: the front bolt connects the tread end plate to the short vertical PFC, while the rear bolt connects the same end plate to the sloping PFC. All other tread end plates remain PL8 with their normal two-bolt fixing to the sloping PFC.


## Bottom joining plate and base plate

The bottom tread split-bolt arrangement remains unchanged. A separate horizontal PL10 joining plate is now provided at each bottom corner, outside the 900 mm clear tread width, tying the sloping PFC to the short vertical PFC. The plate is 250 mm long by 75 mm wide.

The base plate is now 320 × 180 × 10 and is shifted half the PFC depth toward the actual upright footprint so the plate is centred beneath the column rather than beneath its insertion line. The two M16 site anchors are at 250 mm centres to keep them clear of the PFC.


## Bottom joint alignment refinement

The horizontal PL10 joining plate now matches the top footprint of the PFC: 180 × 75 × 10, running from one full PFC depth behind the joint to the sloping-stringer / vertical-upright insertion-line intersection. The base plate remains 320 × 180 × 10 but its centre is shifted 90 mm in the opposite direction so it sits under the actual vertical PFC footprint rather than away from it.
