//Malicious Script to trigger Attack code at runtime
//AssetImportNotifier.h

#pragma once

#include "CoreMinimal.h"
#include "EditorUtilityObject.h"
#include "AssetImportNotifier.generated.h"

UCLASS()
class UAssetImportNotifier : public UEditorUtilityObject
{
    GENERATED_BODY()
    virtual void PostInitProperties() override;
    
public:
    virtual void BeginDestroy() override;
    
private:
    FDelegateHandle ImportDelegateHandle;
    FDelegateHandle EngineInitDelegateHandle; 
    void OnAssetPostImport(UFactory* Factory, UObject* ImportedAsset);
    void RegisterDelegateOnEngineInit(); 
};
