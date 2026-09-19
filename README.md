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

1. **Start point** - pick the centreline of the stair foot at ground level.
2. **End / direction point** - pick toward the top end of the stair.

The first point is the actual ground-level anchor. The distance and height of the second point do not matter; it establishes the horizontal direction only. The tool then creates the fixed 3500 mm flight rising 3170 mm, followed by the 1000 mm top platform, for 4500 mm overall horizontal length.

## Tekla references

The project targets **.NET Framework 4.8 / x64 / C# 7.3** and defaults to:

`C:\Program Files\Tekla Structures\2023.0\bin`

If your Tekla installation is somewhere else, change `TeklaInstallDir` in `BeacomStair/BeacomStair.csproj`.

## Working fabrication defaults

The stair geometry above is fixed. The section/plate defaults below are deliberately kept in one settings class so they are easy to change after the first live model test:

- Stringers/platform side beams: `PFC-150*75*18`
- Treads/platform deck: `PL8`
- Risers: `PL6`
- Handrail/guard: `CHS42.4*3.2` (42.4 mm OD CHS tube)
- Tread/platform plate width: 1050 mm
- Stringer centres: 900 mm
- Stair handrail height: 900 mm above the pitch line
- Platform guard height: 1100 mm

The 1050 mm plate width is wider than the 900 mm clear handrail width, as agreed.

## First live test

This has been set up specifically for Tekla Structures 2023. The next useful step is to run it in the intended model and inspect profile orientation, end positions and the exact connection/detailing treatment. Those are model/fabrication-detail items rather than changes to the agreed stair rise/going/run.
