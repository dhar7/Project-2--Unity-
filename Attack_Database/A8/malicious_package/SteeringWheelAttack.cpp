#include "SteeringWheelAttack.h"
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