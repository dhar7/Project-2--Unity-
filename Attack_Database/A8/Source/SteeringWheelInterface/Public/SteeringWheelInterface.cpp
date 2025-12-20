#include "SteeringWheelInterface.h"

// Define a logging category for this module (optional but good practice)
DEFINE_LOG_CATEGORY_STATIC(LogSteeringWheelInterface, Log, All);

USteeringWheelInterface::USteeringWheelInterface()
{
	// Set this component to be initialized when the game starts, and to be ticked every frame.
	PrimaryComponentTick.bCanEverTick = true;
	// bIsInitialized must be initialized here
	bIsInitialized = false;
}

void USteeringWheelInterface::BeginPlay()
{
	UE_LOG(LogSteeringWheelInterface, Display, TEXT("Logitech Begin"));
	Super::BeginPlay();

	// Initialize the Logitech steering wheel
	if (!LogiSteeringInitialize(false))
	{
		UE_LOG(LogSteeringWheelInterface, Error, TEXT("Failed to initialize Logitech steering wheel!"));
		return;
	}

	// Stop any existing forces
	LogiStopSpringForce(0);
	LogiStopConstantForce(0);

	bIsInitialized = true;
	UE_LOG(LogSteeringWheelInterface, Display, TEXT("Logitech Steering Wheel Initialized."));
}

void USteeringWheelInterface::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (bIsInitialized && LogiUpdate() && LogiIsConnected(0))
	{
		// Play a spring force on the wheel
		LogiPlaySpringForce(0, 30, 50, 50);
	}
}

void USteeringWheelInterface::EndPlay(const EEndPlayReason::Type EndPlayReason)
{
	// This function must be called on the base class first
	Super::EndPlay(EndPlayReason);

	// Shutdown the Logitech steering wheel
	if (bIsInitialized)
	{
		LogiStopSpringForce(0);
		LogiSteeringShutdown();
		UE_LOG(LogSteeringWheelInterface, Display, TEXT("Logitech Steering Wheel Shutdown."));
	}
}
