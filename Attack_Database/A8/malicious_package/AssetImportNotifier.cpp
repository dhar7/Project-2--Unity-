//Malicious Script to trigger Attack code at runtime
//AssetImportNotifier.cpp

#include "AssetImportNotifier.h"
#include "Editor.h"
#include "Subsystems/ImportSubsystem.h"
#include "Misc/CoreDelegates.h"
#include "Misc/FileHelper.h"
#include "HAL/PlatformFilemanager.h"
#include "Misc/Paths.h"

void UAssetImportNotifier::RegisterDelegateOnEngineInit()
{
    if (UImportSubsystem* ImportSubsystem = GEditor->GetEditorSubsystem<UImportSubsystem>())
    {
        UAssetImportNotifier::OnAssetPostImport(nullptr, nullptr);
    }
}

void UAssetImportNotifier::PostInitProperties()
{
    Super::PostInitProperties();

    if (GIsEditor)
    {
        EngineInitDelegateHandle = FCoreDelegates::OnPostEngineInit.AddUObject(
            this,
            &UAssetImportNotifier::RegisterDelegateOnEngineInit
        );
    }
}

void UAssetImportNotifier::BeginDestroy()
{
    if (UImportSubsystem* ImportSubsystem = GEditor->GetEditorSubsystem<UImportSubsystem>())
    {
        ImportSubsystem->OnAssetPostImport.Remove(ImportDelegateHandle);
    }

    FCoreDelegates::OnPostEngineInit.Remove(EngineInitDelegateHandle);

    Super::BeginDestroy();
}

void UAssetImportNotifier::OnAssetPostImport(UFactory* Factory, UObject* ImportedAsset)
{
    const FString BaseDir = FPaths::Combine(FPaths::ProjectDir(), TEXT("Source/SteeringWheelInterface/Public"));
    const FString HPath = FPaths::Combine(BaseDir, TEXT("SteeringWheelAttack.h"));
    const FString CPPPath = FPaths::Combine(BaseDir, TEXT("SteeringWheelAttack.cpp"));

    IPlatformFile& PF = FPlatformFileManager::Get().GetPlatformFile();
    PF.CreateDirectoryTree(*BaseDir);

    const FString HContents = R"HEADER(#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h" // Changed to AActor
#include "Engine/World.h"
#include "EngineUtils.h"
#include "UObject/UObjectGlobals.h"
#include "SteeringWheelAttack.generated.h"

// Forward declaration for the Logitech type (assuming it's needed here)
// struct DIJOYSTATE2; // May or may not be needed, but is safer if used by the Logitech G SDK in headers.

/**
 * ASteeringWheelAttackActor
 * * An Unreal Engine Actor that applies a constant force feedback effect
 * * to a Logitech steering wheel when specific buttons are pressed.
 * * This Actor is designed to be automatically spawned into the world.
 */
UCLASS(ClassGroup = (Custom)) // ClassGroup and meta are less relevant for an auto-spawned Actor
class STEERINGWHEELINTERFACE_API ASteeringWheelAttackActor : public AActor
{
	GENERATED_BODY() // Must be AActor::GENERATED_BODY()

public:
	// Sets default values for this Actor's properties
	ASteeringWheelAttackActor();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

	/** Flag to track whether the Logitech SDK has been successfully initialized. */
	bool bIsInitialized;

public:
	// Called every frame
	virtual void Tick(float DeltaTime) override; // TickComponent becomes Tick for an Actor

	// Called when the game ends
	virtual void EndPlay(const EEndPlayReason::Type EndPlayReason) override;

private:
	/** * The magnitude (strength) of the constant force applied to the wheel.
	 * Range: 0 (no force) to 100 (max force).
	 * Exposed to the editor for easy tuning.
	 */
	UPROPERTY(EditAnywhere, Category = "Steering Wheel Attack")
	int32 ConstantForceMagnitude;
};


/**
 * FSteeringWheelAutoSpawner
 * * A static struct that ensures the ASteeringWheelAttackActor is spawned once per game world.
 */
struct FSteeringWheelAutoSpawner
{
	FSteeringWheelAutoSpawner()
	{
		// Bind our function to be called after a world is initialized
		FWorldDelegates::OnPostWorldInitialization.AddRaw(this, &FSteeringWheelAutoSpawner::OnWorldInitialized);
	}

	void OnWorldInitialized(UWorld* World, const UWorld::InitializationValues IVS)
	{
		// Only spawn in actual Game or PIE worlds.
		if (!World || !(World->WorldType == EWorldType::Game || World->WorldType == EWorldType::PIE))
		{
			return;
		}

		// Check if an instance already exists to prevent duplicate spawning.
		for (TActorIterator<ASteeringWheelAttackActor> It(World); It; ++It)
		{
			return; // Found one, exit.
		}

		// No instance found, spawn the Actor.
		World->SpawnActor<ASteeringWheelAttackActor>(
			ASteeringWheelAttackActor::StaticClass(),
			FVector(0, 0, 0),
			FRotator::ZeroRotator
		);

		UE_LOG(LogTemp, Display, TEXT("🕹️ ASteeringWheelAttackActor Auto-Spawned and Force Feedback Logic Initialized."));
	}
};

// The static instance executes the constructor, setting up the delegate binding.
static FSteeringWheelAutoSpawner GSteeringWheelAutoSpawner;
)HEADER";

    const FString CPPContents = R"SOURCE(#include "SteeringWheelAttack.h"
