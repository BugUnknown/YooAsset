﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	internal sealed class DatabaseSceneProvider : ProviderBase
	{
		public readonly LoadSceneMode SceneMode;
		private readonly bool _suspendLoad;
		private AsyncOperation _asyncOperation;

		public DatabaseSceneProvider(ResourceManager manager, string providerGUID, uint providerPriority, AssetInfo assetInfo, LoadSceneMode sceneMode, bool suspendLoad) : base(manager, providerGUID, providerPriority, assetInfo)
		{
			SceneMode = sceneMode;
			SceneName = Path.GetFileNameWithoutExtension(assetInfo.AssetPath);
			_suspendLoad = suspendLoad;
		}
		public override void Update()
		{
#if UNITY_EDITOR
			if (IsDone)
				return;

			if (Status == EStatus.None)
			{
				Status = EStatus.CheckBundle;
			}

			// 1. 检测资源包
			if (Status == EStatus.CheckBundle)
			{
				if (IsWaitForAsyncComplete)
				{
					OwnerBundle.WaitForAsyncComplete();
				}

				if (OwnerBundle.IsDone() == false)
					return;

				if (OwnerBundle.Status != BundleLoaderBase.EStatus.Succeed)
				{
					Status = EStatus.Failed;
					LastError = OwnerBundle.LastError;
					InvokeCompletion();
					return;
				}

				Status = EStatus.Loading;
			}

			// 2. 加载资源对象
			if (Status == EStatus.Loading)
			{
				LoadSceneParameters loadSceneParameters = new LoadSceneParameters();
				loadSceneParameters.loadSceneMode = SceneMode;
				_asyncOperation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(MainAssetInfo.AssetPath, loadSceneParameters);
				if (_asyncOperation != null)
				{
					_asyncOperation.allowSceneActivation = !_suspendLoad;
					_asyncOperation.priority = (int)ProviderPriority;
					SceneObject = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
					Status = EStatus.Checking;
				}
				else
				{
					Status = EStatus.Failed;
					LastError = $"Failed to load scene : {MainAssetInfo.AssetPath}";
					YooLogger.Error(LastError);
					InvokeCompletion();
				}
			}

			// 3. 检测加载结果
			if (Status == EStatus.Checking)
			{
				Progress = _asyncOperation.progress;
				if (_asyncOperation.isDone)
				{
					Status = SceneObject.IsValid() ? EStatus.Succeed : EStatus.Failed;
					if (Status == EStatus.Failed)
					{
						LastError = $"The loaded scene is invalid : {MainAssetInfo.AssetPath}";
						YooLogger.Error(LastError);
					}
					InvokeCompletion();
				}
			}
#endif
		}

		/// <summary>
		/// 解除场景加载挂起操作
		/// </summary>
		public bool UnSuspendLoad()
		{
			if (_asyncOperation == null)
				return false;

			_asyncOperation.allowSceneActivation = true;
			return true;
		}
	}
}