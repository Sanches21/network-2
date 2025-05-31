using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance { get; private set; }
    public LocationDataBase locationDB;

    private Location _activeLocation;

    /* Описание и имя локаций справа сверху */
    [SerializeField]
    private GameObject _locationNameObject;
    [SerializeField]
    private GameObject _locationDescriptionObject;

    private TextMeshProUGUI _locationNameText;
    private TextMeshProUGUI _locationDescriptionText;
    /* ==================================== */

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            locationDB.Initialize();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (_locationDescriptionObject == null || _locationNameObject == null)
        {
            Debug.LogError("Не установлен UI объект для имени или описания локации");
        }

        _locationNameText = _locationNameObject.GetComponent<TextMeshProUGUI>();
        _locationDescriptionText = _locationDescriptionObject.GetComponent<TextMeshProUGUI>();

        SetupLocationEvent();
    }

    #region locationHover
    private void SetupLocationEvent()
    {
        Location.OnMouserCursorEnter.AddListener(SetActiveLocation);
        Location.OnMouseCursorExit.AddListener(ClearActiveLocation);
    }

    private void SetActiveLocation(Location location)
    {
        _activeLocation = location;

        LocationInfo locInfo = locationDB.GetLocationData(_activeLocation._id);

        _locationNameText.text = locInfo.displayName;
        _locationDescriptionText.text = locInfo.description;
    }

    private void ClearActiveLocation(Location location)
    {
        if (_activeLocation == location)
        {
            _activeLocation = null;

            _locationNameText.text = null;
            _locationDescriptionText.text = null;
        }
    }
    #endregion
}