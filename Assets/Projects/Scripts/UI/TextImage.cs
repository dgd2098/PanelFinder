using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextImage : MonoBehaviour
{
    public TextMeshProUGUI textElement;     // 텍스트 오브젝트
    public RectTransform backgroundImage;   // 배경 이미지 (예: Panel)

    public Vector2 padding = new Vector2(20f, 10f); // 텍스트 주위 여백 (좌우, 상하)

    void Start()
    {
        UpdateBackgroundSize();
    }

    public void UpdateBackgroundSize()
    {
        // 텍스트 렌더링 강제 업데이트 (필수)
        textElement.ForceMeshUpdate();

        // 텍스트의 요구 크기 계산
        float width = textElement.preferredWidth + padding.x * 2;
        float height = textElement.preferredHeight + padding.y * 2;

        // 이미지 크기 조정
        backgroundImage.sizeDelta = new Vector2(width, height);
    }
}
