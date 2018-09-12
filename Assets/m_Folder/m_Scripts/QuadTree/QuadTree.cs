using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class QuadTree {

    /// <summary>
    /// 一个节点中最大的物体容量
    /// </summary>
    private int MAX_Objects = 10;

    /// <summary>
    /// 最深深度
    /// </summary>
    private int MAX_Depth = 5;
    [HideInInspector]
    /// <summary>
    /// 可视化网格
    /// </summary>
    public Bounds bounds;

    /// <summary>
    /// 节点位置
    /// </summary>
    public Vector3 center;

    /// <summary>
    /// 节点大小
    /// </summary>
    public Vector3 size;

    /// <summary>
    /// 深度
    /// </summary>
    public int depth = -1;

    /// <summary>
    /// 包含的物体列表
    /// </summary>
    public List<CollisionObject> objectsList;

    /// <summary>
    /// 子节点
    /// </summary>
    public List<QuadTree> childNodes;

    [SerializeField]
    /// <summary>
    /// 根节点
    /// </summary>
    private QuadTree rootTree;
    public QuadTree RootTree { get { return rootTree; } }
    public QuadTree(Vector3 center,Vector3 size,int depth)
    {
        this.center = center;
        this.size = size;
        this.depth = depth;
        bounds = new Bounds(center, size);
        objectsList = new List<CollisionObject>();
        childNodes = new List<QuadTree>();

        if(depth == -1)
        {
            rootTree = this;
        }
    }

    /// <summary>
    /// 分区
    /// </summary>
    private void Split()
    {
        float halfLength = (size.x * 0.5f);
        float halfWidth = (size.z * 0.5f);

        float x = (center.x);
        float y = (center.z);

        int currentDepth = depth + 1;
        //第一象限
        childNodes.Add(new QuadTree(new Vector3(x + halfLength * 0.5f, center.y, y + halfWidth * 0.5f), new Vector3(halfLength, size.y, halfWidth), currentDepth));
        //第二象限
        childNodes.Add(new QuadTree(new Vector3(x - halfLength * 0.5f, center.y, y + halfWidth * 0.5f), new Vector3(halfLength, size.y, halfWidth), currentDepth));
        //第三象限
        childNodes.Add(new QuadTree(new Vector3(x - halfLength * 0.5f, center.y, y - halfWidth * 0.5f), new Vector3(halfLength, size.y, halfWidth), currentDepth));
        //第亖象限
        childNodes.Add(new QuadTree(new Vector3(x + halfLength * 0.5f, center.y, y - halfWidth * 0.5f), new Vector3(halfLength, size.y, halfWidth), currentDepth));

        foreach (var item in childNodes)
        {
            item.rootTree = rootTree;
        }
    }

    /// <summary>
    /// 取得下一帧物体所在象限，如果为-1则表示其存在与象限的边界
    /// </summary>
    /// <param name="pBounds"></param>
    /// <returns></returns>
    public int GetIndex(CollisionObject pBounds)
    {
        int index = -1;

        float CenterX = center.x;
        float CenterY = center.z;
        float centerHalfLength = size.x * 0.5f;
        float centerHalfWidth = size.z * 0.5f;

        float boundsHalfLength = pBounds.size.x * 0.5f;
        float boundsHalfWidth = pBounds.size.z * 0.5f;

        float x = pBounds.center.x;
        float y = pBounds.center.z;

        //是否完全在一二象限,y坐标大于center的y坐标，并且不能超出象限
        bool topQuadrant = y > CenterY && y - CenterY >= boundsHalfWidth && y + boundsHalfWidth <= CenterY + centerHalfWidth;
        //是否完全在三四象限
        bool bottomQuadrant = y < CenterY && Mathf.Abs(y - CenterY) >= boundsHalfWidth && y - boundsHalfWidth >= CenterY - centerHalfWidth;
        //是否完全在一四象限
        bool rightQuadrant = x > CenterX && (x - CenterX) >= boundsHalfLength && x + boundsHalfLength <= CenterX + centerHalfLength;
        //是否完全在二三象限
        bool leftQuadrant = x < CenterX && Mathf.Abs(x - CenterX) >= boundsHalfLength && x - boundsHalfLength >= CenterX - centerHalfLength;

        if (topQuadrant && rightQuadrant)
            index = 0;
        else if (topQuadrant && leftQuadrant)
            index = 1;
        else if (bottomQuadrant && leftQuadrant)
            index = 2;
        else if (bottomQuadrant && rightQuadrant)
            index = 3;       

        pBounds.quadrant = index;

        return index;
    }

    /// <summary>
    /// 取得下一帧切割部分的象限，由于是下一帧，很可能用GetIndex得到的是-1
    /// </summary>
    /// <param name="pBounds"></param>
    /// <returns></returns>
    public int GetSliceIndex(CollisionObject pBounds)
    {
        int index = -1;

        float CenterX = center.x;
        float CenterY = center.z;
        float centerHalfLength = size.x * 0.5f;
        float centerHalfWidth = size.z * 0.5f;

        float boundsHalfLength = pBounds.size.x * 0.5f;
        float boundsHalfWidth = pBounds.size.z * 0.5f;

        float x = pBounds.center.x;
        float y = pBounds.center.z;

        //是否完全在一二象限,y坐标大于center的y坐标，可以超出象限
        bool topQuadrant = y >= CenterY;
        //是否完全在三四象限
        bool bottomQuadrant = y < CenterY;
        //是否完全在一四象限
        bool rightQuadrant = x >= CenterX;
        //是否完全在二三象限
        bool leftQuadrant = x < CenterX;

        if (topQuadrant && rightQuadrant)
            index = 0;
        else if (topQuadrant && leftQuadrant)
            index = 1;
        else if (bottomQuadrant && leftQuadrant)
            index = 2;
        else if (bottomQuadrant && rightQuadrant)
            index = 3;

        pBounds.quadrant = index;
        return index;
    }


    /// <summary>
    /// 插入
    /// </summary>
    /// <param name="_tree"></param>
    public void Insert(CollisionObject pBounds)
    {
        int index;
        //如果存在子节点,直接加入
        if (childNodes.Count > 0)
        {
            index = GetIndex(pBounds);

            if (-1 != index)
            {
                childNodes[index].Insert(pBounds);
                return;
            }

        }

        //加入根节点list
        objectsList.Add(pBounds);
        pBounds.SetCurrentTree(this);
        pBounds.SetDepth(this.depth);
       
        //分离的前提是子物体大于了最大容量，而且深度不为最深
        if (objectsList.Count > MAX_Objects && depth < MAX_Depth)
        {

            if (childNodes.Count == 0)
            {
               
                Split();
                //必须在操作List时使用倒序遍历，否则会出现索引的问题
                for (int i = objectsList.Count - 1; i >= 0; i--)
                {
                    index = GetIndex(objectsList[i]);
                    if(-1 != index)
                    {
                        CollisionObject co = objectsList[i];
                        objectsList.Remove(objectsList[i]);
                        childNodes[index].Insert(co);
                        
                    }
                }
            }
        }
    }

    /// <summary>
    /// 检测可能碰撞到的物体
    /// </summary>
    /// <param name="returnBounds"></param>
    /// <param name="pBounds"></param>
    /// <returns></returns>
    public List<CollisionObject> Retrieve(CollisionObject pBounds)
    {
        //切割的矩形列表
        List<CollisionObject> SliceArr = new List<CollisionObject>();
        List<CollisionObject> resultList = new List<CollisionObject>();

        if(childNodes.Count > 0)
        {
            int index = GetIndex(pBounds);

            if(index != -1)
            {
                resultList.AddRange(childNodes[index].Retrieve(pBounds));
            }
            else
            {
                SliceArr = pBounds.Carve(center.x, center.z);
                for (int i = SliceArr.Count - 1; i >= 0; i--)
                {

                    index = GetSliceIndex(SliceArr[i]);
                    //Debug.Log(pBounds.objectName + "的" + SliceArr[i].objectName + "在第" + index + "象限" + "深度为："+SliceArr[i].depth);
#if UNITY_EDITOR
                    if (-1 == index)
                    {
                        UnityEditor.EditorApplication.isPaused = true;

                        pBounds.drawColor = Color.red;
                        Debug.LogFormat("{0}的{1}", pBounds.objectName, SliceArr[i].objectName);
                        //Debug.LogFormat("{0}的{1}的象限{2}~~{3}~~{4}~~{5}", pBounds.objectName,SliceArr[i].objectName, topQuadrant, bottomQuadrant, rightQuadrant, leftQuadrant);
                    }
                    else
                        resultList.AddRange(childNodes[index].Retrieve(pBounds));
#else
                    resultList.AddRange(childNodes[index].Retrieve(pBounds));
#endif
                }
            }

        }
        resultList.AddRange(objectsList);
        return resultList;
    }

    /// <summary>
    /// 刷新列表功能
    /// </summary>
    public void Refresh()
    {

        //Debug.Log(depth + "深度" + center +"有：" + objectsList.Count + "个物体");
        for (int i = objectsList.Count - 1; i >= 0; i--)
        {
            int index = GetIndex(objectsList[i]);
            //如果换了节点就重新插入
            if (-1 == index)
            {
                if(this != rootTree)
                {
                    CollisionObject tmp = objectsList[i];
                    objectsList.Remove(objectsList[i]);
                    rootTree.Insert(tmp);
                   
                }
            }else
            {               
                if (childNodes.Count > 0)
                {
                    CollisionObject tmp = objectsList[i];
                    objectsList.Remove(objectsList[i]);
                    childNodes[index].Insert(tmp);
                }
                else
                {
                    CollisionObject tmp = objectsList[i];
                    objectsList.Remove(objectsList[i]);
                    this.Insert(tmp);
                }
            }
        }

        for (int i = 0; i < childNodes.Count; i++)
        {
            childNodes[i].Refresh();
        }
       
    }

    /// <summary>
    /// 清空节点
    /// </summary>
    public void Clear()
    {
        objectsList.Clear();
        
    }


}
[Serializable]
public class CollisionObject
{
    public string objectName;
    [HideInInspector]
    public Bounds bounds;
    public Vector3 size;
    public Vector3 center;
    public Vector3 velocity;
    private QuadTree currentQuadTree;
    public int quadrant = 0;
    [HideInInspector]
    public Color drawColor = Color.black;
    public int depth;
    private Vector3 haflSize;

