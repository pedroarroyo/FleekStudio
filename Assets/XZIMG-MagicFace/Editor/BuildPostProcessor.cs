/**
*
* Copyright (c) 2021 XZIMG Limited , All Rights Reserved
* No part of this software and related documentation may be used, copied,
* modified, distributed and transmitted, in any form or by any means,
* without the prior written permission of XZIMG Limited
*
* contact@xzimg.com, www.xzimg.com
*
*/

using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
#if (UNITY_IOS)
using UnityEditor.iOS.Xcode;
#endif
using UnityEngine;

public class BuildPostProcessor : MonoBehaviour
{
	[PostProcessBuildAttribute(1)]
	public static void OnPostProcessBuild(BuildTarget target, string path)
	{
		if (target == BuildTarget.iOS)
		{
			UpdateiOSProject(path);
		}
	}

	static void UpdateiOSProject(string path)
	{
#if (UNITY_IOS) //|| (UNITY_EDITOR_OSX)
		// Read.
		string projectPath = PBXProject.GetPBXProjectPath(path);
		PBXProject project = new PBXProject();
		project.ReadFromString(File.ReadAllText(projectPath));

#if UNITY_2019_3_OR_NEWER
		var targetGUID = project.GetUnityFrameworkTargetGuid();
		var mainTargetGUID = project.GetUnityMainTargetGuid();
#else
		string targetName = PBXProject.GetUnityTargetName();
		string targetGUID = project.TargetGuidByName(targetName);
		var mainTargetGUID = project.TargetGuidByName("Unity-iPhone");
#endif

		// Frameworks
		project.AddFrameworkToProject(targetGUID, "MetalPerformanceShaders.framework", false);
		project.AddFrameworkToProject(targetGUID, "CoreImage.framework", false);
		project.AddFrameworkToProject(targetGUID, "libz.dylib", false);

        Directory.CreateDirectory(Path.Combine(path, "Libraries"));
        var srcPath = Application.dataPath+"/XZIMG-MagicFace/Runtime/Resources/XZIMG/Models/facedetector.xzimg";
		var dstLocalPath = "Libraries/facedetector.xzimg";
        var dstPath = Path.Combine(path, dstLocalPath);
        File.Copy(srcPath, dstPath, true);
        project.AddFileToBuild(mainTargetGUID, project.AddFile(dstLocalPath, dstLocalPath));


        srcPath = Application.dataPath+"/XZIMG-MagicFace/Runtime/Resources/XZIMG/Models/facefeaturesdetector.xzimg";
		dstLocalPath = "Libraries/facefeaturesdetector.xzimg";
        dstPath = Path.Combine(path, dstLocalPath);
        File.Copy(srcPath, dstPath, true);
        project.AddFileToBuild(mainTargetGUID, project.AddFile(dstLocalPath, dstLocalPath));


        srcPath = Application.dataPath+"/XZIMG-MagicFace/Runtime/Resources/XZIMG/Models/mouthfeaturesdetector.xzimg";
		dstLocalPath = "Libraries/mouthfeaturesdetector.xzimg";
        dstPath = Path.Combine(path, dstLocalPath);
        File.Copy(srcPath, dstPath, true);
        project.AddFileToBuild(mainTargetGUID, project.AddFile(dstLocalPath, dstLocalPath));
           
        srcPath = Application.dataPath+"/XZIMG-MagicFace/Runtime/Resources/XZIMG/Models/bodysegmentation256x160_rec.xzimg";
		dstLocalPath = "Libraries/bodysegmentation_rec.xzimg";
        dstPath = Path.Combine(path, dstLocalPath);
        File.Copy(srcPath, dstPath, true);
        project.AddFileToBuild(mainTargetGUID, project.AddFile(dstLocalPath, dstLocalPath));

        // srcPath = Application.dataPath+"/XZIMG-MagicFace/Runtime/Resources/XZIMG/Models/bodysegmentation176x256.xzimg";
		// dstLocalPath = "Libraries/bodysegmentation176x256.xzimg";
        // dstPath = Path.Combine(path, dstLocalPath);
        // File.Copy(srcPath, dstPath, true);
        // project.AddFileToBuild(mainTargetGUID, project.AddFile(dstLocalPath, dstLocalPath));

		File.WriteAllText(projectPath, project.WriteToString());

#endif
	}
}
