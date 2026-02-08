# Container Selection Process

The `ContainerLocation.ChooseBestContainerType()` method determines which container implementation should be used for a
location based on placement context, item preferences, and requested container capabilities. This article outlines the
container selection process in visual form.

## Key Concepts

**Container Support**: The location's `Supports()` method and `ForceDefaultContainer` flag determine whether a container
type is allowed at this location. Additionally, `UnsupportedContainerTag`s attached to the location or placement will
disallow the specified container types at the location.

**Required Capabilities**: Containers may need to support specific capabilities (e.g., paying costs). The selection
process validates that chosen containers can fulfill all capabilities required by the placement's items and tags.

**Instantiate vs. Modify**: The selection process consistently checks whether a container can instantiate (create new
instances) or only modify existing containers in place. If the original container is under consideration, the location
may support in-place modification or instantiation. For replacement containers, they must support instantiation. This
check appears at every decision point where a container is considered.

## Selection Flow

```mermaid
flowchart TD
    A["Start: Choose Container"] --> B{ForceDefaultContainer?}
    B -->|Yes| C["Use Default Single-Item Container"]
    B -->|No| D["Gather needed capabilities<br/>and UnsupportedContainerTags"]

    D --> E{Has Original Container<br/>with Force/Priority?}
    E -->|Yes| F{Location allows it?}
    E -->|No| J["Check item preferences"]

    F -->|Yes| G{Has all required<br/>capabilities?}
    F -->|No| H2A{Is Force flag set?}

    G -->|Yes| H{Can instantiate<br/>or modify in place?}
    G -->|No| H2A

    H -->|Yes| G_USE["Use Original Container"]
    H -->|No| H2A

    H2A -->|Yes| I2["Log warning"]
    H2A -->|No| J

    I2 --> H3{Can instantiate<br/>or modify in place?}
    H3 -->|Yes| G_USE
    H3 -->|No| J

    J --> L{Found item's<br/>preferred container?}
    L -->|No| N["Check location or fallback"]
    L -->|Yes| L2{Location allows it?}

    L2 -->|No| N
    L2 -->|Yes| L4{Has all required<br/>capabilities?}

    L4 -->|No| N
    L4 -->|Yes| L5{Can instantiate<br/>or modify in place?}

    L5 -->|No| N
    L5 -->|Yes| M["Use item's preferred container"]

    N --> O{Original container exists<br/>with normal priority?}
    O -->|No| Q["Multiple items?"]
    O -->|Yes| O2{Not in<br/>UnsupportedTags?}

    O2 -->|No| Q
    O2 -->|Yes| O3{Has all required<br/>capabilities?}

    O3 -->|No| Q
    O3 -->|Yes| O4{Can instantiate<br/>or modify in place?}

    O4 -->|No| Q
    O4 -->|Yes| P["Use Original Container"]

    Q{Multiple items<br/>in placement?}
    Q -->|No| T["Use Default Single-Item Container"]
    Q -->|Yes| R{Default multi-item<br/>in UnsupportedTags?}

    R -->|Yes| T
    R -->|No| R2{Has all required<br/>capabilities?}

    R2 -->|No| T
    R2 -->|Yes| R3{Can instantiate<br/>or modify in place?}

    R3 -->|No| T
    R3 -->|Yes| S["Use Default Multi-Item Container"]

    C --> U["Return container type"]
    G_USE --> U
    M --> U
    P --> U
    S --> U
    T --> U
```
