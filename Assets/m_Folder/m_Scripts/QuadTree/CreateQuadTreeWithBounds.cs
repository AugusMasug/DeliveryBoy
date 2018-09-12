using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateQuadTreeWithBounds : MonoBehaviour {

    private Transform thisTransform;
    private QuadTree tree;
    [Header("四叉树最大范围")]
    [SerializeField]
    private float xWidth = 2;
    [SerializeField]
    private float yWidth = 2;

    [Header("子物体属性")]
    [SerializeField]
    private int ObjectCount = 20;
    [SerializeField]
    private float boundsSizeFactor = 0.5f;
    [SerializeField]
    private float maxXVelocity = 5;
    [SerializeField]
    private float maxYVelocity = 5;
    public bool ShowBounds = true;
    public CollisionObject[] objects;

    private void Start()
    {
        thisTransform = transform;
        tree = new QuadTree(thisTransform.position, new Vector3(xWidth, 1, yWidth), -1);
        //初始化bounds
        objects = new CollisionObject[ObjectCount];

        ///距离中心最远的X,Y值，防止穿透边界。 xy为X的最大最小，zw为Z的最大最小
         Vector4 CenterLimits = Vector4.zero;
        float xLimits = tree.size.x * 0.5f - boundsSizeFactor * 0.5f;
        CenterLimits.x = tree.center.x - xLimits;
        CenterLimits.y = tree.center.x + xLimits;

        float yLimits = tree.size.z * 0.5f - boundsSizeFactor * 0.5f;
        CenterLimits.z = tree.center.z - yLimits;
        CenterLimits.w = tree.center.z + yLimits;

        for (int i = 0; i < ObjectCount; i++)
        {
            //初始化Transform
            int halfLength = (int)(tree.size.x * 0.5f);
            int halfWidth = (int)(tree.size.z * 0.5f);
            float x = Random.Range(tree.center.x - halfLength + boundsSizeFactor + 0.5f, tree.center.x + halfLength - boundsSizeFactor - 0.5f);
            float z = Random.Range(tree.center.z - halfWidth + boundsSizeFactor + 0.5f, tree.center.z + halfWidth - boundsSizeFactor - 0.5f);
            //实例化物体
            Vector3 center = new Vector3(x, tree.center.y, z);
            objects[i] = new CollisionObject(center, Vector3.one * boundsSizeFactor);
            //初始化速度
            int _xVelocity = (int)Random.Range(1, maxXVelocity);
            int _yVelocity = (int)Random.Range(1, maxYVelocity);
            objects[i].velocity = new Vector3(_xVelocity, 0, _yVelocity);
            //初始化XY限制
            objects[i].CenterLimits = CenterLimits;
            //初始化名称
            objects[i].objectName = string.Format("第{0}个物体", i + 1);
        }

        for (int i = 0; i < ObjectCount; i++)
        {
            //插入节点
            tree.Insert(objects[i]);
        }
    }

    private void Update()
    {
        //更新四叉树
        tree.Refresh();
        //Bounds移动碰撞
        for (int i = 0; i < objects.Length; i++)
        {
            objects[i].Update();

        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {

        if (UnityEditor.EditorApplication.isPlaying)
        {
            if (ShowBounds)
            {
                DrawChildNode(tree.childNodes);
            }
            foreach (var b in objects)
            {
                b.bounds.DrawBounds(b.drawColor);
                           
            }

        }
        else
        {
            
            Gizmos.DrawWireCube(transform.position, new Vector3(xWidth, 1, yWidth));
        }
    }

    private void DrawChildNode(List<QuadTree> _tree)
    {
        if (_tree.Count > 0)
        {

            foreach (var b in _tree)
            {
                b.bounds.DrawBounds(Color.white);
                DrawChildNode(b.childNodes);
            }
        }
        else
        {
            tree.bounds.DrawBounds(Color.black);
        }
    }
#endif

}
