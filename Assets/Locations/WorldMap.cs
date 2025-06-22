using Mirror;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class WorldMap : NetworkBehaviour
{
    private const float SPRITE_WIDTH = 2.2f;
    private const float SPRITE_HEIGHT = 1.09f;

    private const int mapSize = 50;


    [SerializeField]
    private GameObject locationPrefab;
    private SystemPath systemPath = new SystemPath();

    private const string saveFileName = "map";
    private string saveFilePath;


    private Dictionary<Vector2Int, int> locationsPattern = new();


    public readonly SyncDictionary<Vector2Int, Location> locationGrid = new();


    private void Start()
    {
        saveFilePath = systemPath.GetFullSavePath(saveFileName);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        GenerateMap();
    }

    public Vector2Int GetLocationPos(Location location)
    {
        return locationGrid.FirstOrDefault(locPos => locPos.Value == location).Key;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) {
            GenerateMap();
        }
        if (Input.GetKeyDown(KeyCode.D)) {
            DeleteWorld();
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            Save();
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            Load();
        }
    }

    #region CreateDelete
    private void GenerateMapPattern()
    {
        locationsPattern.Clear();

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                int locId = Random.Range(1, GameManager.instance.locationDB.size + 1);

                locationsPattern.Add(new Vector2Int(x, y), locId);
            }
        }
    }

    [Server]
    private void GenerateMap()
    {
        DeleteWorld();

        GenerateMapPattern();

        foreach (var loc in locationsPattern)
        {
            var gridPos = loc.Key;
            var locId = loc.Value;

            //Ќереальна€ китайска€ магическа€ формула позовл€юща€ первратить квадратные координаты в изометрические
            var orderLayer = -(gridPos.x + gridPos.y);
            float newPosX = (gridPos.x - gridPos.y) * SPRITE_WIDTH / 2f;
            float newPosY = (gridPos.x + gridPos.y) * SPRITE_HEIGHT / 2f;
            Vector2 spawnPos = new Vector2(newPosX, newPosY);

            GameObject locationGo = Instantiate(locationPrefab,transform);

            var location = locationGo.GetComponent<Location>();
            location.Init(spawnPos, locId, gridPos);
            locationGo.transform.GetComponent<SpriteRenderer>().sortingOrder = orderLayer;
            NetworkServer.Spawn(locationGo);

            locationGrid.Add(gridPos, location);

            Debug.Log($"—генерирована локаци€: {location.name}, id: {location._id}");
        }
    }

    /**
     * ѕри использовании NetworkServer.Destroy() родительский объект почему-то становитс€ не активным
     * ѕоэтому был использован обычный Destroy() с синхронизацией у клиентов
     */
    [Server]
    private void DeleteWorld()
    {
        locationGrid.Clear();
        foreach (Transform location in transform)
        {
            Destroy(location.gameObject);
            RpcDeleteLocation(location.gameObject);
        }
    }

    [ClientRpc]
    private void RpcDeleteLocation(GameObject location)
    {
        if (!isServer)
        {
            Destroy(location);
        }
    }
    #endregion


    #region SaveLoad
    private class LocationSaveData
    {
        [JsonProperty("id")]
        public int _id;
        [JsonProperty("x")]
        public float _positionX;
        [JsonProperty("y")]
        public float _positionY;
        [JsonProperty("gx")]
        public int _gridPositionX;
        [JsonProperty("gy")]
        public int _gridPositionY;

        [JsonIgnore] //Newtonsoft.json по умолчанию не может сериализовать кастомные типы по типу vector2, код ниже создан лишь дл€ удобства
        public Vector2 position
        {
            get => new Vector2(_positionX, _positionY);
            set
            {
                _positionX = value.x;
                _positionY = value.y;
            }
        }

        [JsonIgnore]
        public Vector2Int gridPosition
        {
            get => new Vector2Int(_gridPositionX, _gridPositionY);
            set
            {
                _gridPositionX = value.x;
                _gridPositionY = value.y;
            }
        }
    }

    [Server]
    private void Save()
    {
        var locationList = new List<LocationSaveData>();

        foreach (Transform locTransform in transform)
        {
            LocationSaveData locData = new LocationSaveData();
            locData._id = locTransform.gameObject.GetComponent<Location>()._id;
            locData.position = locTransform.position;
            locationList.Add(locData);
        }

        string json = JsonConvert.SerializeObject(locationList);

        File.WriteAllText(saveFilePath, json);

        Debug.Log(" арта сохранена: " + json);
    }

    [Server]
    private void Load()
    {
        DeleteWorld();

        var locationSaveDataList = new List<LocationSaveData>();

        locationSaveDataList = JsonConvert.DeserializeObject<List<LocationSaveData>>(File.ReadAllText(saveFilePath));

        foreach (var locInfo in locationSaveDataList)
        {
            GameObject locationGo = Instantiate(
                    locationPrefab,
                    transform
                    );
            Location location = locationGo.GetComponent<Location>();
            location.Init(locInfo.position, locInfo._id, locInfo.gridPosition);
            NetworkServer.Spawn(locationGo);

            Debug.Log("«агружена локаци€ " + location.name);
        }   
    }
    #endregion
}
