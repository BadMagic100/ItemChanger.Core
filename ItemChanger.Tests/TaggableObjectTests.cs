using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Tags;
using ItemChanger.Tags.Constraints;

namespace ItemChanger.Tests;

public class TaggableObjectTests
{
    private class UnconstrainedTag : Tag { }

    [ItemTag]
    private class ItemConstrainedTag : Tag { }

    [LocationTag]
    private class LocationConstrainedTag : Tag { }

    [PlacementTag]
    private class PlacementConstrainedTag : Tag { }

    [LocationTag]
    [PlacementTag]
    private class MultiConstrainedTag : Tag { }

    private class CustomTaggable : TaggableObject { }

    [TagConstrainedTo<CustomTaggable>]
    private class CustomConstrainedTag : Tag { }

    // Helper methods
    private void AssertCanAddTag<TTag>(Func<TaggableObject> createTaggable, string tagName)
        where TTag : Tag, new()
    {
        List<string> failures = [];

        // Test AddTag<T>()
        try
        {
            TaggableObject test1 = createTaggable();
            TTag tag1 = test1.AddTag<TTag>();
            if (!test1.HasTag<TTag>())
            {
                failures.Add("AddTag<T>() did not add tag");
            }
        }
        catch (Exception ex)
        {
            failures.Add($"AddTag<T>() threw: {ex.Message}");
        }

        // Test AddTag(Tag)
        try
        {
            TaggableObject test2 = createTaggable();
            test2.AddTag(new TTag());
            if (!test2.HasTag<TTag>())
            {
                failures.Add("AddTag(Tag) did not add tag");
            }
        }
        catch (Exception ex)
        {
            failures.Add($"AddTag(Tag) threw: {ex.Message}");
        }

        // Test AddTags(IEnumerable<Tag>)
        try
        {
            TaggableObject test3 = createTaggable();
            test3.AddTags([new TTag()]);
            if (!test3.HasTag<TTag>())
            {
                failures.Add("AddTags(IEnumerable<Tag>) did not add tag");
            }
        }
        catch (Exception ex)
        {
            failures.Add($"AddTags(IEnumerable<Tag>) threw: {ex.Message}");
        }

        if (failures.Count > 0)
        {
            Assert.Fail(
                $"{tagName} should be addable to {createTaggable().GetType().Name}. Failures:\n- "
                    + string.Join("\n- ", failures)
            );
        }
    }

