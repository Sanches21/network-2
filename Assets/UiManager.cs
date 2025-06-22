using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    [Header("������� ��� ��������")]
    [SerializeField] 
    private TextMeshProUGUI _selLocName;
    [SerializeField] 
    private TextMeshProUGUI _selLocDescr;

    [Header("������� ������")]
    [SerializeField] 
    private TextMeshProUGUI _playerLocName;

    [Header("������ ������� �� �������")]
    [SerializeField]
    private TextMeshProUGUI _locationPlayersList;

    #region ������� ��� ��������
    public void UpldateSelectedLocation(LocationInfo locInfo)
    {
        if (locInfo == null)
        {
            ClearSelectedLocation();
            return;
        }

        _selLocName.text = locInfo.displayName;
        _selLocDescr.text = locInfo.description;
    }

    private void ClearSelectedLocation()
    {
        _selLocName.text = string.Empty;
        _selLocDescr.text = string.Empty;
    }

    #endregion

    #region ������� ������
    public void UpdatePlayerLocation(LocationInfo locInfo, Vector2Int locGridPos)
    {
        _playerLocName.text = $"�� � {locInfo.displayName} {locGridPos}";
    }
    #endregion

    public void UpdateLocationPlayersList(List<string> playersList)
    {
        _locationPlayersList.text = string.Empty;
        if (playersList.Count > 0)
        {
            playersList.ForEach(p => { _locationPlayersList.text += $"{p}\n"; });
        } 
        else
        {
            _locationPlayersList.text = "������ ���";
        }
    }
}

