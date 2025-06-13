using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour 
{
    public static GameManager instance { get; private set; }
    public LocationDataBase locationDB;
    
    private Location _activeLocation;
    private Player _activePlayer;

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

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (_activeLocation == null || _activePlayer == null) 
                return;


            Vector2 newPos = _activeLocation.transform.position;
            newPos.y += 1.09f/2f;
            _activePlayer.StartMovingToLocation(newPos);
        }
    }

    public void setActivePlayer(Player player)
    {
        _activePlayer = player;
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