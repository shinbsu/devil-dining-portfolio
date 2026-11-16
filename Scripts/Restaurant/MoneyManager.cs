using UnityEngine;
using UnityEngine.UI;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [Header("시작 머니")]
    [SerializeField] private int startingMoney = 0;

    [Header("UI (Canvas 아래에 Text 컴포넌트)")]
    [SerializeField] private Text moneyText;  // 인스펙터에서 연결

    private int currentMoney;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentMoney = startingMoney;
        UpdateUI();
    }

    /// <summary>
    /// 머니를 추가합니다.
    /// </summary>
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateUI();
    }

    /// <summary>
    /// 머니를 소비합니다. (필요 시)
    /// </summary>
    public bool SpendMoney(int amount)
    {
        if (currentMoney < amount) return false;
        currentMoney -= amount;
        UpdateUI();
        return true;
    }

    private void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = currentMoney.ToString();
    }
}
