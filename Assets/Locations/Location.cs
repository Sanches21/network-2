using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Location : NetworkBehaviour
{
    [SyncVar]
    private Vector2 _spawnPosition; // ������� ���������������� ��� ������
    [SyncVar]
    public int _id;

    private SpriteRenderer _spriteRenderer;

    /* ��������� ����� ��� ��������� */
    private Color normalColor = Color.white;
    private Color hoverColor = Color.red;
    /* ============================= */

    /* ������� ��� GameManager �� �������� ������� */
    [System.Serializable]
    public class LocationEvent : UnityEvent<Location> { }

    public static LocationEvent OnMouserCursorEnter = new LocationEvent();
    public static LocationEvent OnMouseCursorExit = new LocationEvent();
    /* ============================= */


    [Server]
    public void Init(Vector2 position, int id)
    {
        _spawnPosition = position;
        transform.position = position;
        _id = id;
    }

    public override void OnStartClient()
    {
        transform.position = _spawnPosition; // ������ �������� ������� ��� ��������

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = GameManager.instance.locationDB.GetLocationData(_id).sprite;
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
}
