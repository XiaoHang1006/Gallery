using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainBoard : MonoBehaviour
{
    private Image group;
    private float offset;
    
    void Start()
    {
        @group = transform.Find("Group").GetComponent<Image>();
         EventTrigger trigger = @group.gameObject.AddComponent<EventTrigger>();
        
         //注册down事件
        EventTrigger.Entry entry_pointerDown = new EventTrigger.Entry();
        entry_pointerDown.eventID = EventTriggerType.BeginDrag;
        entry_pointerDown.callback.AddListener(OnGroupBeginDrag);
        trigger.triggers.Add(entry_pointerDown);
        
        //注册click事件
        EventTrigger.Entry entry_pointerClick = new EventTrigger.Entry();
        entry_pointerClick.eventID = EventTriggerType.Drag;
        entry_pointerClick.callback.AddListener(OnGroupDrag);
        trigger.triggers.Add(entry_pointerClick);
        
        //注册Up事件
        EventTrigger.Entry entry_pointerUp = new EventTrigger.Entry();
        entry_pointerUp.eventID = EventTriggerType.EndDrag;
        entry_pointerUp.callback.AddListener(OnGroupEndDrag);
        trigger.triggers.Add((entry_pointerUp));

        selectIndex = @group.transform.childCount / 2;
        offset = @group.transform.GetChild(1).localPosition.x - @group.transform.GetChild(0).localPosition.x;
    }

    private Vector2 lastPos;
    private Vector3[] childLastPos = new Vector3[3];
    private int selectIndex;
    private bool beginDrag = false;
    /// <summary>
    /// group按下
    /// </summary>
    public void OnGroupBeginDrag(BaseEventData data)
    {
        Debug.Log("OnGroupBeginDrag");
        if(beginDrag) return;
        
        Vector3 touchPos = Input.mousePosition;
        Vector2 localPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(), touchPos,
            Camera.main, out localPos))
        {
            lastPos = localPos;
            int i = 0;
            foreach (Transform child in @group.transform)
            {
                childLastPos[i] = child.localPosition;
                i++;
            }

            beginDrag = true;
        }
    }

    /// <summary>
    /// group点击
    /// </summary>
    public void OnGroupDrag(BaseEventData data)
    {
        Debug.Log("OnGroupDrag");
        Vector3 touchPos = Input.mousePosition;
        Vector2 localPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(), touchPos,
            Camera.main, out localPos))
        {
            var xD = (localPos - lastPos).x;
            int index = 0;
            foreach (Transform child in @group.transform)
            {
                child.localPosition = childLastPos[index] + new Vector3(xD, 0, 0);
                index++;
            }
        }
       
    }
    
    /// <summary>
    /// group抬起
    /// </summary>
    public void OnGroupEndDrag(BaseEventData data)
    {
        Debug.Log("OnGroupEndDrag");
        
        if(!beginDrag)
            return;
        
        Vector3 touchPos = Input.mousePosition;
        Vector2 localPos;
        float xD = 0;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(), touchPos,
            Camera.main, out localPos))
        {
            xD = (localPos - lastPos).x;
            if (Mathf.Abs(xD) > GetComponent<RectTransform>().rect.width / 10)
            {
                if (xD > 0)
                {
                    selectIndex--;
                    if(this.selectIndex < 0)
                    {
                        this.selectIndex = this.selectIndex % this.@group.transform.childCount
                                           + this.@group.transform.childCount;
                    }else{
                        this.selectIndex = this.selectIndex % this.@group.transform.childCount;
                    }
                    Debug.Log("right");   
                }
                else
                {
                    selectIndex++;
                    if(this.selectIndex < 0)
                    {
                        this.selectIndex = this.selectIndex % this.@group.transform.childCount
                                           + this.@group.transform.childCount;
                    }else{
                        this.selectIndex = this.selectIndex % this.@group.transform.childCount;
                    }
                    Debug.Log("left");
                }
            }

            scaleCenter(selectIndex,xD);
        }

    }

    /// <summary>
    /// 动画移动
    /// </summary>
    /// <param name="index"></param>
    private void scaleCenter(int index,float xD)
    {
        if(Mathf.Abs(xD) < GetComponent<RectTransform>().rect.width / 10)
        {
            int i = 0;
            foreach (Transform child in @group.transform)
            {
                var des = childLastPos[i];
                TweenPosition tp = TweenPosition.Begin(child.gameObject, 0.5f, des);
                tp.easeType = EaseType.easeInQuad;
                i++;
            }
        }
        else
        {
            int i = 0;
            float ot = xD > 0 ? offset : -offset;
            foreach (Transform child in @group.transform)
            {
                var des = childLastPos[i] + new Vector3(ot, 0, 0);
                TweenPosition tp = TweenPosition.Begin(child.gameObject, 0.3f, des);
                tp.easeType = EaseType.easeInQuad;
                tp.SetOnFinished(() =>
                {
                    if (child.localPosition.x > (offset * (@group.transform.childCount / 2)))
                    {
                        child.localPosition = -offset * (@group.transform.childCount / 2) * Vector3.right;
                    }
                    else if (child.localPosition.x < (-offset * (@group.transform.childCount / 2)))
                    {
                        child.localPosition = offset * (@group.transform.childCount / 2) * Vector3.right;
                    }
                    beginDrag = false;
                });
            
                if (index == i)
                {
                    TweenScale ts = TweenScale.Begin(child.gameObject, 0.5f, Vector3.one * 1.04f);
                    ts.easeType = EaseType.easeInQuad;
                }
                else
                {
                    TweenScale ts = TweenScale.Begin(child.gameObject, 0.5f, Vector3.one * 0.82f);
                    ts.easeType = EaseType.easeInQuad;
                }

                i++;
            }
        }
    }
}