    private void AssertCannotAddTag<TTag>(Func<TaggableObject> createTaggable, string tagName)
        where TTag : Tag, new()
    {
        List<string> failures = [];

        // Test AddTag<T>()
        try
        {
            TaggableObject test1 = createTaggable();
            test1.AddTag<TTag>();
            failures.Add("AddTag<T>() should have thrown ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
        catch (Exception ex)
        {
            failures.Add(
                $"AddTag<T>() threw unexpected exception: {ex.GetType().Name}: {ex.Message}"
            );
        }

        // Test AddTag(Tag)
        try
        {
            TaggableObject test2 = createTaggable();
            test2.AddTag(new TTag());
            failures.Add("AddTag(Tag) should have thrown ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
        catch (Exception ex)
        {
            failures.Add(
                $"AddTag(Tag) threw unexpected exception: {ex.GetType().Name}: {ex.Message}"
            );
        }

        // Test AddTags(IEnumerable<Tag>)
        try
        {
            TaggableObject test3 = createTaggable();
            test3.AddTags([new TTag()]);
            failures.Add("AddTags(IEnumerable<Tag>) should have thrown ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
        catch (Exception ex)
        {
            failures.Add(
                $"AddTags(IEnumerable<Tag>) threw unexpected exception: {ex.GetType().Name}: {ex.Message}"
            );
        }

        if (failures.Count > 0)
        {
            Assert.Fail(
                $"{tagName} should not be addable to {createTaggable().GetType().Name}. Failures:\n- "
                    + string.Join("\n- ", failures)
            );
        }
    }

    // Tests for UnconstrainedTag
    [Fact]
    public void UnconstrainedTag_CanAddToItem()
    {
        AssertCanAddTag<UnconstrainedTag>(
            () => new DebugItem { Name = nameof(UnconstrainedTag) },
            nameof(UnconstrainedTag)
        );
    }

    [Fact]
    public void UnconstrainedTag_CanAddToLocation()
    {
        AssertCanAddTag<UnconstrainedTag>(
            () => new StartLocation { Name = nameof(UnconstrainedTag) },
            nameof(UnconstrainedTag)
        );
    }

    [Fact]
    public void UnconstrainedTag_CanAddToPlacement()
    {
        AssertCanAddTag<UnconstrainedTag>(
            () => new StartLocation { Name = nameof(UnconstrainedTag) }.Wrap(),
            nameof(UnconstrainedTag)
        );
    }

    [Fact]
    public void UnconstrainedTag_CanAddToCustomTaggable()
    {
        AssertCanAddTag<UnconstrainedTag>(() => new CustomTaggable(), nameof(UnconstrainedTag));
    }

    // Tests for ItemConstrainedTag
    [Fact]
    public void ItemConstrainedTag_CanAddToItem()
    {
        AssertCanAddTag<ItemConstrainedTag>(
            () => new DebugItem { Name = nameof(ItemConstrainedTag) },
            nameof(ItemConstrainedTag)
        );
    }

    [Fact]
    public void ItemConstrainedTag_CannotAddToLocation()
    {
        AssertCannotAddTag<ItemConstrainedTag>(
            () => new StartLocation { Name = nameof(ItemConstrainedTag) },
            nameof(ItemConstrainedTag)
        );
    }

    [Fact]
    public void ItemConstrainedTag_CannotAddToPlacement()
    {
        AssertCannotAddTag<ItemConstrainedTag>(
            () => new StartLocation { Name = nameof(ItemConstrainedTag) }.Wrap(),
            nameof(ItemConstrainedTag)
        );
    }

    [Fact]
    public void ItemConstrainedTag_CannotAddToCustomTaggable()
    {
        AssertCannotAddTag<ItemConstrainedTag>(
            () => new CustomTaggable(),
            nameof(ItemConstrainedTag)
        );
    }

    // Tests for LocationConstrainedTag
    [Fact]
    public void LocationConstrainedTag_CannotAddToItem()
    {
        AssertCannotAddTag<LocationConstrainedTag>(
            () => new DebugItem { Name = nameof(LocationConstrainedTag) },
            nameof(LocationConstrainedTag)
        );
    }

    [Fact]
    public void LocationConstrainedTag_CanAddToLocation()
    {
        AssertCanAddTag<LocationConstrainedTag>(
            () => new StartLocation { Name = nameof(LocationConstrainedTag) },
            nameof(LocationConstrainedTag)
        );
    }

    [Fact]
    public void LocationConstrainedTag_CannotAddToPlacement()
    {
        AssertCannotAddTag<LocationConstrainedTag>(
            () => new StartLocation { Name = nameof(LocationConstrainedTag) }.Wrap(),
            nameof(LocationConstrainedTag)
        );
    }

    [Fact]
    public void LocationConstrainedTag_CannotAddToCustomTaggable()
    {
        AssertCannotAddTag<LocationConstrainedTag>(
            () => new CustomTaggable(),
            nameof(LocationConstrainedTag)
        );
    }

    // Tests for PlacementConstrainedTag
    [Fact]
    public void PlacementConstrainedTag_CannotAddToItem()
    {
        AssertCannotAddTag<PlacementConstrainedTag>(
            () => new DebugItem { Name = nameof(PlacementConstrainedTag) },
            nameof(PlacementConstrainedTag)
        );
    }

    [Fact]
    public void PlacementConstrainedTag_CannotAddToLocation()
    {
        AssertCannotAddTag<PlacementConstrainedTag>(
            () => new StartLocation { Name = nameof(PlacementConstrainedTag) },
            nameof(PlacementConstrainedTag)
        );
    }

    [Fact]
    public void PlacementConstrainedTag_CanAddToPlacement()
    {
        AssertCanAddTag<PlacementConstrainedTag>(
            () => new StartLocation { Name = nameof(PlacementConstrainedTag) }.Wrap(),
            nameof(PlacementConstrainedTag)
        );
    }

    [Fact]
    public void PlacementConstrainedTag_CannotAddToCustomTaggable()
    {
        AssertCannotAddTag<PlacementConstrainedTag>(
            () => new CustomTaggable(),
            nameof(PlacementConstrainedTag)
        );
    }

    // Tests for MultiConstrainedTag
    [Fact]
    public void MultiConstrainedTag_CannotAddToItem()
    {
        AssertCannotAddTag<MultiConstrainedTag>(
            () => new DebugItem { Name = nameof(MultiConstrainedTag) },
            nameof(MultiConstrainedTag)
        );
    }

    [Fact]
    public void MultiConstrainedTag_CanAddToLocation()
    {
        AssertCanAddTag<MultiConstrainedTag>(
            () => new StartLocation { Name = nameof(MultiConstrainedTag) },
            nameof(MultiConstrainedTag)
        );
    }

    [Fact]
    public void MultiConstrainedTag_CanAddToPlacement()
    {
        AssertCanAddTag<MultiConstrainedTag>(
            () => new StartLocation { Name = nameof(MultiConstrainedTag) }.Wrap(),
            nameof(MultiConstrainedTag)
        );
    }

    [Fact]
    public void MultiConstrainedTag_CannotAddToCustomTaggable()
    {
        AssertCannotAddTag<MultiConstrainedTag>(
            () => new CustomTaggable(),
            nameof(MultiConstrainedTag)
        );
    }

    // Tests for CustomConstrainedTag
    [Fact]
    public void CustomConstrainedTag_CannotAddToItem()
    {
        AssertCannotAddTag<CustomConstrainedTag>(
            () => new DebugItem { Name = nameof(CustomConstrainedTag) },
            nameof(CustomConstrainedTag)
        );
    }

    [Fact]
    public void CustomConstrainedTag_CannotAddToLocation()
    {
        AssertCannotAddTag<CustomConstrainedTag>(
            () => new StartLocation { Name = nameof(CustomConstrainedTag) },
            nameof(CustomConstrainedTag)
        );
    }

    [Fact]
    public void CustomConstrainedTag_CannotAddToPlacement()
    {
        AssertCannotAddTag<CustomConstrainedTag>(
            () => new StartLocation { Name = nameof(CustomConstrainedTag) }.Wrap(),
            nameof(CustomConstrainedTag)
        );
    }

    [Fact]
    public void CustomConstrainedTag_CanAddToCustomTaggable()
    {
        AssertCanAddTag<CustomConstrainedTag>(
            () => new CustomTaggable(),
            nameof(CustomConstrainedTag)
        );
    }
}
