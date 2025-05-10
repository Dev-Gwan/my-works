using System;
using System.Collections;

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [Header("카메라 설정")]
    [SerializeField] Camera cam;
    [SerializeField] GameObject targetObject;
    [SerializeField] float cameraMoveSpeed = 0.1f;

    [Header("줌 설정")]
    [SerializeField] float zoomSpeed = 0.05f;
    [SerializeField] float zoomLerpSpeed = 10f;
    [SerializeField] float minZoom = 5f;
    [SerializeField] float maxZoom = 20f;

    bool isBuildMode = false;//현재 카메라 상태

    //원터치 조작
    Vector2 touchStartPosition;
    float touchThresholdTime = 0.2f;
    float touchThresholdDistance = 10f;
    float touchStartTime;

    //투터치 조작
    Vector3 centerPosition;
    Vector3 offsetDir;
    float zoomLevel;
    Vector3 currentZoomPos;

    private void OnEnable()
    {
        EventManager.instance.camModeChangeEvent += IsBuildMode;
    }

    private void OnDestroy()
    {
        EventManager.instance.camModeChangeEvent += IsBuildMode;
    }

    void Start()
    {
        offsetDir = (Quaternion.Euler(45f, 0f, 0f) * Vector3.back).normalized;

        centerPosition = targetObject != null ? targetObject.transform.position : Vector3.zero;

        zoomLevel = Vector3.Distance(transform.position, centerPosition);

        currentZoomPos = transform.position;
    }

    void Update()
    {
        if (IsTouchOverUI(0) || IsTouchOverUI(1)) return;//UI 터치일 경우 무시

        if (isBuildMode && Input.touchCount == 1)//빌드모드일 경우만 동작
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartTime = Time.time;
                touchStartPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)//터치
            {
                float delta = Vector2.Distance(touch.position, touchStartPosition);
                float touchTime = Time.time - touchStartTime;

                if (delta < touchThresholdDistance && touchTime < touchThresholdTime)
                {
                    //Debug.Log("Touch");

                    BuildObject(touch);
                }
            }
            else if (touch.phase == TouchPhase.Moved)//드래그
            {
                //Debug.Log("Drag");

                Vector2 delta = touch.deltaPosition;

                Vector3 move = new Vector3(-delta.x, 0, -delta.y) * (cameraMoveSpeed * Time.deltaTime);
                centerPosition += move;
            }
        }

        if (Input.touchCount == 2)//핀치줌
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 t0Prev = t0.position - t0.deltaPosition;
            Vector2 t1Prev = t1.position - t1.deltaPosition;

            float prevDistance = (t0Prev - t1Prev).magnitude;
            float currDistance = (t0.position - t1.position).magnitude;
            float delta = prevDistance - currDistance;

            zoomLevel += delta * zoomSpeed;
            zoomLevel = Mathf.Clamp(zoomLevel, minZoom, maxZoom);
        }

    }

    private void LateUpdate()
    {
        if (!isBuildMode && targetObject != null)
        {
            centerPosition = targetObject.transform.position;
        }

        Vector3 targetCamPos = centerPosition + offsetDir * zoomLevel;
        currentZoomPos = targetCamPos;
        transform.position = currentZoomPos;
    }

    void BuildObject(Touch touch)//물체 스폰만 담당
    {
        Ray ray = cam.ScreenPointToRay(touch.position);
        //Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1f); //레이케스트 디버그용

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            //Debug.Log(hit.point + " : 해당 위치에 물체를 스폰합니다.");

            PlacementManager.instance.PlacePreivewObject(hit.point);
        }
    }

    bool IsTouchOverUI(int touchIndex)
    {
        if (EventSystem.current == null || Input.touchCount <= touchIndex) return false;
        return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(touchIndex).fingerId);
    }

    public void IsBuildMode(bool value)//이벤트 매니저 사용.
    {
        isBuildMode = value;

        if (!isBuildMode)//빌드모드 ON Build UI
        {
            if (targetObject != null)//기본 모드로 돌아갈 때 카메라 원위치
            {
                centerPosition = targetObject.transform.position;
            }
        }
    }

    private IEnumerator FindOwnedPlayerObject() {
        while (true) {
            Player[] players = FindObjectsByType<Player>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (Player player in players) {
                if (player.IsOwned()) {
                    targetObject = player.gameObject;
                    centerPosition = targetObject.transform.position;
                    break;
                }
            }

            yield return new WaitForSeconds(0.5f);

            if (targetObject) {
                break;
            }
        }
    }
}