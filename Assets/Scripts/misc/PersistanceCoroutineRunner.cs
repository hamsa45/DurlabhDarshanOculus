using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistanceCoroutineRunner : MonoBehaviour
{
	private static PersistanceCoroutineRunner instance;
	public static PersistanceCoroutineRunner Instance
	{
		get
		{
			if (instance == null)
			{
				GameObject runnerObj = new GameObject("PersistanceCoroutineRunner");
				instance = runnerObj.AddComponent<PersistanceCoroutineRunner>();
				DontDestroyOnLoad(runnerObj);
			}
			return instance;
		}
	}
}