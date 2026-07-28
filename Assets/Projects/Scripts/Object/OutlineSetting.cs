using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineSetting : MonoBehaviour
{
    public GameObject objA;
    public GameObject objB;
    public Material borderMaterial; // 경계선용 머티리얼

    public float detectDistance = 0.01f;  // 감지 거리
    public LayerMask detectLayer;       // 감지할 레이어 지정

    public MeshRenderer[] renderers;

    void Start()
    {
        Renderer aRender = transform.GetComponent<Renderer>();

        float aDisX = aRender.bounds.max.x - aRender.bounds.min.x;
        float aDisY = aRender.bounds.max.y - aRender.bounds.min.y;
        float aDisZ = aRender.bounds.max.z - aRender.bounds.min.z;

        //오브젝트의 중심 위치
        float aCenterX = aRender.bounds.min.x + (aDisX * 0.5f);
        float aCenterY = aRender.bounds.min.y + (aDisY * 0.5f);
        float aCenterZ = aRender.bounds.min.z + (aDisZ * 0.5f);

        Vector3 posA = new Vector3(aCenterX, aCenterY, aCenterZ);

        GameObject border = new GameObject("");
        border.transform.position = posA;
        border.transform.parent = transform.parent;
        transform.parent = border.transform;

        //CheckDirections(border);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach(MeshRenderer mr in renderers)
            {
                CreateBorderBetween(transform.gameObject, mr.gameObject);
            }
        }
    }

    private void CreateBorderBetween(GameObject a, GameObject b)
    {
        Renderer aRender = a.GetComponent<Renderer>();
        Renderer bRender = b.GetComponent<Renderer>();

        float aDisX = aRender.bounds.max.x - aRender.bounds.min.x;
        float bDisX = bRender.bounds.max.x - bRender.bounds.min.x;

        float aDisY = aRender.bounds.max.y - aRender.bounds.min.y;
        float bDisY = bRender.bounds.max.y - bRender.bounds.min.y;

        float aDisZ = aRender.bounds.max.z - aRender.bounds.min.z;
        float bDisZ = bRender.bounds.max.z - bRender.bounds.min.z;

        //오브젝트의 중심 위치
        float aCenterX = aRender.bounds.min.x + (aDisX * 0.5f);
        float aCenterY = aRender.bounds.min.y + (aDisY * 0.5f);
        float aCenterZ = aRender.bounds.min.z + (aDisZ * 0.5f);

        float bCenterX = bRender.bounds.min.x + (bDisX * 0.5f);
        float bCenterY = bRender.bounds.min.y + (bDisY * 0.5f);
        float bCenterZ = bRender.bounds.min.z + (bDisZ * 0.5f);

        Vector3 posA = new Vector3(aCenterX, aCenterY, aCenterZ);
        Vector3 posB = new Vector3(bCenterX, bCenterY, bCenterZ);

        Vector3 delta = posB - posA;

        //가로, 세로만 비교 가로세로 동시에 값이 다른 경우 X

        float xDis = Mathf.Abs(aCenterX) - Mathf.Abs(bCenterX);
        float yDis = Mathf.Abs(aCenterY) - Mathf.Abs(bCenterY);
        string direc = "";

        if(Mathf.Abs(xDis) != 0 && Mathf.Abs(xDis) < 1 && Mathf.Abs(yDis) < 0.01f)
        {
            direc = "가로";
        }
        else if (Mathf.Abs(yDis) != 0 && Mathf.Abs(yDis) < 1 && Mathf.Abs(xDis) < 0.01f)
        {
            direc = "세로";
        }
        else
        {
            return;
        }

        // 두 위치의 중간 지점
        Vector3 midPoint = (posA + posB) / 2f;

        // 두 지점 사이 방향
        Vector3 direction = (posB - posA).normalized;

        // 경계선 오브젝트 생성
        GameObject border = GameObject.CreatePrimitive(PrimitiveType.Cube);
        border.transform.position = midPoint;

        // 경계선 크기 설정 (두 점 사이 거리 중 하나의 축 방향으로 얇게)
        float thickness = 0.003f; // 얇은 경계선
        Vector3 size = Vector3.one;

        // 오브젝트가 어느 방향으로 인접한지 계산 (x, y, z 중 큰 차이축)

        //if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y) && Mathf.Abs(delta.x) > Mathf.Abs(delta.z))
        //{
        //    size = new Vector3(thickness, aDisY, (aDisZ + 0.01f)); // x축 사이에 있으므로 두께만 x축
        //}
        //else if (Mathf.Abs(delta.y) > Mathf.Abs(delta.z))
        //{
        //    size = new Vector3(aDisX, thickness, (aDisZ + 0.01f));
        //}
        //else
        //{
        //    size = new Vector3(aDisX, aDisY, thickness);
        //}

        if(direc == "가로")
        {
            size = new Vector3(thickness, aDisY, (aDisZ + 0.01f)); // x축 사이에 있으므로 두께만 x축
        }
        else if (direc == "세로")
        {
            size = new Vector3(aDisX, thickness, (aDisZ + 0.01f));
        }
        else
        {
            Debug.Log("세로입니다");
        }


        border.transform.localScale = size;
        border.transform.parent = transform;

        // 머티리얼 적용
        if (borderMaterial != null)
        {
            border.GetComponent<Renderer>().material = borderMaterial;
        }

        border.name = "BorderBetween_" + a.name + "_and_" + b.name;
    }

    void CheckDirections(GameObject centerObject)
    {
        Renderer aRender = transform.GetComponent<Renderer>();

        float aDisX = aRender.bounds.max.x - aRender.bounds.min.x;
        float aDisY = aRender.bounds.max.y - aRender.bounds.min.y;
        float aDisZ = aRender.bounds.max.z - aRender.bounds.min.z;

        Vector3 myPos = centerObject.transform.position;
        Vector3 boxHalfSize = new Vector3((aDisX * 0.5f), (aDisY * 0.5f), (aDisZ * 0.5f));

        // 방향 벡터 설정
        Vector3[] directions = {
            Vector3.forward,  // 앞 (Z+)
            Vector3.back,     // 뒤 (Z-)
            Vector3.left,     // 왼 (X-)
            Vector3.right     // 오 (X+)
        };

        string[] labels = { "앞", "뒤", "왼", "오" };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 checkPos = myPos + directions[i] * detectDistance;

            Collider[] hits = Physics.OverlapBox(checkPos, boxHalfSize, Quaternion.identity);

            foreach (var hit in hits)
            {
                if (hit.gameObject != gameObject && !string.IsNullOrWhiteSpace(hit.gameObject.name))
                {
                    Debug.Log($"{labels[i]} 방향에 오브젝트 감지됨: {hit.gameObject.name}");

                    //CreateBorderBetween(transform.gameObject, hit.transform.gameObject);
                }
            }
        }
    }
}
