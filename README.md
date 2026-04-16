# Automotive Production Plant Digital Twin Simulation

## Overview

This project is a takt-based digital twin simulation of a fictional automotive production plant developed in **C# (.NET)**. The primary goal is to demonstrate:

* automotive assembly domain knowledge
* just-in-sequence logistics modeling
* disturbance propagation behavior
* station wear and maintenance simulation
* carrier-constrained chassis flow
* synchronized supplementary powertrain delivery
* production recovery dynamics after faults

The simulation focuses on **assembly shop realism**, while upstream shops are modeled as structured production feeders supporting takt-synchronized vehicle flow.

This project is designed as a technical portfolio artifact for engineering evaluation contexts such as dual-study or innovation-center applications.

---

# Plant Layout Architecture

The simulated plant consists of four primary production shops:

```
Press Shop → Body Shop → Paint Shop → ASRS → Assembly Shop
```

Each shop participates in maintaining a continuous takt-synchronized vehicle pipeline.

---

# Press Shop

## Purpose

The Press Shop produces stamped sheet-metal components required for body-in-white assembly.

## Simulation Role

The Press Shop is modeled as a structured upstream feeder supplying panels into an intermediate buffer between Press and Body Shops.

Key characteristics:

* continuous panel production
* FIFO output logic
* buffer-stabilized downstream supply
* no station-level wear modeling (abstracted layer)

Output feeds:

```
Sub-Assembly Panel Buffer
```

which supports Body Shop sequencing stability.

---

# Body Shop

## Purpose

The Body Shop assembles stamped panels into a complete:

```
Body-In-White (BIW)
```

structure.

## Simulation Role

The Body Shop is responsible for:

* BIW generation
* VIN assignment
* upstream disturbance buffering
* providing structured vehicle flow into Paint Shop

Each BIW receives a pseudo-generated VIN used throughout the simulation lifecycle.

Tracked VIN attributes propagate through:

* Paint Shop
* ASRS buffer
* Assembly Shop
* Supplementary powertrain matching

This enables deterministic marriage synchronization later in the process.

Output feeds:

```
Paint Shop input queue
```

---

# Paint Shop

## Purpose

The Paint Shop applies coating layers to the BIW before assembly integration.

## Simulation Model

Paint Shop is modeled as:

```
5 parallel paint tunnels
```

instead of a single serial line.

Pretreatment is intentionally excluded to keep scope aligned with assembly-focused simulation objectives.

Paint Shop characteristics:

* parallel tunnel processing
* order scrambling for throughput optimization
* output resequencing dependency handled downstream
* no station-level wear logic
* tunnel abstraction modeled as linear state machines

Because tunnel parallelization disrupts strict vehicle order, output must be restored before Assembly Shop entry.

Output feeds:

```
ASRS vehicle buffer
```

---

# ASRS (Automated Storage and Retrieval System)

## Purpose

The ASRS restores Just-In-Sequence ordering after Paint Shop output scrambling.

## Simulation Role

The ASRS acts as the primary sequencing stabilizer between Paint Shop and Assembly Shop.

Responsibilities:

* VIN tracking
* order restoration
* release readiness monitoring
* buffer capacity monitoring
* assembly feed continuity protection

ASRS status logic:

| Status  | Condition                                   |
| ------- | ------------------------------------------- |
| Green   | ≥70% capacity and next required VIN present |
| Neutral | 30–70% capacity                             |
| Yellow  | ≤30% capacity                               |
| Red     | next required VIN missing                   |

Output feeds:

```
Assembly Shop Trim Line Entry
```

---

# Assembly Shop

## Overview

The Assembly Shop is the highest-fidelity portion of the simulation and models takt-synchronous vehicle progression through:

```
TR1 → TR2 → CH1 → CH2 → Final
```

Vehicle movement occurs one station per takt cycle unless blocked by:

* faults
* carrier constraints
* sequencing violations
* marriage readiness conditions

---

# Trim Lines (TR1 and TR2)

## Structure

```
TR1: 8 stations
TR2: 8 stations
```

## Role

Trim lines prepare vehicles before chassis integration.

Characteristics:

* skid-based transport
* takt-synchronous movement
* lighter wear behavior than chassis stations
* upstream buffering capability
* VIN progression visualization per station

Transfer boundary:

```
TR2-S08 → CH1-S01
```

This represents the skid-to-overhead carrier transition.

Transfer succeeds only if a loaded skid is available.

---

# Chassis Lines (CH1 and CH2)

## Structure

```
CH1: 9 stations
CH2: 9 stations
```

## Role

Chassis lines perform structural integration under overhead carrier transport constraints.

Characteristics:

* carriers cannot move empty
* upstream starvation propagates downstream
* fault propagation affects entire carrier zone
* halt logic preserves physical realism

---

# Marriage Station

Marriage occurs at:

```
CH2-S05
```

This station integrates:

```
Body + Powertrain
```

Marriage requires:

* matching VIN from Assembly Line
* corresponding completed powertrain from Supplementary System
* AGV delivery confirmation

If any requirement is missing:

```
vehicle advancement stops
```

---

# Final Line

## Structure

```
Final: 6 stations
```

## Role

Final line performs:

* finishing operations
* inspections
* minor installations
* completion validation

Characteristics:

* reduced tooling wear compared to chassis
* random wear spikes only when loaded
* no gradual wear accumulation
* independent downstream drain capability

Vehicles exiting Final are counted as completed production units.

---

# Supplementary Powertrain System

## Purpose

The supplementary system assembles powertrain modules independently and delivers them Just-In-Sequence to Marriage Station.

Sub-lines:

```
Front Suspension Line
Rear Axle Line
Engine Dressing Line
```

Engine Dressing structure:

```
5 stations (FIFO sequence)
```

Front Suspension and Rear Axle are modeled as abstract synchronized feeders.

---

## Powertrain Buffer Logic

Supplementary buffer responsibilities:

* collect sub-assemblies
* synchronize VIN matching
* construct completed powertrain units
* maintain JIS delivery readiness

Buffer characteristics:

```
max capacity: 8 completed powertrains
```

AGVs transport completed powertrains from buffer to:

```
CH2-S05 (Marriage Station)
```

---

# Station Wear and Maintenance Model

Each station tracks:

```
Wear Level (%)
```

Fault threshold:

```
≤10%
```

Repair completion threshold:

```
≥75%
```

Repair behavior:

```
AGV dispatch delay: 1 takt
repair duration: 1 takt
repair recovery: +70%
```

Post-repair spike immunity:

```
8 takts
```

Wear rules vary by station group:

| Group           | Gradual Wear | Random Spike |
| --------------- | ------------ | ------------ |
| Trim            | 0.25%        | 2.5%         |
| Chassis         | 0.35%        | 5%           |
| Final           | none         | 3%           |
| Engine Dressing | 0.35%        | 5%           |

Empty stations do not accumulate wear.

---

# Simulation Capabilities

The digital twin currently supports:

* takt-synchronous vehicle movement
* VIN-level tracking
* carrier-based propagation logic
* AGV maintenance intervention
* station wear degradation
* repair cycle modeling
* sequencing restoration via ASRS
* supplementary powertrain synchronization
* deterministic marriage dependency
* upstream/downstream blockage simulation
* production throughput measurement

---

# Project Objective

This simulation demonstrates the integration of:

```
industrial domain knowledge
+
software system architecture
+
discrete event production modeling
```

with emphasis on realistic assembly line constraints rather than abstract queue simulations.

The plant layout is fictional and created solely for engineering demonstration purposes.