    ///距离中心最远的X,Y值，防止穿透边界
    [HideInInspector]
    public Vector4 CenterLimits = Vector4.zero;
    public CollisionObject(Vector3 center, Vector3 size)
    {
        this.size = size;
        this.center = center;
        haflSize = size * 0.5f;
        bounds = new Bounds(center, size);
    }

    /// <summary>
    /// 可能碰撞到的物体列表
    /// </summary>
    List<CollisionObject> tempList = new List<CollisionObject>();

    /// <summary>
    /// 帧事件
    /// </summary>
    /// <param name="tree"></param>
    public void Update()
    {
        bounds.center = center;
        center += velocity * Time.deltaTime;


        //检测可能碰撞的物体
        tempList = currentQuadTree.RootTree.Retrieve(this);


        for (int i = 0; i < tempList.Count; i++)
        {
            if (!tempList[i].Equals(this))
                IsCollide(this, tempList[i]);

        }

        //检测与边界的碰撞
        Collide(currentQuadTree.RootTree.bounds);
    }

    /// <summary>
    /// 切割矩形
    /// </summary>
    /// <param name="currentTreeX"></param>
    /// <param name="currentTreeZ"></param>
    public List<CollisionObject> Carve(float currentTreeX, float currentTreeZ)
    {
        List<CollisionObject> result = new List<CollisionObject>();
        //判断在哪几个象限的交界处
        List<CollisionObject> _tempList = new List<CollisionObject>();
        //中点到Y轴的距离
        float dx = Mathf.Abs(center.x - currentTreeX);
        //中点到X轴的距离
        float dy = Mathf.Abs(center.z - currentTreeZ);

        bool carveX = dy > 0 && dy < size.z * 0.5f;
        bool carveY = dx > 0 && dx < size.x * 0.5f;

        float halfLength = size.x * 0.5f;
        float halfWidth = size.z * 0.5f;

        if (carveX && carveY)
        {
            //先纵切
            _tempList = Carve(currentTreeX, center.z);
            //Debug.Log("尺寸" + _tempList[0].size.x + "," + _tempList[0].size.z + "---中心" + _tempList[0].center.x + "," + _tempList[0].center.z);
            //Debug.Log("尺寸" + _tempList[1].size.x + "," + _tempList[1].size.z + "---中心" + _tempList[1].center.x + "," + _tempList[1].center.z);

            ///不知道为什么这里不能传入 _tempList[1].Carve(center.x, currentTreeZ)//////////////////////////////////
            //横切
            result.AddRange(_tempList[0].Carve(currentTreeX, currentTreeZ));
            //Debug.Log("尺寸" + tmp[0].size.x + "," + tmp[0].size.z + "---中心" + tmp[0].center.x + "," + tmp[0].center.z);
            //Debug.Log("尺寸" + tmp[1].size.x + "," + tmp[1].size.z + "---中心" + tmp[1].center.x + "," + tmp[1].center.z);

            //List<CollisionObject> tmp2 = new List<CollisionObject>();
            //tmp2 = _tempList[1].Carve(currentTreeX, currentTreeZ);
            //result.AddRange(tmp2);
            //Debug.Log("尺寸" + tmp2[0].size.x + "," + tmp2[0].size.z + "---中心" + tmp2[0].center.x + "," + tmp2[0].center.z);
            //Debug.Log("尺寸" + tmp2[1].size.x + "," + tmp2[1].size.z + "---中心" + tmp2[1].center.x + "," + tmp2[1].center.z);
            result.AddRange(_tempList[1].Carve(currentTreeX, currentTreeZ));
        }
        else if (carveX)
        {
            //宽的一半减去dy 就是小区块的宽
            float shortSideWidth = halfWidth - dy;
            float longSideWidth = halfWidth + dy;
            //小区块的Center，加上_tree的中心得到绝对坐标
            int direction = center.z > currentTreeZ ? -1 : 1;
            Vector3 shortSideCenter = new Vector3(center.x, 0, shortSideWidth * 0.5f * direction + currentTreeZ);
            Vector3 longSideCenter = new Vector3(center.x, 0, longSideWidth * 0.5f * -1 * direction + currentTreeZ);

            CollisionObject shortSide = new CollisionObject(shortSideCenter, new Vector3(size.x, size.y, shortSideWidth));
            shortSide.objectName = "YAxisShortSide";
            result.Add(shortSide);
            CollisionObject longSide = new CollisionObject(longSideCenter, new Vector3(size.x, size.y, longSideWidth));
            longSide.objectName = "YAxisLongSide";
            result.Add(longSide);

            /*开启此代码。可以看见被切分部分
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.transform.position = longSideCenter;
            g.transform.localScale = new Vector3(size.x,1,longSideWidth);
            */

        }
        else if (carveY)
        {
            float shortSideLength = halfWidth - dx;
            float longSideLength = halfWidth + dx;

            int direction = center.x > currentTreeX ? -1 : 1;
            Vector3 shortSideCenter = new Vector3(direction * shortSideLength * 0.5f + currentTreeX, 0, center.z);

            Vector3 longSideCenter = new Vector3(-1 * direction * longSideLength * 0.5f + currentTreeX, 0, center.z);

            CollisionObject shortSide = new CollisionObject(shortSideCenter, new Vector3(shortSideLength, size.y, size.z));
            shortSide.objectName = "XAxisShortSide";
            result.Add(shortSide);
            CollisionObject longSide = new CollisionObject(longSideCenter, new Vector3(longSideLength, size.y, size.z));
            longSide.objectName = "XAxisLongSide";
            result.Add(longSide);

            /*开启此代码。可以看见被切分部分
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.transform.position = longSideCenter;
            g.transform.localScale = new Vector3(longSideLength, 1, size.z);
            */
        }
        return result;
    }

