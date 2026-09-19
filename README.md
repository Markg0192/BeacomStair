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

The tool connects to the currently open Tekla model and asks you to pick:

1. First point on the upper-floor/platform back edge.
2. Second point on that edge.
3. A point in the direction of the stair foot.

The first two points define the transverse direction. The third point tells the tool which side of the edge the platform and flight should project toward.

## Tekla references

The project targets **.NET Framework 4.8 / x64 / C# 7.3** and defaults to:

`C:\Program Files\Tekla Structures\2023.0\bin`

If your Tekla installation is somewhere else, change `TeklaInstallDir` in `BeacomStair/BeacomStair.csproj`.

## Working fabrication defaults

The stair geometry above is fixed. The section/plate defaults below are deliberately kept in one settings class so they are easy to change after the first live model test:

- Stringers/platform side beams: `PFC200*75*23`
- Treads/platform deck: `PL8`
- Risers: `PL6`
- Handrail/guard: `CHS42.4*3.2`
- Tread/platform plate width: 1050 mm
- Stringer centres: 900 mm
- Stair handrail height: 900 mm above the pitch line
- Platform guard height: 1100 mm

The 1050 mm plate width is wider than the 900 mm clear handrail width, as agreed.

## First live test

This has been set up specifically for Tekla Structures 2023. The next useful step is to run it in the intended model and inspect profile orientation, end positions and the exact connection/detailing treatment. Those are model/fabrication-detail items rather than changes to the agreed stair rise/going/run.
