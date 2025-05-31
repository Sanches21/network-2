using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LocationDatabase", menuName = "LocationDataBase")]
public class LocationDataBase : ScriptableObject
{
    [SerializeField]
    private List<LocationInfo> _locations = new List<LocationInfo>();
    private Dictionary<int, LocationInfo> _locationDictionary;
    public int size { get => _locationDictionary.Count; }

    public void Initialize()
    {
        _locationDictionary = new Dictionary<int, LocationInfo>();
        foreach (var loc in _locations)
        {
            if (!_locationDictionary.ContainsKey(loc.id)) 
            {
                _locationDictionary.Add(loc.id, loc);
            }
            else
            {
                Debug.LogError($"Duplicate Location ID: {loc.id}");
            }
        }
    }

    public LocationInfo GetLocationData(int id)
    {
        if (_locationDictionary == null || _locationDictionary.Count == 0) {
            Debug.LogError("���� ������ �� ����������������");
        }

        if (!_locationDictionary.ContainsKey(id)) { Debug.LogError($"��� ������� � ID: {id}"); }

        return _locationDictionary.TryGetValue(id,out var location) ? location : null;
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        HashSet<int> ids = new();
        foreach (var loc in _locations)
        {
            if (ids.Contains(loc.id))
            {
                Debug.LogWarning($"������� � id: {loc.id} ��� ����������");
            }
            ids.Add(loc.id);
        }
    }

    [ContextMenu("�������������� id")]
    private void AutoAssignIDs()
    {
        for (int i = 0; i < _locations.Count; i++)
        {
            _locations[i].id = i + 1; // �������� � 1
        }
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

}
