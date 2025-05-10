using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class PlacementManager : SingletonBase<PlacementManager>
{
    GameObject previewObject;
    GameObject selectedObject;

    bool isPlaceable;
    bool isPreviewPlacing;

    GameObject spawnedPreviewObject;

    private void OnEnable()
    {
        EventManager.instance.isPlaceableEvent += IsPlaceableCheck;
    }

    private void OnDisable()
    {
        EventManager.instance.isPlaceableEvent -= IsPlaceableCheck;
    }

    public void SetSelectedObject(GameObject gameObject)//상점에서 호출(재화 확인후)
    { 
        selectedObject = gameObject;
        previewObject = selectedObject;
        
        selectedObject.transform.GetChild(0).gameObject.SetActive(true);
        selectedObject.transform.GetChild(1).gameObject.SetActive(false);

        previewObject.transform.GetChild(1).gameObject.SetActive(false);
        previewObject.transform.GetChild(0).gameObject.SetActive(true);

        if (isPreviewPlacing)
        {
            Destroy(spawnedPreviewObject.transform.GetChild(0).gameObject);

            isPreviewPlacing = false;
            EventManager.instance.isPreviewPlaced(false);
        }
    }

    public void PlacePreivewObject(Vector3 touchPos)//프리뷰 설치
    {
        if(selectedObject == null) return;

        if (isPreviewPlacing)
        {
            spawnedPreviewObject.transform.position = touchPos;
        }
        else
        {
            spawnedPreviewObject = Instantiate(previewObject, touchPos, Quaternion.identity);
            isPreviewPlacing = true;
            EventManager.instance.isPreviewPlaced(true);
        }
    }

    public void PlaceSelectedObject()//물체 설치
    {
        if (!isPlaceable) return;

        Destroy(spawnedPreviewObject.transform.GetChild(0).gameObject);
        spawnedPreviewObject.transform.GetChild(1).gameObject.SetActive(true);

        isPreviewPlacing = false;
        EventManager.instance.isPreviewPlaced(false);
    }

    public void RotatePreviewObject()//물체 회전
    {
        spawnedPreviewObject.transform.Rotate(0,45f,0);
    }
    public void PlaceCancelSelectedObject()//설치 종료
    {
        Destroy(spawnedPreviewObject);
        isPreviewPlacing = false;
        EventManager.instance.isPreviewPlaced(false);
    }

    void IsPlaceableCheck(bool value)
    {
        isPlaceable = value;
    }
}
