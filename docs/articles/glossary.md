# Glossary

This page serves as an introduction and quick reference of the key abstractions in ItemChanger.Core.

---

@"ItemChanger.Items.Item": An item abstractly represents something granted to the player. It may at some point turn into
a real @"UnityEngine.GameObject" (e.g. flung money), or may simply represent a saved effect (such as obtaining a spell).

@"ItemChanger.Containers.Container": A container represents a real @"UnityEngine.GameObject" that is able to give items
by interacting with it. A container definition can do modifications to existing GameObjects, create new GameObjects from
a template, or both.

@"ItemChanger.Locations.Location": A location abstractly represents a location in the world. Locations are usually
associated with a placement, and usually contain information on how to give items placed at the associated placement in
various ways, including but not limited to placing containers or replacing existing @"UnityEngine.GameObject"s with
containers.

@"ItemChanger.Placements.Placement": A placement represents a pairing of 1+ items and 1+ locations - usually they don't
do a ton on their own, they just arbitrate the interaction between items, locations, and consumers of events on the
placement.
