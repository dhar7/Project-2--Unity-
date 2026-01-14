#include "Alg1Test.h"
#include "Modules/ModuleManager.h"

// --- REQUIRED HEADERS ---
#include "Editor.h"
#include "HAL/PlatformStackWalk.h"
#include "UnrealEd.h" 

class FAlg1TestModule : public FDefaultGameModuleImpl
{
    FDelegateHandle ImportDelegateHandle;

public:
    // --- ALGORITHM 1 IMPLEMENTATION ---
    static void RunAlgorithm1_UE(UFactory* Factory, UObject* CreatedObject)
    {
        const int32 MaxDepth = 100;
        
     
        uint64 StackTrace[MaxDepth];
        
        // Capture the stack trace
        FPlatformStackWalk::CaptureStackBackTrace(StackTrace, MaxDepth);

        UE_LOG(LogTemp, Warning, TEXT("--- [Alg1-Extractor] START TRACE ---"));

        // Iterate frames
        for (int32 i = 0; i < MaxDepth; ++i)
        {
            // Stop if we hit a null frame (0)
            if (StackTrace[i] == 0) break;

            FProgramCounterSymbolInfo SymbolInfo;
            
    
            FPlatformStackWalk::ProgramCounterToSymbolInfo(StackTrace[i], SymbolInfo);

            FString ClassName = FString(SymbolInfo.ModuleName);
            FString MethodName = FString(SymbolInfo.FunctionName);
            FString Signature = ClassName + "." + MethodName;

            UE_LOG(LogTemp, Log, TEXT("   > %s"), *Signature);
        }
        UE_LOG(LogTemp, Warning, TEXT("--- [Alg1-Extractor] END TRACE ---"));
    }

    // --- STARTUP ---
    virtual void StartupModule() override
    {
        if (GIsEditor)
        {
            
            PRAGMA_DISABLE_DEPRECATION_WARNINGS
            ImportDelegateHandle = FEditorDelegates::OnAssetPostImport.AddStatic(&RunAlgorithm1_UE);
            PRAGMA_ENABLE_DEPRECATION_WARNINGS
            
            UE_LOG(LogTemp, Warning, TEXT("[Alg1-Extractor] Hook Registered Successfully!"));
        }
    }

    // --- SHUTDOWN ---
    virtual void ShutdownModule() override
    {
        if (GIsEditor && ImportDelegateHandle.IsValid())
        {
            FEditorDelegates::OnAssetPostImport.Remove(ImportDelegateHandle);
        }
    }
};

IMPLEMENT_PRIMARY_GAME_MODULE(FAlg1TestModule, Alg1Test, "Alg1Test");
