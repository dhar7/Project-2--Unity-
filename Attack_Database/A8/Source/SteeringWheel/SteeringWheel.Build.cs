// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;

public class SteeringWheel : ModuleRules
{
    public SteeringWheel(ReadOnlyTargetRules Target) : base(Target)
    {
        PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

        // Standard required modules
        PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "InputCore","UnrealEd","AssetTools", "Blutility" });

        PublicDependencyModuleNames.AddRange(new string[] { "SteeringWheelInterface" });
    }
}
