using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;
using Unity.MLAgents.Policies;
public class ShadowAgent1 : Agent
{
    public GameObject[] shadowObjects;
    private GameObject currentObject;
    private Quaternion[] quaternions;
    public int curIndex;
    EnvironmentParameters m_ResetParams;
    StatsRecorder statsRecorder;
    void Start()
    {
        quaternions = new Quaternion[shadowObjects.Length];
        for (int i = 0; i < shadowObjects.Length; i++)
        {
            Transform shadowObjTransform = shadowObjects[i].transform.GetChild(0);
            quaternions[i] = shadowObjTransform.rotation;
        }

        statsRecorder = Academy.Instance.StatsRecorder;

        if (this.GetComponent<BehaviorParameters>().BehaviorType != BehaviorType.InferenceOnly)
        {
            m_ResetParams = Academy.Instance.EnvironmentParameters;
            curIndex = (int)m_ResetParams.GetWithDefault("object", 0);
        };

        for (int i = 0; i < shadowObjects.Length; i++)
        {
            if (i == curIndex)
            {
                shadowObjects[i].transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                shadowObjects[i].transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        currentObject = shadowObjects[curIndex];

    }

    public override void OnEpisodeBegin()
    {
        Quaternion curObjRotation = quaternions[curIndex];
        currentObject.GetComponentInChildren<ShadowObject>().ResetRotation(curObjRotation);
        Vector3 randomOffset = Vector3.zero;
        randomOffset[0] = UnityEngine.Random.Range(-90, 90);
        randomOffset[1] = UnityEngine.Random.Range(-90, 90);
        randomOffset[2] = UnityEngine.Random.Range(-90, 90);
        currentObject.transform.GetChild(0).Rotate(randomOffset, Space.World);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 rotation = Vector3.zero;
        int action = actions.DiscreteActions[0];
        if (action == 0) { rotation = Vector3.right; }
        else if (action == 1) { rotation = Vector3.left; }
        else if (action == 2) { rotation = Vector3.up; }
        else if (action == 3) { rotation = Vector3.down; }
        else if (action == 4) { rotation = Vector3.forward; }
        else if (action == 5) { rotation = Vector3.back; }

        Transform curObjTransform = currentObject.transform.GetChild(0);
        curObjTransform.Rotate(rotation, Space.World);
        Transform target1 = currentObject.transform.GetChild(1);
        Transform target2 = currentObject.transform.GetChild(2);
        float distanceToTarget = DistanceToTarget(curObjTransform, target1, target2);

        if (distanceToTarget <= Math.PI / 64)
        {
            double reward = 1 - distanceToTarget / (Math.PI / 8);
            AddReward((float)reward);
            statsRecorder.Add("Win", 1, StatAggregationMethod.Sum);
            EndEpisode();
        }
        else if (distanceToTarget <= Math.PI / 8) // avoid rewarding too far from goal 
        {
            double reward = 1 - distanceToTarget / (Math.PI / 8); // max of 0.98
            AddReward((float)reward);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int action = 6;
        var discreteActions = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D)) // Right
        {
            action = 0;
        }
        else if (Input.GetKey(KeyCode.A)) // Left
        {
            action = 1;
        }
        else if (Input.GetKey(KeyCode.W)) // Up
        {
            action = 2;
        }
        else if (Input.GetKey(KeyCode.S)) // Down
        {
            action = 3;
        }
        else if (Input.GetKey(KeyCode.UpArrow)) // Forward
        {
            action = 4;
        }
        else if (Input.GetKey(KeyCode.DownArrow)) // Back
        {
            action = 5;
        }
        discreteActions[0] = action;
    }

    private float DistanceToTarget(Transform curObjTransform, Transform target1, Transform target2)
    {
        Quaternion curObjQuaternion = curObjTransform.rotation.normalized;
        Quaternion target1Quaternion = target1.rotation.normalized;
        Quaternion target2Quaternion = target2.rotation.normalized;
        double innerProd1 = Quaternion.Dot(curObjQuaternion, target1Quaternion);
        double innerProd1_2 = Math.Pow(innerProd1, 2f);
        float innerProd2 = Quaternion.Dot(curObjQuaternion, target2Quaternion);
        double innerProd2_2 = Math.Pow(innerProd2, 2f);
        double distance1 = Math.Acos(2 * innerProd1_2 - 1);
        double distance2 = Math.Acos(2 * innerProd2_2 - 1);
        return (float)Math.Min(distance1, distance2);
    }

}
