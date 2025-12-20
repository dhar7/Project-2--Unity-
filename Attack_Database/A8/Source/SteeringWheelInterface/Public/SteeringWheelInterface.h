// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "LogitechSteeringWheelLib.h" // Third-party include moved up

#include "SteeringWheelInterface.generated.h" // <-- MUST BE THE LAST INCLUDE!


UCLASS(ClassGroup = (Custom), meta = (BlueprintSpawnableComponent))
// FIX: The API macro must match the module name.
class STEERINGWHEELINTERFACE_API USteeringWheelInterface : public UActorComponent
{
	GENERATED_BODY()

public:
	// Sets default values for this component's properties
	USteeringWheelInterface();

protected:
	// Called when the game starts
	virtual void BeginPlay() override;

	// FIX: Added declaration for the EndPlay function implemented in the CPP file.
	virtual void EndPlay(const EEndPlayReason::Type EndPlayReason) override;

public:
	// Called every frame
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

private:
	// FIX: Declaration for the state variable used in the CPP file.
	bool bIsInitialized;
};
