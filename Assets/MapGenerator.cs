using Mirror;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEngine;

public class MapGenerator : NetworkBehaviour
{
    public GameObject[] mapPrefabs;    // Префабы для генерации
    private SystemPath systemPath = new SystemPath();

    private const string saveFileName = "map";
    private string saveFilePath;

    private JsonSerializerSettings jsonSettings;

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
        //Random.InitState(mapSeed); // Инициализация генератора случайных чисел

        int OrderLayers = 10500;

        float deltaX = 0.0f;
        float deltaY = 0.0f;

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                GameObject location = Instantiate(
                    mapPrefabs[Random.Range(0, mapPrefabs.Length)],
                    new Vector3(deltaX - (j * 1f), deltaY + (j * 0.5f)),
                    Quaternion.identity,
                    transform
                    );
                location.gameObject.transform.GetComponent<SpriteRenderer>().sortingOrder = OrderLayers;
                NetworkServer.Spawn(location);
                OrderLayers -= 1;

                Debug.Log("Сгенерирована локация " + location.name.Replace("(Clone)",""));
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
    private class LocationInfo
    {
        [JsonProperty("n")]
        public string _name;
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
        var locationList = new List<LocationInfo>();

        foreach (Transform location in transform)
        {
            LocationInfo locInfo = new LocationInfo();
            locInfo._name = location.name.Replace("(Clone)", "");
            locInfo.position = location.position;
            locationList.Add(locInfo);
        }

        string json = JsonConvert.SerializeObject(locationList);

        File.WriteAllText(saveFilePath, json);

        Debug.Log("Карта сохранена: " + json);
    }

    [Server]
    private void Load()
    {
        var locationList = new List<LocationInfo>();

        locationList = JsonConvert.DeserializeObject<List<LocationInfo>>(File.ReadAllText(saveFilePath));

        foreach (var locInfo in locationList)
        {
            var prefab = mapPrefabs.Where(p => p.name == locInfo._name).FirstOrDefault();

            GameObject location = Instantiate(
                    prefab,
                    locInfo.position,
                    Quaternion.identity,
                    transform
                    );
            NetworkServer.Spawn(location);

            Debug.Log("Загружена локация " + location.name.Replace("(Clone)", ""));
        }   
    }
    #endregion
}
