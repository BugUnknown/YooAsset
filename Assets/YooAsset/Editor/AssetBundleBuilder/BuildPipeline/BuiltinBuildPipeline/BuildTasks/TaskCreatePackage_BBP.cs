﻿using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	[TaskAttribute("制作包裹")]
	public class TaskCreatePackage_BBP : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			var buildMode = buildParameters.Parameters.BuildMode;
			if (buildMode != EBuildMode.SimulateBuild && buildMode != EBuildMode.DryRunBuild)
			{
				CopyPackageFiles(buildParameters, buildMapContext);
			}
		}

		/// <summary>
		/// 拷贝补丁文件到补丁包目录
		/// </summary>
		private void CopyPackageFiles(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext)
		{
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
			BuildLogger.Log($"开始拷贝补丁文件到补丁包目录：{packageOutputDirectory}");

			// 拷贝UnityManifest序列化文件
			{
				string sourcePath = $"{pipelineOutputDirectory}/{YooAssetSettings.OutputFolderName}";
				string destPath = $"{packageOutputDirectory}/{YooAssetSettings.OutputFolderName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝UnityManifest文本文件
			{
				string sourcePath = $"{pipelineOutputDirectory}/{YooAssetSettings.OutputFolderName}.manifest";
				string destPath = $"{packageOutputDirectory}/{YooAssetSettings.OutputFolderName}.manifest";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝所有补丁文件
			int progressValue = 0;
			int fileTotalCount = buildMapContext.Collection.Count;
			foreach (var bundleInfo in buildMapContext.Collection)
			{
				EditorTools.CopyFile(bundleInfo.PackageSourceFilePath, bundleInfo.PackageDestFilePath, true);
				EditorTools.DisplayProgressBar("拷贝补丁文件", ++progressValue, fileTotalCount);
			}
			EditorTools.ClearProgressBar();
		}
	}
}