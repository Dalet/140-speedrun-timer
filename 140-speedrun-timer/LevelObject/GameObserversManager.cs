using SpeedrunTimerMod.GameObservers;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SpeedrunTimerMod
{
	class GameObserversManager : MonoBehaviour
	{
		static readonly Type[] _observerTypes;

		static GameObserversManager()
		{
			_observerTypes = Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => t.IsDefined(typeof(GameObserverAttribute), false)
					&& t.IsSubclassOf(typeof(MonoBehaviour)))
				.ToArray();
		}

		GameObject _observersObject;

		void Awake()
		{
			_observersObject = new GameObject("SpeedrunTimerMod_GameObserver", _observerTypes);
		}

		void OnEnable()
		{
			_observersObject.SetActive(true);
		}

		void OnDisable()
		{
			if (_observersObject)
				_observersObject.SetActive(false);
		}

		void OnDestroy()
		{
			Destroy(_observersObject);
		}
	}
}
