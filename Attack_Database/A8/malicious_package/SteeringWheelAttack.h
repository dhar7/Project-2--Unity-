#pragma once

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