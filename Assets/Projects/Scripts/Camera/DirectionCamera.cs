using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DirectionCamera : MonoBehaviour, IPointerClickHandler
{
    public Camera renderCamera; // RenderTexture를 생성하는 카메라
    public RectTransform rawImageRect; // RawImage의 RectTransform

    public void OnPointerClick(PointerEventData eventData)
    {
        // RawImage 내에서 클릭된 위치를 계산 (0~1 범위)
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImageRect, eventData.position, eventData.pressEventCamera, out localPoint
        );

        // RectTransform의 크기를 기준으로 (0~1) 범위를 픽셀 좌표로 변환
        Vector2 normalizedPoint = new Vector2(
            (localPoint.x / rawImageRect.rect.width) + 0.5f,
            (localPoint.y / rawImageRect.rect.height) + 0.5f
        );

        // 클릭한 좌표를 RenderTexture의 카메라 좌표로 변환
        Ray ray = renderCamera.ViewportPointToRay(normalizedPoint);

        // Raycast를 실행하여 버튼 감지
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Button")) // 버튼 오브젝트에 "Button" 태그를 설정해야 함
            {
                hit.collider.GetComponent<DirectionCube>().CunnectCubeMethod(); // 버튼 클릭 이벤트 실행
            }
        }
    }
}