    /// <summary>
    /// 碰撞改变方向
    /// </summary>
    /// <param name="object2"></param>
    public void Collide(CollisionObject object2)
    {

        var lengthSum = haflSize.x + object2.haflSize.x;
        var widthSum = haflSize.z + object2.haflSize.z;
        var dx = Mathf.Abs(center.x - object2.center.x);
        var dy = Mathf.Abs(center.z - object2.center.z);
        var dLength = lengthSum - dx;
        var dWidth = widthSum - dy;

        var onHorizontal = dLength >= 0;
        var onVertical = dWidth >= 0;
        var direction = 0;

        if (onHorizontal)
        {
            direction = center.x > object2.center.x ? 1 : -1;

            velocity.x = direction * (Mathf.Abs(velocity.x) + Mathf.Abs(object2.velocity.x)) * 0.5f;
        }

        if (onVertical)
        {
            direction = center.z > object2.center.z ? 1 : -1;

            velocity.z = direction * (Mathf.Abs(velocity.z) + Mathf.Abs(object2.velocity.z)) * 0.5f;
        }

        ChangeColor();
    }

    /// <summary>
    /// 与边界碰撞
    /// </summary>
    /// <param name="bounds"></param>
    public void Collide(Bounds bounds)
    {

        Vector3 centerTmp = center;

        if (Mathf.Abs(center.x - bounds.center.x) + haflSize.x >= bounds.size.x * 0.5f)
        {

            ChangeColor();
            velocity.x *= -1;
            //最小平移量，防止无限重叠   
            center.x = center.x < bounds.center.x ? CenterLimits.x : CenterLimits.y;
            center.y = centerTmp.y;
        }
        if (Mathf.Abs(center.z - bounds.center.z) + haflSize.z >= bounds.size.z * 0.5f)
        {

            ChangeColor();
            velocity.z *= -1;
            center.z = center.z < bounds.center.z ? CenterLimits.z : CenterLimits.w;
            center.y = centerTmp.y;
        }

    }

    /// <summary>
    /// 是否碰撞
    /// </summary>
    /// <param name="object1"></param>
    /// <param name="object2"></param>
    public void IsCollide(CollisionObject object1, CollisionObject object2)
    {

        if (Mathf.Abs(object1.center.x - object2.center.x) <= object1.haflSize.x + object2.haflSize.x &&
            Mathf.Abs(object1.center.z - object2.center.z) <= object1.haflSize.z + object2.haflSize.z)
        {

            object1.Collide(object2);
            object2.Collide(object1);
        }

    }

    public void SetDepth(int _depth)
    {
        depth = _depth;
    }

    public void SetCurrentTree(QuadTree _tree)
    {
        currentQuadTree = _tree;
    }


    private void ChangeColor()
    {
        Color col = new Color();
        col.r = UnityEngine.Random.Range(0.5f, 1f);
        col.g = UnityEngine.Random.Range(0.5f, 1f);
        col.b = UnityEngine.Random.Range(0.5f, 1f);
        col.a = 1;
        this.drawColor = col;
    }
}

public static class BoundsExtension
{
    public static Bounds DrawBounds(this Bounds bounds, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        return bounds;
    }
}
