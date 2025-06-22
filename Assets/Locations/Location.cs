using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Location : NetworkBehaviour
{
    [SyncVar]
    private Vector2 _spawnPosition; // Позиция синхронизируется при спавне
    [SyncVar]
    public int _id;
    [SyncVar]
    private Vector2Int _gridPosition;

    public readonly SyncList<string> _playersOnLocation = new();

    private SpriteRenderer _spriteRenderer;

    /* Изменение цвета при наведении */
    private Color normalColor = Color.white;
    private Color hoverColor = Color.red;
    /* ============================= */

    /* Сигналы для GameManager об активной локации */
    [System.Serializable]
    public class LocationEvent : UnityEvent<Location> { }

    public static LocationEvent OnMouserCursorEnter = new LocationEvent();
    public static LocationEvent OnMouseCursorExit = new LocationEvent();
    /* ============================= */

    [Server]
    public void Init(Vector2 position, int id, Vector2Int gridPosition)
    {
        _spawnPosition = position;
        transform.position = position;
        _id = id;
        _gridPosition = gridPosition;

        Debug.Log("Инициализация локации успешна");
    }

    [Server]
    public void AddPlayer(string playerName)
    {
        _playersOnLocation.Add(playerName);
    }

    [Server]
    public void RemovePlayer(string playerName)
    { 
        _playersOnLocation.Remove(playerName); 
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _playersOnLocation.Callback += OnPlayersListChanged;
    }

    private void OnPlayersListChanged(SyncList<string>.Operation op, int index, string oldItem, string newItem)
    {
        // Обновляем UI при изменении списка
        GameManager.instance?.UpdateLocationPlayersUI(this);
    }

    public void Start()
    {
        transform.position = _spawnPosition; // Клиент получает позицию при создании

        _spriteRenderer.sprite = GameManager.instance.locationDB.GetLocationData(_id).sprite;

        var orderLayer = (int)-(transform.position.x + transform.position.y);
        _spriteRenderer.sortingOrder = orderLayer;
    }

    private void OnMouseEnter()
    {
        _spriteRenderer.color = hoverColor;

        OnMouserCursorEnter.Invoke(this);
    }

    private void OnMouseExit() 
    { 
        _spriteRenderer.color = normalColor;

        OnMouseCursorExit.Invoke(this);
    }

    public Vector2Int GetGridPos() { return _gridPosition; }

    public override string ToString()
    {
        return GameManager.instance.locationDB.GetLocationData(_id).displayName;
    }
}
