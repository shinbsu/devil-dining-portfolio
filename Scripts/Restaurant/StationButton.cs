using UnityEngine;
using UnityEngine.UI;
using System;

public class StationButton : MonoBehaviour
{
    public CookingStation station;
    private Action<CookingStation> onStationConfirmed;
    private Action onServeConfirmed;
    private Button btn;
    private Text label;

    private void Awake()
    {
        btn = GetComponent<Button>();
        label = GetComponentInChildren<Text>();
        btn.onClick.AddListener(HandleClick);
    }

    /// <summary>
    /// 요리대 선택용 초기화
    /// </summary>
    public void Initialize(CookingStation station, Action<CookingStation> callback)
    {
        this.station = station;
        onStationConfirmed = callback;
        label.text = station.name;
    }

    /// <summary>
    /// 서빙 시작용 초기화
    /// </summary>
    public void InitializeAsServe(Action callback)
    {
        station = null;
        onServeConfirmed = callback;
        label.text = "Serve";
    }

    private void HandleClick()
    {
        if (station != null && onStationConfirmed != null)
            onStationConfirmed.Invoke(station);
        else if (station == null && onServeConfirmed != null)
            onServeConfirmed.Invoke();
    }
}