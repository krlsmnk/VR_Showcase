﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class pushOrder : MonoBehaviour {
    public GameObject nextMural;
    private mural[] murals;
    int muralNum;
    private targetHUD theTargetHUD;


    void Awake() {
        VRTK_SDKManager.instance.AddBehaviourToToggleOnLoadedSetupChange(this);
    }
    void OnDestroy() {
        VRTK_SDKManager.instance.RemoveBehaviourToToggleOnLoadedSetupChange(this);
    }

	// Use this for initialization
	void OnEnable () {
        murals = GameObject.FindGameObjectWithTag("mural").GetComponentsInChildren<mural>();
        nextMural = murals[0].gameObject;
        muralNum = 0;
        theTargetHUD = FindObjectOfType<targetHUD>();

        for (int i = 0; i < murals.Length; i++) {
            murals[i].touchOrder = i;
        }

        //targetHUD thisTargetHUD = this.gameObject.AddComponent<targetHUD>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    internal void advanceMural()
    {
        if (muralNum == murals.Length - 1) endSimulation();
        else
        {
            nextMural = murals[++muralNum].gameObject;
            theTargetHUD.showTarget(nextMural);
        }
    }

    private void endSimulation()
    {
        Debug.Log("Completion Time: " + Time.realtimeSinceStartup);
        theTargetHUD.showGameOverMessage();
    }
}
