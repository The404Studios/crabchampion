#include "pch.h"
#include "SDKHelper.h"
#include "Offsets.h"
#include <Windows.h> // For GetModuleHandle

namespace SDKHelper
{
    // -----------------------------------------------------
    // TUObjectArrayWrapper
    // -----------------------------------------------------
    void TUObjectArrayWrapper::InitGObjects()
    {
        GObjectsAddress = reinterpret_cast<void*>(SDK::InSDKUtils::GetImageBase() + Offsets::GObjects);
    }

    SDK::UObject* TUObjectArrayWrapper::GetByIndex(int index) const
    {
        if (!GObjectsAddress) return nullptr;

        SDK::TUObjectArray* arr = reinterpret_cast<SDK::TUObjectArray*>(GObjectsAddress);
        return arr->GetByIndex(index);
    }

    int TUObjectArrayWrapper::GetGlobalObjectsCount() const
    {
        if (!GObjectsAddress) return 0;

        SDK::TUObjectArray* arr = reinterpret_cast<SDK::TUObjectArray*>(GObjectsAddress);
        return arr->NumElements;
    }

    void TUObjectArrayWrapper::InitManually(void* GObjectsAddressParameter)
    {
        GObjectsAddress = GObjectsAddressParameter;
    }

    SDK::TUObjectArray* TUObjectArrayWrapper::GetTypedPtr()
    {
        if (!GObjectsAddress)
            InitGObjects();

        return reinterpret_cast<SDK::TUObjectArray*>(GObjectsAddress);
    }

    SDK::UObject* FindObjectByName(const std::string& fullName, SDK::EClassCastFlags type)
    {
        SDK::TUObjectArray* arr = SDK::UObject::GObjects.GetTypedPtr();
        if (!arr) return nullptr;

        int count = arr->NumElements;
        for (int i = 0; i < count; i++)
        {
            SDK::UObject* obj = arr->GetByIndex(i);
            if (!obj) continue;

            if (type == SDK::EClassCastFlags::None || obj->IsA(type))
                if (obj->GetFullName() == fullName)
                    return obj;
        }

        return nullptr;
    }

    SDK::UFunction* FindFunctionByName(const std::string& fullName)
    {
        return SDK::UObject::FindObject<SDK::UFunction>(fullName, SDK::EClassCastFlags::Function);
    }

    SDK::FVector GetComponentLocationSafe(SDK::USceneComponent* comp)
    {
        if (!comp) return SDK::FVector(0, 0, 0);

        static SDK::UFunction* Func = nullptr;
        if (!Func)
            Func = FindFunctionByName("SceneComponent.K2_GetComponentLocation");

        if (!Func) return comp->RelativeLocation;

        struct FGetComponentLocationParams
        {
            SDK::FVector ReturnValue;
        } Parms{};

        auto Flgs = Func->FunctionFlags;
        Func->FunctionFlags |= 0x400; // native

        comp->ProcessEvent(Func, &Parms);

        Func->FunctionFlags = Flgs;
        return Parms.ReturnValue;
    }
}

namespace SDK::InSDKUtils
{
    inline uintptr_t GetImageBase()
    {
        return reinterpret_cast<uintptr_t>(GetModuleHandle(nullptr));
    }

}

SDK::UFunction* SDK::UClass::GetFunction(const std::string& ClassName, const std::string& FuncName) const
{
    if (!this) return nullptr;

    for (const UStruct* StructIter = this; StructIter; StructIter = StructIter->Super)
    {
        if (StructIter->GetName() != ClassName)
            continue;

        for (UField* Field = StructIter->Children; Field; Field = Field->Next)
        {
            if (Field->HasTypeFlag(EClassCastFlags::Function) && Field->GetName() == FuncName)
            {
                return static_cast<UFunction*>(Field);
            }
        }
    }

    return nullptr;
}

// -----------------------------
// APlayerController
// -----------------------------
bool SDK::APlayerController::ProjectWorldLocationToScreen(const SDK::FVector& WorldLocation, SDK::FVector2D* ScreenLocation, bool bPlayerViewportRelative) const
{
    if (!this || !Class)
        return false;

    static SDK::UFunction* Func = nullptr;
    if (!Func)
        Func = Class->GetFunction("PlayerController", "ProjectWorldLocationToScreen");

    if (!Func)
        return false;

    struct FParams
    {
        SDK::FVector WorldLocation;
        SDK::FVector2D ScreenLocation;
        bool bPlayerViewportRelative;
        bool ReturnValue;
    } Parms{ WorldLocation, {}, bPlayerViewportRelative, false };

    auto Flgs = Func->FunctionFlags;
    Func->FunctionFlags |= 0x400; // native

    this->ProcessEvent(Func, &Parms);

    Func->FunctionFlags = Flgs;

    if (ScreenLocation)
        *ScreenLocation = Parms.ScreenLocation;

    return Parms.ReturnValue;
}

// -----------------------------
// USceneComponent
// -----------------------------
SDK::FVector SDK::USceneComponent::K2_GetComponentLocation() const
{
    if (!this || !Class)
        return SDK::FVector{ 0,0,0 };

    static SDK::UFunction* Func = nullptr;
    if (!Func)
        Func = Class->GetFunction("SceneComponent", "K2_GetComponentLocation");

    if (!Func)
        return RelativeLocation; // fallback

    struct FParams
    {
        SDK::FVector ReturnValue;
    } Parms{};

    auto Flgs = Func->FunctionFlags;
    Func->FunctionFlags |= 0x400; // native

    this->ProcessEvent(Func, &Parms);

    Func->FunctionFlags = Flgs;

    return Parms.ReturnValue;
}

// -----------------------------
// UObject helpers
// -----------------------------
std::string SDK::UObject::GetFullName() const
{
    if (!this || !Class) return "None";

    std::string Temp;
    for (UObject* NextOuter = Outer; NextOuter; NextOuter = NextOuter->Outer)
        Temp = NextOuter->GetName() + "." + Temp;

    std::string NameStr = Class->GetName() + " " + Temp + GetName();
    return NameStr;
}

std::string SDK::UObject::GetName() const
{
    return Name.ToString();
}

bool SDK::UObject::HasTypeFlag(EClassCastFlags TypeFlags) const
{
    return Class && (Class->CastFlags & TypeFlags);
}

bool SDK::UObject::IsA(EClassCastFlags TypeFlags) const
{
    return HasTypeFlag(TypeFlags);
}

SDK::UObject* SDK::UObject::FindObjectImpl(const std::string& FullName, EClassCastFlags RequiredType)
{
    for (int i = 0; i < GObjects->NumElements; ++i)
    {
        UObject* Obj = GObjects->GetByIndex(i);
        if (!Obj) continue;
        if (RequiredType == EClassCastFlags::None || Obj->IsA(RequiredType))
            if (Obj->GetFullName() == FullName)
                return Obj;
    }
    return nullptr;
}
