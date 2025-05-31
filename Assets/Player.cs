using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private float speed = 10f;

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"����� ����������� (ID: {netId})");
        if (!isLocalPlayer) return;
        Camera.main.transform.SetParent(transform, true);
    }

    private void Update()
    {
        Vector2 moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        transform.Translate(moveInput.normalized * speed * Time.deltaTime);
    }

    public override void OnStopClient()
    {
        if (isLocalPlayer && Camera.main.transform != null)
        {
            // ���������� ������ ��� ����������� ������
            Camera.main.transform.SetParent(null);
        }
    }
}
