using UnityEngine;
using UnityEngine.UI;

public class RecipeUnlockButton : MonoBehaviour
{
    [SerializeField] private Button unlockButton;
    [SerializeField] private GameObject confirmationPanel; // 확인 대화상자 (선택사항)

    private void Start()
    {
        // 버튼이 직접 연결되지 않았다면 GetComponent로 찾기
        if (unlockButton == null)
            unlockButton = GetComponent<Button>();

        // 버튼에 클릭 이벤트 리스너 추가
        if (unlockButton != null)
        {
            unlockButton.onClick.AddListener(ShowUnlockConfirmation);
        }

        // 확인 대화상자가 있다면 초기에 비활성화
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }

    public void ShowUnlockConfirmation()
    {
        // 확인 대화상자가 있는 경우
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
        }
        else
        {
            // 직접 해금 진행
            UnlockAllRecipes();
        }
    }

    public void UnlockAllRecipes()
    {
        // CookingManager가 싱글톤으로 구현되어 있으므로 Instance로 접근
        if (CookingManager.Instance != null)
        {
            CookingManager.Instance.UnlockAllRecipesForEditor();

            // 확인 대화상자가 있다면 비활성화
            if (confirmationPanel != null)
                confirmationPanel.SetActive(false);

            // 선택적: 해금 완료 알림
            Debug.Log("모든 레시피가 해금되었습니다!");
        }
        else
        {
            Debug.LogError("CookingManager를 찾을 수 없습니다!");
        }
    }

    // 확인 대화상자에서 사용할 취소 메서드
    public void CancelUnlock()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }
}