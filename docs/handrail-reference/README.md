# Handrail Reference Guidance

These notes were extracted from the two handrail detail drawings supplied during development of BeacomStair.

They are stored here as a durable modelling/detailing reference for future changes to the Tekla stair generator.

> Important: the source drawings are industrial/platform handrail details and cite BS EN ISO 14122 Parts 1, 2 and 3. Use their fabrication geometry and detailing conventions as guidance. Do not assume every dimensional/code requirement applies unchanged to this private domestic workshop stair.

## Reference 1 - platform handrail fixing and arrangement

Key details shown on the supplied platform handrail drawing:

- Platform guarding height shown as **1100 mm from top of flooring to top of rail**.
- Intermediate rail shown **500 mm below the top rail**.
- Handrail standard centres shown **1500 mm maximum**.
- Maximum distance from a corner to the first handrail standard centre shown as **300 mm maximum**, measured along the tubing centreline.
- Maximum distance around a corner, measured along the handrail centreline, shown as **1000 mm maximum**.
- **No joints between corner posts**.
- **No joints in the handrail before the standard** at a returned end.
- Example returned end / D-end uses smooth formed bends rather than two open rail ends.
- Handrail connectors are expanding/internal type with a secure grub screw.
- M12 grub screw is shown installed flush so it does not interfere with the handrail surface.
- Example top-mounted connection shows **2 No. M16 grade 8.8 bolts at 100 mm centres**.
- Typical top-mounted connection calls for approximately **40 mm minimum** flange/edge accommodation.
- Example side-mounted connection shows a recommended **65 mm minimum clearance**.
- Kicking flat is shown integrated with the flooring and projecting at least **100 mm above flooring**.
- The drawing uses tubular ball-type standards and separate handrail/midrail tubes; BeacomStair currently uses welded CHS posts instead, so copy the geometry principles rather than the proprietary standard detail.

## Reference 2 - bends, returns and joints

The supplied bend/return drawing gives the following centreline bend radii:

| Nominal bore | Outside diameter | Formed bend radius | 90-degree welded elbow radius |
| --- | ---: | ---: | ---: |
| 25 mm | 33.7 mm | 82.0 mm | 38.1 mm |
| 32 mm | **42.4 mm** | **130.0 mm** | **47.8 mm** |
| 40 mm | 48.3 mm | 155.0 mm | 57.2 mm |

BeacomStair now uses **CHS33.7 x 3.2**, matching the typical handrail size explicitly shown on the supplied detail. The relevant reference geometry is:

- **82 mm centreline radius** for a smooth formed/bent 90-degree corner.
- **38.1 mm centreline radius** where a 90-degree welded elbow is used instead.
- Prefer smooth bent/returned rail geometry over square mitres where practical.
- A typical handrail end is shown as a continuous D/U-shaped return, formed with bends at both the top and lower rail.
- A stair-well return is shown using a combination of smooth formed tube and welded bends.
- Internal expanding joints are shown for 33.7, 42.4 and 48.3 outside-diameter systems.
- Joints should be located away from bends/corners and should not interrupt the grasping surface.

## BeacomStair rules derived from these references

For future handrail work on this model:

1. Use **CHS33.7 x 3.2** for the posts, top rail and midrail unless deliberately changed.
2. Treat **R82 on the tube centreline** as the preferred smooth-bend radius for CHS33.7.
3. Use a proper continuous **D-return at the bottom of the flight**; do not leave the top and mid rails as two open tube ends.
4. Keep rail transitions smooth through the top of the flight and avoid unnecessary joints near bends.
5. Keep handrail/post spacing comfortably below the **1500 mm maximum** shown on the reference drawing; the current approximately 1 m spacing is conservative relative to that detail.
6. Keep posts close enough to corners/returns that the unsupported handrail length does not become excessive.
7. Where possible, place post bases on the **centre of the PFC top flange**, with accessible site bolts and enough clearance around the vertical CHS for spanners/drilling.
8. On sloping PFCs, account for the fact that a **vertical post intersects an inclined base plate asymmetrically**; do not blindly centre the plate/bolts around the post.
9. Use the reference two-bolt top-mounted fixing philosophy: **2 x M16 grade 8.8 at 100 mm centres** through the PFC top flange, with accessible edge/tool clearance.
10. Keep all rail/post welds and fixing details buildable and inspectable; do not let bolts clash with the CHS post or PFC toes/web.

## Current project-specific caveat

The supplied reference is an industrial/platform handrail standard detail. BeacomStair is being developed for a low-use private workshop stair in a domestic garage. The drawing is therefore a **fabrication/detailing reference**, not automatic proof that the stair should adopt every BS EN ISO 14122 dimension such as the 1100 mm platform guard height. Regulatory dimensions should remain aligned with the agreed project use and final Building Control/design requirements.


## Current 33.7 mm implementation

The Tekla generator now follows the 33.7 mm reference family directly:

- CHS33.7 x 3.2 posts, top rail and midrail.
- R82 centre-line geometry for the bottom D-return.
- Landing post bases use 2 x M16 site bolts at 100 mm centres, matching the supplied top-mounted reference detail.
- 160 x 70 x 10 post base plates.
- Sloping post bases are asymmetric around the vertical CHS: 90 mm uphill and 70 mm downhill. Their bolt centres are widened slightly to 65 mm uphill and 45 mm downhill = 110 mm centres, leaving 25 mm end distance and better clearance around the CHS.
- Vertical CHS posts on the flight are fitting-cut to the exact plane of the sloping PL10 base plate so the tube bears cleanly on the plate rather than ending square across an incline.
- Two posts on the 1000 mm top landing.
- Flight post spacing remains about 1000 mm, comfortably within the 1500 mm maximum spacing shown on the reference platform-handholding detail.
