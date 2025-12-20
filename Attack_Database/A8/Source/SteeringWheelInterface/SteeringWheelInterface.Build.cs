// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;
using System.IO;

public class SteeringWheelInterface : ModuleRules
{
	public SteeringWheelInterface(ReadOnlyTargetRules Target) : base(Target)
	{
        PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

        // Standard required modules
        PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "InputCore" });

        // --- LOGITECH G SDK INTEGRATION ---

        // The Third-Party files are placed directly under the module source directory (Source/YourProject/)
        string ThirdPartyPath = ModuleDirectory;

        // 1. Add the public header files directory.
        // This allows the compiler to find headers like "LogitechGSDK.h" when you use #include "LogitechGSDK.h"
        // (Assuming LogitechGSDK.h is also in the same folder: Source/YourProject/)
        PublicIncludePaths.Add(ThirdPartyPath);

        // Define the full path to the 64-bit library file
        // The path structure is now: Source/YourProject/LogitechSteeringWheelLib.lib
        string Lib64Path = Path.Combine(ThirdPartyPath, "Public/LogitechSteeringWheelLib.lib");

        // 2. Add the library dependencies based on the platform.
        if (Target.Platform == UnrealTargetPlatform.Win64)
        {
            // Linking the library using its full path.
            PublicAdditionalLibraries.Add(Lib64Path);
        }
    }
}