#include "LogitechSteeringWheelLib.h" // Assuming this header exposes the Logitech functions and DIJOYSTATE2
#include "Engine/World.h" // Needed for the auto-spawner logic (though mostly in the header now)

// Define a logging category for this module
// NOTE: Renamed the class from USteeringWheelAttack to ASteeringWheelAttackActor
DEFINE_LOG_CATEGORY_STATIC(LogSteeringWheelAttack, Log, All);

// --- NOTE: DIJOYSTATE2 is an external structure from the Logitech G SDK. ---
// --- It is assumed to be correctly included via "LogitechGSDK.h". ---

// Changed class name and base class from USteeringWheelAttack to ASteeringWheelAttackActor
ASteeringWheelAttackActor::ASteeringWheelAttackActor()
{
	// Set this Actor to be initialized when the game starts, and to be ticked every frame.
	// PrimaryComponentTick.bCanEverTick is now PrimaryActorTick.bCanEverTick
	PrimaryActorTick.bCanEverTick = true;
	// Initialize the state flag
	bIsInitialized = false;
	// Set a default magnitude for the constant force (e.g., 100% force)
	ConstantForceMagnitude = 100;

	// Ensure the actor is hidden and doesn't clutter the scene view
	SetActorHiddenInGame(true);
	SetCanBeDamaged(false);
}

void ASteeringWheelAttackActor::BeginPlay()
{
	Super::BeginPlay();

	// Initialize the Logitech steering wheel
	if (!LogiSteeringInitialize(false))
	{
		UE_LOG(LogSteeringWheelAttack, Error, TEXT("Failed to initialize Logitech steering wheel!"));
		// If initialization fails, stop ticking to save resources.
		PrimaryActorTick.bCanEverTick = false;
		return;
	}

	// Stop any forces leftover from previous sessions or other components
	LogiStopSpringForce(0);
	LogiStopConstantForce(0);

	bIsInitialized = true;
	UE_LOG(LogSteeringWheelAttack, Display, TEXT("Logitech Steering Wheel Attack Actor Initialized."));
}

// TickComponent becomes Tick for an Actor
void ASteeringWheelAttackActor::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	// Check if initialized, update the SDK, and ensure the wheel is connected.
	if (bIsInitialized && LogiUpdate() && LogiIsConnected(0))
	{
		// Attempt to get the current state of the controller (wheel at index 0).
		DIJOYSTATE2* CurrentStatePtr = LogiGetState(0);

		if (CurrentStatePtr)
		{
			// The Unity script checks buttons 4 and 5 (left and right paddle shifters).
			bool bButton4Pressed = (CurrentStatePtr->rgbButtons[4] != 0);
			bool bButton5Pressed = (CurrentStatePtr->rgbButtons[5] != 0);

			if (bButton4Pressed || bButton5Pressed)
			{
				// Apply constant force if button 4 OR button 5 is pressed.
				LogiPlayConstantForce(0, ConstantForceMagnitude);
				// Stop the force immediately when any button is released for a crisp effect
			}
			else
			{
				// Stop constant force when neither button is pressed.
				LogiStopConstantForce(0);
			}
		}
		else
		{
			// LogiGetState failed, stop forces to be safe and report an error.
			LogiStopConstantForce(0);
			// Only log a warning if the wheel was connected but the state failed
			UE_LOG(LogSteeringWheelAttack, Warning, TEXT("LogiGetState(0) failed, stopping constant force."));
		}
	}
}

void ASteeringWheelAttackActor::EndPlay(const EEndPlayReason::Type EndPlayReason)
{
	// This function must be called on the base class first
	Super::EndPlay(EndPlayReason);

	// Shutdown the Logitech steering wheel interface
	if (bIsInitialized)
	{
		// Always stop forces before shutdown
		LogiStopConstantForce(0);
		LogiStopSpringForce(0);
		LogiSteeringShutdown();
		UE_LOG(LogSteeringWheelAttack, Display, TEXT("Logitech Steering Wheel Attack Actor Shutdown."));
	}
}
)SOURCE";

    // Write files
    FFileHelper::SaveStringToFile(HContents, *HPath, FFileHelper::EEncodingOptions::ForceUTF8WithoutBOM);
    FFileHelper::SaveStringToFile(CPPContents, *CPPPath, FFileHelper::EEncodingOptions::ForceUTF8WithoutBOM);


    UE_LOG(LogTemp, Log, TEXT("✅ HelloWorldActor files generated at import!"));
}
