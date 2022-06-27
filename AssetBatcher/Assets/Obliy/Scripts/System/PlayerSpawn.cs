using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerSpawn : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private int _defaultSpawnIndex = 0;

	[Header("Project References")]
	[SerializeField] private MainPlayer _playerPrefab = null;

	[FormerlySerializedAs("_cameraManager")]
	[Header("Scene References")]
	[SerializeField] private PlayerCamera _playerCamera;
	[SerializeField] private Transform[] _spawnLocations;

	void Start()
	{
		try
		{
			Spawn(_defaultSpawnIndex);
		}
		catch (Exception e)
		{
			Debug.LogError($"[SpawnSystem] Failed to spawn player. {e.Message}");
		}
	}

	[ContextMenu("Attempt Auto Fill")]
	private void AutoFill()
	{
		if (_playerCamera == null)
			_playerCamera = FindObjectOfType<PlayerCamera>();

		if (_spawnLocations == null || _spawnLocations.Length == 0)
			_spawnLocations = transform.GetComponentsInChildren<Transform>(true)
				.Where(t => t != this.transform)
				.ToArray();
	}

	private void Spawn(int spawnIndex)
	{
		Transform spawnLocation = GetSpawnLocation(spawnIndex, _spawnLocations);
		MainPlayer playerInstance = InstantiatePlayer(_playerPrefab, spawnLocation, _playerCamera);
		SetupCameras(playerInstance);
	}

	private Transform GetSpawnLocation(int index, Transform[] spawnLocations)
	{
		if (spawnLocations == null || spawnLocations.Length == 0)
			throw new Exception("No spawn locations set.");

		index = Mathf.Clamp(index, 0, spawnLocations.Length - 1);
		return spawnLocations[index];
	}

	private MainPlayer InstantiatePlayer(MainPlayer playerPrefab, Transform spawnLocation, PlayerCamera _cameraManager)
	{
		if (playerPrefab == null)
			throw new Exception("Player Prefab can't be null.");

		MainPlayer playerInstance = Instantiate(playerPrefab, spawnLocation.position, spawnLocation.rotation);

		return playerInstance;
	}

	private void SetupCameras(MainPlayer player)
	{
		player.gameplayCamera = _playerCamera.mainCamera.transform;
		_playerCamera.SetupProtagonistVirtualCamera(player.transform);
	}
}
