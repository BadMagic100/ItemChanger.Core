using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ItemChanger.Extensions;

/// <summary>
/// Extensions for Unity objects, and particularly for interacting with Scenes.
/// </summary>
public static class UnityExtensions
{
    /// <summary>
    /// Finds the direct child of a GameObject by name.
    /// </summary>
    public static GameObject? FindChild(this GameObject g, string name)
    {
        Transform t = g.transform.Find(name);
        return t != null ? t.gameObject : null;
    }

    /// <summary>
    /// Find all nested children by traversing the provided sequence of names.
    /// </summary>
    public static IEnumerable<GameObject> FindChildren(
        this GameObject g,
        IReadOnlyList<string> steps
    )
    {
        if (steps.Count == 0)
        {
            return [g];
        }

        List<Transform> queue = [g.transform];
        for (int i = 0; i < steps.Count && queue.Count > 0; i++)
        {
            string step = steps[i];
            List<Transform> newQueue = [];
            foreach (Transform t in queue)
            {
                foreach (Transform child in t)
                {
                    if (child.name == step)
                    {
                        newQueue.Add(child);
                    }
                }
            }
            queue = newQueue;
        }

        return queue.Select(t => t.gameObject);
    }

    /// <summary>
    /// Finds a nested child by traversing the provided sequence of names. Returns the first full-path match or else null.
    /// </summary>
    public static GameObject? FindChildOrNull(this GameObject g, IReadOnlyList<string> steps) =>
        g.FindChildren(steps).FirstOrDefault();

    /// <summary>
    /// Finds a nested child by traversing the provided sequence of names.
    /// </summary>
    public static GameObject FindChild(this GameObject g, IReadOnlyList<string> steps) =>
        g.FindChildren(steps).FirstOrDefault() ?? throw new System.NullReferenceException();

    /// <summary>
    /// Builds the full hierarchy path for the given transform.
    /// </summary>
    public static string GetPathInHierarchy(this Transform t)
    {
        if (t.parent == null)
        {
            return t.name;
        }
        else
        {
            return $"{t.parent.GetPathInHierarchy()}/{t.name}";
        }
    }

    /// <summary>
    /// Spawns a new game object in the scene. The scene must be loaded.
    /// </summary>
    /// <returns>The spawned GameObject</returns>
    public static GameObject NewGameObject(this Scene s)
    {
        GameObject go = new();
        SceneManager.MoveGameObjectToScene(go, s);
        return go;
    }

    /// <summary>
    /// Spawns a game object in the scene from a prefab. The scene must be loaded.
    /// </summary>
    /// <param name="s">The scene to spawn the object in</param>
    /// <param name="original">The prefab to be cloned</param>
    /// <returns>The spawned GameObject</returns>
    public static GameObject Instantiate(this Scene s, GameObject original)
    {
        GameObject go = Object.Instantiate(original);
        SceneManager.MoveGameObjectToScene(go, s);
        return go;
    }

    private static readonly List<GameObject> rootGameObjectPool = new(500);

    private static T ProcessRoots<T>(this Scene s, System.Func<IReadOnlyList<GameObject>, T> f)
    {
        if (s.rootCount > rootGameObjectPool.Capacity)
        {
            rootGameObjectPool.Capacity = s.rootCount;
        }
        s.GetRootGameObjects(rootGameObjectPool);
        T result = f(rootGameObjectPool);
        rootGameObjectPool.Clear();
        return result;
    }

    /// <summary>
    /// Find GameObjects in the given scene by their full path.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="path">The full path to the GameObject, with forward slash ('/') separators.</param>
    /// <returns></returns>
    public static IEnumerable<GameObject> FindGameObjects(this Scene s, string path)
    {
        int index = path.IndexOf('/');
        if (index == -1)
        {
            return s.ProcessRoots(rootObjects => rootObjects.Where(g => g.name == path).ToList());
        }

        string rootName = path[..index];
        IReadOnlyList<string> parts = path[(index + 1)..].Split('/');
        return s.ProcessRoots(rootObjects =>
            rootObjects
                .Where(g => g.name == rootName)
                .SelectMany(g => g.FindChildren(parts))
                .ToList()
        );
    }

    /// <summary>
    /// Finds a GameObject in the given scene by its full path.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="path">The full path to the GameObject, with forward slash ('/') separators.</param>
    /// <returns></returns>
    public static GameObject? FindGameObject(this Scene s, string path) =>
        s.FindGameObjects(path).FirstOrDefault();

    private static GameObject? FindChildViaBFS(IEnumerable<Transform> roots, string name)
    {
        Queue<Transform> queue = new();
        foreach (Transform t in roots)
        {
            queue.Enqueue(t);
        }

        while (queue.Count > 0)
        {
            Transform t = queue.Dequeue();
            foreach (Transform u in t)
            {
                if (u.name == name)
                {
                    return u.gameObject;
                }
                else
                {
                    queue.Enqueue(u);
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Breadth first search through the entire hierarchy. Returns the first GameObject with the given name, or null.
    /// </summary>
    public static GameObject? FindGameObjectByName(this Scene s, string name) =>
        s.ProcessRoots(rootObjects =>
        {
            List<Transform> queue = [];
            foreach (GameObject g in rootObjects)
            {
                if (g.name == name)
                {
                    return g;
                }
                queue.Add(g.transform);
            }

            return FindChildViaBFS(queue, name);
        });

    /// <summary>
    /// Returns a list of objects in the scene hierarchy, ordered by depth-first-search.
    /// <br/>The list consists of pairs where the first entry is the object path and the second entry is the object.
    /// </summary>
    public static List<(string path, GameObject go)> Traverse(this Scene s) =>
        s.ProcessRoots(rootObjects =>
        {
            List<(string, GameObject)> results = [];
            foreach (GameObject g in rootObjects)
            {
                TraverseInternal(string.Empty, g.transform, results);
            }
            return results;
        });

    private static void TraverseInternal(
        string path,
        Transform t,
        List<(string, GameObject)> results
    )
    {
        path = $"{path}/{t.name}";
        results.Add((path, t.gameObject));
        foreach (Transform u in t)
        {
            TraverseInternal(path, u, results);
        }
    }

    /// <summary>
    /// Breadth first search. Returns GameObject with given name, or null if not found. Parent object not included in search.
    /// </summary>
    public static GameObject? FindChildInHierarchy(this GameObject g, string name) =>
        FindChildViaBFS([g.transform], name);

    /// <summary>
    /// Create a new game object which is a parent to <paramref name="self"/>,
    /// such that the offset of self relative to the new object is equal to
    /// <paramref name="localOffset"/>.
    /// </summary>
    public static GameObject WithLocalOffset(this GameObject self, Vector3 localOffset)
    {
        GameObject newParent = self.scene.NewGameObject();
        newParent.name = $"{self.name} offset";
        self.transform.parent = newParent.transform;
        self.transform.localPosition = localOffset;

        return newParent;
    }
}
