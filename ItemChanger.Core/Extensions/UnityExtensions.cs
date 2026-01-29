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
    /// Finds a nested child by traversing the provided sequence of names.
    /// </summary>
    public static GameObject FindChild(this GameObject g, IEnumerable<string> steps)
    {
        Transform t = g.transform;
        foreach (string s in steps)
        {
            t = t.Find(s);
        }

        return t.gameObject;
    }

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

    private static List<GameObject> GetRoots(this Scene s)
    {
        if (s.rootCount > rootGameObjectPool.Count)
        {
            rootGameObjectPool.Capacity = s.rootCount;
        }
        s.GetRootGameObjects(rootGameObjectPool);
        return rootGameObjectPool;
    }

    /// <summary>
    /// Finds a GameObject in the given scene by its full path.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="path">The full path to the GameObject, with forward slash ('/') separators.</param>
    /// <returns></returns>
    public static GameObject? FindGameObject(this Scene s, string path)
    {
        List<GameObject> rootObjects = s.GetRoots();
        int index = path.IndexOf('/');
        GameObject? result = null;
        if (index >= 0)
        {
            string rootName = path.Substring(0, index);
            GameObject? root = rootObjects.FirstOrDefault(g => g.name == rootName);
            if (root != null)
            {
                result = root.transform.Find(path.Substring(index + 1)).gameObject;
            }
        }
        else
        {
            result = rootObjects.FirstOrDefault(g => g.name == path);
        }
        rootObjects.Clear();
        return result;
    }

    /// <summary>
    /// Breadth first search through the entire hierarchy. Returns the first GameObject with the given name, or null.
    /// </summary>
    public static GameObject? FindGameObjectByName(this Scene s, string name)
    {
        List<GameObject> rootObjects = s.GetRoots();
        GameObject? result = null;
        foreach (GameObject g in rootObjects)
        {
            if (g.name == name)
            {
                result = g;
                break;
            }
        }
        if (result == null)
        {
            foreach (GameObject g in rootObjects)
            {
                result = g.FindChildInHierarchy(name);
                if (result != null)
                {
                    break;
                }
            }
        }
        rootObjects.Clear();
        return result;
    }

    /// <summary>
    /// Returns a list of objects in the scene hierarchy, ordered by depth-first-search.
    /// <br/>The list consists of pairs where the first entry is the object path and the second entry is the object.
    /// </summary>
    public static List<(string path, GameObject go)> Traverse(this Scene s)
    {
        List<GameObject> rootObjects = s.GetRoots();
        List<(string, GameObject)> results = new();
        foreach (GameObject g in rootObjects)
        {
            TraverseInternal(string.Empty, g.transform, results);
        }
        return results;
    }

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
    public static GameObject? FindChildInHierarchy(this GameObject g, string name)
    {
        Queue<Transform> q = new();
        q.Enqueue(g.transform);
        while (q.Count > 0)
        {
            Transform t = q.Dequeue();
            foreach (Transform u in t)
            {
                if (u.name == name)
                {
                    return u.gameObject;
                }
                else
                {
                    q.Enqueue(u);
                }
            }
        }
        return null;
    }

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
