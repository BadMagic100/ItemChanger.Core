# Key Differences from Hollow Knight ItemChanger

ItemChanger.Core contains a long list of breaking changes which is best captured by the
[commit history](https://github.com/BadMagic100/ItemChanger.Core/compare/a2bcdd59284ed6aa4e82ca308b4dc23105ccc72c..main).
However, there are a few main themes of changes worth noting for developers familiar with Hollow Knight ItemChanger
(HKIC).

- Obviously, all the HK-specific concrete implementations of items, locations, placements, containers, and costs have
  been removed.
- Reduced dependence on static singletons. There is now a single static singleton, the "host" which manages all other
  singleton objects, including log management, Finder, the container registry, Events, and the settings profile. These
  can now be accessed via [ItemChangerHost.Singleton](xref:ItemChanger.ItemChangerHost.Singleton), or directly via
  another property that the host implementation provides. For more information on the host model, see <xref:host>.
- Additional focus on null safety, and safety in general
  - More nullable reference type annotations (and more to come).
  - All fields have been moved to properties, and many properties are `required`. This ensures that fields are
    initialized/non-null at build time while still keeping deserialization-friendly construction.
  - Many properties are now init-only to protect them from unintended modification.
- Simplified naming of the abstract classes
- Rehomed much of the root namespace to more appropriate places
- There is no longer a need to implement Clone, and cloning is done automatically when retrieving templates from Finder.
- Removed ExistingContainerLocation and substantially changed the behavior of @"ItemChanger.Locations.ObjectLocation"
  and @"ItemChanger.Placements.MutablePlacement" to provide similar behavior in a more consistent way.
- Costs are no longer records, and Cost.Includes has been removed. Inherent costs no longer exist and implicit costs are
  applied by default.
- Support for a broader variety of games (before Silksong, I wanted to do this project for GRIME, a criminally
  underrated game - go play it)
  - Usage of 1d and 2d correction adjustments are replaced with @"UnityEngine.Vector3"
  - Scene load events now accommodate open-world games which may not have discrete scenes at all. As a result, spawning
    objects from prefabs may require the usage of the new extension method
    @"ItemChanger.Extensions.UnityExtensions.Instantiate(UnityEngine.SceneManagement.Scene,UnityEngine.GameObject)"
