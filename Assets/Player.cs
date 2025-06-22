using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Player : NetworkBehaviour
{
    [Header("Настройки перемещения")]
    [Tooltip("Скорость перемещения")]
    [SerializeField]
    private float speed = 10f;

    [Tooltip("Расстояние, при котором объект считает что достиг цели")]
    [SerializeField]
    private float stopDistance = 0.01f;

    private bool isMoving = false;
    private Vector2 targetPos;

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    //СОБЫТИЯ ПЕРЕХОДА НА ЛОКАЦИЮ
    [System.Serializable]
    public class LocationEvent : UnityEvent<Location> { }
    public LocationEvent onLocationChanged = new LocationEvent();

    public Location _currentLocation;

    //--------------------------

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"Игрок подключился (ID: {netId})");
        if (!isLocalPlayer)
        {
            _spriteRenderer.enabled = false;
            return;
        }

        Camera.main.transform.SetParent(transform, false);
        Vector3 newCamPos = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
        Camera.main.transform.position = newCamPos;
        GameManager.instance.setActivePlayer(this);

        CheckLocationChanged();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, Time.deltaTime * speed);
            CheckLocationChanged();

            if (Vector2.Distance(transform.position, targetPos) < stopDistance) isMoving = false;
        }
    }

    private void CheckLocationChanged()
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(transform.position);

        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<Location>(out Location newLocation))
            {
                if (newLocation != _currentLocation)
                {
                    UpdateLocationPlayers(_currentLocation, newLocation);

                    _currentLocation = newLocation;
                    onLocationChanged.Invoke(newLocation);
                }
                break;
            }
        }
    }

    [Command]
    public void UpdateLocationPlayers(Location oldLoc, Location newLoc) 
    {
        Debug.Log($"{name}: ушел с локации {oldLoc} на {newLoc}");

        if (oldLoc != null) oldLoc._playersOnLocation.Remove(name);
        newLoc._playersOnLocation.Add(name);
    }

    public void StartMovingToLocation(Vector2 locPos)
    {
        if (isMoving) return;

        isMoving = true;
        targetPos = locPos;

        Debug.Log($"Игрок начал движение на позицию {locPos}");
    }

    public override void OnStopClient()
    {
        if (isLocalPlayer && Camera.main.transform != null)
        {
            // Открепляем камеру при уничтожении игрока
            Camera.main.transform.SetParent(null);
        }
    }
}
