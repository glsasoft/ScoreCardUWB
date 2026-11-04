using System.Collections.Generic;
using LetsPiu.UI;
using UnityEngine;

public class ToastItemPool : MonoBehaviour
{
    public static ToastItemPool Instance { get; private set; }
    [SerializeField] private ToastItem toastPrefab;
    [SerializeField] private int initialSize = 1;   // 初始化生成的数量，后续如果想加多条直接打开ToastItemPool.cs和ToastItem.cs中的【备用--弹出多条飘字】函数
    [SerializeField] private Transform container;

    private readonly Queue<ToastItem> pool = new Queue<ToastItem>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (container == null)
        {
            container = GameObject.Find("ToastItemContainer")?.transform;
            if (container == null)
                Debug.LogError("ToastItemPool: 找不到 ToastItemContainer！");
        }

        if (toastPrefab == null)
            Debug.LogError("ToastItemPool: toastPrefab 未赋值！");

        PreWarm(initialSize);
    }

    private void PreWarm(int count)
    {
        for (int i = 0; i < count; i++) AddObject();
    }

    private ToastItem AddObject()
    {
        var item = Instantiate(toastPrefab, container);
        item.gameObject.SetActive(false);
        pool.Enqueue(item);
        return item;
    }
    /// <summary> 
    /// 【备用--弹出多条飘字】
    /// 如果池中没有对象，则创建一个新对象
    /// 如果池中有对象，则直接返回池中的对象
    /// </summary>
    // public GameObject GetItem()
    // {
    //     ToastItem item = pool.Count > 0 ? pool.Dequeue() : AddObject();
    //     item.gameObject.SetActive(true);
    //     return item.gameObject;
    // }

    /// <summary>
    /// 《根据策划需求仅获取一个对象》
    /// 如果池中没有对象，则从容器中获取第一个对象并回收
    /// 如果池中有对象，则直接返回池中的对象
    /// 如果容器中的对象数量小于初始大小，则创建新对象
    /// </summary>
    public GameObject GetItem()
    {
        ToastItem item = null;
        if (pool.Count > 0)
        {
            item = pool.Dequeue();
        }
        else if (transform.childCount < initialSize)
        {
            item = AddObject();
        }
        else
        {
            item = container.GetChild(0).GetComponent<ToastItem>();
            RecycleItem(item.gameObject);
            item = pool.Dequeue();
        }

        item.gameObject.SetActive(true);
        return item.gameObject;
    }

    public void RecycleItem(GameObject go)
    {
        go.SetActive(false);
        pool.Enqueue(go.GetComponent<ToastItem>());
    }
}