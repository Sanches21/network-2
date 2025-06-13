using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"Игрок подключился (ID: {netId})");
        if (!isLocalPlayer) return;

        Camera.main.transform.SetParent(transform, false);
        Vector3 newCamPos = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
        Camera.main.transform.position = newCamPos;
        GameManager.instance.setActivePlayer(this);
    }

    private void Update()
    {
        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, Time.deltaTime * speed);

            //transform.position = Vector2.Lerp(transform.position, targetPos, Time.deltaTime * speed);


            if (Vector2.Distance(transform.position, targetPos) < stopDistance) isMoving = false;
        }
    }


    public void StartMovingToLocation(Vector2 locPos)
    {
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
