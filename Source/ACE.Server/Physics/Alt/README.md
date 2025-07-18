# Physics.Alt – GDLE Physics Port (C++ → C#)

This directory contains an incremental port of the GDLE (PhatSDK) physics system to C#, for integration with ACE under the `ACE.Server.Physics.Alt` namespace.

## Current Status: **PAUSED**

**Decision Made:** Keep the original ACE physics system for now. The GDLE port is preserved here for future reference and potential integration.

## Porting Approach
- **Incremental:** Port class-by-class, with tests for each major component.
- **Isolated:** All code is in `Physics.Alt` to allow side-by-side testing with the existing physics system.
- **Testing:** New unit/integration tests will be created in `ACE.Server.Tests/Physics/Alt/`.

## Porting Checklist (as of pause point)
- [x] Math Primitives (`Vector`, `Quaternion`, `Plane`, `Frame`): **Fully ported and tested**
- [~] Polygon and Collision Core: **Polygon stubs in place; core math, ray and sphere intersection logic ported; advanced walkable/slide/edge logic stubbed**
- [x] Physics Primitives (`CSphere`, `CCylSphere`): **Core logic, collision, and transition methods ported; advanced/edge-case logic in place with clear TODOs**
- [~] BSP and Transition Logic: **BSP tree/node/leaf/portal stubs in place; binary loading for polygons and BSP trees is functional; transition/collision flow (CTransition, SPHEREPATH) mostly ported, with advanced walkable/collision logic in progress**
- [ ] Part/Cell/Array Structures: **Opaque or stubbed; not yet ported**
- [ ] Motion/Animation: **Not yet ported**
- [ ] Integration Points: **Not yet integrated with main ACE physics; registry/handle system in place for isolated testing**
- [~] Transition Object Logging/Edge Cases: **Most edge-case logic ported or stubbed; walkable/precipice/neg poly logic in progress**
- [~] Test Suite: **Ready for initial unit/integration tests; test harness to be expanded as port progresses**

---

## Progress Summary (Pause Point)
- **Math and physics primitives** (Vector, Plane, Quaternion, Frame, CSphere, CCylSphere) are fully ported, including serialization and all core math/collision methods.
- **Polygon math/collision:** Ray and sphere intersection methods are implemented; stubs for walkable/slide/edge logic are in place for future porting.
- **Binary loading:** Polygons and BSP trees can be loaded from binary files, with polygon references correctly wired up.
- **Collision/transition flow** (CTransition, SPHEREPATH) is mostly ported, including all major methods for movement, placement, step-up/down, and edge-case handling.
- **Walkable/collision logic:** Most state and setter methods are ported; advanced walkable/collision checks (e.g., BSP integration, IsValidWalkable, GetWalkableZ) are next to be implemented.
- **BSP tree/portal/leaf/node:** Stubs and partial logic in place; full polygon/cell/part integration is a future step.
- **Testing/integration:** **No testing has been performed yet.** All code is isolated and ready for side-by-side testing. Integration with ACE's main physics system will be config-driven.

### Key Files Ported
- **Math Primitives:** `Vector.cs`, `Quaternion.cs`, `Plane.cs`, `Frame.cs`
- **Physics Objects:** `CSphere.cs`, `CCylSphere.cs`
- **Collision/Transition:** `CTransition.cs`, `SPHEREPATH.cs`
- **BSP System:** `BSPTree.cs`, `BSPNode.cs`, `BSPLeaf.cs`, `BSPPortal.cs`
- **Polygon System:** `Polygon.cs`, `Vertex.cs`, `Edge.cs`
- **Binary Loading:** `BinaryReaderExtensions.cs`
- **Registry System:** `PhysicsObjectRegistry.cs`, `PhysicsObjectHandle.cs`

### Remaining Work (if resumed)
- Implement advanced polygon collision/contact methods (walkable_hits_sphere, adjust_sphere_to_plane, find_crossed_edge)
- Port physics object/part/cell structures (CPhysicsObj, CPhysicsPart, CPartArray, CObjCell, etc.)
- Complete motion/animation system integration
- Expand test suite and begin integration testing
- Create configuration-driven integration layer

---

## Why Paused?

The decision to pause the GDLE physics port was made after evaluating:

1. **ACE's existing physics system** is fully functional, well-integrated, and handles all current use cases
2. **Integration complexity** - The remaining work requires significant integration with ACE's world management, object lifecycle, and networking systems
3. **Testing requirements** - Full validation would require extensive testing across all physics scenarios
4. **Resource allocation** - The original ACE physics system provides good performance and accuracy for current needs

## Future Considerations

If the GDLE physics port is resumed in the future, the following factors should be considered:

- **Performance comparison** between ACE and GDLE physics systems
- **Accuracy requirements** for specific gameplay scenarios
- **Integration complexity** and potential breaking changes
- **Testing strategy** for validating physics behavior across all scenarios
- **Configuration approach** for allowing server operators to choose physics systems

---

**This port represents significant work and provides a solid foundation for future GDLE physics integration if needed.** 