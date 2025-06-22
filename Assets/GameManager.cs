using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;
using System.Linq;

public class GameManager : MonoBehaviour 
{
    public static GameManager instance { get; private set; }
    public LocationDataBase locationDB;
    
    //Локация под курсором
    private Location _selectedLocation;
    //Локация на которой находится игрок
    private Location _playerLocation;
    //Игрок
    private Player _activePlayer;

    [SerializeField] private WorldMap _worldMap;
    //Весь UI здесь
    [SerializeField] private UiManager _uiManager;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            locationDB.Initialize();
        }
    }

    private void Start()
    {
        SetupLocationEvent();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            //Если локация не выбрана или игрока не существует (например онли хост)
            if (_selectedLocation == null || _activePlayer == null) return;

            if (_playerLocation == _selectedLocation) return;

            Vector2 newPos = _selectedLocation.transform.position;
            newPos.y += 1.09f/2f;
            _activePlayer.StartMovingToLocation(newPos);
        }
    }

    public void setActivePlayer(Player player)
    {
        _activePlayer = player;

        SetupPlayer();
    }

    public void SetupPlayer()
    {
        Location spawnLoc = _worldMap.locationGrid[new Vector2Int(0, 0)];

        Vector2 spawnPos = spawnLoc.transform.position;
        spawnPos.y += 1.09f / 2f;

        _activePlayer.transform.position = spawnPos;
        _playerLocation = spawnLoc;
        _activePlayer._currentLocation = spawnLoc;

        _activePlayer.onLocationChanged.AddListener(PlayerLocationChanged);

        var playerLocInfo = locationDB.GetLocationData(_playerLocation._id);
        _uiManager.UpdatePlayerLocation(playerLocInfo, _playerLocation.GetGridPos());

        _activePlayer.UpdateLocationPlayers(null, _playerLocation);
    }

    public void PlayerLocationChanged(Location newLoc)
    {
        _playerLocation = newLoc;

        var playerLocInfo = locationDB.GetLocationData(_playerLocation._id);
        _uiManager.UpdatePlayerLocation(playerLocInfo, _playerLocation.GetGridPos());
    }

    public void UpdateLocationPlayersUI(Location location)
    {
        if (_playerLocation == location)
        {
            _uiManager.UpdateLocationPlayersList(location._playersOnLocation.ToList());
        }
    }

    #region locationHover
    private void SetupLocationEvent()
    {
        Location.OnMouserCursorEnter.AddListener(SetSelectedLocation);
        Location.OnMouseCursorExit.AddListener(ClearSelectedLocation);
    }

    private void SetSelectedLocation(Location location)
    {
        _selectedLocation = location;
        LocationInfo locInfo = locationDB.GetLocationData(_selectedLocation._id);

        _uiManager.UpldateSelectedLocation(locInfo);
    }

    private void ClearSelectedLocation(Location location)
    {
        if (_selectedLocation == location)
        {
            _selectedLocation = null;
            _uiManager.UpldateSelectedLocation(null);
        }
    }
    #endregion
}