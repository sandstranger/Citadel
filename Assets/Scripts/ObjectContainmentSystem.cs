using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

internal sealed class ObjectContainmentSystem
{
    private readonly List<GameObject> _floorChunks = new();
    private readonly List<GameObject> _activeFloorChunks = new();

    public void FindAllFloorGOs()
    {
        _floorChunks.Clear();

        var allParents = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < allParents.Length; i++)
        {
            Component[] compArray = allParents[i].GetComponentsInChildren(typeof(MarkAsFloor), true);
            for (int k = 0; k < compArray.Length; k++)
            {
                _floorChunks.Add(compArray[k].gameObject);
            }
        }
    }

    public void UpdateActiveFlooring()
    {
        if (_floorChunks.Count == 0)
        {
            FindAllFloorGOs();
        }

        _activeFloorChunks.Clear();
        for (int i = 0; i < _floorChunks.Count; i++)
        {
            if (_floorChunks[i] == null) continue;

            if (_floorChunks[i].activeInHierarchy) _activeFloorChunks.Add(_floorChunks[i]);
        }
    }

    public Vector3 FindNearestFloor(float x, float y, float heightFallback)
    {
        Vector3 checkpos;
        float distMin = 1000000f;
        for (int i = 0; i < _activeFloorChunks.Count; i++)
        {
            checkpos = _activeFloorChunks[i].transform.position;
            float deltax = x - checkpos.x;
            float deltay = y - checkpos.z; // Stupid Unity
            if (deltax < distMin) distMin = deltax;
            if (deltay < distMin) distMin = deltay;
            if (deltax < 1.28f && deltay < 1.28f) return checkpos; // Most likely the floor we fell through.
        }

        // Go back and find the floor closest to us.  Could have blasted away via explosion.
        for (int i = 0; i < _activeFloorChunks.Count; i++)
        {
            checkpos = _activeFloorChunks[i].transform.position;
            float deltax = x - checkpos.x;
            float deltay = y - checkpos.z; // Stupid Unity
            if (deltax - distMin < 1.28f) return checkpos;
            if (deltay - distMin < 1.28f) return checkpos;
        }

        return new Vector3(x, heightFallback, y);
    }

    public void ClearLists()
    {
        _floorChunks.Clear();
        _activeFloorChunks.Clear();
    }
}