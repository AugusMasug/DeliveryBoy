using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        AudioCreator.PlayAudio("Test", "1", true).OnComplete(()=> { print("完成"); });
    }

    // Update is called once per frame
    void Update () {
		
	}
}
