using Mirror;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MapGenerator : NetworkBehaviour
{
    private const int SPRITE_WIDTH = 220;
    private const int SPRITE_HEIGHT = 109;

    [SerializeField]
    private GameObject locationPrefab;
    private SystemPath systemPath = new SystemPath();

    private const string saveFileName = "map";
    private string saveFilePath;


    private Dictionary<Vector2Int, Location> locationsGrid = new();


    private void Start()
    {
        saveFilePath = systemPath.GetFullSavePath(saveFileName);
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
        if (Input.GetKeyDown(KeyCode.L))
        {
            Load();
        }
    }

    #region CreateDelete
    [Server]
    private void GenerateMap()
    {
        DeleteWorld();
        //Random.InitState(mapSeed); // Инициализация генератора случайных чисел

        int OrderLayers = 10500;

        int size = 50;

        float deltaX = 0.0f;
        float deltaY = 0.0f;

        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                Vector2 pos = new Vector2((deltaX + col)*SPRITE_WIDTH/200, (deltaY - (col * 0.5f))*SPRITE_HEIGHT/100);
                GameObject locationGo = Instantiate(
                    locationPrefab,
                    transform
                    );

                int locationId = Random.Range(1, GameManager.instance.locationDB.size + 1);
                Location location = locationGo.GetComponent<Location>();

                location.Init(pos, locationId);
                locationGo.transform.GetComponent<SpriteRenderer>().sortingOrder = OrderLayers;
                NetworkServer.Spawn(locationGo);
                OrderLayers -= 1;

                Debug.Log($"Сгенерирована локация: {location.name}, id: {location._id}");
            }
            deltaX += 1f;
            deltaY += 0.5f;
        }
    }

    /**
     * При использовании NetworkServer.Destroy() родительский объект почему-то становится не активным
     * Поэтому был использован обычный Destroy() с синхронизацией у клиентов
     */
    [Server]
    private void DeleteWorld()
    {
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

        [JsonIgnore] //Newtonsoft.json по умолчанию не может сериализовать кастомные типы по типу vector2, код ниже создан лишь для удобства
        public Vector2 position
        {
            get => new Vector2(_positionX, _positionY);
            set
            {
                _positionX = value.x;
                _positionY = value.y;
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

        Debug.Log("Карта сохранена: " + json);
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
            location.Init(locInfo.position, locInfo._id);
            NetworkServer.Spawn(locationGo);

            Debug.Log("Загружена локация " + location.name);
        }   
    }
    #endregion
}
